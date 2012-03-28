﻿using System.Collections.ObjectModel;
using System.Windows;
using Rhit.Applications.Models;

namespace Rhit.Applications.ViewModels.Utilities {
    public class CampusService : DependencyObject {
        private static int _lastId = 0;

        public CampusService() {
            Id = ++_lastId;
            Links = new ObservableCollection<Link>();
            Children = new ObservableCollection<CampusService>();
        }

        public CampusService(CampusServicesCategory_DC model)
            : this() {
            Label = model.Name;
            foreach(CampusServicesCategory_DC service in model.Children)
                Children.Add(new CampusService(service));
            foreach(Link_DC link in model.Links)
                Links.Add(new Link() { Name = link.Name, Address = link.Address, });

        }

        public CampusServicesCategory_DC ToLightWeightDataContract()
        {
            CampusServicesCategory_DC result = new CampusServicesCategory_DC();
            result.Name = Label;

            return result;
        }

        #region Label
        public string Label {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
           DependencyProperty.Register("Label", typeof(string), typeof(CampusService), new PropertyMetadata(""));
        #endregion

        public int Id { get; private set; }

        public ObservableCollection<Link> Links { get; private set; }

        public ObservableCollection<CampusService> Children { get; private set; }
    }
}
