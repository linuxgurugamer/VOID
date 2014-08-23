// VOID
//
// VOID_CBInfoBrowser.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_CBInfoBrowser : VOID_WindowModule
	{
		[AVOID_SaveValue("selectedBodyIdx1")]
		protected VOID_SaveValue<int> selectedBodyIdx1 = 1;

		[AVOID_SaveValue("selectedBodyIdx2")]
		protected VOID_SaveValue<int> selectedBodyIdx2 = 2;

		protected CelestialBody selectedBody1;
		protected CelestialBody selectedBody2;

		[AVOID_SaveValue("toggleOrbital")]
		protected VOID_SaveValue<bool> toggleOrbital = false;

		[AVOID_SaveValue("togglePhysical")]
		protected VOID_SaveValue<bool> togglePhysical = false;

		[AVOID_SaveValue("toggleScience")]
		protected VOID_SaveValue<bool> toggleScience = false;

		public VOID_CBInfoBrowser()
		{
			this._Name = "Celestial Body Information Browser";

			this.WindowPos.x = 10;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(150));

			selectedBody1 = this.core.allBodies[selectedBodyIdx1];
			selectedBody2 = this.core.allBodies[selectedBodyIdx2];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx1--;
				if (selectedBodyIdx1 < 0) selectedBodyIdx1 = this.core.allBodies.Count - 1;
			}
			GUILayout.Label(this.core.allBodies[selectedBodyIdx1].bodyName, this.core.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx1++;
				if (selectedBodyIdx1 > this.core.allBodies.Count - 1) selectedBodyIdx1 = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx2--;
				if (selectedBodyIdx2 < 0) selectedBodyIdx2 = this.core.allBodies.Count - 1;
			}
			GUILayout.Label(this.core.allBodies[selectedBodyIdx2].bodyName, this.core.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx2++;
				if (selectedBodyIdx2 > this.core.allBodies.Count - 1) selectedBodyIdx2 = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			//toggle for orbital info chunk
			if (GUILayout.Button("Orbital Characteristics", GUILayout.ExpandWidth(true))) toggleOrbital.value = !toggleOrbital;

			if (toggleOrbital)
			{
				//begin orbital into horizontal chunk
				//print("begin orbital info section...");
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				//begin orbital value labels column
				GUILayout.BeginVertical(GUILayout.Width(150));

				//print("printing row labels...");

				GUILayout.Label("Apoapsis:");
				GUILayout.Label("Time to Ap:");
				GUILayout.Label("Periapsis:");
				GUILayout.Label("Time to Pe:");
				GUILayout.Label("Semi-major axis:");
				GUILayout.Label("Eccentricity:");
				GUILayout.Label("Orbital period:");
				GUILayout.Label("Rotational period:");
				GUILayout.Label("Velocity:");
				GUILayout.Label("Mean anomaly:");
				GUILayout.Label("True anomaly:");
				GUILayout.Label("Eccentric anomaly:");
				GUILayout.Label("Inclination:");
				GUILayout.Label("Long. ascending node:");
				GUILayout.Label("Arg. of periapsis:");
				GUILayout.Label("Tidally locked:");

				//end orbital value labels column
				GUILayout.EndVertical();

				//begin primary orbital values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_orbital_info(selectedBody1);

				//end primary orbital values column
				GUILayout.EndVertical();

				//begin secondary orbital values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_orbital_info(selectedBody2);

				//end secondary orbital values column
				GUILayout.EndVertical();

				//end orbital info horizontal chunk
				GUILayout.EndHorizontal();
			}

			//toggle for physical info chunk
			if (GUILayout.Button("Physical Characteristics", GUILayout.ExpandWidth(true))) togglePhysical.value = !togglePhysical;

			if (togglePhysical)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				//begin physical info value label column
				GUILayout.BeginVertical(GUILayout.Width(150));

				GUILayout.Label("Radius:");
				GUILayout.Label("Surface area:");
				GUILayout.Label("Volume:");
				GUILayout.Label("Mass:");
				GUILayout.Label("Density:");
				GUILayout.Label("Sphere of influence:");
				GUILayout.Label("Natural satellites:");
				GUILayout.Label("Artificial satellites:");
				GUILayout.Label("Surface gravity:");
				GUILayout.Label("Atmosphere altitude:");
				GUILayout.Label("Atmospheric O\u2082:");
				GUILayout.Label("Has ocean:");

				//end physical info value label column
				GUILayout.EndVertical();

				//begin primary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_physical_info(selectedBody1);

				//end primary physical column
				GUILayout.EndVertical();

				//begin secondary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_physical_info(selectedBody2);

				//end target physical values column
				GUILayout.EndVertical();

				//end physical value horizontal chunk
				GUILayout.EndHorizontal();
			}

			if (GUILayout.Button("Scientific Parameters", GUILayout.ExpandWidth(true)))
			{
				toggleScience.value = !toggleScience;
			}

			if (toggleScience)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				//begin physical info value label column
				GUILayout.BeginVertical(GUILayout.Width(150));


				/*
				 *  public float RecoveryValue = 1f;

					public float InSpaceHighDataValue = 1f;

					public float spaceAltitudeThreshold = 250000f;

					public float flyingAltitudeThreshold = 18000f;

					public float InSpaceLowDataValue = 1f;

					public float SplashedDataValue = 1f;

					public float LandedDataValue = 1f;

					public float FlyingHighDataValue = 1f;

					public float FlyingLowDataValue = 1f;
					*/

				GUILayout.Label("Surface Multiplier:");
				GUILayout.Label("Ocean Multiplier:");
				GUILayout.Label("Flying-Low Multiplier:");
				GUILayout.Label("Flying-High Multiplier:");
				GUILayout.Label("Low Orbit Multiplier:");
				GUILayout.Label("High Orbit Multiplier:");
				GUILayout.Label("'Flying-High' Altitude:");
				GUILayout.Label("'High Orbit' Altitude:");
				GUILayout.Label("Recovery Multiplier:");

				//end physical info value label column
				GUILayout.EndVertical();

				//begin primary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				this.cbColumnScience(selectedBody1);

				//end primary physical column
				GUILayout.EndVertical();

				//begin secondary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				this.cbColumnScience(selectedBody2);

				//end target physical values column
				GUILayout.EndVertical();

				//end physical value horizontal chunk
				GUILayout.EndHorizontal();
			}

			GUI.DragWindow();
		}

		private void body_OP_show_orbital_info(CelestialBody body)
		{
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.ApA / 1000).ToString("##,#") + "km", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(VOID_Tools.ConvertInterval(body.orbit.timeToAp), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.PeA / 1000).ToString("##,#") + "km", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(VOID_Tools.ConvertInterval(body.orbit.timeToPe), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.eccentricity.ToString("F4") + "", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(VOID_Tools.ConvertInterval(body.orbit.period), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(VOID_Tools.ConvertInterval(body.rotationPeriod), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.orbitalSpeed / 1000).ToString("F2") + "km/s", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			// Toadicus edit: convert mean anomaly into degrees.
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.trueAnomaly.ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			// Toadicus edit: convert eccentric anomaly into degrees.
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.inclination.ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.LAN.ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.argumentOfPeriapsis.ToString("F3") + "°", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

		    if (body.bodyName == "Sun") GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else
		    {
		        string body_tidally_locked = "No";
		        if (body.tidallyLocked) body_tidally_locked = "Yes";
		        GUILayout.Label(body_tidally_locked, this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    }
		}

		private void body_OP_show_physical_info(CelestialBody body)
		{

			GUILayout.Label((body.Radius / 1000).ToString("##,#") + "km", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			GUILayout.Label((((body.Radius * body.Radius) * 4 * Math.PI) / 1000).ToString("0.00e+00") + "km²", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			// divide by 1000 to convert m to km
			GUILayout.Label((((4d / 3) * Math.PI * (body.Radius * body.Radius * body.Radius)) / 1000).ToString("0.00e+00") + "km³", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			GUILayout.Label(body.Mass.ToString("0.00e+00") + "kg", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			double p = body.Mass / ((body.Radius * body.Radius * body.Radius) * (4d / 3) * Math.PI);

			GUILayout.Label(p.ToString("##,#") + "kg/m³", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			if (body.bodyName == "Sun") GUILayout.Label(Tools.MuMech_ToSI(body.sphereOfInfluence), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
			else GUILayout.Label(Tools.MuMech_ToSI(body.sphereOfInfluence), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			GUILayout.Label(body.orbitingBodies.Count.ToString(), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			//show # artificial satellites
			int num_art_sats = 0;
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (v.mainBody == body && v.situation.ToString() == "ORBITING") num_art_sats++;
			}

			GUILayout.Label(num_art_sats.ToString(), this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			double g_ASL = (VOID_Core.Constant_G * body.Mass) / (body.Radius * body.Radius);

			GUILayout.Label(Tools.MuMech_ToSI(g_ASL) + "m/s²", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));

			if (body.atmosphere)
			{
				GUILayout.Label("≈ " + Tools.MuMech_ToSI(body.maxAtmosphereAltitude) + "m",
					this.core.LabelStyles["right"],
					GUILayout.ExpandWidth(true));

				string O2 = "No";
				if (body.atmosphereContainsOxygen == true) O2 = "Yes";
				GUILayout.Label(O2, this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
			}
			else
			{
				GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
				GUILayout.Label("N/A", this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
			}

			string ocean = "No";
			if (body.ocean == true) ocean = "Yes";
			GUILayout.Label(ocean, this.core.LabelStyles["right"], GUILayout.ExpandWidth(true));
		}

		private void cbColumnScience(CelestialBody body)
		{
			/*GUILayout.Label("Surface Science Multiplier:");
			GUILayout.Label("Ocean Science Multiplier:");
			GUILayout.Label("Low-Atmosphere Science Multiplier:");
			GUILayout.Label("High-Atmosphere Science Multiplier:");
			GUILayout.Label("Low Orbit Science Multiplier:");
			GUILayout.Label("High Orbit Science Multiplier:");
			GUILayout.Label("'In Space' Altitude:");
			GUILayout.Label("'Flying' Altitude:");
			GUILayout.Label("Recovery Multiplier:");*/

			var scienceValues = body.scienceValues;

			GUILayout.Label(scienceValues.LandedDataValue.ToString("0.0#"),
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(
				body.ocean ? scienceValues.SplashedDataValue.ToString("0.0#") : "N/A",
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(
				body.atmosphere ? scienceValues.FlyingLowDataValue.ToString("0.0#") : "N/A",
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(
				body.atmosphere ? scienceValues.FlyingHighDataValue.ToString("0.0#") : "N/A",
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(scienceValues.InSpaceLowDataValue.ToString("0.0#"),
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(scienceValues.InSpaceHighDataValue.ToString("0.0#"),
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(
				body.atmosphere ? scienceValues.flyingAltitudeThreshold.ToString("N0") : "N/A",
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(
				scienceValues.spaceAltitudeThreshold.ToString("N0"),
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));

			GUILayout.Label(scienceValues.RecoveryValue.ToString("0.0#"),
				this.core.LabelStyles["right"],
				GUILayout.ExpandWidth(true));
		}
	}
}