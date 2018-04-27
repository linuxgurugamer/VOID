// VOID
//
// VOID_DataLogger.cs
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using ToadicusTools;
using ToadicusTools.DebugTools;
using ToadicusTools.GUIUtils;
using ToadicusTools.Text;
using UnityEngine;

namespace VOID
{
	public class VOID_DataLogger : VOID_WindowModule, IVOID_BehaviorModule
	{
		/*
		 * Fields
		 * */
		#region Fields

		protected bool _loggingActive;
		protected bool firstWrite;

		[AVOID_SaveValue("waitForLaunch")]
		protected VOID_SaveValue<bool> waitForLaunch;

		[AVOID_SaveValue("logInterval")]
		protected VOID_SaveValue<float> logInterval;
		protected string logIntervalStr;

		protected float csvCollectTimer;

		protected List<byte> csvBytes;

		protected string _fileName;
		protected FileStream _outputFile;

		protected uint outstandingWrites;

		protected System.Text.UTF8Encoding _utf8Encoding;

		#endregion

		/*
		 * Properties
		 * */

		#region Properties

		// TODO: Add configurable or incremental file names.
		protected bool loggingActive
		{
			get
			{
				return this._loggingActive;
			}
			set
			{
				if (value != this._loggingActive)
				{
					if (value)
					{
						this.csvCollectTimer = 0f;
					}
					else
					{
						this.CloseFileIfOpen();
					}

					this._loggingActive = value;
				}
			}
		}

		protected string fileName
		{
			get
			{
				if (this._fileName == null || this._fileName == string.Empty)
				{
					this._fileName = string.Format(
						"{0}/{1}_{2}",
						this.core.SaveGamePath,
						this.Vessel.vesselName,
						"data.csv"
					);
				}

				return this._fileName;
			}
		}

		protected FileStream outputFile
		{
			get
			{
				if (this._outputFile == null)
				{
					using (PooledDebugLogger logger = PooledDebugLogger.New(this))
					{
						logger.AppendFormat("Initializing output file '{0}' with mode ", this.fileName);

						if (File.Exists(this.fileName))
						{
							logger.Append("append");
							this._outputFile = new FileStream(
								this.fileName,
								FileMode.Append,
								FileAccess.Write,
								FileShare.Read,
								512,
								true
							);
						}
						else
						{
							logger.Append("create");
							this._outputFile = new FileStream(
								this.fileName,
								FileMode.Create,
								FileAccess.Write,
								FileShare.Read,
								512,
								true
							);

							byte[] byteOrderMark = utf8Encoding.GetPreamble();

							logger.Append(" and writing preamble");
							this._outputFile.Write(byteOrderMark, 0, byteOrderMark.Length);
						}

						logger.Append('.');

						logger.AppendFormat("  File is {0}opened asynchronously.", this._outputFile.IsAsync ? "" : "not ");

						logger.Print();
					}
				}

				return this._outputFile;
			}
		}

		public UTF8Encoding utf8Encoding
		{
			get
			{
				if (this._utf8Encoding == null)
				{
					this._utf8Encoding = new UTF8Encoding(true);
				}

				return this._utf8Encoding;
			}
		}

		#endregion

		/*
		 * Methods
		 * */
		#region Monobehaviour Lifecycle
		public void Update()
		{
			if (this.csvBytes != null && this.csvBytes.Count > 0)
			{
				// csvList is not empty, write it
				this.AsyncWriteData();
			}

			// CSV Logging
			// from ISA MapSat
			if (loggingActive && (!waitForLaunch || this.Vessel.situation != Vessel.Situations.PRELAUNCH))
			{
				//data logging is on
				//increment timers
				this.csvCollectTimer += Time.deltaTime;

				if (this.csvCollectTimer >= this.logInterval)
				{
					//data logging is on, vessel is not prelaunch, and interval has passed
					//write a line to the list
					this.CollectLogData();
				}
			}
		}

		public void FixedUpdate() {}

		public void OnDestroy()
		{
			using (PooledDebugLogger logger = PooledDebugLogger.New(this))
			{
				logger.Append("Destroying...");

				this.CloseFileIfOpen();

				logger.Append(" Done.");
				logger.Print(false);
			}
		}

		#endregion

		#region VOID_Module Overrides

		public override void LoadConfig(KSP.IO.PluginConfiguration config)
		{
			base.LoadConfig(config);

			this.logIntervalStr = this.logInterval.value.ToString("#.0##");
		}

		public override void ModuleWindow(int id)
		{
			GUILayout.BeginVertical();

			GUILayout.Label(
				string.Format("System time: {0}", DateTime.Now.ToString("HH:mm:ss")),
				GUILayout.ExpandWidth(true)
			);
			GUILayout.Label(
				string.Format("Kerbin time: {0}", VOID_Tools.FormatDate(Planetarium.GetUniversalTime())),
				GUILayout.ExpandWidth(true)
			);

			GUIStyle activeLabelStyle = VOID_Styles.labelRed;
			string activeLabelText = "Inactive";
			if (loggingActive)
			{
				activeLabelText = "Active";
				activeLabelStyle = VOID_Styles.labelGreen;
			}

			this.loggingActive = Layout.Toggle(
				loggingActive,
				string.Format("Data logging: {0}", activeLabelText),
				null,
				activeLabelStyle
			);

			this.waitForLaunch.value = Layout.Toggle(
				this.waitForLaunch,
				"Wait for launch"
			);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label("Interval: ", GUILayout.ExpandWidth(false));

			logIntervalStr = GUILayout.TextField(logIntervalStr, GUILayout.ExpandWidth(true));
			GUILayout.Label("s", GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();

			float newLogInterval;
			if (float.TryParse(logIntervalStr, out newLogInterval))
			{
				logInterval.value = newLogInterval;
				this.logIntervalStr = this.logInterval.value.ToString("#.0##");
			}

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}

		#endregion

		#region Data Collection

		private void CollectLogData()
		{
			if (this.csvBytes == null)
			{
				this.csvBytes = new List<byte>();
			}

			//called if logging is on and interval has passed
			//writes one line to the csvList

			using (PooledStringBuilder line = PooledStringBuilder.Get())
			{
				if (firstWrite)
				{
					firstWrite = false;
					line.Append(
						"\"Kerbin Universal Time (s)\"," +
						"\"Mission Elapsed Time (s)\t\"," +
						"\"Altitude ASL (m)\"," +
						"\"Altitude above terrain (m)\"," +
						"\"Surface Latitude (°)\"," +
						"\"Surface Longitude (°)\"," +
						"\"Apoapsis Altitude (m)\"," +
						"\"Periapsis Altitude (m)\"," +
						"\"Orbital Inclination (°)\"," +
						"\"Orbital Velocity (m/s)\"," +
						"\"Surface Velocity (m/s)\"," +
						"\"Vertical Speed (m/s)\"," +
						"\"Horizontal Speed (m/s)\"," +
						"\"Current Thrust (kN)\"," +
						"\"Gee Force (gees)\"," +
						"\"Temperature (°C)\"," +
						"\"Gravity (m/s²)\"," +
						"\"Atmosphere Density (g/m³)\"," +
						"\"Downrange Distance  (m)\"," +
						"\"Main Throttle\"," +
						"\n"
					);
				}

				// Universal time
				line.Append(Planetarium.GetUniversalTime().ToString("F2"));
				line.Append(',');

				//Mission time
				line.Append(Vessel.missionTime.ToString("F3"));
				line.Append(',');

				//Altitude ASL
				line.Append(VOID_Data.orbitAltitude.Value.ToString("G9"));
				line.Append(',');

				//Altitude (true)
				line.Append(VOID_Data.trueAltitude.Value.ToString("G9"));
				line.Append(',');

				// Surface Latitude
				line.Append('"');
				line.Append(VOID_Data.surfLatitude.Value.ToString("F3"));
				line.Append('"');
				line.Append(',');

				// Surface Longitude
				line.Append('"');
				line.Append(VOID_Data.surfLongitude.Value.ToString("F3"));
				line.Append('"');
				line.Append(',');

				// Apoapsis Altitude
				line.Append(VOID_Data.orbitApoAlt.Value.ToString("G9"));
				line.Append(',');

				// Periapsis Altitude
				line.Append(VOID_Data.oribtPeriAlt.Value.ToString("G9"));
				line.Append(',');

				// Orbital Inclination
				line.Append(VOID_Data.orbitInclination.Value.ToString("F2"));
				line.Append(',');

				//Orbital velocity
				line.Append(VOID_Data.orbitVelocity.Value.ToString("G9"));
				line.Append(',');

				//surface velocity
				line.Append(VOID_Data.surfVelocity.Value.ToString("G9"));
				line.Append(',');

				//vertical speed
				line.Append(VOID_Data.vertVelocity.Value.ToString("G9"));
				line.Append(',');

				//horizontal speed
				line.Append(VOID_Data.horzVelocity.Value.ToString("G9"));
				line.Append(',');

				// Current Thrust
				line.Append(VOID_Data.currThrust.Value.ToString("G9"));
				line.Append(',');

				//gee force
				line.Append(VOID_Data.geeForce.Value.ToString("G9"));
				line.Append(',');

				//temperature
				line.Append(VOID_Data.temperature.Value.ToString("F3"));
				line.Append(',');

				//gravity
				line.Append(VOID_Data.gravityAccel.Value.ToString("G9"));
				line.Append(',');

				//atm density
				line.Append(VOID_Data.atmDensity.Value.ToString("G9"));
				line.Append(',');

				// Downrange Distance
				line.Append((VOID_Data.downrangeDistance.Value.ToString("G9")));
				line.Append(',');

				// Main Throttle
				line.Append(VOID_Data.mainThrottle.Value.ToString("P2"));

				line.Append('\n');

				csvBytes.AddRange(this.utf8Encoding.GetBytes(line.ToString()));

				this.csvCollectTimer = 0f;
			}
		}

		#endregion

		#region File IO Methods

		protected void AsyncWriteCallback(IAsyncResult result)
		{
			Logging.PostDebugMessage(this, "Got async callback, IsCompleted = {0}", result.IsCompleted);

			this.outputFile.EndWrite(result);
			this.outstandingWrites--;
		}

		private void AsyncWriteData()
		{
			WriteState state = new WriteState();

			state.bytes = this.csvBytes.ToArray();
			state.stream = this.outputFile;

			this.outstandingWrites++;
			var writeCallback = new AsyncCallback(this.AsyncWriteCallback);

			this.outputFile.BeginWrite(state.bytes, 0, state.bytes.Length, writeCallback, state);

			this.csvBytes.Clear();
		}

		private void CloseFileIfOpen()
		{
			using (PooledDebugLogger logger = PooledDebugLogger.New(this))
			{
				logger.AppendFormat("Cleaning up file {0}...", this.fileName);

				if (this.csvBytes != null && this.csvBytes.Count > 0)
				{
					logger.Append(" Writing remaining data...");
					this.AsyncWriteData();
				}

				logger.Append(" Waiting for writes to finish.");
				while (this.outstandingWrites > 0)
				{
					logger.Append('.');
					System.Threading.Thread.Sleep(10);
				}

				if (this._outputFile != null)
				{
					this._outputFile.Close();
					this._outputFile = null;
					logger.Append(" File closed.");
				}

				logger.Print(false);
			}
		}

		#endregion

		#region Constructors & Destructors

		public VOID_DataLogger()
		{
			this.Name = "CSV Data Logger";

			this.loggingActive = false;
			this.firstWrite = true;

			this.waitForLaunch = (VOID_SaveValue<bool>)true;

			this.logInterval = (VOID_SaveValue<float>)0.5f;
			this.csvCollectTimer = (VOID_SaveValue<float>)0f;

			this.outstandingWrites = 0;

			this.WindowPos.x = Screen.width - 520f;
			this.WindowPos.y = 85f;

			this.core.onApplicationQuit += delegate(object sender)
			{
				this.CloseFileIfOpen();
			};
		}

		~VOID_DataLogger()
		{
			this.OnDestroy();
		}

		#endregion

		#region Subclasses

		private class WriteState
		{
			public byte[] bytes;
			public FileStream stream;
		}

		#endregion
	}
}

