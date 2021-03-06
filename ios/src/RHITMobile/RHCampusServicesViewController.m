//
//  RHCampusServicesViewController.m
//  Rose-Hulman Mobile
//
//  Copyright 2012 Rose-Hulman Institute of Technology
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

#import "RHCampusServicesViewController.h"
#import "RHServiceCategory.h"
#import "RHServiceLink.h"
#import "RHAppDelegate.h"
#import "RHWebViewController.h"

#define kLoadingReuseIdentifier @"LoadingCell"
#define kReuseIdentifier @"ServiceItemCell"

@implementation RHCampusServicesViewController

@synthesize serviceItems = serviceItems_;
@synthesize tableView = tableView_;

- (id)initWithNibName:(NSString *)nibNameOrNil
               bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (NSManagedObjectContext *)managedObjectContext {
    RHAppDelegate *appDelegate = (RHAppDelegate *)[[UIApplication sharedApplication] delegate];
    return appDelegate.managedObjectContext; 
}

#pragma mark - View lifecycle

- (void)viewDidLoad {
    [super viewDidLoad];
    if ([self.navigationController.viewControllers objectAtIndex:0] == self) {
        [self loadRootServiceCategories];
    }
}

- (void)viewDidUnload {
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

#pragma mark - UITableViewDataSource Methods

- (NSInteger)tableView:(UITableView *)tableView
 numberOfRowsInSection:(NSInteger)section {
    return self.serviceItems.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView
         cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:kReuseIdentifier];
    
    if (cell == nil) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleValue1 reuseIdentifier:kReuseIdentifier];
        cell.accessoryType = UITableViewCellAccessoryDisclosureIndicator;
    }
    
    RHServiceItem *serviceItem = [self.serviceItems objectAtIndex:indexPath.row];

    cell.textLabel.text = serviceItem.name;
    
    return cell;
}

- (void)loadRootServiceCategories {
    // Only execute on the main thread since we're using the main thread's MOC
    if (![NSThread isMainThread]) {
        [self performSelectorOnMainThread:@selector(loadRootServiceCategories)
                               withObject:nil
                            waitUntilDone:NO];
        return;
    }
    
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:kRHServiceItemEntityName];
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"parent == nil"];
    fetchRequest.predicate = predicate;
    
    NSError *error;
    self.serviceItems = [self.managedObjectContext
                         executeFetchRequest:fetchRequest
                         error:&error];
    
    if (error) {
        NSLog(@"Problem loading root campus services: %@", error);
    }
    
    [self.tableView reloadData];
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    
    RHServiceItem *serviceItem = [self.serviceItems objectAtIndex:indexPath.row];
    
    if ([serviceItem isKindOfClass:[RHServiceCategory class]]) {
        RHServiceCategory *category = (RHServiceCategory *) serviceItem;
        
        RHCampusServicesViewController *nextViewController = [[RHCampusServicesViewController alloc]
                                                  initWithNibName:kRHCampusServicesViewControllerNibName
                                                  bundle:nil];
        nextViewController.serviceItems = category.contents.allObjects;
        nextViewController.title = category.name;
        [self.navigationController pushViewController:nextViewController
                                             animated:YES];
    } else if ([serviceItem isKindOfClass:[RHServiceLink class]]) {
        RHServiceLink *link = (RHServiceLink *) serviceItem;
        
        RHWebViewController *webViewController = [[RHWebViewController alloc]
                                                initWithNibName:kRHWebViewControllerNibName
                                                bundle:nil];
        webViewController.url = [NSURL URLWithString:link.url];
        webViewController.title = link.name;
        
        [self.navigationController pushViewController:webViewController animated:YES];
    }
}

#pragma mark - RHCampusServicesRequesterDelegate Methods

- (void)didFinishUpdatingCampusServices {
    [self loadRootServiceCategories];
}

@end
