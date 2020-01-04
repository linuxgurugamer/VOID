// VOID
//
// VOIDCore_Generic.cs
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

using KerbalEngineer.Editor;
using KerbalEngineer.Helpers;
using KerbalEngineer.VesselSimulator;
using KSP;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Text;
using ToadicusTools;
using ToadicusTools.DebugTools;
using ToadicusTools.Extensions;
using ToadicusTools.GUIUtils;
using ToadicusTools.Wrappers;
using UnityEngine;
using ToolbarControl_NS;

namespace VOID
{
    public abstract class VOIDCore_Generic<T> : VOID_SingletonCore<T>, IVOID_Module, IDisposable
        where T : VOID_Module, new()
    {
        /*
		 * Fields
		 * */
        internal const string MODID = "coreToggle";
        internal const string MODNAME = "VOID";

//        protected string VoidName = "VOID";
        protected string VoidVersion;

        [AVOID_SaveValue("configValue")]
        protected VOID_SaveValue<int> configVersion = (VOID_SaveValue<int>)VOIDCore.CONFIG_VERSION;

        protected List<IVOID_Module> modules = new List<IVOID_Module>();
        protected bool modulesLoaded = false;

        protected Texture2D VOIDIconTexture;
        protected string VOIDIconOnActivePath;
        protected string VOIDIconOnInactivePath;
        protected string VOIDIconOffActivePath;
        protected string VOIDIconOffInactivePath;

        protected GUIStyle iconStyle;

        protected int windowBaseID = -96518722;
        protected int windowID = 0;

        protected bool GUIStylesLoaded = false;

        protected CelestialBody homeBody;

        [AVOID_SaveValue("togglePower")]
        public VOID_SaveValue<bool> togglePower = (VOID_SaveValue<bool>)true;

        public override bool powerAvailable { get; protected set; }

        [AVOID_SaveValue("consumeResource")]
        protected VOID_SaveValue<bool> consumeResource = (VOID_SaveValue<bool>)false;

        [AVOID_SaveValue("resourceName")]
        protected VOID_SaveValue<string> resourceName = (VOID_SaveValue<string>)"ElectricCharge";

        [AVOID_SaveValue("resourceRate")]
        protected VOID_SaveValue<float> resourceRate = (VOID_SaveValue<float>)0.2f;

        [AVOID_SaveValue("updatePeriod")]
        protected VOID_SaveValue<double> updatePeriod = (VOID_SaveValue<double>)0.1f;// (1001f / 15000f);
        protected string stringFrequency;

        [AVOID_SaveValue("vesselSimActive")]
        protected VOID_SaveValue<bool> vesselSimActive;

        [AVOID_SaveValue("timeScaleFlags")]
        protected VOID_SaveValue<UInt32> timeScaleFlags;

        // Load-up housekeeping
        protected bool vesselTypesLoaded = false;
        protected bool simManagerLoaded = false;

        protected string defaultSkin = "KSPSkin";

        [AVOID_SaveValue("defaultSkin")]
        protected VOID_SaveValue<string> skinName;
        protected int skinIdx;

        protected Dictionary<string, GUISkin> validSkins;
        protected List<string> skinNames;
        protected string[] forbiddenSkins =
            {
                "PlaqueDialogSkin",
                "FlagBrowserSkin",
                "SSUITextAreaDefault",
                "ExperimentsDialogSkin",
                "ExpRecoveryDialogSkin",
                "KSP window 1",
                "KSP window 3",
                "KSP window 5",
                "KSP window 6",
                "PartTooltipSkin",
                "KSCContextMenuSkin"
            };
        protected bool skinsLoaded = false;

        public override bool configDirty { get; set; }

        //protected IButton ToolbarButton;
        //protected ApplicationLauncherButton AppLauncherButton;
        ToolbarControl toolbarControl;
        protected IconState iconState;

        /*
		 * Properties
		 * */
        public override bool Active
        {
            get
            {
                return base.Active;
            }
            set
            {
                if (value != base.Active)
                {
                    this.SetIconTexture(this.powerState | this.activeState);
                }

                base.Active = value;
            }
        }

        public override VesselType[] AllVesselTypes
        {
            get;
            protected set;
        }

        public override int ConfigVersion
        {
            get
            {
                return this.configVersion;
            }
        }

        public bool FactoryReset
        {
            get;
            protected set;
        }

        public override CelestialBody HomeBody
        {
            get
            {
                if (this.homeBody == null)
                {
                    if (Planetarium.fetch != null)
                    {
                        this.homeBody = Planetarium.fetch.Home;
                    }
                }

                return this.homeBody;
            }
        }

        public override IList<IVOID_Module> Modules
        {
            get
            {
                return this.modules.AsReadOnly();
            }
        }

        public override List<CelestialBody> SortedBodyList
        {
            get;
            protected set;
        }

        public override double UpdatePeriod
        {
            get
            {
                return this.updatePeriod;
            }
        }

        public override float UpdateTimer
        {
            get;
            protected set;
        }

        public override int WindowID
        {
            get
            {
                if (this.windowID == 0)
                {
                    this.windowID = this.windowBaseID;
                }
                return this.windowID++;
            }
        }

        public override Stage[] Stages
        {
            get;
            protected set;
        }

        public override Stage LastStage
        {
            get;
            protected set;
        }

        public override VOID_TimeScale TimeScale
        {
            get
            {
                return (VOID_TimeScale)this.timeScaleFlags.value;
            }
            protected set
            {
                this.timeScaleFlags.value = (UInt32)value;
            }
        }

        protected IconState activeState
        {
            get
            {
                if (this.Active)
                {
                    return IconState.Inactive;
                }
                else
                {
                    return IconState.Active;
                }

            }
        }

        protected IconState powerState
        {
            get
            {
                if (this.togglePower && this.powerAvailable)
                {
                    return IconState.PowerOn;
                }
                else
                {
                    return IconState.PowerOff;
                }

            }
        }

        protected virtual ApplicationLauncher.AppScenes appIconVisibleScenes
        {
            get
            {
                return HighLogic.LoadedScene.ToAppScenes();
            }
        }

        /*
		 * Events
		 * */
        // public 
        public override event VOIDEventHandler onApplicationQuit;
        public override event VOIDEventHandler onSkinChanged;
        public override event VOIDEventHandler onUpdate;

        public override event VOIDEventHandler onPreForEach;
        public override event VOIDForEachPartHandler onForEachPart;
        public override event VOIDForEachPartModuleHandler onForEachModule;
        public override event VOIDEventHandler onPostForEach;

        /*
		 * Methods
		 * */
        public override void DrawGUI(object sender)
        {
            this.windowID = this.windowBaseID;

            if (!this.modulesLoaded)
            {
                this.LoadModulesOfType<IVOID_Module>();

                FireOnModulesLoaded(this);
            }

            if (!this.skinsLoaded)
            {
                this.LoadSkins();
            }
            else
            {
                GUI.skin = this.Skin;
            }

            if (!this.GUIStylesLoaded)
            {
                this.LoadGUIStyles();
            }

            if (
                this.iconState != (this.powerState | this.activeState) ||
                (this.VOIDIconTexture == null/*  && this.AppLauncherButton != null */)
            )
            {
                this.iconState = this.powerState | this.activeState;

                this.SetIconTexture(this.iconState);
            }

            if (this.Active)
            {
                base.DrawGUI(sender);
            }
        }

        public virtual void Update()
        {
            this.LoadBeforeUpdate();

            if (
                this.vesselSimActive &&
                (
                    this.Vessel != null ||
                    (
                        HighLogic.LoadedSceneIsEditor &&
                        EditorLogic.RootPart != null &&
                        EditorLogic.SortedShipList.Count > 0
                    )
                )
            )
            {
                Logging.PostDebugMessage(this, "Updating SimManager.");
                this.UpdateSimManager();

                VOIDForEachPartArgs partArgs;
                VOIDForEachPartModuleArgs moduleArgs;

                Part part;
                PartModule partModule;

                bool doForEachPart = this.onForEachPart != null;
                bool doForEachModule = this.onForEachModule != null;

                if (
                    (doForEachPart || doForEachModule) &&
                    (this.Vessel != null) &&
                    (this.Vessel.parts != null) &&
                    this.timeToUpdate
                )
                {
                    if (this.onPreForEach != null)
                    {
                        this.onPreForEach(this);
                    }

                    for (int pIdx = 0; pIdx < this.Vessel.parts.Count; pIdx++)
                    {
                        part = this.Vessel.parts[pIdx];
                        partArgs = new VOIDForEachPartArgs(part);

                        if (doForEachPart)
                        {
                            this.onForEachPart(this, partArgs);
                        }

                        if (doForEachModule && part.Modules != null)
                        {
                            for (int mIdx = 0; mIdx < part.Modules.Count; mIdx++)
                            {
                                partModule = part.Modules[mIdx];
                                moduleArgs = new VOIDForEachPartModuleArgs(partModule);

                                if (doForEachModule)
                                {
                                    this.onForEachModule(this, moduleArgs);
                                }
                            }
                        }
                    }

                    if (this.onPostForEach != null)
                    {
                        this.onPostForEach(this);
                    }
                }
            }

            if (!this.GUIRunning && !this.gameUIHidden)
            {
                this.StartGUI();
            }

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                if (
                    !module.GUIRunning &&
                    module.Active &&
                    module.InValidScene &&
                    (
                        !HighLogic.LoadedSceneIsEditor ||
                        (EditorLogic.RootPart != null && EditorLogic.SortedShipList.Count > 0)
                    )
                )
                {
                    module.StartGUI();
                }
                if (
                    module.GUIRunning &&
                    (
                        !module.Active ||
                        !this.togglePower ||
                        !module.InValidScene ||
                        this.FactoryReset ||
                        (
                            HighLogic.LoadedSceneIsEditor &&
                            (EditorLogic.RootPart == null || EditorLogic.SortedShipList.Count == 0)
                        )
                    )
                )
                {
                    module.StopGUI();
                }

                if (module is IVOID_BehaviorModule)
                {
                    ((IVOID_BehaviorModule)module).Update();
                }
            }

            if (toolbarControl == null)
            {
                toolbarControl = ThisGameObject.thisGameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(this.ToggleMainWindow, this.ToggleMainWindow,
                    this.appIconVisibleScenes,
                    MODID,
                    "voidButton",
                    "",
                    "",
                    MODNAME
                );
            }
            this.SetIconTexture(this.powerState | this.activeState);
            if (this.onUpdate != null)
            {
                this.onUpdate(this);
            }

            this.saveTimer += Time.deltaTime;

            if (this.modulesLoaded && this.saveTimer > 2f)
            {
                if (this.configDirty)
                {

                    Logging.PostDebugMessage(string.Format(
                            "{0}: Time to save, checking if configDirty: {1}",
                            this.GetType().Name,
                            this.configDirty
                        ));

                    this.SaveConfig();
                    this.saveTimer = 0;
                }
            }

            this.UpdateTimer += Time.deltaTime;
        }

        public virtual void FixedUpdate()
        {
            bool newPowerState = this.powerAvailable;

            if (this.togglePower && this.consumeResource &&
                this.Vessel.vesselType != VesselType.EVA &&
                TimeWarp.deltaTime != 0)
            {
                double powerReceived = this.Vessel.rootPart.RequestResource(
                    this.resourceName,
                    (double)this.resourceRate * TimeWarp.fixedDeltaTime
                );

                if (powerReceived > 0)
                {
                    newPowerState = true;
                }
                else
                {
                    newPowerState = false;
                }

                if (this.powerAvailable != newPowerState)
                {
                    this.powerAvailable = newPowerState;
                }
            }

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                if (module is IVOID_BehaviorModule)
                {
                    ((IVOID_BehaviorModule)module).FixedUpdate();
                }
            }
        }

        public void OnDestroy()
        {
            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                if (module is IVOID_BehaviorModule)
                {
                    ((IVOID_BehaviorModule)module).OnDestroy();
                }
            }

            FireOnModulesDestroyed(this);

            this.Dispose();
        }

        public virtual void OnApplicationQuit()
        {
            if (this.onApplicationQuit != null)
            {
                this.onApplicationQuit(this);
            }

            this.OnDestroy();
        }

        public override void StartGUI()
        {
            if (!this.GUIRunning)
            {
                Logging.PostDebugMessage(this, "Adding DrawGUI to onGui");
                this.onPostRender += this.DrawGUI;
            }
        }

        public void ResetGUI()
        {
            this.StopGUI();

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                module.StopGUI();
                module.StartGUI();
            }

            this.StartGUI();
        }

        public override void ModuleWindow(int id)
        {
            GUILayout.BeginVertical();

            if (this.powerAvailable || !HighLogic.LoadedSceneIsFlight)
            {
                if (!HighLogic.LoadedSceneIsEditor)
                {
                    string str = string.Intern("ON");
                    if (togglePower)
                        str = string.Intern("OFF");
                    if (GUILayout.Button("Power " + str))
                    {
                        togglePower.value = !togglePower;
                    }
                }

                if (togglePower || !HighLogic.LoadedSceneIsFlight)
                {
                    IVOID_Module module;
                    for (int idx = 0; idx < this.modules.Count; idx++)
                    {
                        module = this.modules[idx];

                        if (module is VOID_ConfigWindow)
                        {
                            continue;
                        }

                        module.Active = Layout.Toggle(module.Active, module.Name);
                    }
                }
            }
            else
            {
                GUILayout.Label("-- POWER LOST --", VOID_Styles.labelRed);
            }

            VOID_ConfigWindow.Instance.Active = Layout.Toggle(
                VOID_ConfigWindow.Instance.Active,
                "Configuration"
            );

            GUILayout.EndVertical();

            base.ModuleWindow(id);
        }

        public override void DrawConfigurables()
        {
            GUIContent _content;
            

            this.vesselSimActive.value = Layout.Toggle(this.vesselSimActive.value,
                "Enable Engineering Calculations");

            bool useEarthTime = (this.TimeScale & VOID_TimeScale.KERBIN_TIME) == 0u;
            bool useSiderealTime = (this.TimeScale & VOID_TimeScale.SOLAR_DAY) == 0u;
            bool useRoundedScale = (this.TimeScale & VOID_TimeScale.ROUNDED_SCALE) != 0u;

            useEarthTime = Layout.Toggle(useEarthTime, "Use Earth Time (changes KSP option)");

            GameSettings.KERBIN_TIME = !useEarthTime;

            useSiderealTime = Layout.Toggle(
                useSiderealTime,
                string.Format(
                    "Time Scale: {0}",
                    useSiderealTime ? "Sidereal" : "Solar"
                )
            );

            useRoundedScale = Layout.Toggle(
                useRoundedScale,
                string.Format(
                    "Time Scale: {0}",
                    useRoundedScale ? "Rounded" : "True"
                )
            );

            if (useEarthTime)
            {
                this.TimeScale &= ~VOID_TimeScale.KERBIN_TIME;
            }
            else
            {
                this.TimeScale |= VOID_TimeScale.KERBIN_TIME;
            }

            if (useSiderealTime)
            {
                this.TimeScale &= ~VOID_TimeScale.SOLAR_DAY;
            }
            else
            {
                this.TimeScale |= VOID_TimeScale.SOLAR_DAY;
            }

            if (useRoundedScale)
            {
                this.TimeScale |= VOID_TimeScale.ROUNDED_SCALE;
            }
            else
            {
                this.TimeScale &= ~VOID_TimeScale.ROUNDED_SCALE;
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            int oldSkinIdx = this.skinIdx;
            GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

            _content = new GUIContent();

            _content.text = "◄";
            _content.tooltip = "Select previous skin";
            if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
            {
                this.skinIdx--;
                Logging.PostDebugMessage(string.Format(
                    "{0}: new this.skinIdx = {1} :: skin_list.Count = {2}",
                    this.GetType().Name,
                    this.skinName,
                    this.validSkins.Count
                ));
            }

            _content.text = this.Skin.name;
            _content.tooltip = "Current skin";
            GUILayout.Label(_content, VOID_Styles.labelCenter, GUILayout.ExpandWidth(true));

            _content.text = "►";
            _content.tooltip = "Select next skin";
            if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
            {
                this.skinIdx++;
                Logging.PostDebugMessage(string.Format(
                    "{0}: new this.skinIdx = {1} :: skin_list.Count = {2}",
                    this.GetType().Name,
                    this.skinName,
                    this.validSkins.Count
                ));
            }

            this.skinIdx %= this.skinNames.Count;
            if (this.skinIdx < 0)
            {
                this.skinIdx += this.skinNames.Count;
            }

            if (this.skinIdx != oldSkinIdx)
            {
                this.Skin = this.validSkins[this.skinNames[this.skinIdx]];
            }

            if (this.skinName != skinNames[this.skinIdx])
            {
                this.skinName.value = skinNames[this.skinIdx];
                this.GUIStylesLoaded = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Update Rate (Hz):");
            if (this.stringFrequency == null)
            {
                this.stringFrequency = (1f / this.UpdatePeriod).ToString();
            }
            this.stringFrequency = GUILayout.TextField(this.stringFrequency.ToString(), 5, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Apply"))
            {
                double updateFreq = 1f / this.UpdatePeriod;
                double.TryParse(stringFrequency, out updateFreq);
                this.updatePeriod.value = 1 / updateFreq;
            }
            GUILayout.EndHorizontal();

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                module.DrawConfigurables();
            }

            this.FactoryReset = Layout.Toggle(this.FactoryReset, "Factory Reset");
        }

        protected void UpdateSimManager()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                double radius = this.Vessel.Radius();
                SimManager.Gravity = this.Vessel.mainBody.gravParameter / (radius * radius);
                SimManager.Atmosphere = this.Vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres;
                SimManager.Mach = this.Vessel.mach;
                BuildAdvanced.Altitude = this.Vessel.altitude;
                CelestialBodies.SelectedBody = this.Vessel.mainBody;
            }

#if DEBUG
			SimManager.logOutput = true;
#endif

            SimManager.TryStartSimulation();

            Logging.PostDebugMessage(this, "Started Engineer simulation with Atmosphere={0} atm and Gravity={1} m/s²",
                SimManager.Atmosphere,
                SimManager.Gravity
            );
        }

        protected void GetSimManagerResults()
        {
            Logging.PostDebugMessage(this, "VesselSimulator results ready, setting Stages.");

            this.Stages = SimManager.Stages;

            if (this.Stages != null && this.Stages.Length > 0)
            {
                this.LastStage = this.Stages[this.Stages.Length - 1];
            }
        }


        protected void LoadModulesOfType<U>()
        {

            using (PooledDebugLogger sb = PooledDebugLogger.New(this))
            {
                sb.AppendLine("Loading modules...");

                AssemblyLoader.LoadedAssembly assy;
                for (int aIdx = 0; aIdx < AssemblyLoader.loadedAssemblies.Count; aIdx++)
                {

                    assy = AssemblyLoader.loadedAssemblies[aIdx];

                    Type[] loadedTypes;
                    try
                    {
                        loadedTypes = assy.assembly.GetExportedTypes();


                        Type loadedType;

                        for (int tIdx = 0; tIdx < loadedTypes.Length; tIdx++)
                        {
                            loadedType = loadedTypes[tIdx];

                            if (
                                loadedType.IsInterface ||
                                loadedType.IsAbstract ||
                                !typeof(U).IsAssignableFrom(loadedType) ||
                                typeof(VOIDCore).IsAssignableFrom(loadedType))
                            {
                                continue;
                            }

                            sb.AppendFormat("Checking IVOID_Module type {0}...", loadedType.Name);

                            try
                            {
                                this.LoadModule(loadedType);
                                sb.AppendLine("Success.");
                            }
                            catch (Exception ex)
                            {
                                sb.AppendFormat("Failed, caught {0}\n", ex.GetType().Name);

#if DEBUG
						Debug.LogException(ex);
#endif
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }

                this.LoadConfig();

                this.modulesLoaded = true;

                sb.AppendFormat("Loaded {0} modules.\n", this.Modules.Count);

                sb.Print();
            }
        }

        protected void LoadModule(Type T)
        {
            /*var existingModules = this.modules.Where(mod => mod.GetType().Name == T.Name);
			if (existingModules.Any())*/
            for (int mIdx = 0; mIdx < this.modules.Count; mIdx++)
            {
                if (this.modules[mIdx].Name == T.Name)
                {
                    Logging.PostErrorMessage("{0}: refusing to load {1}: already loaded", this.GetType().Name, T.Name);
                    return;
                }
            }

            var InstanceProperty = T.GetProperty(
                "Instance",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.FlattenHierarchy
            );

            object modInstance = null;
            IVOID_Module module;

            if (InstanceProperty != null)
            {
                modInstance = InstanceProperty.GetValue(null, null);
            }

            if (modInstance != null)
            {
                module = modInstance as IVOID_Module;
            }
            else
            {
                module = Activator.CreateInstance(T) as IVOID_Module;
            }

            if (module.InValidGame && module.InValidScene)
            {
                this.modules.Add(module);

                Logging.PostDebugMessage(string.Format(
                        "{0}: loaded module {1}.",
                        this.GetType().Name,
                        T.Name
                    ));
            }
            else
            {
                if (module is IDisposable)
                {
                    (module as IDisposable).Dispose();
                }
            }
        }

       

        protected void LoadSkins()
        {
            this.validSkins = new Dictionary<string, GUISkin>();
            this.skinNames = new List<string>();

            UnityEngine.Object[] skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin));
            GUISkin skin;
            for (int sIdx = 0; sIdx < skins.Length; sIdx++)
            {
                skin = (GUISkin)skins[sIdx];

                if (!this.forbiddenSkins.Contains(skin.name))
                {
                    if (!this.skinNames.Contains(skin.name))
                    {
                        Logging.PostLogMessage("[{0}]: Found skin: {1}", this.GetType().Name, skin.name);
                        Debug.Log("["+ this.GetType().Name+"]: Found skin: "+ skin.name);
                        this.validSkins[skin.name] = skin;
                        this.skinNames.Add(skin.name);
                    }
                }
            }

            Logging.PostLogMessage(string.Format(
                "{0}: loaded {1} GUISkins.",
                this.GetType().Name,
                this.validSkins.Count
            ));

            this.skinNames.Sort();

            int defaultIdx = int.MinValue;

            for (int i = 0; i < this.skinNames.Count; i++)
            {
                if (this.skinNames[i] == this.skinName)
                {
                    this.skinIdx = i;
                }
                if (this.skinNames[i] == this.defaultSkin)
                {
                    defaultIdx = i;
                }
                if (this.skinIdx != int.MinValue && defaultIdx != int.MinValue)
                {
                    break;
                }
            }

            if (this.skinIdx == int.MinValue)
            {
                this.skinIdx = defaultIdx;
            }

            this.Skin = this.validSkins[this.skinNames[this.skinIdx]];

            Logging.PostLogMessage(string.Format(
                "{0}: _skinIdx = {1}.",
                this.GetType().Name,
                this.skinName.ToString()
            ));

            this.skinsLoaded = true;
        }

        protected void LoadGUIStyles()
        {
            VOID_Styles.OnSkinChanged();

            if (this.onSkinChanged != null)
            {
                this.onSkinChanged(this);
            }

            this.GUIStylesLoaded = true;
        }

        protected void LoadBeforeUpdate()
        {
            if (!this.vesselTypesLoaded)
            {
                Array typeObjs = Enum.GetValues(typeof(VesselType));
                this.AllVesselTypes = new VesselType[typeObjs.Length];

                for (int idx = 0; idx < typeObjs.Length; idx++)
                {
                    this.AllVesselTypes[idx] = (VesselType)typeObjs.GetValue(idx);
                }

                this.vesselTypesLoaded = true;
            }

            if (this.SortedBodyList == null && FlightGlobals.Bodies != null && FlightGlobals.Bodies.Count > 0)
            {
                this.SortedBodyList = new List<CelestialBody>(FlightGlobals.Bodies);
                this.SortedBodyList.Sort(new CBListComparer());
                this.SortedBodyList.Reverse();
            }

            // SimManager initialization that we don't necessarily want to repeat every Update.
            if (!this.simManagerLoaded && this.HomeBody != null)
            {
                SimManager.Gravity = VOID_Data.KerbinGee;
                SimManager.Atmosphere = 0d;
                SimManager.Mach = 1d;
                CelestialBodies.SelectedBody = this.HomeBody;
                BuildAdvanced.Altitude = 0d;
                SimManager.OnReady += this.GetSimManagerResults;

                this.simManagerLoaded = true;
            }
        }

        protected void ToggleMainWindow()
        {
            this.Active = !this.Active;
        }

        protected void SetIconTexture(IconState state)
        {
            switch (state)
            {
                case (IconState.PowerOff | IconState.Inactive):
                    this.SetIconTexture(this.VOIDIconOffInactivePath);
                    break;
                case (IconState.PowerOff | IconState.Active):
                    this.SetIconTexture(this.VOIDIconOffActivePath);
                    break;
                case (IconState.PowerOn | IconState.Inactive):
                    this.SetIconTexture(this.VOIDIconOnInactivePath);
                    break;
                case (IconState.PowerOn | IconState.Active):
                    this.SetIconTexture(this.VOIDIconOnActivePath);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected void SetIconTexture(string texturePath)
        {
#if false
            if (texturePath == null)
            {
                return;
            }

            if (this.ToolbarButton != null)
            {
                this.ToolbarButton.TexturePath = texturePath;
            }

            if (this.AppLauncherButton != null)
            {
                this.VOIDIconTexture = GameDatabase.Instance.GetTexture(texturePath.Replace("icon", "appIcon"), false);

                this.AppLauncherButton.SetTexture(VOIDIconTexture);
            }
#endif
            if (toolbarControl != null)
                toolbarControl.SetTexture(texturePath.Replace("icon", "appIcon"), texturePath);
        }

        public void LoadConfig()
        {

            if (!System.IO.File.Exists(this.VOIDSettingsPath) && KSP.IO.File.Exists<VOID_Module>("config.xml"))
            {
                Logging.PostLogMessage(
                    "VOID: No per-save config file but old file detected; copying from old file."
                );

                System.IO.File.Copy(
                    KSP.IO.IOUtils.GetFilePathFor(typeof(VOID_Module), "config.xml"),
                    this.VOIDSettingsPath
                );
            }

            this.LoadConfig(new PluginConfiguration(this.VOIDSettingsPath));
        }

        public override void LoadConfig(KSP.IO.PluginConfiguration config)
        {
            base.LoadConfig(config);

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                module.LoadConfig(config);
            }

            this.TimeScale |= GameSettings.KERBIN_TIME ? VOID_TimeScale.KERBIN_TIME : 0u;
        }

        public override void SaveConfig()
        {
            if (this.configNeedsUpdate && this is VOIDCore_Flight)
            {
                KSP.IO.File.Delete<T>("config.xml");
                System.IO.File.Delete(this.VOIDSettingsPath);
            }

            KSP.IO.PluginConfiguration config = new PluginConfiguration(this.VOIDSettingsPath);

            config.load();

            this.Save(config, this.SceneKey);

            IVOID_Module module;
            for (int idx = 0; idx < this.modules.Count; idx++)
            {
                module = this.modules[idx];

                module.Save(config, this.SceneKey);
            }

            config.save();

            this.configDirty = false;
        }

        public VOIDCore_Generic()
        {
            System.Version version = this.GetType().Assembly.GetName().Version;

            this.VoidVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);

            this.Name = string.Format("VOID {0}", this.VoidVersion.ToString());

            this.powerAvailable = true;

            this.Active = true;

            this.skinName = (VOID_SaveValue<string>)this.defaultSkin;
            this.skinIdx = int.MinValue;

            this.VOIDIconOnActivePath = "VOID/Textures/void_icon_light_glow";
            this.VOIDIconOnInactivePath = "VOID/Textures/void_icon_dark_glow";
            this.VOIDIconOffActivePath = "VOID/Textures/void_icon_light";
            this.VOIDIconOffInactivePath = "VOID/Textures/void_icon_dark";

            this.saveTimer = 0f;
            this.UpdateTimer = 0f;

            this.vesselSimActive = (VOID_SaveValue<bool>)true;
            

            this.SaveGamePath = string.Format("{0}saves/{1}", IOTools.KSPRootPath, HighLogic.SaveFolder);
            this.VOIDSettingsPath = string.Format("{0}/VOIDConfig.xml", this.SaveGamePath);

            this.FactoryReset = false;

            GameEvents.onHideUI.Add(() =>
                {
                    this.gameUIHidden = true;
                    this.StopGUI();
                }
            );

            GameEvents.onShowUI.Add(() =>
                {
                    this.gameUIHidden = false;
                    this.StartGUI();
                }
            );
        }

        public override void Dispose()
        {
            this.StopGUI();

            if (this.onSkinChanged != null)
                this.onSkinChanged(this);
#if false
            if (this.AppLauncherButton != null)
            {

                ApplicationLauncher.Instance.RemoveModApplication(this.AppLauncherButton);
                this.AppLauncherButton = null;
            }
#endif
            if (this.toolbarControl != null)
            {

                toolbarControl.OnDestroy();
                ThisGameObject.Destroy(toolbarControl);
                this.toolbarControl = null;
            }

            _instance = null;
            _initialized = false;
        }
    }
}
