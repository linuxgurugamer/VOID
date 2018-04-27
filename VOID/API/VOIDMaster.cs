// VOID
//
// VOIDMaster.cs
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
//
///////////////////////////////////////////////////////////////////////////////
//
//  Much, much credit to Younata, Adammada, Nivvydaskrl and to all the authors
//  behind MechJeb, RemoteTech Relay Network, ISA MapSat, and Protractor for some
//  invaluable functions and making your nicely written code available to learn from.
//
///////////////////////////////////////////////////////////////////////////////
//
//  This software uses VesselSimulator and Engineer.Extensions from Engineer Redux.
//  Engineer Redux (c) 2013 cybutek
//  Used by permission.
//
///////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using KerbalEngineer.VesselSimulator;
using ToadicusTools.Extensions;

namespace VOID
{
    public class ThisGameObject
    {
        static internal GameObject thisGameObject;
        internal static void Destroy(UnityEngine.Object go)
        {
            UnityEngine.Object.Destroy(go);
        }

    }
	public abstract class VOIDMaster<T> : MonoBehaviour
		where T : VOIDCore_Generic<T>, new()
	{
		protected T Core;

        void Start()
        {
            ThisGameObject.thisGameObject = this.gameObject;
        }

        public abstract void Awake();

		public virtual void Update()
		{
			if (this.Core != null && !this.InValidScene())
			{
				this.LogDebug("We have a Core but the scene is not valid for this master.  Saving and disposing.");

				this.Core.SaveConfig ();
				this.Core.Dispose();
				this.Core = null;
				return;
			}

			if (this.Core == null && this.InValidScene())
			{
				this.LogDebug("We have no Core and the scene is valid for this master; re-trying Awake.");
				this.Awake();
				return;
			}

			this.Core.Update ();

			if (this.Core.FactoryReset)
			{
				this.LogDebug("Factory reset is true; deleting config and disposing!");

				KSP.IO.File.Delete<T>("config.xml");
				System.IO.File.Delete(this.Core.VOIDSettingsPath);

				this.Core.Dispose();
				this.Core = null;
			}
		}

		public virtual void FixedUpdate()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.FixedUpdate ();
		}

		public virtual void OnGUI()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnGUI();
		}

		public virtual void OnDestroy()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnDestroy();
		}

		public virtual void OnApplicationQuit()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnApplicationQuit();
		}

		protected virtual bool InValidScene()
		{
			object[] attributes = this.GetType().GetCustomAttributes(true);
			object attr;
			for (int idx = 0; idx < attributes.Length; idx++)
			{
				attr = attributes[idx];
				if (attr is KSPAddon)
				{
					KSPAddon addonAttr = (KSPAddon)attr;

					switch (addonAttr.startup)
					{
						case KSPAddon.Startup.EveryScene:
							return true;
						case KSPAddon.Startup.EditorAny:
							return HighLogic.LoadedSceneIsEditor;
						case KSPAddon.Startup.Flight:
							return HighLogic.LoadedSceneIsFlight;
						case KSPAddon.Startup.MainMenu:
							return HighLogic.LoadedScene == GameScenes.MAINMENU;
						case KSPAddon.Startup.SpaceCentre:
							return HighLogic.LoadedScene == GameScenes.SPACECENTER;
						case KSPAddon.Startup.TrackingStation:
							return HighLogic.LoadedScene == GameScenes.TRACKSTATION;
						default:
							return false;
					}
				}
			}

			return false;
		}
	}
}
