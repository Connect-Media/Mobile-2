﻿//Main page documetation for automatic doxygen generation.


// Doxygen group definitions for the project

/// \defgroup pages WP7 Pages
/// All of the page that can be navigated to within the app.
/// 
/// \defgroup services Major Services
/// This includes static and singleton classes used by many other classes to
/// gather/store information.
/// 
/// \defgroup objects Object Models
/// Various object models.
/// 
/// \defgroup tile_sources Tile Sources
/// Map tile source classes. This includes cutom overlays.

// Doxygen main page documentation

/// \mainpage RHIT Mobile WP7 Developer Documentation
///
/// \section intro_sec Introduction
///
/// Welcome to the WP7 developer documetation for the RHIT Mobile project.
/// The purpose of this documentation is to provide an easily digestable overview of how the project is structured, specifically for future teams who may be assigned maintenance of this work, project advisors, or even curious developers.
///
/// \section where_to_start Where To Start
///
/// For help getting a build environment set up, refer to README.md that can be found <a href="https://github.com/RHIT/Mobile/tree/master/wp7">here</a>, the top-level wp7 directory of the repository.
/// This documentation assumes that you have a working development environment for the project.
///
/// \section project_overview_sec Project Overview
///
/// RHIT Mobile WP7 Edition is the Windows Phone 7 branch of the RHIT Mobile project, a senior project commissioned during the 2011-2012 school year and maintained by various other Rose-Hulman bodies afterwards.
/// 
/// \section overview Architecture Overview
/// 
/// Each view of the app has both a xaml and C# file associated with it.
/// All of these files can be found in the following group: \ref pages.
/// Very little information is stored in the actual view itself.
/// They each utilize one of the classes found in the \ref services group.
/// \ref objects conatins all of the object models.
/// \ref tile_sources contains all of the available map tile sources and overlays.
/// 
/// - The views handle page navigation and user events.
/// - Anytime one of the views need the map, manipulate it, or get information from it they will use the \link Rhit.Applications.View.Views.ObjectModel.RhitMap RhitMap \endlink singleton class.
/// This way the map stays up-to-date at all times.
///  - \link Rhit.Applications.View.Views.ObjectModel.RhitMap RhitMap \endlink keeps track of the current properties of the map as well as the possible tile sources avaible.
/// - If any of the views need any other information, like location data for instance, they will use the \link Rhit.Applications.View.Views.Services.DataCollector DataCollector \endlink singleton class.
///  - The \link Rhit.Applications.View.Views.Services.DataCollector DataCollector \endlink will try to get the requested data from the \link Rhit.Applications.View.Views.Services.DataStorage DataStorage \endlink singleton class (i.e. local storage) first.
///  If \link Rhit.Applications.View.Views.Services.DataStorage DataStorage \endlink has the data, then it is returned. Otherwise, it uses \link Rhit.Applications.View.Views.Services.Requests.RequestBuilder RequestBuilder \endlink and \link Rhit.Applications.View.Views.Services.GeoService GeoService \endlink to make a request to the server.
///  - \link Rhit.Applications.View.Views.Services.DataCollector DataCollector \endlink will also check for updates from the server on the app's initialization.
/// - All views listen to at least one \link Rhit.Applications.View.Views.Events.ServiceEvent ServiceEvent \endlink handler to handle updated data.
///  - All server responses are handled by \link Rhit.Applications.View.Views.Services.ResponseHandler ResponseHandler \endlink, parsed, and made into a \link Rhit.Applications.View.Views.Events.ServiceEvent ServiceEvent \endlink.
///  - Data from the server responses are parsed uses the classes in \link Rhit.Applications.View.Views.ObjectModel.DataContracts DataContracts \endlink, which are then usually converted into \link Rhit.Applications.View.Views.ObjectModel.RhitLocation RhitLocation \endlink objects.
///  
/// \section features Feature Mapping
/// 
/// \subsection main_page Main Page (MapPage)
/// 
/// The app starts out in the \link Rhit.Applications.View.Views.MapPage MapPage \endlink and uses it as the main page for the app.
/// The \link Rhit.Applications.View.Views.MapPage MapPage \endlink contains an app bar with buttons: 'me', 'rhit', and 'search'.
/// The 'me' and 'rhit' buttons both indirectly link to methods in \link Rhit.Applications.View.Views.ObjectModel.RhitMap RhitMap \endlink that change the map's viewport.
/// The 'search' button switches to the \link Rhit.Applications.View.Views.SearchPage SearchPage \endlink.
/// The app bar also contains two menu options: 'quicklist' and 'settings' which open the \link Rhit.Applications.View.Views.QuickListPage QuickListPage \endlink and \link Rhit.Applications.View.Views.SettingsPage SettingsPage \endlink respectively.
/// Apart from the app bar, the \link Rhit.Applications.View.Views.MapPage MapPage \endlink also always the user to click on buildings.
/// When a user clicks (taps) on a building, the building is highlighted and a PushPin appears with the name of the building.
/// If the user then clicks (taps) on the PushPin, the app navigates to the \link Rhit.Applications.View.Views.InfoPage InfoPage \endlink for that building.
/// Once the user navigates away from this page, they only need to use the device's 'back' hardware button to return to this page.
/// 
/// \subsection info_page Location Information Page (InfoPage)
/// 
/// The \link Rhit.Applications.View.Views.InfoPage InfoPage \endlink contains all of the information pertinent to the given location.
/// This includes the location's name, alternate names, description, links to relevant websites, and other locations contained within this location (child locations).
/// The child locations are displayed as links to their information page.
/// All of this information is conatined within the corresponding \link Rhit.Applications.View.Views.ObjectModel.RhitLocation RhitLocation\endlink.
/// This page also provides links back to map with it's location selected on the map.
/// 
/// \subsection quicklist_page Top Locations Page (QuickListPage)
/// \link Rhit.Applications.View.Views.QuickListPage QuickListPage \endlink is just a simple list of \link Rhit.Applications.View.Views.ObjectModel.RhitLocation RhitLocations \endlink that link back to the map with the selected location PushPin shown.
/// 
/// \subsection search_page Search Page (SearchPage)
/// \link Rhit.Applications.View.Views.SearchPage SearchPage \endlink allows the user to search for both people and places with Rose-Hulman.
/// 
/// \subsection settings_page Application Settings Page (SettingsPage)
/// \link Rhit.Applications.View.Views.SettingsPage SettingsPage \endlink allows the user to change several different options for the app.
/// For example, the user can switch between using Bing maps to using Google maps.
/// 
/// \section doc_maintenance_sec Maintaining This Documentation
/// 
/// For developers working on this project after its initial creation, please
/// document any code you add or change inline, using the existing files as
/// an example. For administrative-level documentation, including this index
/// page, make changes to DocContent.cs (this documentation won't be visible
/// in the file if you view it with the built-in Doxygen source viewer).