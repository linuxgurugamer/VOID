// VOID
//
// VOID_VesselRegister.cs
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
using UnityEngine;

namespace VOID
{
	public class VOID_VesselRegister : VOID_WindowModule
	{
		[AVOID_SaveValue("selectedBodyIdx")]
		protected VOID_SaveValue<int> selectedBodyIdx;
		protected CelestialBody seletedBody;

		[AVOID_SaveValue("selectedVesselTypeIdx")]
		protected VOID_SaveValue<int> selectedVesselTypeIdx;
		protected VesselType selectedVesselType;

		protected string vesselSituation;

		protected Vector2 selectorScrollPos;

		protected Vessel _selectedVessel;

		public Vessel selectedVessel
		{
			get
			{
				return this._selectedVessel;
			}
		}

		public VOID_VesselRegister() : base()
		{
			this.Name = "Vessel Register";

			this.WindowPos.x = 845;
			this.WindowPos.y = 275;
			this.defHeight = 375;

			this.selectedBodyIdx = (VOID_SaveValue<int>)0;
			this.selectedVesselTypeIdx = (VOID_SaveValue<int>)0;

			this.vesselSituation = "Orbiting";

			this.selectorScrollPos = new Vector2();
		}

		public override void ModuleWindow(int id)
		{
			if (this.core.AllVesselTypes.Length < 1)
			{
				return;
			}

			if (selectedBodyIdx >= this.core.SortedBodyList.Count)
			{
				selectedBodyIdx.value %= this.core.SortedBodyList.Count;
			}

			if (selectedBodyIdx < 0)
			{
				selectedBodyIdx.value += this.core.SortedBodyList.Count;
			}

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<"))
			{
				selectedBodyIdx.value--;
				if (selectedBodyIdx < 0)
				{
					selectedBodyIdx.value = this.core.SortedBodyList.Count - 1;
				}
			}
			GUILayout.Label(this.core.SortedBodyList[selectedBodyIdx].bodyName, VOID_Styles.labelCenterBold, GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">"))
			{
				selectedBodyIdx.value++;
				if (selectedBodyIdx > this.core.SortedBodyList.Count - 1)
				{
					selectedBodyIdx.value = 0;
				}
			}
			GUILayout.EndHorizontal();

			seletedBody = this.core.SortedBodyList[selectedBodyIdx];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<"))
			{
				selectedVesselTypeIdx.value--;
				if (selectedVesselTypeIdx < 0)
				{
					selectedVesselTypeIdx.value = this.core.AllVesselTypes.Length - 1;
				}
			}
			GUILayout.Label(this.core.AllVesselTypes[selectedVesselTypeIdx].ToString(), VOID_Styles.labelCenterBold, GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">"))
			{
				selectedVesselTypeIdx.value++;
				if (selectedVesselTypeIdx > this.core.AllVesselTypes.Length - 1)
				{
					selectedVesselTypeIdx.value = 0;
				}
			}
			GUILayout.EndHorizontal();

			selectedVesselType = this.core.AllVesselTypes[selectedVesselTypeIdx];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Landed", GUILayout.ExpandWidth(true))) vesselSituation = "Landed";
			if (GUILayout.Button("Orbiting", GUILayout.ExpandWidth(true))) vesselSituation = "Orbiting";
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(
				VOID_Tools.UppercaseFirst(vesselSituation) + " " + selectedVesselType.ToString() + "s  @ " + seletedBody.bodyName,
				VOID_Styles.labelCenter,
				GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			selectorScrollPos = GUILayout.BeginScrollView(selectorScrollPos, false, false);

			Vessel v;
			for (int idx = 0; idx < FlightGlobals.Vessels.Count; idx++)
			{
				v = FlightGlobals.Vessels[idx];

				if (v != Vessel && v.vesselType == selectedVesselType && v.mainBody == seletedBody)
				{
					if ((vesselSituation == "Landed" &&
					     (v.situation == Vessel.Situations.LANDED ||
					 v.situation == Vessel.Situations.PRELAUNCH ||
					 v.situation == Vessel.Situations.SPLASHED)) ||
					    (vesselSituation == "Orbiting" &&
					 (v.situation == Vessel.Situations.ESCAPING ||
					 v.situation == Vessel.Situations.FLYING ||
					 v.situation == Vessel.Situations.ORBITING ||
					 v.situation == Vessel.Situations.SUB_ORBITAL))
					    )
					{
						if (GUILayout.Button(v.vesselName, GUILayout.ExpandWidth(true)))
						{
							if (_selectedVessel != v)
							{
								_selectedVessel = v; //set clicked vessel as selected_vessel
								this.Active = true;    //turn bool on to open the window if closed
							}
							else
							{
								_selectedVessel = null;
							}
						}
					}
				}
			}

			GUILayout.EndScrollView();

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}
	}
}