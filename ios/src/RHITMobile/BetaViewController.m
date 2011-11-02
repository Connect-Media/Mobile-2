//
//  BetaViewController.m
//  RHIT Mobile Campus Directory
//
//  Copyright 2011 Rose-Hulman Institute of Technology
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

#include <sys/types.h>
#include <sys/sysctl.h>

#import "BetaViewController.h"
#import "RHBeta.h"
#import "CJSONDeserializer.h"
#import "NSDictionary_JSONExtensions.h"
#import "RHITMobileAppDelegate.h"
#import "BetaRegistrationViewController.h"

#ifdef RHITMobile_RHBeta

#define kBetaServer @"http://rhitmobilebeta.heroku.com"
#define kBetaUpdatePath @"/platform/ios/builds/current"
#define kBetaRegisterPath @"/device/register"

#define kBetaUpdateTimeDefault @"LastUpdateTime"
#define kBetaAuthTokenDefault @"AuthToken"
#define kBetaCurrentBuildDefault @"CurrentBuild"

#define kBetaApplicationVersionLabel @"Application Version"
#define kBetaApplicationVersionCell @"ApplicationVersionCell"
#define kBetaBuildNumberLabel @"Build Number"
#define kBetaBuildNumberCell @"BuildNumberCell"
#define kBetaBuildTypeLabel @"Build Type"
#define kBetaBuildTypeCell @"BuildTypeCell"
#define kBetaUpdateTimeLabel @"Last Updated"
#define kBetaUpdateTimeCell @"UpdateTimeCell"
#define kBetaAuthTokenLabel @"Beta Authentication Token"
#define kBetaAuthTokenCell @"AuthTokenCell"
#define kBetaMapDebugLabel @"Map Debugging Tools"
#define kBetaMapDebugCell @"MapDebugCell"

@interface BetaViewController ()

@property (nonatomic, retain) NSArray *sections;
@property (nonatomic, assign) BOOL checkingForUpdates;
@property (nonatomic, retain) NSString *authToken;
@property (nonatomic, retain) NSDate *updateDate;
@property (nonatomic, assign) NSInteger knownCurrentBuild;
@property (nonatomic, retain) NSOperationQueue *operations;

- (IBAction)switchInstallationType:(id)sender;
- (IBAction)checkForUpdates:(id)sender;
- (void)didFindNoUpdates;
- (void)didFindUpdateWithURL:(NSURL *)url;
- (void)performCheckForUpdates:(NSNumber *)official;
- (void)performNotificationOfUpdate;
- (void)setLoadingText:(NSString *)text;
- (void)clearLoadingText;

@end


@implementation BetaViewController

@synthesize registrationName;
@synthesize registrationEmail;

@synthesize sections;
@synthesize checkingForUpdates;
@synthesize updateDate;
@synthesize knownCurrentBuild;
@synthesize authToken;
@synthesize operations;

- (id)initWithNibName:(NSString *)nibNameOrNil
               bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        self.navigationItem.title = @"Beta Tools and Info";
        self.sections = [NSArray arrayWithObjects:kBetaApplicationVersionLabel,
                         kBetaBuildNumberLabel, kBetaBuildTypeLabel,
                         kBetaUpdateTimeLabel, kBetaAuthTokenLabel, nil];
        self.operations = [[[NSOperationQueue alloc] init] autorelease];
    }
    return self;
}

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)performInitialSetup {
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    self.authToken = [defaults stringForKey:kBetaAuthTokenDefault];
    
    if (self.authToken == nil) {
        BetaRegistrationViewController *registrationController = [[[BetaRegistrationViewController alloc] initWithNibName:@"BetaRegistrationView" bundle:nil] autorelease];
        registrationController.betaViewController = self;
        [self presentModalViewController:registrationController animated:YES];
    }
    
    double updateNumber = [defaults doubleForKey:kBetaUpdateTimeDefault];
    
    if (updateNumber == 0) {
        updateNumber = (double) [[NSDate date] timeIntervalSince1970];
    }
    
    self.knownCurrentBuild = [defaults integerForKey:kBetaCurrentBuildDefault];
    
    if (self.knownCurrentBuild != kRHBetaBuildNumber) {
        self.knownCurrentBuild = kRHBetaBuildNumber;
        [defaults setInteger:self.knownCurrentBuild forKey:kBetaCurrentBuildDefault];
        updateNumber = (double) [[NSDate date] timeIntervalSince1970];

        NSInvocationOperation* operation = [NSInvocationOperation alloc];
        operation = [[operation
                      initWithTarget:self
                      selector:@selector(performNotificationOfUpdate)
                      object:nil] autorelease];
        [self.operations addOperation:operation];
    }
    
    [defaults setDouble:updateNumber forKey:kBetaUpdateTimeDefault];
    self.updateDate = [NSDate dateWithTimeIntervalSince1970:updateNumber];
}

#pragma mark - View lifecycle

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
}

- (void)viewDidUnload {
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)io {
    // Return YES for supported orientations
    return (io == UIInterfaceOrientationPortrait);
}


#pragma mark - UITableViewDelegate Method

- (UIView *)tableView:(UITableView *)tableView
viewForFooterInSection:(NSInteger)section {
    NSString *sectionLabel = [self.sections objectAtIndex:section];
    
    if ([sectionLabel isEqualToString:kBetaBuildTypeLabel]) {
        UIView *parentView = [[[UIView alloc] initWithFrame:CGRectZero] autorelease];
        parentView.backgroundColor = [UIColor clearColor];
        
        UIButton *updateButton = [UIButton buttonWithType:UIButtonTypeRoundedRect];
        updateButton.frame = CGRectMake(10.0, 10.0, 300.0, 44.0);
        [updateButton addTarget:self
                         action:@selector(switchInstallationType:)
               forControlEvents:UIControlEventTouchUpInside];
        
        NSString *buttonTitle = nil;
        
        if (kRHBetaBuildType == kRHBetaBuildTypeOfficial) {
            buttonTitle = @"Switch to Bleeding Edge";
        } else {
            buttonTitle = @"Switch to Stable";
        }
        
        [updateButton setTitle:buttonTitle forState:UIControlStateNormal];
        [parentView addSubview:updateButton];
        return parentView;
    } else if ([sectionLabel isEqualToString:kBetaUpdateTimeLabel]) {
        UIView *parentView = [[[UIView alloc] initWithFrame:CGRectZero] autorelease];
        parentView.backgroundColor = [UIColor clearColor];
        
        UIButton *updateButton = [UIButton buttonWithType:UIButtonTypeRoundedRect];
        updateButton.frame = CGRectMake(10.0, 10.0, 300.0, 44.0);
        [updateButton addTarget:self
                         action:@selector(checkForUpdates:)
               forControlEvents:UIControlEventTouchUpInside];
        [updateButton setTitle:@"Check for Updates"
                      forState:UIControlStateNormal];
        
        [parentView addSubview:updateButton];
        
        return parentView; 
    }
    
    return nil;
}

-(CGFloat)tableView:(UITableView *)tableView
heightForFooterInSection:(NSInteger)section {
    NSString *sectionLabel = [self.sections objectAtIndex:section];
    
    if ([sectionLabel isEqualToString:kBetaBuildTypeLabel]) {
        return 64;
    } if ([sectionLabel isEqualToString:kBetaUpdateTimeLabel]) {
        return 64;
    }
    
    return 0;
}

#pragma mark - UITableViewDataSource Method

- (UITableViewCell *)tableView:(UITableView *)tableView
         cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    NSString *sectionLabel = [self.sections objectAtIndex:[indexPath indexAtPosition:0]];
    UITableViewCell *cell = nil;
    
    if (sectionLabel == kBetaApplicationVersionLabel) {
        cell = [tableView dequeueReusableCellWithIdentifier:kBetaApplicationVersionCell];
        
        if (cell == nil) {
            cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:kBetaApplicationVersionCell] autorelease];
            cell.selectionStyle = UITableViewCellSelectionStyleNone;
        }
        
        cell.textLabel.text = [[[NSBundle mainBundle]
                                infoDictionary]
                               objectForKey:@"CFBundleVersion"];
    } else if (sectionLabel == kBetaBuildNumberLabel) {
        cell = [tableView dequeueReusableCellWithIdentifier:kBetaBuildNumberCell];
        
        if (cell == nil) {
            cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:kBetaBuildNumberCell] autorelease];
            cell.selectionStyle = UITableViewCellSelectionStyleNone;
        }
        
        cell.textLabel.text = [[[NSString alloc] initWithFormat:@"%d",
                                kRHBetaBuildNumber] autorelease];
    } else if (sectionLabel == kBetaBuildTypeLabel) {
        cell = [tableView dequeueReusableCellWithIdentifier:kBetaBuildTypeCell];
        
        if (cell == nil) {
            cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:kBetaBuildTypeCell] autorelease];
            cell.selectionStyle = UITableViewCellSelectionStyleNone;
        }
        
        if (kRHBetaBuildType == kRHBetaBuildTypeOfficial) {
            cell.textLabel.text = @"Stable";
        } else {
            cell.textLabel.text = @"Bleeding Edge";
        }
    } else if (sectionLabel == kBetaUpdateTimeLabel) {
        cell = [tableView dequeueReusableCellWithIdentifier:kBetaUpdateTimeCell];
        
        if (cell == nil) {
            cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:kBetaUpdateTimeCell] autorelease];
            cell.selectionStyle = UITableViewCellSelectionStyleNone;
        }
        
        cell.textLabel.text = self.updateDate.description;
    } else if (sectionLabel == kBetaAuthTokenLabel) {
        cell = [tableView dequeueReusableCellWithIdentifier:kBetaAuthTokenCell];
        
        if (cell == nil) {
            cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:kBetaAuthTokenCell] autorelease];
            cell.selectionStyle = UITableViewCellSelectionStyleNone;
            cell.textLabel.font = [UIFont systemFontOfSize:14];
        }
        
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        cell.textLabel.text = [defaults stringForKey:kBetaAuthTokenDefault];
    }
    
    return cell;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return self.sections.count;
}

- (NSString *)tableView:(UITableView *)tableView
titleForHeaderInSection:(NSInteger)section {
    return [self.sections objectAtIndex:section];
}

- (NSInteger)tableView:(UITableView *)tableView
 numberOfRowsInSection:(NSInteger)section  {
    NSString *sectionLabel = [self.sections objectAtIndex:section];
    
    if (sectionLabel == kBetaApplicationVersionLabel) {
        return 1;
    } else if (sectionLabel == kBetaBuildNumberLabel) {
        return 1;
    } else if (sectionLabel == kBetaBuildTypeLabel) {
        return 1;
    } else if (sectionLabel == kBetaUpdateTimeLabel) {
        return 1;
    } else if (sectionLabel == kBetaAuthTokenLabel)  {
        return 1;
    }
    
    return 0;
}

#pragma mark - UIActionSheetDelegate Methods

- (void)actionSheet:(UIActionSheet *)actionSheet
didDismissWithButtonIndex:(NSInteger)buttonIndex {
    if (buttonIndex == 0) {
        [self setLoadingText:@"Fetching Update"];
        self.checkingForUpdates = YES;
        NSInvocationOperation* operation = [NSInvocationOperation alloc];
        operation = [[operation
                      initWithTarget:self
                      selector:@selector(performCheckForUpdates:)
                      object:[NSNumber numberWithBool:kRHBetaBuildNumber != kRHBetaBuildTypeOfficial]] autorelease];
        [self.operations addOperation:operation];
    }
}

#pragma mark - Private Methods

- (IBAction)switchInstallationType:(id)sender {
    UIActionSheet *actionSheet = [[[UIActionSheet alloc] initWithTitle:@"Are You Sure?" delegate:self cancelButtonTitle:@"Cancel" destructiveButtonTitle:@"Switch Build Types" otherButtonTitles:nil] autorelease];
    
    RHITMobileAppDelegate *appDelegate = (RHITMobileAppDelegate *) [UIApplication sharedApplication].delegate;
    [actionSheet showFromTabBar:appDelegate.tabBarController.tabBar];
}

- (IBAction)checkForUpdates:(id)sender {
    if (self.checkingForUpdates) {
        return;
    }
    
    self.checkingForUpdates = YES;
    [self setLoadingText:@"Checking for Updates"];
    NSInvocationOperation* operation = [NSInvocationOperation alloc];
    operation = [[operation
                  initWithTarget:self
                  selector:@selector(performCheckForUpdates:)
                  object:[NSNumber numberWithBool:kRHBetaBuildNumber == kRHBetaBuildTypeOfficial]] autorelease];
    [self.operations addOperation:operation];
}

- (void)performCheckForUpdates:(NSNumber *)officialNumber {
    BOOL official = officialNumber.boolValue;
    NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL URLWithString:[kBetaServer stringByAppendingString:kBetaUpdatePath]]];
    NSURLResponse *response = nil;
    NSData *data = [NSURLConnection sendSynchronousRequest:request returningResponse:&response error:nil];
    NSDictionary *parsed = [NSDictionary dictionaryWithJSONData:data error:nil];
    
    NSDictionary *relevantBuild = nil;
    if (official) {
        relevantBuild = [parsed objectForKey:@"official"];
    } else {
        relevantBuild = [parsed objectForKey:@"rolling"];
    }
    
    if ([relevantBuild isKindOfClass:[NSNull class]]) {
        return [self performSelectorOnMainThread:@selector(didFindNoUpdates) withObject:nil waitUntilDone:NO];
    }
    
    NSNumberFormatter *formatter = [[[NSNumberFormatter alloc] init] autorelease];
    [formatter setNumberStyle:NSNumberFormatterDecimalStyle];
    NSNumber *newBuildNumber = [formatter numberFromString:[relevantBuild objectForKey:@"buildNumber"]];
    NSNumber *oldBuildNumber = [NSNumber numberWithInt:kRHBetaBuildNumber];
    
    if ([newBuildNumber compare:oldBuildNumber] != NSOrderedDescending) {
        [self performSelectorOnMainThread:@selector(didFindNoUpdates) withObject:nil waitUntilDone:NO];
    } else {
        NSURL *url = [NSURL URLWithString:[relevantBuild objectForKey:@"downloadURL"]];
        [self performSelectorOnMainThread:@selector(didFindUpdateWithURL:) withObject:url waitUntilDone:NO];
    }
}

- (void)setLoadingText:(NSString *)text {
    self.navigationItem.title = text;
    UIActivityIndicatorView* activityIndicatorView = [[[UIActivityIndicatorView alloc] initWithFrame:CGRectMake(20, 0, 20, 20)] autorelease];
    [activityIndicatorView startAnimating];
    
    UIBarButtonItem *activityButtonItem = [[[UIBarButtonItem alloc] initWithCustomView:activityIndicatorView] autorelease];
    self.navigationItem.leftBarButtonItem = activityButtonItem;
}

- (void)clearLoadingText {
    self.navigationItem.title = @"Beta Tools and Info";
    self.navigationItem.leftBarButtonItem = nil;
}

- (void)didFindNoUpdates {
    [self clearLoadingText];
    self.checkingForUpdates = NO;
    [[[[UIAlertView alloc] initWithTitle:@"No Updates Found" message:@"You are already using the latest version of Rose-Hulman." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil] autorelease]show];
}

- (void)didFindUpdateWithURL:(NSURL *)url {
    [self clearLoadingText];
    self.checkingForUpdates = NO;
    [[UIApplication sharedApplication] openURL:url];
}

- (void)performRegistration {
    UIDevice *device = [UIDevice currentDevice];
    
    size_t size;
    sysctlbyname("hw.model", NULL, &size, NULL, 0);
    char *machine = malloc(size);
    sysctlbyname("hw.model", machine, &size, NULL, 0);
    NSString *platform = [NSString stringWithCString:machine encoding:NSUTF8StringEncoding];
    free(machine);
    
    NSString *name = self.registrationName;
    NSString *email = self.registrationEmail;
    NSString *deviceID = device.uniqueIdentifier;
    NSString *operatingSystem = [NSString stringWithFormat:@"%@ %@", device.systemName, device.systemVersion];
    NSString *model = platform;
    
    name = [name stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    email = [email stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    deviceID = [deviceID stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    operatingSystem = [operatingSystem stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    model = [model stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSString *parameters = [NSString stringWithFormat:@"name=%@&email=%@&deviceID=%@&build=%d&operatingSystem=%@&model=%@&platform=ios", name, email, deviceID, kRHBetaBuildNumber, operatingSystem, model];
    
    NSURL *url = [NSURL URLWithString:[kBetaServer stringByAppendingString:kBetaRegisterPath]];
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:url];
    
    request.HTTPMethod = @"POST";
    request.HTTPBody = [parameters dataUsingEncoding:NSUTF8StringEncoding];
    
    NSData *data = [NSURLConnection sendSynchronousRequest:request returningResponse:nil error:nil];
    
    NSString* body = [[[NSString alloc] initWithData:data
                                           encoding:NSUTF8StringEncoding] autorelease];
    NSLog(@"%@", body);
    
    NSDictionary *response = [NSDictionary dictionaryWithJSONData:data error:nil];
    
    self.authToken = [response valueForKey:@"authToken"];
    
    if (self.authToken != nil) {
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        [defaults setValue:self.authToken forKey:kBetaAuthTokenDefault];
    }
}

- (void)performNotificationOfUpdate {
    
}

@end

#endif
