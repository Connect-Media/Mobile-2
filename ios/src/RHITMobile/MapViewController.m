//
//  MapViewController.m
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

#import "MapViewController.h"
#import "MKMapView+ZoomLevel.h"
#import "RHConstants.h"
#import "RHAnnotation.h"
#import "RHAnnotationView.h"
#import "RHLocation.h"
#import "RHLabelNode.h"
#import "RHRestHandler.h"
#import "RHLocationOverlay.h"


@interface MapViewController()

@property (nonatomic, retain) RHLocationOverlay *currentOverlay;

@end


@implementation MapViewController

@synthesize mapView;
@synthesize fetchedResultsController;
@synthesize managedObjectContext;
@synthesize remoteHandler = remoteHandler_;
@synthesize currentOverlay;

- (void)viewDidLoad {
    [super viewDidLoad];
    
    // Initialize what's visible on the map
    CLLocationCoordinate2D center = {RH_CAMPUS_CENTER_LATITUDE,
        RH_CAMPUS_CENTER_LONGITUDE};
    self.mapView.mapType = MKMapTypeSatellite;
    [self.mapView setCenterCoordinate:center
                            zoomLevel:RH_INITIAL_ZOOM_LEVEL
                             animated:NO];
    
    [self.remoteHandler fetchAllLocations];
}


- (BOOL) shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

- (void) didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc. that aren't in use.
}

- (void) viewDidUnload {
    [super viewDidUnload];
    self.mapView = nil;
}

# pragma mark -
# pragma mark MKMapViewDelegate Methods

- (MKAnnotationView *)mapView:(MKMapView *)mapView
            viewForAnnotation:(id <MKAnnotation>)inAnnotation {
    RHAnnotation *annotation = (RHAnnotation *)inAnnotation;
    NSString *identifier = annotation.location.name;
    
    RHAnnotationView *annotationView = (RHAnnotationView *)[self.mapView dequeueReusableAnnotationViewWithIdentifier:identifier];
    
    if (annotationView == nil) {
        annotationView = [[[RHAnnotationView alloc] initWithAnnotation:annotation
                                                       reuseIdentifier:identifier] autorelease];
    }
    
    [annotationView setEnabled:YES];
    [annotationView setCanShowCallout:YES];
    [annotationView setDraggable:NO];
    [annotationView setDelegate:(RHAnnotationViewDelegate *)self];
    
    UIButton *newButton = [UIButton buttonWithType:UIButtonTypeDetailDisclosure];
    [annotationView setRightCalloutAccessoryView:newButton];
    
    return annotationView;
}

- (MKOverlayView *)mapView:(MKMapView *)mapView
            viewForOverlay:(id<MKOverlay>)overlay {
    if ([overlay isKindOfClass:[RHLocationOverlay class]]) {
        MKPolygon *polygon = ((RHLocationOverlay *) overlay).polygon;
        MKPolygonView *view = [[[MKPolygonView alloc] initWithPolygon:polygon]
                               autorelease];
        
        view.fillColor = [[UIColor cyanColor] colorWithAlphaComponent:0.2];
        view.strokeColor = [[UIColor blueColor] colorWithAlphaComponent:0.7];
        view.lineWidth = 3;
        
        return view;
    }
    
    return nil;
}

#pragma mark -
#pragma mark RHRemoteHandlerDelegate Methods

- (RHRemoteHandler *)remoteHandler {
    if (remoteHandler_ == nil) {
        remoteHandler_ = [[RHRestHandler alloc]
                          initWithContext:self.managedObjectContext
                          delegate:(RHRemoteHandlerDelegate *)self];
    }
    
    return remoteHandler_;
}

- (void)didFetchAllLocations:(NSSet *)locations {
    for (RHLocation *location in locations) {
        RHAnnotation *annotation = [RHAnnotation alloc];
        annotation = [[annotation initWithLocation:location
                                    annotationType:RHAnnotationTypeText]
                      autorelease];
        [self.mapView addAnnotation:annotation];
    }
}

- (void)didFailFetchingAllLocationsWithError:(NSError *)error {
    NSString *title = @"Error Updating Map";
    NSString *message = error.localizedDescription;
    
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:title
                                                    message:message
                                                   delegate:self
                                          cancelButtonTitle:@"OK"
                                          otherButtonTitles:nil, nil];
    [alert show];
    [alert release];
}

#pragma mark -
#pragma mark RHAnnotationView Delegate Methods

-(void)focusMapViewToLocation:(RHLocation *)location {
    [self.mapView removeOverlay:self.currentOverlay];
    RHLocationOverlay *overlay = [[RHLocationOverlay alloc]
                                  initWithLocation:location];
    CLLocationCoordinate2D center = [[location labelLocation] coordinate];
    [self.mapView setCenterCoordinate:center zoomLevel:16 animated:YES];
    [self.mapView addOverlay:overlay];
    self.currentOverlay = overlay;
    [overlay release];
}

@end