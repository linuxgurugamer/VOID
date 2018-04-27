// VOID © 2015 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/

using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public static class VOID_StageExtensions
	{
		public static double NominalThrust(this Stage stage)
		{
			if (stage.actualThrust == 0d)
			{
				return stage.thrust;
			}
			else
			{
				return stage.actualThrust;
			}
		}

		public static double MassFlow(this Stage stage)
		{
			double stageIsp = VOID_Data.Core.LastStage.isp;
			double stageThrust = stage.NominalThrust();

			return stageThrust / (stageIsp * VOID_Data.KerbinGee);
		}
	}
}

