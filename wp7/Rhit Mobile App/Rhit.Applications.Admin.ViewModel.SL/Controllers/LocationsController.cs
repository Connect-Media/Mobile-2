﻿using System.Collections.ObjectModel;
using System.Windows;
using Rhit.Applications.Model;
using System;
using System.Collections.Generic;
using Rhit.Applications.Model.Events;
using Rhit.Applications.Model.Services;

namespace Rhit.Applications.ViewModel.Controllers {
    public class LocationsController : DependencyObject {
        private static LocationsController _instance;

        private LocationsController() {
            All = new ObservableCollection<RhitLocation>();
            InnerLocations = new ObservableCollection<RhitLocation>();
            Buildings = new ObservableCollection<RhitLocation>();
            QuickList = new ObservableCollection<RhitLocation>();
            PointsOfInterest = new ObservableCollection<RhitLocation>();
        }

        #region Singleton Instance
        public static LocationsController Instance {
            get {
                if(_instance == null)
                    _instance = new LocationsController();
                return _instance;
            }
        }
        #endregion

        #region Events
        #region CurrentLocationChanged
        public event LocationEventHandler CurrentLocationChanged;
        protected virtual void OnCurrentLocationChanged(LocationEventArgs e) {
            if(CurrentLocationChanged != null) CurrentLocationChanged(this, e);
        }
        #endregion

        #region LocationsChanged
        public event LocationEventHandler LocationsChanged;
        protected virtual void OnLocationsChanged(LocationEventArgs e) {
            if(LocationsChanged != null) LocationsChanged(this, e);
        }
        #endregion

        #endregion

        public ObservableCollection<RhitLocation> All { get; set; }

        public ObservableCollection<RhitLocation> Buildings { get; set; }

        public ObservableCollection<RhitLocation> InnerLocations { get; set; }

        public ObservableCollection<RhitLocation> QuickList { get; set; }

        public ObservableCollection<RhitLocation> PointsOfInterest { get; set; }

        public void SelectLocation(int id) {
            foreach(RhitLocation location in Buildings)
                if(location.Id == id) {
                    SelectLocation(location);
                    return;
                }
        }

        public void SelectLocation(RhitLocation location) {
            LocationEventArgs args = new LocationEventArgs();
            args.OldLocation = CurrentLocation;
            CurrentLocation = location;
            args.NewLocation = CurrentLocation;

            InnerLocations.Clear();
            List<RhitLocation> locations = DataCollector.Instance.GetChildLocations(null, CurrentLocation.Id);
            if(locations != null) foreach(RhitLocation child in locations) InnerLocations.Add(child);

            OnCurrentLocationChanged(args);
        }

        public void UnSelect() {
            LocationEventArgs args = new LocationEventArgs();
            args.OldLocation = CurrentLocation;
            CurrentLocation = null;
            args.NewLocation = CurrentLocation;
            InnerLocations.Clear();
            OnCurrentLocationChanged(args);
        }

        public void SetLocations(ICollection<RhitLocation> locations) {
            LocationEventArgs args = new LocationEventArgs();
            args.OldLocations = All;
            All.Clear();
            foreach(RhitLocation location in locations) All.Add(location);
            args.NewLocations = All;
            UpdateCollections();
            OnLocationsChanged(args);
        }

        private void UpdateCollections() {
            PointsOfInterest.Clear();
            QuickList.Clear();
            foreach(RhitLocation location in All) {
                if(location.Type == LocationType.OnQuickList) {
                    QuickList.Add(location);
                    PointsOfInterest.Add(location);
                } else if(location.Type == LocationType.PointOfInterest)
                    PointsOfInterest.Add(location);
            }
        }

        #region Dependency Properties
        #region CurrentLocation
        public RhitLocation CurrentLocation {
            get { return (RhitLocation) GetValue(CurrentLocationProperty); }
            set { SetValue(CurrentLocationProperty, value); }
        }

        public static readonly DependencyProperty CurrentLocationProperty =
           DependencyProperty.Register("CurrentLocation", typeof(RhitLocation), typeof(LocationsController), new PropertyMetadata(null));
        #endregion
        #endregion

        public void RemoveBuilding(RhitLocation location) {
            if(Buildings.Contains(location)) Buildings.Remove(location);
        }

        public void AddBuilding(RhitLocation location) {
            if(!Buildings.Contains(location)) Buildings.Add(location);
        }
    }
}
