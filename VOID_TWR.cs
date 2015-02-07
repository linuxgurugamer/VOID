﻿// VOID © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/

using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	[VOID_Scenes(GameScenes.EDITOR, GameScenes.FLIGHT)]
	public class VOID_TWR : VOID_WindowModule
	{
		public VOID_TWR() : base()
		{
			this.Name = "IP Thrust-to-Weight Ratios";
		}

		public override void ModuleWindow(int id)
		{
			if (
				!HighLogic.LoadedSceneIsFlight ||
				(TimeWarp.WarpMode == TimeWarp.Modes.LOW) ||
				(TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate)
			)
			{
				KerbalEngineer.VesselSimulator.SimManager.RequestSimulation();
			}

			GUILayout.BeginVertical();

			if (core.SortedBodyList == null)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				GUILayout.Label("Unavailable");

				GUILayout.EndHorizontal();
			}
			else
			{
				foreach (CelestialBody body in core.SortedBodyList)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

					GUILayout.Label(body.bodyName);
					GUILayout.FlexibleSpace();
					GUILayout.Label(
						(VOID_Data.nominalThrustWeight.Value / body.GeeASL).ToString("0.0##"),
						GUILayout.ExpandWidth(true)
					);

					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}
	}
}

