//
//  RHLocationTests.h
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

#import <SenTestingKit/SenTestingKit.h>
#import <UIKit/UIKit.h>

/// \test
/// Tests targetting the RHLocation model object.
@interface RHLocationTests : SenTestCase

/// Verify that basic creation still works.
- (void)testInitSmokeTest;

/// Test synthetic ordering of boundary nodes.
- (void)testBoundaryNodeOrdering;

/// Verify that storage and retrieval still works.
- (void)testStorageAndRetrieval;

/// Test values computed on the fly
- (void)testCompotedValues;

@end