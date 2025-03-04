﻿#region

using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.General
{
    static class MarkerBuilder
    {
        public static void Make(GameObject body, string name, PlanetConfig config)
        {
            MapMarker mapMarker = body.AddComponent<MapMarker>();
            mapMarker._labelID = (UITextType)TranslationHandler.AddUI(config.name);

            var markerType = MapMarker.MarkerType.Planet;

            if (config.Orbit.isMoon)
            {
                markerType = MapMarker.MarkerType.Moon;
            }
            else if (config.Star != null)
            {
                markerType = MapMarker.MarkerType.Sun;
            }
            else if (config.FocalPoint != null)
            {
                markerType = MapMarker.MarkerType.HourglassTwins;
            }
            /*
            else if (config.Base.IsSatellite)
            {
                markerType = MapMarker.MarkerType.Probe;
            }
            */

            mapMarker._markerType = markerType;
        }
    }
}
