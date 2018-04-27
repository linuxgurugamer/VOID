// VOID
//
// VOID_Module.cs
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

// TODO: Remove ToadicusTools. prefixes after refactor is done.

using System;
using System.Collections.Generic;
using System.Reflection;
using ToadicusTools.Extensions;
using ToadicusTools.GUIUtils;
using UnityEngine;

namespace VOID
{
	public abstract class VOID_Module : IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("Active")]
		protected VOID_SaveValue<bool> active = (VOID_SaveValue<bool>)false;

		protected float lastUpdate = 0;

		private GameScenes[] validScenes;
		private Game.Modes[] validModes;

		/*
		 * Properties
		 * */

		public virtual bool Active
		{
			get
			{
				return this.active && this.InValidGame && this.InValidScene;
			}
			set
			{
				this.active.value = value && this.InValidGame && this.InValidScene;
			}
		}

		public virtual bool GUIRunning
		{
			get
			{
				using (var log = ToadicusTools.DebugTools.PooledDebugLogger.New(this))
				{
					log.AppendFormat("this.core: {0}\n", this.core != null ? this.core.ToString() : "null");
					if (this.core != null)
					{
						log.AppendFormat("this.core.MethodInPostRenderQueue(this.DrawGUI): {0}\n",
							this.core.MethodInPostRenderQueue(this.DrawGUI));
					}

					log.AppendFormat("this.GUIRunning: {0}\n",
						this.core != null && this.core.MethodInPostRenderQueue(this.DrawGUI));

					log.Print(false);
				}

				return this.core != null && this.core.MethodInPostRenderQueue(this.DrawGUI);
			}
		}

		public virtual bool InValidGame
		{
			get
			{
				return this.ValidModes.Contains(HighLogic.CurrentGame.Mode);
			}
		}

		public virtual bool InValidScene
		{
			get
			{
				return this.ValidScenes.Contains(HighLogic.LoadedScene);
			}
		}

		public virtual string Name
		{
			get;
			protected set;
		}

		public virtual Game.Modes[] ValidModes
		{
			get
			{
				if (this.validModes == null)
				{
					ToadicusTools.Logging.PostDebugMessage(this, "validModes is null when checking inValidGame; fetching attribute.");

					object[] attributes = this.GetType().GetCustomAttributes(false);
					object attr;
					for (int idx = 0; idx < attributes.Length; idx++)
					{
						attr = attributes[idx];

						if (attr is VOID_GameModesAttribute)
						{
							VOID_GameModesAttribute addonAttr = (VOID_GameModesAttribute)attr;

							this.validModes = addonAttr.ValidModes;

							ToadicusTools.Logging.PostDebugMessage("Found VOID_GameModesAttribute; validScenes set.");

							break;
						}
					}

					if (this.validModes == null)
					{
						this.validModes = new Game.Modes[]
						{
							Game.Modes.CAREER,
							Game.Modes.SANDBOX,
							Game.Modes.SCENARIO,
							Game.Modes.SCENARIO_NON_RESUMABLE,
							Game.Modes.SCIENCE_SANDBOX
						};

						ToadicusTools.Logging.PostDebugMessage("No VOID_GameModesAttribute found; validScenes defaulted to flight.");
					}
				}

				return this.validModes;
			}
		}

		public virtual GameScenes[] ValidScenes
		{
			get
			{
				if (this.validScenes == null)
				{
					ToadicusTools.Logging.PostDebugMessage(this, "validScenes is null when checking inValidScene; fetching attribute.");
					object[] attributes = this.GetType().GetCustomAttributes(false);
					object attr;
					for (int idx = 0; idx < attributes.Length; idx++)
					{
						attr = attributes[idx];

						if (attr is VOID_ScenesAttribute)
						{
							VOID_ScenesAttribute addonAttr = (VOID_ScenesAttribute)attr;

							this.validScenes = addonAttr.ValidScenes;

							ToadicusTools.Logging.PostDebugMessage("Found VOID_ScenesAttribute; validScenes set.");

							break;
						}
					}

					if (this.validScenes == null)
					{
						this.validScenes = new GameScenes[] { GameScenes.FLIGHT };
						ToadicusTools.Logging.PostDebugMessage("No VOID_ScenesAttribute found; validScenes defaulted to flight.");
					}
				}

				return this.validScenes;
			}
		}

		public virtual Vessel Vessel
		{
			get
			{
				return FlightGlobals.ActiveVessel;
			}
		}

		protected virtual VOIDCore core
		{
			get
			{
				return VOID_Data.Core;
			}
		}

		protected virtual bool timeToUpdate
		{
			get
			{
				return (
					(this.core.UpdateTimer - this.lastUpdate) > this.core.UpdatePeriod ||
					this.lastUpdate > this.core.UpdateTimer
				);
			}
		}

		/*
		 * Methods
		 * */
		public virtual void StartGUI()
		{
			if (!this.Active || this.GUIRunning)
			{
				return;
			}

			ToadicusTools.Logging.PostDebugMessage (string.Format("Adding {0} to the draw queue.", this.GetType().Name));
			this.core.onPostRender += this.DrawGUI;
		}

		public virtual void StopGUI()
		{
			if (!this.GUIRunning)
			{
				return;
			}
			ToadicusTools.Logging.PostDebugMessage (string.Format("Removing {0} from the draw queue.", this.GetType().Name));
			this.core.onPostRender -= this.DrawGUI;
		}

		public abstract void DrawGUI(object sender);

		public virtual void DrawConfigurables() {}

		public virtual void LoadConfig(KSP.IO.PluginConfiguration config)
		{
			config.load ();

			if (this is VOIDCore)
			{
				int configVersion = config.GetValue("VOID_Core_configValue", 2);

				if (configVersion < VOIDCore.CONFIG_VERSION)
				{
					((VOIDCore)this).configNeedsUpdate = true;
				}
			}

			MemberInfo[] members = this.GetType().GetMembers(
				                         BindingFlags.NonPublic |
				                         BindingFlags.Public |
				                         BindingFlags.Instance |
				                         BindingFlags.FlattenHierarchy
			                         );
			MemberInfo member;

			for (int fIdx = 0; fIdx < members.Length; fIdx++)
			{
				member = members[fIdx];

				if (!(member is FieldInfo || member is PropertyInfo))
				{
					continue;
				}

				if (member is PropertyInfo && (member as PropertyInfo).GetIndexParameters().Length > 0)
				{
					continue;
				}

				object[] attrs = member.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				AVOID_SaveValue attr;

				if (attrs.Length > 0)
				{
					attr = (AVOID_SaveValue)attrs[0];
				}
				else
				{
					continue;
				}

				string fieldName = string.Empty;

				if (this is VOIDCore || this.core.configNeedsUpdate)
				{
					string typeName = this.GetType().Name;;

					if (this is VOIDCore && ((VOIDCore)this).configNeedsUpdate)
					{
						if (this is VOIDCore_Flight)
						{
							typeName = "VOID_Core";
						}
						else if (this is VOIDCore_Editor)
						{
							typeName = "VOID_EditorCore";
						}
					}

					fieldName = string.Format("{0}_{1}", typeName, attr.Name);
				}
				else
				{
					fieldName = string.Format(
						"{0}_{1}_{2}",
						this.GetType().Name,
						Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene),
						attr.Name
					);
				}

				ToadicusTools.Logging.PostDebugMessage(string.Format("{0}: Loading field {1}.", this.GetType().Name, fieldName));

				object fieldValue;

				if (member is FieldInfo)
				{
					fieldValue = (member as FieldInfo).GetValue(this);
				}
				else
				{
					fieldValue = (member as PropertyInfo).GetValue(this, null);
				}

				bool convertBack = false;
				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).value;
					convertBack = true;
				}

				fieldValue = config.GetValue(fieldName, fieldValue);

				if (convertBack)
				{
					Type type = typeof(VOID_SaveValue<>).MakeGenericType (fieldValue.GetType ());
					IVOID_SaveValue convertValue = Activator.CreateInstance (type) as IVOID_SaveValue;
					convertValue.SetValue (fieldValue);
					fieldValue = convertValue;
				}

				if (member is FieldInfo)
				{
					(member as FieldInfo).SetValue(this, fieldValue);
				}
				else
				{
					(member as PropertyInfo).SetValue(this, fieldValue, null);
				}

				ToadicusTools.Logging.PostDebugMessage(string.Format("{0}: Loaded field {1}.", this.GetType().Name, fieldName));
			}
		}

		public virtual void Save(KSP.IO.PluginConfiguration config, string sceneKey)
		{
			if (config == null)
			{
				ToadicusTools.Logging.PostErrorMessage(
					"{0}: config argument was null, bailing out.",
					this.GetType().Name
				);
			}

			if (sceneKey == null)
			{
				ToadicusTools.Logging.PostErrorMessage(
					"{0}: sceneKey argument was null, bailing out.",
					this.GetType().Name
				);
			}

			MemberInfo[] members = this.GetType().GetMembers(
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy
			);
			MemberInfo member;

			for (int fIdx = 0; fIdx < members.Length; fIdx++)
			{
				member = members[fIdx];

				object[] attrs = member.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				AVOID_SaveValue attr;

				if (attrs.Length > 0)
				{
					attr = (AVOID_SaveValue)attrs[0];
				}
				else
				{
					continue;
				}

				string fieldName;

				if (this is VOIDCore)
				{
					fieldName = string.Format("{0}_{1}", this.GetType().Name, attr.Name);
				}
				else
				{
					fieldName = string.Format(
						"{0}_{1}_{2}",
						this.GetType().Name,
						sceneKey,
						attr.Name
					);
				}

				object fieldValue;

				if (member is FieldInfo)
				{
					fieldValue = (member as FieldInfo).GetValue(this);
				}
				else
				{
					fieldValue = (member as PropertyInfo).GetValue(this, null);
				}

				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).value;
				}

				config.SetValue(fieldName, fieldValue);

				ToadicusTools.Logging.PostDebugMessage(string.Format("{0}: Saved field {1}.", this.GetType().Name, fieldName));
			}
		}
	}

	public abstract class VOID_WindowModule : VOID_Module
	{
		[AVOID_SaveValue("WindowPos")]
		protected Rect WindowPos;
		protected float defWidth;
		protected float defHeight;

		protected bool decorateWindow;

		protected string inputLockName;

		public VOID_WindowModule() : base()
		{
			this.defWidth = 250f;
			this.defHeight = 50f;

			this.decorateWindow = true;

			this.inputLockName = string.Concat(this.Name, "_edlock");

			this.WindowPos = new Rect(Screen.width / 2, Screen.height / 2, this.defWidth, this.defHeight);
		}

		public virtual void ModuleWindow(int id)
		{
			GUIStyle buttonStyle = this.core.Skin.button;
			RectOffset padding = buttonStyle.padding;
			RectOffset border = buttonStyle.border;

			Rect closeRect = new Rect(
				0f,
				0f,
				border.left + border.right,
				border.top + border.bottom
			);

			closeRect.width = Mathf.Max(closeRect.width, 16f);
			closeRect.height = Mathf.Max(closeRect.height, 16f);

			closeRect.x = this.WindowPos.width - closeRect.width - 2f;
			closeRect.y = 2f;

			GUI.Button(closeRect, GUIContent.none, buttonStyle);

			if (Event.current.type == EventType.repaint && Input.GetMouseButtonUp(0))
			{
				if (closeRect.Contains(Event.current.mousePosition))
				{
					this.Active = false;
					this.removeUILock();
				}
			}

			GUI.DragWindow();
		}

		public override void DrawGUI(object sender)
		{
			if (this.core == null)
			{
				return;
			}

			GUI.skin = this.core.Skin;

			Rect _Pos = this.WindowPos;

			_Pos = GUILayout.Window(
				this.core.WindowID,
				_Pos,
				VOID_Tools.GetWindowHandler(this.ModuleWindow),
				this.Name,
				GUILayout.Width(this.defWidth),
				GUILayout.Height(this.defHeight)
			);

			bool cursorInWindow = _Pos.Contains(Mouse.screenPos);

			if (cursorInWindow)
			{
				this.setUILock();
			}
			else
			{
				this.removeUILock();
			}

			if (HighLogic.LoadedSceneIsEditor)
			{
				_Pos = WindowTools.ClampRectToEditorPad(_Pos);
			}
			else
			{
				_Pos = WindowTools.ClampRectToScreen(_Pos);
			}

			if (_Pos != this.WindowPos)
			{
				this.WindowPos = _Pos;
				this.core.configDirty = true;
			}
		}

		protected void setUILock()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.EDITOR:
					InputLockManager.SetControlLock(
					ControlTypes.EDITOR_LOCK |
						ControlTypes.EDITOR_ICON_HOVER | ControlTypes.EDITOR_ICON_PICK |
						ControlTypes.EDITOR_PAD_PICK_COPY | ControlTypes.EDITOR_PAD_PICK_COPY,
						this.inputLockName
					);
					EditorLogic.fetch.Lock(false, false, false, this.inputLockName);
					break;
				case GameScenes.FLIGHT:
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, this.inputLockName);
					break;
				case GameScenes.SPACECENTER:
					InputLockManager.SetControlLock(
						ControlTypes.KSC_FACILITIES | ControlTypes.CAMERACONTROLS,
						this.inputLockName
					);
					break;
			}
		}

		protected void removeUILock()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.EDITOR:
					EditorLogic.fetch.Unlock(this.inputLockName);
					break;
				case GameScenes.FLIGHT:
					InputLockManager.RemoveControlLock(this.inputLockName);
					break;
				case GameScenes.SPACECENTER:
					InputLockManager.RemoveControlLock(this.inputLockName);
					break;
			}
		}
	}
}

