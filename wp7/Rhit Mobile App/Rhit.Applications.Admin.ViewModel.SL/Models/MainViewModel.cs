﻿using System.Windows;
using Rhit.Applications.Model;
using Rhit.Applications.ViewModel.Controllers;
using Microsoft.Maps.MapControl;
using Rhit.Applications.Model.Services;
using Rhit.Applications.Model.Events;
using System.Collections.Generic;
using System.Windows.Input;
using Rhit.Applications.Mvvm.Commands;
using System.Collections.ObjectModel;
using Rhit.Applications.ViewModel.Behaviors;
using Rhit.Applications.ViewModel.Providers;

namespace Rhit.Applications.ViewModel.Models {
    public class MainViewModel : DependencyObject {
        public MainViewModel(Map map, IBitmapProvider imageProvider, IBuildingCornersProvider cornerProvider) {
            Locations = LocationsController.Instance;
            InitializeBehaviors(cornerProvider);
            MapController.CreateMapController(map);
            ImageController.CreateImageController(imageProvider);
            Image = ImageController.Instance;
            Map = MapController.Instance;
            GotoRhitCommand = new RelayCommand(p => GotoRhit());

            List<RhitLocation> locations = DataCollector.Instance.GetAllLocations(null);
            if(locations == null || locations.Count <= 0)
                DataCollector.Instance.UpdateAvailable += new ServiceEventHandler(OnLocationsRetrieved);
            else OnLocationsRetrieved(this, new ServiceEventArgs());
        }

        private void OnLocationsRetrieved(object sender, ServiceEventArgs e) {
            List<RhitLocation> locations = DataCollector.Instance.GetAllLocations(null);
            if(locations == null || locations.Count <= 0) return;
            Locations.SetLocations(locations);
        }

        private void InitializeBehaviors(IBuildingCornersProvider cornerProvider) {
            Behaviors = new ObservableCollection<MapBehavior>() {
                new BuildingsBehavior(cornerProvider),
                new LocationsBehavior(),
                new PathsBehavior(),
            };
            Behavior = Behaviors[0];
        }

        #region Dependency Properties
        #region Behavior
        public MapBehavior Behavior {
            get { return (MapBehavior) GetValue(BehaviorProperty); }
            set { SetValue(BehaviorProperty, value); }
        }

        public static readonly DependencyProperty BehaviorProperty =
           DependencyProperty.Register("Behavior", typeof(MapBehavior), typeof(MainViewModel),
           new PropertyMetadata(null, new PropertyChangedCallback(OnBehaviorChanged)));

        private static void OnBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MainViewModel instance = (MainViewModel) d;
            instance.Behavior.Update();
        }
        #endregion
        #endregion

        public ObservableCollection<MapBehavior> Behaviors { get; set; }

        public ICommand GotoRhitCommand { get; private set; }

        public ImageController Image { get; private set; }

        public LocationsController Locations { get; private set; }

        public MapController Map { get; private set; }

        public void GotoRhit() {
            //TODO: Don't hard code
            Map.MapControl.Center = new GeoCoordinate(39.4820263, -87.3248677);
            Map.MapControl.ZoomLevel = 16;
        }

        public void PolygonClick(MapPolygon polygon, MouseButtonEventArgs e) {
            Map.EventCoordinate = Map.MapControl.ViewportPointToLocation(e.GetPosition(Map.MapControl)) as GeoCoordinate;
            Behavior.SelectLocation(polygon);
        }
    }
}
