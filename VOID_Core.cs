//
//  VOID_Core.cs
//
//  Author:
//       toadicus <>
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP;
using UnityEngine;

namespace VOID
{
	public class VOID_Core : VOID_Module, IVOID_Module
	{
		/*
		 * Static Members
		 * */
		protected static bool _initialized = false;
		public static bool Initialized
		{
			get 
			{
			return _initialized;
			}
		}

		protected static VOID_Core _instance;
		public static VOID_Core Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new VOID_Core();
					_initialized = true;
				}
				return _instance;
			}
		}

		public static double Constant_G = 6.674e-11;

		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion = "0.9.9";

		[AVOID_SaveValue("configValue")]
		protected VOID_SaveValue<int> configVersion = 1;

		protected List<IVOID_Module> _modules = new List<IVOID_Module>();
		protected bool _modulesLoaded = false;

		protected List<Callback> _configurableCallbacks = new List<Callback>();

		[AVOID_SaveValue("mainWindowPos")]
		protected VOID_SaveValue<Rect> mainWindowPos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f);

		[AVOID_SaveValue("mainGuiMinimized")]
		protected VOID_SaveValue<bool> mainGuiMinimized = false;

		[AVOID_SaveValue("configWindowPos")]
		protected VOID_SaveValue<Rect> configWindowPos = new Rect(Screen.width / 2, Screen.height  /2, 10f, 10f);

		[AVOID_SaveValue("configWindowMinimized")]
		protected VOID_SaveValue<bool> configWindowMinimized = true;

		[AVOID_SaveValue("VOIDIconPos")]
		protected VOID_SaveValue<Rect> VOIDIconPos = new Rect(Screen.width / 2 - 200, Screen.height - 30, 30f, 30f);
		protected Texture2D VOIDIconOff = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconOn = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnPath = "VOID/Textures/void_icon_on";
		protected string VOIDIconOffPath = "VOID/Textures/void_icon_off";

		protected int windowBaseID = -96518722;

		[AVOID_SaveValue("togglePower")]
		public VOID_SaveValue<bool> togglePower = true;

		public bool powerAvailable = true;

		[AVOID_SaveValue("consumeResource")]
		protected VOID_SaveValue<bool> consumeResource = false;

		[AVOID_SaveValue("resourceName")]
		protected VOID_SaveValue<string> resourceName = "ElectricCharge";

		[AVOID_SaveValue("resourceRate")]
		protected VOID_SaveValue<float> resourceRate = 0.2f;

		public float saveTimer = 0;

		protected string defaultSkin = "KSP window 2";
		protected VOID_SaveValue<string> _skin;

		public bool configDirty;

		/*
		 * Properties
		 * */
		public List<IVOID_Module> Modules
		{
			get
			{
				return this._modules;
			}
		}

		public GUISkin Skin
		{
			get
			{
				if (this._skin == null)
				{
					this._skin = this.defaultSkin;
				}
				return AssetBase.GetGUISkin(this._skin);
			}
		}

		/*
		 * Methods
		 * */
		protected VOID_Core()
		{
			this._Name = "VOID Core";

			this.VOIDIconOn = GameDatabase.Instance.GetTexture (this.VOIDIconOnPath, false);
			this.VOIDIconOff = GameDatabase.Instance.GetTexture (this.VOIDIconOffPath, false);

			this.LoadConfig ();
		}

		protected void LoadModules()
		{
			var types = AssemblyLoader.loadedAssemblies
				.Select (a => a.assembly.GetExportedTypes ())
					.SelectMany (t => t)
					.Where (v => typeof(IVOID_Module).IsAssignableFrom (v)
					        && !(v.IsInterface || v.IsAbstract) &&
					        !typeof(VOID_Core).IsAssignableFrom (v)
					        );

			Tools.PostDebugMessage (string.Format (
				"{0}: Found {1} modules to check.",
				this.GetType ().Name,
				types.Count ()
				));
			foreach (var voidType in types)
			{
				Tools.PostDebugMessage (string.Format (
					"{0}: found Type {1}",
					this.GetType ().Name,
					voidType.Name
					));

				this.LoadModule(voidType);
			}

			this._modulesLoaded = true;

			Tools.PostDebugMessage (string.Format ("VOID_Core: Loaded {0} modules.", this.Modules.Count));
		}

		protected void LoadModule(Type T)
		{
			var existingModules = this._modules.Where (mod => mod.GetType ().Name == T.Name);
			if (existingModules.Any())
			{
				Tools.PostDebugMessage(string.Format(
					"{0}: refusing to load {1}: already loaded",
					this.GetType().Name,
					T.Name
				));
				return;
			}
			IVOID_Module module = Activator.CreateInstance (T) as IVOID_Module;
			module.LoadConfig();
			this._modules.Add (module);

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded module {1}.",
				this.GetType().Name,
				T.Name
			));
		}

		public void Update()
		{
			this.saveTimer += Time.deltaTime;

			if (!this.guiRunning)
			{
				this.StartGUI ();
			}

			if (!HighLogic.LoadedSceneIsFlight && this.guiRunning)
			{
				this.StopGUI ();
			}

			foreach (IVOID_Module module in this.Modules)
			{
				if (!module.guiRunning && module.toggleActive)
				{
					module.StartGUI ();
				}
				if (module.guiRunning && !module.toggleActive || !this.togglePower || !HighLogic.LoadedSceneIsFlight)
				{
					module.StopGUI();
				}
			}

			if (this.saveTimer > 15f)
			{
				this.SaveConfig ();
				this.saveTimer = 0;
			}
		}

		public void FixedUpdate()
		{
			if (this.consumeResource &&
			    this.vessel.vesselType != VesselType.EVA &&
			    TimeWarp.deltaTime != 0
			    )
			{
				float powerReceived = this.vessel.rootPart.RequestResource(this.resourceName,
				                                                          this.resourceRate * TimeWarp.fixedDeltaTime);
				if (powerReceived > 0)
				{
					this.powerAvailable = true;
				}
				else
				{
					this.powerAvailable = false;
				}
			}
		}

		public void VOIDMainWindow(int _)
		{
			GUILayout.BeginVertical();
			
			if (this.powerAvailable)
			{
				string str = "ON";
				if (togglePower) str = "OFF";
				if (GUILayout.Button("Power " + str)) togglePower = !togglePower;
			    if (togglePower)
			    {
					foreach (IVOID_Module module in this.Modules)
					{
						module.toggleActive = GUILayout.Toggle (module.toggleActive, module.Name);
					}
			    }
			}
			else
			{
			    GUIStyle label_txt_red = new GUIStyle(GUI.skin.label);
			    label_txt_red.normal.textColor = Color.red;
			    label_txt_red.alignment = TextAnchor.MiddleCenter;
			    GUILayout.Label("-- POWER LOST --", label_txt_red);
			}

			this.configWindowMinimized = !GUILayout.Toggle (!this.configWindowMinimized, "Configuration");

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		public void VOIDConfigWindow(int _)
		{
			GUILayout.BeginVertical ();

			this.DrawConfigurables ();

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public override void DrawConfigurables()
		{
			this.consumeResource = GUILayout.Toggle (this.consumeResource, "Consume Resources");

			foreach (IVOID_Module mod in this.Modules)
			{
				mod.DrawConfigurables ();
			}
		}

		public override void DrawGUI()
		{
			if (!this._modulesLoaded)
			{
				this.LoadModules ();
			}

			GUI.skin = this.Skin;

			int windowID = this.windowBaseID;

			this.VOIDIconTexture = this.VOIDIconOff;  //icon off default
			if (this.togglePower) this.VOIDIconTexture = this.VOIDIconOn;     //or on if power_toggle==true
			if (GUI.Button(new Rect(VOIDIconPos), VOIDIconTexture, new GUIStyle()))
			{
				this.mainGuiMinimized = !this.mainGuiMinimized;
			}

			if (!this.mainGuiMinimized)
			{
				Rect _mainWindowPos = this.mainWindowPos;

				_mainWindowPos = GUILayout.Window (
					++windowID,
					_mainWindowPos,
					this.VOIDMainWindow,
					string.Join (" ", new string[] {this.VoidName, this.VoidVersion}),
					GUILayout.Width (250),
					GUILayout.Height (50)
				);

				if (_mainWindowPos != this.mainWindowPos)
				{
					this.mainWindowPos = _mainWindowPos;
				}
			}

			if (!this.configWindowMinimized)
			{
				Rect _configWindowPos = this.configWindowPos;

				_configWindowPos = GUILayout.Window (
					++windowID,
					_configWindowPos,
					this.VOIDConfigWindow,
					string.Join (" ", new string[] {this.VoidName, "Configuration"}),
					GUILayout.Width (250),
					GUILayout.Height (50)
				);

				if (_configWindowPos != this.configWindowPos)
				{
					this.configWindowPos = _configWindowPos;
				}
			}
		}

		public override void LoadConfig()
		{
			base.LoadConfig ();

			foreach (IVOID_Module module in this.Modules)
			{
				module.LoadConfig ();
			}
		}

		public override void SaveConfig()
		{
			if (!this.configDirty)
			{
				return;
			}

			base.SaveConfig ();

			foreach (IVOID_Module module in this.Modules)
			{
				module.SaveConfig ();
			}

			this.configDirty = false;
		}
	}
}

