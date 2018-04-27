// VOID
//
// VOID_SurfAtmo.cs
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
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_SurfAtmo : VOID_WindowModule
	{
		[AVOID_SaveValue("precisionValues")]
		protected VOID_SaveValue<long> _precisionValues;
		protected IntCollection precisionValues;

		public VOID_SurfAtmo()
		{
			this.Name = "Surface & Atmospheric Info";

			this.WindowPos.x = Screen.width - 260f;
			this.WindowPos.y = 85;

			this._precisionValues = (VOID_SaveValue<long>)384307168202282325;
		}

		public override void ModuleWindow(int id)
		{
			int idx = 0;

			GUILayout.BeginVertical();

			this.precisionValues[idx] = (ushort)VOID_Data.trueAltitude.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			VOID_Data.surfLatitudeString.DoGUIHorizontal();

			VOID_Data.surfLongitudeString.DoGUIHorizontal();

			VOID_Data.vesselHeading.DoGUIHorizontal();

			this.precisionValues[idx] = (ushort)VOID_Data.terrainElevation.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.downrangeDistance.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.surfVelocity.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.vertVelocity.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.horzVelocity.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			VOID_Data.temperature.DoGUIHorizontal("F2");

			this.precisionValues[idx] = (ushort)VOID_Data.atmDensity.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.atmPressure.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			this.precisionValues[idx] = (ushort)VOID_Data.atmLimit.DoGUIHorizontal(this.precisionValues[idx]);
			idx++;

			// Toadicus edit: added Biome
			VOID_Data.currBiome.DoGUIHorizontal();

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}

		public override void LoadConfig(KSP.IO.PluginConfiguration config)
		{
			base.LoadConfig(config);

			this.precisionValues = new IntCollection(4, this._precisionValues);
		}

		public override void Save(KSP.IO.PluginConfiguration config, string sceneKey)
		{
			this._precisionValues.value = this.precisionValues.collection;

			base.Save(config, sceneKey);
		}
	}
}