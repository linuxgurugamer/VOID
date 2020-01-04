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
using ToadicusTools.GUIUtils;
using ToadicusTools.Text;
using UnityEngine;

namespace VOID
{
	public class VOID_HUDAdvanced : VOID_HUDModule, IVOID_Module
	{
		/*
		 * Fields
		 * */
		protected VOID_HUD primaryHUD;

		protected HUDWindow leftHUD;
		protected HUDWindow rightHUD;

		/*
		 * Properties
		 * */
		public override int ColorIndex
		{
			get
			{
				if (this.primaryHUD != null)
				{
					return this.primaryHUD.ColorIndex;
				}

				return base.ColorIndex;
			}
			set
			{
				base.ColorIndex = value;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_HUDAdvanced() : base()
		{
			this.Name = "Advanced Heads-Up Display";

			this.Active = true;

			this.leftHUD = new HUDWindow("leftHUD",
				this.leftHUDWindow,
				new Rect(
					Screen.width * .5f - 300f - 220f * GameSettings.UI_SCALE,
					Screen.height - 200f,
					300f, 90f)
			);
			this.Windows.Add(this.leftHUD);

			this.rightHUD = new HUDWindow(
				"rightHUD",
				this.rightHUDWindow,
				new Rect(
					Screen.width * .5f + 180f * GameSettings.UI_SCALE,
					Screen.height - 200f,
					300f, 90f)
			);
			this.Windows.Add(this.rightHUD);

			this.positionsLocked.value = true;

			Logging.PostDebugMessage (this, "Constructed.");
		}

		protected void leftHUDWindow(int id)
		{
			using (PooledStringBuilder leftHUD = PooledStringBuilder.Get())
			{
				VOID_Styles.labelHud.alignment = TextAnchor.UpperRight;

				if (this.core.powerAvailable)
				{
					leftHUD.AppendFormat(
						string.Intern("Mass: {0}\n"),
						VOID_Data.totalMass.ToSIString(2)
					);

					if (VOID_Data.vesselCrewCapacity > 0)
					{
						leftHUD.AppendFormat(
							string.Intern("Crew: {0} / {1}\n"),
							VOID_Data.vesselCrewCount.Value,
							VOID_Data.vesselCrewCapacity.Value
						);
					}

					leftHUD.AppendFormat(
						string.Intern("Acc: {0} T:W: {1}\n"),
						VOID_Data.vesselAccel.ToSIString(2),
						VOID_Data.currThrustWeight.Value.ToString("f2")
					);

					leftHUD.AppendFormat(
						string.Intern("Ang Vel: {0}\n"),
						VOID_Data.vesselAngularVelocity.ToSIString(2)
					);

					if (VOID_Data.stageNominalThrust != 0d)
					{
						leftHUD.AppendFormat(
							string.Intern("Thrust Offset: {0}\n"),
							VOID_Data.vesselThrustOffset.Value.ToString("F1")
						);
					}
				}
				else
				{
					VOID_Styles.labelHud.normal.textColor = Color.red;
					leftHUD.Append(string.Intern("-- POWER LOST --"));
				}

				GUILayout.Label(
					leftHUD.ToString(),
					VOID_Styles.labelHud,
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true)
				);

				if (!this.positionsLocked)
				{
					GUI.DragWindow();
				}

				GUI.BringWindowToBack(id);
			}
		}

		protected void rightHUDWindow(int id)
		{
			using (PooledStringBuilder rightHUD = PooledStringBuilder.Get())
			{
				VOID_Styles.labelHud.alignment = TextAnchor.UpperLeft;

				if (this.core.powerAvailable)
				{
					rightHUD.AppendFormat(
						"Burn Δv (Rem/Tot): {0} / {1}\n",
						VOID_Data.currManeuverDVRemaining.ValueUnitString("f2"),
						VOID_Data.currManeuverDeltaV.ValueUnitString("f2")
					);

					if (VOID_Data.upcomingManeuverNodes > 1)
					{
						rightHUD.AppendFormat("Next Burn Δv: {0}\n",
							VOID_Data.nextManeuverDeltaV.ValueUnitString("f2")
						);
					}

					rightHUD.AppendFormat("Burn Time (Rem/Total): {0} / {1}\n",
						VOID_Tools.FormatInterval(VOID_Data.currentNodeBurnRemaining.Value),
						VOID_Tools.FormatInterval(VOID_Data.currentNodeBurnDuration.Value)
					);

					if (VOID_Data.burnTimeDoneAtNode.Value != string.Empty)
					{
						rightHUD.AppendFormat("{0} (done @ node)\n",
							VOID_Data.burnTimeDoneAtNode.Value
						);

						rightHUD.AppendFormat("{0} (½ done @ node)",
							VOID_Data.burnTimeHalfDoneAtNode.Value
						);
					}
					else
					{
						rightHUD.Append("Node is past");
					}
				}
				else
				{
					VOID_Styles.labelHud.normal.textColor = Color.red;
					rightHUD.Append(string.Intern("-- POWER LOST --"));
				}

				GUILayout.Label(
					rightHUD.ToString(),
					VOID_Styles.labelHud,
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true)
				);

				if (!this.positionsLocked)
				{
					GUI.DragWindow();
				}

				GUI.BringWindowToBack(id);
			}
		}

		public override void DrawGUI(object sender)
		{
			if (this.primaryHUD == null)
			{
				IVOID_Module module;
				for (int idx = 0; idx < this.core.Modules.Count; idx++)
				{
					module = this.core.Modules[idx];

					if (module is VOID_HUD)
					{
						this.primaryHUD = module as VOID_HUD;
					}
				}
			}

			if (VOID_Data.upcomingManeuverNodes < 1 && this.Windows.Contains(this.rightHUD))
			{
				this.Windows.Remove(this.rightHUD);
			}
			else if (VOID_Data.upcomingManeuverNodes > 0 && !this.Windows.Contains(this.rightHUD))
			{
				this.Windows.Add(this.rightHUD);
			}

			base.DrawGUI(sender);
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button(string.Intern("Reset Advanced HUD Positions"), GUILayout.ExpandWidth(false)))
			{
				HUDWindow window;
				for (int idx = 0; idx < this.Windows.Count; idx++)
				{
					window = this.Windows[idx];

					window.WindowPos = new Rect(window.defaultWindowPos);
				}
			}

			this.positionsLocked.value = Layout.Toggle(this.positionsLocked, string.Intern("Lock Advanced HUD Positions"));
		}
	}
}
