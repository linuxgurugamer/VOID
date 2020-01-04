// VOID
//
// VOID_EditorHUD.cs
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
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Text;
using ToadicusTools;
using ToadicusTools.Text;
using UnityEngine;

namespace VOID
{
	[VOID_Scenes(GameScenes.EDITOR)]
	public class VOID_EditorHUD : VOID_HUDModule
	{
		/*
		 * Fields
		 * */
		protected HUDWindow ehudWindow;
		protected EditorVesselOverlays _vesselOverlays;

		[AVOID_SaveValue("snapToLeft")]
		protected VOID_SaveValue<bool> snapToLeft;

		/*
		 * Properties
		 * */
		protected EditorVesselOverlays vesselOverlays
		{
			get
			{
				if (this._vesselOverlays == null)
				{
					UnityEngine.Object[] overlayObjs = Resources.FindObjectsOfTypeAll(typeof(EditorVesselOverlays));

					if (overlayObjs.Length > 0)
					{
						this._vesselOverlays = (EditorVesselOverlays)overlayObjs[0];
					}
				}

				return this._vesselOverlays;
			}
		}

		protected EditorMarker_CoM CoMmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoMmarker;
			}
		}

		protected EditorMarker_CoT CoTmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoTmarker;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_EditorHUD() : base()
		{
			this.Name = "Heads-Up Display";

			this.Active = true;

			this.snapToLeft.value = true;

			this.ehudWindow = new HUDWindow(
				"editorHUD",
				this.ehudWindowFunc,
				new Rect(10f, 125f, 300f, 64f)
			);
			this.Windows.Add(this.ehudWindow);

			Logging.PostDebugMessage (this.GetType().Name + ": Constructed.");
		}

		public void ehudWindowFunc(int id)
		{
			using (PooledStringBuilder hudString = PooledStringBuilder.Get())
			{
				if (this.core.LastStage == null)
				{
					return;
				}

				VOID_Styles.labelHud.alignment = TextAnchor.UpperLeft;

				hudString.Append("Total Mass: ");
				hudString.Append(this.core.LastStage.totalMass.ToString("F3"));
				hudString.Append('t');

				hudString.Append(' ');

				hudString.Append("Part Count: ");
				hudString.Append(EditorLogic.SortedShipList.Count);

				hudString.Append('\n');

				hudString.Append("Total Delta-V: ");
				hudString.Append(SIFormatProvider.ToSI(this.core.LastStage.totalDeltaV));
				hudString.Append("m/s");

				hudString.Append('\n');

				hudString.Append("Bottom Stage Delta-V: ");
				hudString.Append(SIFormatProvider.ToSI(this.core.LastStage.deltaV));
				hudString.Append("m/s");

				hudString.Append('\n');

				hudString.Append("Bottom Stage T/W Ratio: ");
				hudString.Append(this.core.LastStage.thrustToWeight.ToString("F3"));

				Logging.PostDebugMessage(this,
					"CoMmarker.gameObject.activeInHierarchy: {0};" +
					"CoTmarker.gameObject.activeInHierarchy: {1}",
					this.CoMmarker.gameObject.activeInHierarchy,
					this.CoTmarker.gameObject.activeInHierarchy
				);

				if (this.CoMmarker.gameObject.activeInHierarchy && this.CoTmarker.gameObject.activeInHierarchy)
				{
					Logging.PostDebugMessage(this, "CoM and CoT markers are active, doing thrust offset.");
					hudString.Append('\n');

					hudString.Append("Thrust Offset: ");
					hudString.Append(
						Vector3.Cross(
							this.CoTmarker.dirMarkerObject.transform.forward,
							this.CoMmarker.posMarkerObject.transform.position - this.CoTmarker.posMarkerObject.transform.position
						).ToString("F3"));
				}
				#if DEBUG
			else
			{
				Logging.PostDebugMessage(this, "CoM and CoT markers are not active, thrust offset skipped.");
			}
				#endif

				GUILayout.Label(
					hudString.ToString(),
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
			float hudLeft;

			try
			{
				switch (EditorLogic.fetch.editorScreen)
				{
					case EditorScreen.Parts:
						hudLeft = 16f + EditorPanels.Instance.partsEditor.panelTransform.rect.width +
							EditorPanels.Instance.partcategorizerModes.transform.localPosition.x;
						break;
					case EditorScreen.Actions:
						hudLeft = EditorPanels.Instance.actions.transform.localPosition.x + 464f;
						break;
					default:
						return;
				}
			}
			catch (NullReferenceException)
			{
				Logging.PostErrorMessage(
					"[{0}]: Something was null when fetching panel geometry; skipping frame.",
					this.GetType().FullName
				);

				return;
			}

			base.DrawGUI(this);

			Rect hudPos = this.ehudWindow.WindowPos;

			if (this.snapToLeft && this.positionsLocked)
			{
				hudPos.xMin = hudLeft;
			}
			else
			{
				hudPos.xMin = Mathf.Max(hudLeft, hudPos.xMin);
			}

			hudPos.width = this.ehudWindow.defaultWindowPos.width;

			this.ehudWindow.WindowPos = hudPos;

			this.snapToLeft.value = Mathf.Abs(this.ehudWindow.WindowPos.xMin - hudLeft) < 15f;
		}
	}
}
