// VOID
//
// IVOID_Core.cs
//
// Copyright © 2015, toadicus
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
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public abstract class VOIDCore : VOID_WindowModule, IVOID_Module
	{
		public const double Constant_G = 6.674e-11;
		public const int CONFIG_VERSION = 2;

		public static event VOIDEventHandler onModulesLoaded;
		public static event VOIDEventHandler onModulesDestroyed;

		protected static void FireOnModulesLoaded(object sender)
		{
			if (onModulesLoaded != null)
			{
				onModulesLoaded(sender);
			}
		}

		protected static void FireOnModulesDestroyed(object sender)
		{
			if (onModulesDestroyed != null)
			{
				onModulesDestroyed(sender);
			}
		}

		public abstract int ConfigVersion { get; }
		public virtual bool configNeedsUpdate { get; set; }

		public virtual string SaveGamePath { get; protected set; }
		public virtual string VOIDSettingsPath { get; protected set; }

		public abstract string SceneKey { get; }

		public abstract int WindowID { get; }
		public abstract bool configDirty { get; set; }
		public abstract bool powerAvailable	{ get; protected set; }

		public virtual bool gameUIHidden { get; protected set; }

		public abstract IList<IVOID_Module> Modules { get; }

		public abstract float UpdateTimer { get; protected set; }
		public abstract double UpdatePeriod { get; }

		public virtual float saveTimer { get; protected set; }

		public virtual GUISkin Skin { get; protected set; }

		public abstract CelestialBody HomeBody { get; }
		public abstract List<CelestialBody> SortedBodyList { get; protected set; }

		public abstract VesselType[] AllVesselTypes { get; protected set; }
		public abstract Stage LastStage { get; protected set; }
		public abstract Stage[] Stages { get; protected set; }

		public abstract VOID_TimeScale TimeScale { get; protected set; }

		public abstract event VOIDEventHandler onApplicationQuit;
		public abstract event VOIDEventHandler onSkinChanged;
		public abstract event VOIDEventHandler onUpdate;

		public abstract event VOIDEventHandler onPreForEach;
		public abstract event VOIDForEachPartHandler onForEachPart;
		public abstract event VOIDForEachPartModuleHandler onForEachModule;
		public abstract event VOIDEventHandler onPostForEach;

		public virtual event VOIDEventHandler onPreRender;
		public virtual event VOIDEventHandler onPostRender;

		public virtual bool MethodInPreRenderQueue(VOIDEventHandler method)
		{
			if (this.onPreRender != null)
			{
				ToadicusTools.Logging.PostDebugMessage(this, "Looking in onPreRender for method {0} in onGui", method);

				foreach (Delegate invoker in this.onPreRender.GetInvocationList())
				{
					ToadicusTools.Logging.PostDebugMessage(this, "Checking invoker {0}", invoker);

					if (System.Object.Equals(invoker, method))
					{
						ToadicusTools.Logging.PostDebugMessage(this, "Found match.");
						return true;
					}
				}
			}
			#if DEBUG
			else
			{
			ToadicusTools.Logging.PostDebugMessage(this, "this.onPreRender == null");
			}
			#endif


			return false;
		}

		public virtual bool MethodInPostRenderQueue(VOIDEventHandler method)
		{
			if (this.onPostRender != null)
			{
				ToadicusTools.Logging.PostDebugMessage(this, "Looking in onPostRender for method {0} in onGui", method);

				foreach (Delegate invoker in this.onPostRender.GetInvocationList())
				{
					ToadicusTools.Logging.PostDebugMessage(this, "Checking invoker {0}", invoker);

					if (System.Object.Equals(invoker, method))
					{
						ToadicusTools.Logging.PostDebugMessage(this, "Found match.");
						return true;
					}
				}
			}
			#if DEBUG
			else
			{
				ToadicusTools.Logging.PostDebugMessage(this, "this.onPostRender == null");
			}
			#endif


			return false;
		}

		public void OnGUI()
		{
			if (this.gameUIHidden)
			{
				return;
			}

			if (Event.current.type == EventType.Repaint || Event.current.isMouse)
			{
				if (this.onPreRender != null)
				{
					ToadicusTools.Logging.PostDebugMessage(this, "In OnGUI; doing 'pre draw' stuff");
					this.onPreRender(this);
				}
			}

			if (this.onPostRender != null)
			{
				ToadicusTools.Logging.PostDebugMessage(this, "In OnGUI; doing 'post draw' stuff");
				this.onPostRender(this);
			}
		}

		public abstract void SaveConfig();

		public override void Save(KSP.IO.PluginConfiguration config, string sceneKey)
		{
			base.Save(config, sceneKey);

		}
	}

	public delegate void VOIDEventHandler(object sender);
	public delegate void VOIDForEachPartHandler(object sender, VOIDForEachPartArgs args);
	public delegate void VOIDForEachPartModuleHandler(object sender, VOIDForEachPartModuleArgs args);

	public abstract class VOIDForEachEventArgs<T> : EventArgs where T : class
	{
		public T Data;

		public VOIDForEachEventArgs(T data)
		{
			this.Data = data;
		}
	}

	public class VOIDForEachPartArgs : VOIDForEachEventArgs<Part>
	{
		public VOIDForEachPartArgs(Part data) : base(data) {}
	}
	public class VOIDForEachPartModuleArgs : VOIDForEachEventArgs<PartModule>
	{
		public VOIDForEachPartModuleArgs(PartModule data) : base(data) {}
	}
}

