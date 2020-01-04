// VOID
//
// VOID_CareerStatus.cs
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
using System.Text;
using ToadicusTools;
using ToadicusTools.Text;
using UnityEngine;

namespace VOID
{
	[VOID_Scenes(GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPACECENTER)]
	[VOID_GameModes(Game.Modes.CAREER, Game.Modes.SCIENCE_SANDBOX)]
	public class VOID_CareerStatus : VOID_SingletonWindow<VOID_CareerStatus>
	{
		public static string formatDelta(double delta, string numberFormat)
		{
			if (delta > 0)
			{
				return string.Format("<color='lime'>{0}↑</color>", delta.ToString(numberFormat, SIFormatProvider.SIFormatter));
			}
			else if (delta < 0)
			{
				return string.Format("<color='red'>{0}↓</color>", delta.ToString(numberFormat, SIFormatProvider.SIFormatter));
			}
			else
			{
				return "0";
			}
		}

		public static string formatDelta(double delta)
		{
			return formatDelta(delta, "#,##0.##");
		}

		public static string formatDelta(float delta)
		{
			return formatDelta((double)delta);
		}

		private GUIContent fundsContent;
		private GUIContent repContent;
		private GUIContent scienceContent;

		#pragma warning disable 0414
		private Texture2D fundsIconGreen;
		private Texture2D fundsIconRed;
		private Texture2D reputationIconGreen;
		private Texture2D reputationIconRed;
		private Texture2D scienceIcon;
		#pragma warning restore 0414

		public double lastFundsChange
		{
			get;
			private set;
		}

		public float lastRepChange
		{
			get;
			private set;
		}

		public float lastScienceChange
		{
			get;
			private set;
		}

		public double currentFunds
		{
			get;
			private set;
		}

		public float currentReputation
		{
			get;
			private set;
		}

		public float currentScience
		{
			get;
			private set;
		}

		private bool currenciesInitialized
		{
			get
			{
				Logging.PostDebugMessage(
					this,
					"Checking init state:" +
					"\n\tcurrentFunds={0}" +
					"\n\tcurrentScience={1}" +
					"\n\tcurrentReputation={2}",
					this.currentFunds,
					this.currentScience,
					this.currentReputation
				);

				return !(
					double.IsNaN(this.currentFunds) ||
					float.IsNaN(this.currentScience) ||
					float.IsNaN(this.currentReputation)
				);
			}
		}

		public override void DrawGUI(object sender)
		{
			if (Event.current.type != EventType.Layout && !this.currenciesInitialized)
			{
				this.initCurrencies();
			}

			base.DrawGUI(sender);
		}

		public override void ModuleWindow(int id)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.fundingStatus.Label);
			GUILayout.FlexibleSpace();
			this.fundsContent.text = VOID_Data.fundingStatus.Value;
			GUILayout.Label(this.fundsContent, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.reputationStatus.Label);
			GUILayout.FlexibleSpace();
			this.repContent.text = VOID_Data.reputationStatus.Value;
			GUILayout.Label(this.repContent, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.scienceStatus.Label);
			GUILayout.FlexibleSpace();
			this.scienceContent.text = VOID_Data.scienceStatus.Value;
			GUILayout.Label(this.scienceContent, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}

		// TODO: Update event handlers to do something useful with the new "reasons" parameter.
		private void onFundsChange(double newValue, TransactionReasons reasons)
		{
			this.lastFundsChange = newValue - this.currentFunds;
			this.currentFunds = newValue;
		}

		private void onRepChange(float newValue, TransactionReasons reasons)
		{
			this.lastRepChange = newValue - this.currentReputation;
			this.currentReputation = newValue;
		}

		private void onScienceChange(float newValue, TransactionReasons reasons)
		{
			this.lastScienceChange = newValue - this.currentScience;
			this.currentScience = newValue;
		}

		private void onGameStateLoad(ConfigNode node)
		{
			this.initCurrencies();
		}

		private void initCurrencies()
		{
			Logging.PostDebugMessage(
				this,
				"Initializing currencies." +
				"\n\tFunding.Instance={0}" +
				"ResearchAndDevelopment.Instance={1}" +
				"Reputation.Instance={2}",
				Funding.Instance == null ? "NULL" : Funding.Instance.ToString(),
				ResearchAndDevelopment.Instance == null ? "NULL" : ResearchAndDevelopment.Instance.ToString(),
				Reputation.Instance == null ? "NULL" : Reputation.Instance.ToString()
			);

			this.currentFunds = Funding.Instance != null ? Funding.Instance.Funds : double.NaN;
			this.currentReputation = Reputation.Instance != null ? Reputation.Instance.reputation : float.NaN;
			this.currentScience = ResearchAndDevelopment.Instance != null ?
				ResearchAndDevelopment.Instance.Science : float.NaN;
		}

		/*
		 *  MissionRecoveryDialog::fundsIconGreen.name: UiElements_05
         *  MissionRecoveryDialog::fundsIconRed.name: UiElements_06
         *  MissionRecoveryDialog::reputationIconGreen.name: UiElements_07
         *  MissionRecoveryDialog::reputationIconRed.name: UiElements_08
         *  MissionRecoveryDialog::scienceIcon.name: UiElements_12
		 * */
		public VOID_CareerStatus() : base()
		{
			this.Name = "Career Status";

			GameEvents.OnFundsChanged.Add(this.onFundsChange);
			GameEvents.OnReputationChanged.Add(this.onRepChange);
			GameEvents.OnScienceChanged.Add(this.onScienceChange);
			GameEvents.onGameStateLoad.Add(this.onGameStateLoad);

			bool texturesLoaded;

			texturesLoaded = IOTools.LoadTexture(out this.fundsIconGreen, "VOID/Textures/fundsgreen.png", 10, 18);
			texturesLoaded &= IOTools.LoadTexture(out this.fundsIconRed, "VOID/Textures/fundsred.png", 10, 18);
			texturesLoaded &= IOTools.LoadTexture(out this.reputationIconGreen, "VOID/Textures/repgreen.png", 16, 18);
			texturesLoaded &= IOTools.LoadTexture(out this.reputationIconRed, "VOID/Textures/repred.png", 16, 18);
			texturesLoaded &= IOTools.LoadTexture(out this.scienceIcon, "VOID/Textures/science.png", 16, 18);

			this.fundsContent = new GUIContent();
			this.repContent = new GUIContent();
			this.scienceContent = new GUIContent();

			if (texturesLoaded)
			{
				this.fundsContent.image = this.fundsIconGreen;
				this.repContent.image = this.reputationIconGreen;
				this.scienceContent.image = this.scienceIcon;
			}

			this.currentFunds = double.NaN;
			this.currentScience = float.NaN;
			this.currentReputation = float.NaN;
		}

		public override void Dispose()
		{
			GameEvents.OnFundsChanged.Remove(this.onFundsChange);
			GameEvents.OnReputationChanged.Remove(this.onRepChange);
			GameEvents.OnScienceChanged.Remove(this.onScienceChange);
			GameEvents.onGameStateLoad.Remove(this.onGameStateLoad);

			base.Dispose();
		}

		~VOID_CareerStatus()
		{
			this.Dispose();
		}
	}
}

