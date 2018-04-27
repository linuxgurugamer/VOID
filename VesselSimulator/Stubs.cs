// VesselSimulator © 2015 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/
using System;

namespace KerbalEngineer.Helpers
{
    public static class CelestialBodies
    {
        public static CelestialBody SelectedBody
        {
            get;
            set;
        }

        public static double GetDensity(this CelestialBody body, double altitude)
        {
            return body.GetDensity(body.GetPressure(altitude), body.GetTemperature(altitude));
        }
    }
}

namespace KerbalEngineer.Editor
{
    public static class BuildAdvanced
    {
        public static double Altitude
        {
            get;
            set;
        }
    }
}
