// VOID
//
// cs
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

using System;
using UnityEngine;

namespace VOID
{
	public static class VOID_Styles
	{
		public static GUIStyle labelDefault
		{
			get;
			private set;
		}

		public static GUIStyle labelLink
		{
			get;
			private set;
		}

		public static GUIStyle labelCenter
		{
			get;
			private set;
		}

		public static GUIStyle labelCenterBold
		{
			get;
			private set;
		}

		public static GUIStyle labelGreen
		{
			get;
			private set;
		}

		public static GUIStyle labelHud
		{
			get;
			private set;
		}

		public static GUIStyle labelRight
		{
			get;
			private set;
		}

		public static GUIStyle labelRed
		{
			get;
			private set;
		}

		public static void OnSkinChanged()
		{
			labelDefault = new GUIStyle(GUI.skin.label);

			labelLink = new GUIStyle(GUI.skin.label);
			labelLink.fontStyle = FontStyle.Italic;
			labelLink.fontSize = (int)((float)labelLink.fontSize * .8f);

			labelCenter = new GUIStyle(GUI.skin.label);
			labelCenter.normal.textColor = Color.white;
			labelCenter.alignment = TextAnchor.UpperCenter;

			labelCenterBold = new GUIStyle(GUI.skin.label);
			labelCenterBold.normal.textColor = Color.white;
			labelCenterBold.alignment = TextAnchor.UpperCenter;
			labelCenterBold.fontStyle = FontStyle.Bold;

			labelHud = new GUIStyle(labelDefault);

			labelRight = new GUIStyle(GUI.skin.label);
			labelRight.normal.textColor = Color.white;
			labelRight.alignment = TextAnchor.UpperRight;

			labelRed = new GUIStyle(GUI.skin.label);
			labelRed.normal.textColor = Color.red;

			labelGreen = new GUIStyle(GUI.skin.label);
			labelGreen.normal.textColor = Color.green;
		}

		static VOID_Styles()
		{
			OnSkinChanged();
		}
	}
}
