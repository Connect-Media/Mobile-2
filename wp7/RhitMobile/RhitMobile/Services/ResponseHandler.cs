﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using RhitMobile.Events;

namespace RhitMobile.Services {
    public class ResponseHandler {

        #region Events
        public static event ServerEventHandler ResponseReceived;
        public static event ServerEventHandler AllResponseReceived;
        public static event ServerEventHandler TopResponseReceived;
        public static event ServerEventHandler SearchResponseReceived;
        #endregion

        #region Event Raising Methods
        protected static void OnResponse(ServerEventArgs e) {
            if(ResponseReceived != null) ResponseReceived(null, e);
        }

        protected static void OnAllResponse(ServerEventArgs e) {
            if(AllResponseReceived != null) AllResponseReceived(null, e);
        }

        protected static void OnTopResponse(ServerEventArgs e) {
            if(TopResponseReceived != null) TopResponseReceived(null, e);
        }

        protected static void OnSearchResponse(ServerEventArgs e) {
            if(SearchResponseReceived != null) SearchResponseReceived(null, e);
        }
        #endregion

        #region Callbacks
        public static void RequestCallback(IAsyncResult asyncResult) {
            SendResults(ParseResponse(asyncResult), ResponseType.Basic);
        }

        public static void AllRequestCallback(IAsyncResult asyncResult) {
            SendResults(ParseResponse(asyncResult), ResponseType.All);
        }

        public static void TopRequestCallback(IAsyncResult asyncResult) {
            SendResults(ParseResponse(asyncResult), ResponseType.Top);
        }

        public static void SearchRequestCallback(IAsyncResult asyncResult) {
            SendResults(ParseResponse(asyncResult), ResponseType.Search);
        }
        #endregion

        private static ServerEventArgs ParseResponse(IAsyncResult asyncResult) {
            HttpWebRequest request = (HttpWebRequest) asyncResult.AsyncState;
            HttpWebResponse response;
            try {
                response = (HttpWebResponse) request.EndGetResponse(asyncResult);
            } catch(WebException e) {
                response = (HttpWebResponse) e.Response;
            }
            ServerObject obj = null;
            if(response.StatusCode == HttpStatusCode.OK) {
                using(StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    string responseString = reader.ReadToEnd();
                    reader.Close();
                    using(var ms = new MemoryStream(Encoding.Unicode.GetBytes(responseString))) {
                        var serializer = new DataContractJsonSerializer(typeof(ServerObject));
                        obj = (ServerObject) serializer.ReadObject(ms);
                    }
                }
            }
            ServerEventArgs args = new ServerEventArgs() {
                ResponseObject = obj,
                ServerResponse = response.StatusCode,
            };
            response.Close();
            return args;
        }

        private static void SendResults(ServerEventArgs args, ResponseType type) {
            switch(type) {
                case ResponseType.Basic:
                    GeoService.Dispatcher.BeginInvoke(new Action(() => OnResponse(args)));
                    break;
                case ResponseType.All:
                    GeoService.Dispatcher.BeginInvoke(new Action(() => OnAllResponse(args)));
                    break;
                case ResponseType.Top:
                    GeoService.Dispatcher.BeginInvoke(new Action(() => OnTopResponse(args)));
                    break;
                case ResponseType.Search:
                    GeoService.Dispatcher.BeginInvoke(new Action(() => OnSearchResponse(args)));
                    break;
            }
        }
    }
}
