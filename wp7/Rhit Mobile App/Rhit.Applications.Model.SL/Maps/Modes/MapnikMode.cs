﻿using System.Collections.Generic;
using Rhit.Applications.Model.Maps.Sources;

namespace Rhit.Applications.Extentions.Maps.Modes {
    public class MapnikMode : RhitMode {
        public MapnikMode() {
            Label = "Mapnik";
            Sources = new List<BaseTileSource>() {
                new MapnikSource(),
            };
            CurrentSource = Sources[0];
        }
    }
}
