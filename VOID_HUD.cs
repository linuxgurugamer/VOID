// VOID
//
// VOID_HUD.cs
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

using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_HUD : VOID_HUDModule, IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("leftHUDPos")]
		protected VOID_SaveValue<Rect> leftHUDPos;
		[AVOID_SaveValue("rightHUDPos")]
		protected VOID_SaveValue<Rect> rightHUDPos;

		protected HUDWindow leftWindow;
		protected HUDWindow rightWindow;

		/*
		 * Properties
		 * */

		/* 
		 * Methods
		 * */
		public VOID_HUD() : base()
		{
			this._Name = "Heads-Up Display";

			this.toggleActive = true;

			this.leftWindow = new HUDWindow(this.leftHUDWindow, new Rect(Screen.width * .375f - 300f, 0f, 300f, 90f));
			this.Windows.Add(this.leftWindow);

			this.leftHUDPos = this.leftWindow.WindowPos;

			this.rightWindow = new HUDWindow(this.rightHUDWindow, new Rect(Screen.width * .625f, 0f, 300f, 90f));
			this.Windows.Add(this.rightWindow);

			this.rightHUDPos = this.rightWindow.WindowPos;

			Tools.PostDebugMessage ("VOID_HUD: Constructed.");
		}

		public override void DrawGUI()
		{
			base.DrawGUI();

			this.leftHUDPos.value = this.leftWindow.WindowPos;
			this.rightHUDPos.value = this.rightWindow.WindowPos;
		}

		protected void leftHUDWindow(int id)
		{
			StringBuilder leftHUD;

			leftHUD = new StringBuilder();

			VOID_Styles.labelHud.alignment = TextAnchor.UpperRight;

			if (this.core.powerAvailable)
			{
				leftHUD.AppendFormat("Primary: {0} Inc: {1}",
					VOID_Data.primaryName.ValueUnitString(),
					VOID_Data.orbitInclination.ValueUnitString("F3")
				);
				leftHUD.AppendFormat("\nObt Alt: {0} Obt Vel: {1}",
					VOID_Data.orbitAltitude.ToSIString(),
					VOID_Data.orbitVelocity.ToSIString()
				);
				leftHUD.AppendFormat("\nAp: {0} ETA {1}",
					VOID_Data.orbitApoAlt.ToSIString(),
					VOID_Data.timeToApo.ValueUnitString()
				);
				leftHUD.AppendFormat("\nPe: {0} ETA {1}",
					VOID_Data.oribtPeriAlt.ToSIString(),
					VOID_Data.timeToPeri.ValueUnitString()
				);
				leftHUD.AppendFormat("\nTot Δv: {0} Stg Δv: {1}",
					VOID_Data.totalDeltaV.ToSIString(2),
					VOID_Data.stageDeltaV.ToSIString(2)
				);
			}
			else
			{
				VOID_Styles.labelHud.normal.textColor = Color.red;
				leftHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(leftHUD.ToString(), VOID_Styles.labelHud, GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}

		protected void rightHUDWindow(int id)
		{
			StringBuilder rightHUD;

			rightHUD = new StringBuilder();

			VOID_Styles.labelHud.alignment = TextAnchor.UpperLeft;

			if (this.core.powerAvailable)
			{
				rightHUD.AppendFormat("Biome: {0} Sit: {1}",
					VOID_Data.currBiome.ValueUnitString(),
					VOID_Data.expSituation.ValueUnitString()
				);
				rightHUD.AppendFormat("\nSrf Alt: {0} Srf Vel: {1}",
					VOID_Data.trueAltitude.ToSIString(),
					VOID_Data.surfVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nVer: {0} Hor: {1}",
					VOID_Data.vertVelocity.ToSIString(),
					VOID_Data.horzVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nLat: {0} Lon: {1}",
					VOID_Data.surfLatitude.ValueUnitString(),
					VOID_Data.surfLongitude.ValueUnitString()
				);
				rightHUD.AppendFormat("\nHdg: {0} Pit: {1}",
					VOID_Data.vesselHeading.ValueUnitString(),
					VOID_Data.vesselPitch.ToSIString(2)
				);

				if (
					this.core.vessel.mainBody == this.core.HomeBody &&
					(
						this.core.vessel.situation == Vessel.Situations.FLYING ||
						this.core.vessel.situation == Vessel.Situations.SUB_ORBITAL ||
						this.core.vessel.situation == Vessel.Situations.LANDED ||
						this.core.vessel.situation == Vessel.Situations.SPLASHED
					)
				)
				{
					rightHUD.AppendFormat("\nRange to KSC: {0}", VOID_Data.downrangeDistance.ValueUnitString(2));
				}
			}
			else
			{
				VOID_Styles.labelHud.normal.textColor = Color.red;
				rightHUD.Append(string.Intern("-- POWER LOST --"));
			}


			GUILayout.Label(rightHUD.ToString(), VOID_Styles.labelHud, GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}
	}
}
