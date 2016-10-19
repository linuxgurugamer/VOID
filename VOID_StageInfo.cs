// VOID © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/

//using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools.GUIUtils;
using UnityEngine;

namespace VOID
{
	[VOID_Scenes(GameScenes.EDITOR, GameScenes.FLIGHT)]
	public class VOID_StageInfo : VOID_WindowModule
	{
		private Table stageTable;

		private Table.Column<int> stageNumberCol;
		private Table.Column<double> stageDeltaVCol;
		private Table.Column<double> stageTotalDVCol;
		private Table.Column<double> stageInvertDVCol;
		private Table.Column<double> stageMassCol;
		private Table.Column<double> stageTotalMassCol;
		private Table.Column<double> stageThrustCol;
		private Table.Column<double> stageTWRCol;
		private Table.Column<string> stageTimeCol;

		private bool stylesApplied;

		private bool showBodyList;
		private Rect bodyListPos;

		private CelestialBody _selectedBody;
		[AVOID_SaveValue("bodyIdx")]
		private VOID_SaveValue<int> bodyIdx;
		private int lastIdx;

		private bool showAdvanced;

		[AVOID_SaveValue("UseSealLevel")]
		private VOID_SaveValue<bool> useSeaLevel;
		private GUIContent seaLevelToggle;

		private CelestialBody selectedBody
		{
			get
			{
				return this._selectedBody;
			}
			set
			{
				this._selectedBody = value;
				KerbalEngineer.Helpers.CelestialBodies.SelectedBody = value;
			}
		}

		public VOID_StageInfo() : base()
		{
			this.Name = "Stage Information";
			this.defWidth = 20f;
			this.bodyIdx = (VOID_SaveValue<int>)4;

			this.stylesApplied = false;
			this.showBodyList = false;

			this.bodyListPos = new Rect();

			this.stageTable = new Table();

			this.stageNumberCol = new Table.Column<int>("Stage", 20f);
			this.stageTable.Add(this.stageNumberCol);

			this.stageDeltaVCol = new Table.Column<double>("DeltaV [m/s]", 20f);
			this.stageDeltaVCol.Format = "S2";
			this.stageTable.Add(this.stageDeltaVCol);

			this.stageTotalDVCol = new Table.Column<double>("Total ΔV [m/s]", 20f);
			this.stageTotalDVCol.Format = "S2";
			this.stageTable.Add(this.stageTotalDVCol);

			this.stageInvertDVCol = new Table.Column<double>("Invert ΔV [m/s]", 20f);
			this.stageInvertDVCol.Format = "S2";
			this.stageTable.Add(this.stageInvertDVCol);

			this.stageMassCol = new Table.Column<double>("Mass [Mg]", 20f);
			this.stageMassCol.Format = "#.#";
			this.stageTable.Add(this.stageMassCol);

			this.stageTotalMassCol = new Table.Column<double>("Total [Mg]", 20f);
			this.stageTotalMassCol.Format = "#.#";
			this.stageTable.Add(this.stageTotalMassCol);

			this.stageThrustCol = new Table.Column<double>("Thrust [N]", 20f);
			this.stageThrustCol.Format = "S2";
			this.stageTable.Add(this.stageThrustCol);

			this.stageTWRCol = new Table.Column<double>("T/W Ratio", 20f);
			this.stageTWRCol.Format = "#.#";
			this.stageTable.Add(this.stageTWRCol);

			this.stageTimeCol = new Table.Column<string>("Burn Time", 20f);
			this.stageTable.Add(this.stageTimeCol);

			this.showAdvanced = false;

			this.useSeaLevel = (VOID_SaveValue<bool>)false;

			seaLevelToggle = new GUIContent(
				"Use Sea Level",
				"Use 'sea' level atmospheric conditions on bodies with atmospheres."
			);
		}

		public override void DrawGUI(object sender)
		{
			base.DrawGUI(sender);

			if (this.showBodyList)
			{
				GUILayout.Window(core.WindowID, this.bodyListPos, this.BodyPickerWindow, string.Empty);
			}
		}

		public override void ModuleWindow(int id)
		{
			if (this.selectedBody == null)
			{
				this.selectedBody = core.HomeBody;
			}

			if (
				!HighLogic.LoadedSceneIsFlight ||
				(TimeWarp.WarpMode == TimeWarp.Modes.LOW) ||
				(TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate)
			)
			{
				KerbalEngineer.VesselSimulator.SimManager.RequestSimulation();
			}

			if (!this.stylesApplied)
			{
				this.stageTable.ApplyCellStyle(VOID_Styles.labelCenter);
				this.stageTable.ApplyHeaderStyle(VOID_Styles.labelCenterBold);
			}

			this.stageTable.ClearColumns();

			if (core.Stages == null || core.Stages.Length == 0)
			{
				GUILayout.BeginVertical();

				GUILayout.Label("No stage data!");

				GUILayout.EndVertical();

				return;
			}

			if (HighLogic.LoadedSceneIsEditor && this.selectedBody.atmosphere && this.useSeaLevel)
			{
				SimManager.Atmosphere = this.selectedBody.GetPressure(0) * PhysicsGlobals.KpaToAtmospheres;
			}
			else
			{
				SimManager.Atmosphere = 0d;
			}

			Stage stage;
			for (int idx = 0; idx < core.Stages.Length; idx++)
			{
				stage = core.Stages[idx];

				if (stage.deltaV == 0 && stage.mass == 0)
				{
					continue;
				}

				this.stageNumberCol.Add(stage.number);

				this.stageDeltaVCol.Add(stage.deltaV);
				this.stageTotalDVCol.Add(stage.totalDeltaV);
				this.stageInvertDVCol.Add(stage.inverseTotalDeltaV);

				this.stageMassCol.Add(stage.mass);
				this.stageTotalMassCol.Add(stage.totalMass);      

				this.stageThrustCol.Add(stage.thrust * 1000f);
				this.stageTWRCol.Add(stage.thrustToWeight / (this.selectedBody ?? core.HomeBody).GeeASL);

				this.stageTimeCol.Add(VOID_Tools.FormatInterval(stage.time));
			}

			this.stageTable.Render();

			if (core.SortedBodyList != null)
			{
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("◄"))
				{
					this.bodyIdx.value--;
				}

				this.showBodyList = GUILayout.Toggle(
					this.showBodyList,
					(this.selectedBody ?? core.HomeBody).bodyName,
					GUI.skin.button
				);

				Rect bodyButtonPos = GUILayoutUtility.GetLastRect();

				if (Event.current.type == EventType.Repaint)
				{
					this.bodyListPos.width = bodyButtonPos.width;
					this.bodyListPos.x = bodyButtonPos.xMin + this.WindowPos.xMin;
					this.bodyListPos.y = bodyButtonPos.yMax + this.WindowPos.yMin;
				}

				if (GUILayout.Button("►"))
				{
					this.bodyIdx.value++;
				}

				this.bodyIdx.value %= core.SortedBodyList.Count;

				if (this.bodyIdx < 0)
				{
					this.bodyIdx.value += core.SortedBodyList.Count;
				}

				if (this.lastIdx != this.bodyIdx)
				{
					this.lastIdx = this.bodyIdx;
					this.selectedBody = core.SortedBodyList[this.bodyIdx];
				}

				if (HighLogic.LoadedSceneIsEditor)
				{
					if (
						GUILayout.Button(
							this.showAdvanced ? "▲" : "▼",
							GUILayout.ExpandWidth(false)
						)
					)
					{
						this.showAdvanced = !this.showAdvanced;
					}
				}

				GUILayout.EndHorizontal();
			}

			if (this.showAdvanced && HighLogic.LoadedSceneIsEditor)
			{
				GUILayout.BeginHorizontal();

				this.useSeaLevel.value = Layout.Toggle(this.useSeaLevel, this.seaLevelToggle, false);

				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();

			if (
				GUILayout.Button("Engineering data powered by <i>VesselSimulator from KER</i>.",
					VOID_Styles.labelLink)
			)
			{
				Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/18230");
			}

			GUILayout.EndHorizontal();

			base.ModuleWindow(id);
		}

		public override void DrawConfigurables()
		{
			
		}

		private void BodyPickerWindow(int _)
		{
			CelestialBody body;
			for (int idx = 0; idx < core.SortedBodyList.Count; idx++)
			{
				body = core.SortedBodyList[idx];
				if (GUILayout.Button(body.bodyName, VOID_Styles.labelDefault))
				{
					Debug.Log("Picked new body focus: " + body.bodyName);
					this.bodyIdx.value = core.SortedBodyList.IndexOf(body);
					this.showBodyList = false;
				}
			}
		}
	}
}

