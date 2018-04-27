// VOID
//
// VOID_VesselInfo.cs
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
using KerbalEngineer.Extensions;
using KSP;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public class VOID_VesselInfo : VOID_WindowModule
	{
		public VOID_VesselInfo() : base()
		{
			this.Name = "Vessel Information";

			this.WindowPos.x = Screen.width - 260;
			this.WindowPos.y = 450;
		}

		public override void ModuleWindow(int id)
		{
			if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
			{
				SimManager.RequestSimulation();
			}

			GUILayout.BeginVertical();

			GUILayout.Label(
				Vessel.vesselName,
				VOID_Styles.labelCenterBold,
				GUILayout.ExpandWidth(true));

			VOID_Data.geeForce.DoGUIHorizontal("F2");

			VOID_Data.partCount.DoGUIHorizontal();

			VOID_Data.totalMass.DoGUIHorizontal("F3");

			VOID_Data.stageResourceMass.DoGUIHorizontal("F3");

			VOID_Data.resourceMass.DoGUIHorizontal("F3");

			VOID_Data.stageDeltaV.DoGUIHorizontal(3, false);

			VOID_Data.totalDeltaV.DoGUIHorizontal(3, false);

			VOID_Data.mainThrottle.DoGUIHorizontal("P0");

			VOID_Data.currmaxThrust.DoGUIHorizontal();

			VOID_Data.currmaxThrustWeight.DoGUIHorizontal();

			VOID_Data.surfaceThrustWeight.DoGUIHorizontal("F2");

			VOID_Data.intakeAirStatus.DoGUIHorizontal();

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}
	}
}
