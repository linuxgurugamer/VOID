// VOID
//
// VOID_Data.cs
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

using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools;
using ToadicusTools.Extensions;
using ToadicusTools.MuMechTools;
using UnityEngine;

namespace VOID
{
	public static class VOID_Data
	{
		private static Dictionary<int, IVOID_DataValue> dataValues = new Dictionary<int, IVOID_DataValue>();

		public static Dictionary<int, IVOID_DataValue> DataValues
		{
			get
			{
				return dataValues;
			}
		}

		#region Constants

		private static double kerbinGee;

		public static double KerbinGee
		{
			get
			{
				if (kerbinGee == default(double))
				{
					kerbinGee = Core.HomeBody.gravParameter / (Core.HomeBody.Radius * Core.HomeBody.Radius);
				}

				return kerbinGee;
			}
		}

		#endregion

		#region Core Data

		public static VOIDCore Core
		{
			get
			{
				if (!CoreInitialized)
				{
					return null;
				}

				switch (HighLogic.LoadedScene)
				{
					case GameScenes.EDITOR:
						return (VOIDCore)VOIDCore_Editor.Instance;
					case GameScenes.FLIGHT:
						return (VOIDCore)VOIDCore_Flight.Instance;
					case GameScenes.SPACECENTER:
						return (VOIDCore)VOIDCore_SpaceCentre.Instance;
					default:
						return null;
				}
			}
		}

		public static bool CoreInitialized
		{
			get
			{
				switch (HighLogic.LoadedScene)
				{
					case GameScenes.EDITOR:
						return VOIDCore_Editor.Initialized;
					case GameScenes.FLIGHT:
						return VOIDCore_Flight.Initialized;
					case GameScenes.SPACECENTER:
						return VOIDCore_SpaceCentre.Initialized;
					default:
						return false;
				}
			}
		}

		#endregion

		#region Atmosphere

		public static readonly VOID_DoubleValue atmDensity =
			new VOID_DoubleValue(
				"Atmosphere Density",
				new Func<double>(() => Core.Vessel.atmDensity * 1000d),
				"g/m³"
			);

		public static readonly VOID_DoubleValue atmLimit =
			new VOID_DoubleValue(
				"Atmosphere Depth",
				new Func<double>(() => Core.Vessel.mainBody.atmosphereDepth),
				"m"
			);

		public static readonly VOID_DoubleValue atmPressure =
			new VOID_DoubleValue(
				"Static Pressure",
				new Func<double>(() => Core.Vessel.staticPressurekPa * 1000d),
				"Pa"
			);

		public static readonly VOID_DoubleValue temperature =
			new VOID_DoubleValue(
				"Temperature",
				new Func<double>(() => Core.Vessel.atmosphericTemperature),
				"K"
			);

		#endregion

		#region Attitude

		public static readonly VOID_StrValue vesselHeading =
			new VOID_StrValue(
				"Heading",
				delegate()
				{
					double heading = Core.Vessel.getSurfaceHeading();
					string cardinal = VOID_Tools.get_heading_text(heading);

					return string.Format(
						"{0}° {1}",
						heading.ToString("F2"),
						cardinal
					);
				}
			);

		public static readonly VOID_DoubleValue vesselPitch =
			new VOID_DoubleValue(
				"Pitch",
				() => Core.Vessel.getSurfacePitch(),
				"°"
			);

		#endregion

		#region Career

		public static readonly VOID_StrValue fundingStatus =
			new VOID_StrValue(
				string.Intern("Funds"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentFunds.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastFundsChange)
					);
				}
			);

		public static readonly VOID_StrValue reputationStatus =
			new VOID_StrValue(
				string.Intern("Reputation"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentReputation.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastRepChange)
					);
				}
			);

		public static readonly VOID_StrValue scienceStatus =
			new VOID_StrValue(
				string.Intern("Science"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentScience.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastScienceChange)
					);
				}
			);

		#endregion

		#region Control

		public static readonly VOID_FloatValue mainThrottle =
			new VOID_FloatValue(
				"Throttle",
				new Func<float>(() => Core.Vessel.ctrlState.mainThrottle),
				""
			);

		#endregion

		#region Engineering

		public static readonly VOID_IntValue partCount =
			new VOID_IntValue(
				"Parts",
				new Func<int>(() => Core.Vessel.Parts.Count),
				""
			);

		#region Mass

		public static readonly VOID_StrValue comboResourceMass =
			new VOID_StrValue(
				"Resource Mass (curr / total)",
				delegate()
				{
					return string.Format("{0} / {1}",
						stageResourceMass.ValueUnitString("F3"),
						resourceMass.ValueUnitString("F3")
					);
				}
			);

		public static readonly VOID_DoubleValue resourceMass =
			new VOID_DoubleValue(
				"Resource Mass",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.resourceMass;
				},
				"tons"
			);

		public static readonly VOID_DoubleValue stageResourceMass =
			new VOID_DoubleValue(
				"Resource Mass (Stage)",
				delegate()
				{
					if (Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.resourceMass;
				},
				"tons"
			);

		public static readonly VOID_DoubleValue totalMass =
			new VOID_DoubleValue(
				"Total Mass",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.totalMass;
				},
				"tons"
			);

		#endregion

		#region DeltaV

		public static readonly VOID_DoubleValue stageDeltaV =
			new VOID_DoubleValue(
				"DeltaV (Current Stage)",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return double.NaN;
					return Core.LastStage.deltaV;
				},
				"m/s"
			);

		public static readonly VOID_DoubleValue totalDeltaV =
			new VOID_DoubleValue(
				"DeltaV (Total)",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return double.NaN;
					return Core.LastStage.totalDeltaV;
				},
				"m/s"
			);

		#endregion

		#region Propulsion

		public static readonly VOID_StrValue currmaxThrustWeight =
			new VOID_StrValue(
				"T:W (curr/max)",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return "N/A";

					return string.Format(
						"{0} / {1}",
						(VOID_Data.currThrustWeight.Value).ToString("F2"),
						(VOID_Data.maxThrustWeight.Value).ToString("F2")
					);
				}
			);

		public static readonly VOID_DoubleValue currThrust =
			new VOID_DoubleValue(
				"Current Thrust",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return double.NaN;

					return Core.LastStage.actualThrust;
				},
				"kN"
			);

		public static readonly VOID_StrValue currmaxThrust =
			new VOID_StrValue(
				"Thrust (curr/max)",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return "N/A";

					double currThrust = Core.LastStage.actualThrust;
					double maxThrust = Core.LastStage.thrust;

					return string.Format(
						"{0} / {1}",
						currThrust.ToString("F1"),
						maxThrust.ToString("F1")
					);
				}
			);

		public static readonly VOID_DoubleValue stageMassFlow =
			new VOID_DoubleValue(
				"Stage Mass Flow",
				delegate()
				{
					if (Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.MassFlow();
				},
				"Mg/s"
			);

		public static readonly VOID_DoubleValue stageNominalThrust =
			new VOID_DoubleValue(
				"Nominal Stage Thrust",
				delegate()
				{
					if (Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.NominalThrust();
				},
				"kN"
			);

		#endregion

		#region Kinetics

		public static readonly VOID_DoubleValue currThrustWeight =
			new VOID_DoubleValue(
				"T:W Ratio",
				delegate()
				{
					if (Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.actualThrustToWeight;
				},
				""
			);



		public static readonly VOID_DoubleValue maxThrustWeight =
			new VOID_DoubleValue(
				"T:W Ratio",
				delegate()
				{
					if (Core.LastStage == null)
					{
						return double.NaN;
					}

					return Core.LastStage.thrustToWeight;
				},
				""
			);

		public static readonly VOID_DoubleValue nominalThrustWeight =
			new VOID_DoubleValue(
				"Thrust-to-Weight Ratio",
				delegate()
				{
					if (HighLogic.LoadedSceneIsEditor || currThrustWeight.Value == 0d)
					{
						return maxThrustWeight.Value;
					}

					return currThrustWeight.Value;
				},
				""
			);

		public static readonly VOID_DoubleValue surfaceThrustWeight =
			new VOID_DoubleValue(
				"Max T:W @ surface",
				delegate()
				{
					if (Core.Stages == null || Core.LastStage == null)
						return double.NaN;

					double maxThrust = Core.LastStage.thrust;
					double mass = Core.LastStage.totalMass;
					double gravity = (VOIDCore.Constant_G * Core.Vessel.mainBody.Mass) /
					                 (Core.Vessel.mainBody.Radius * Core.Vessel.mainBody.Radius);
					double weight = mass * gravity;

					return maxThrust / weight;
				},
				""
			);

		private static Vector3 thrustPos;
		private static Vector3 thrustDir;
		private static float thrust;

		private static void thrustOffsetPreForEach(object sender)
		{
			thrustPos = Vector3.zero;
			thrustDir = Vector3.zero;
			thrust = 0;
		}

		private static void thrustOffSetPerModule(object sender, VOIDForEachPartModuleArgs args)
		{
			PartModule module = args.Data;

			float moduleThrust = 0;

			switch (module.moduleName)
			{
				case "ModuleEngines":
				case "ModuleEnginesFX":
					break;
				default:
					return;
			}

			if (!module.isEnabled)
			{
				return;
			}

			CenterOfThrustQuery cotQuery = new CenterOfThrustQuery();

			if (module is ModuleEngines)
			{
				ModuleEngines engineModule = module as ModuleEngines;

				moduleThrust = engineModule.finalThrust;

				engineModule.OnCenterOfThrustQuery(cotQuery);
			}
			else // engine is ModuleEnginesFX
			{
				ModuleEnginesFX engineFXModule = module as ModuleEnginesFX;

				moduleThrust = engineFXModule.finalThrust;

				engineFXModule.OnCenterOfThrustQuery(cotQuery);
			}

			if (moduleThrust != 0d)
			{
				cotQuery.thrust = moduleThrust;
			}

			thrustPos += cotQuery.pos * cotQuery.thrust;
			thrustDir += cotQuery.dir * cotQuery.thrust;
			thrust += cotQuery.thrust;
		}

		public static readonly VOID_Vector3Value vesselThrustOffset =
			new VOID_Vector3Value(
				"Thrust Offset",
				delegate()
				{
					Vector3 pos = thrustPos;
					Vector3 dir = thrustDir;

					if (thrust != 0)
					{
						pos /= thrust;
						dir /= thrust;
					}

					Transform vesselTransform = Core.Vessel.transform;

					pos = vesselTransform.InverseTransformPoint(pos);
					dir = vesselTransform.InverseTransformDirection(dir);

					Vector3 thrustOffset = VectorTools.PointDistanceToLine(
						pos, dir.normalized, Core.Vessel.CoM);

					Logging.PostDebugMessage(typeof(VOID_Data), "vesselThrustOffset:\n" +
					"\tthrustPos: {0}\n" +
					"\tthrustDir: {1}\n" +
					"\tthrustOffset: {2}\n" +
					"\tvessel.CoM: {3}",
						pos,
						dir.normalized,
						thrustOffset,
						Core.Vessel.CoM
					);

					return thrustOffset;
				},
				"m"
			);

		#endregion

		#region Air Breathing

		private static double airFlowCurrent;
		private static double airFlowRequired;
		private static string intakeAirString;

		private static void intakeAirPreForEach(object sender)
		{
			airFlowCurrent = 0d;
			airFlowRequired = 0d;
			intakeAirString = string.Empty;
		}

		private static void intakeAirForEachModule(object sender, VOIDForEachPartModuleArgs args)
		{
			PartModule module = args.Data;
			List<Propellant> propellantList = null;

			if (!module.part.enabled)
			{
				return;
			}

			if (module is ModuleEngines)
			{
				propellantList = ((ModuleEngines)module).propellants;
			}
			else if (module is ModuleEnginesFX)
			{
				propellantList = ((ModuleEnginesFX)module).propellants;
			}
			else if (module is ModuleResourceIntake)
			{
				ModuleResourceIntake intakeModule = (ModuleResourceIntake)module;

				if (intakeModule.resourceName == "IntakeAir")
				{
					airFlowCurrent += intakeModule.airFlow;
				}
			}

			if (propellantList != null)
			{
				Propellant propellant;
				for (int propIdx = 0; propIdx < propellantList.Count; propIdx++)
				{
					propellant = propellantList[propIdx];

					if (propellant.name == "IntakeAir")
					{
						airFlowRequired += propellant.currentRequirement / TimeWarp.fixedDeltaTime;
						break;
					}
				}
			}
		}

		public static readonly VOID_StrValue intakeAirStatus =
			new VOID_StrValue(
				"Intake Air (Curr / Req)",
				delegate()
				{
					if (airFlowCurrent == 0 && airFlowRequired == 0)
					{
						intakeAirString = "N/A";
					}
					else
					{
						intakeAirString = string.Format("{0:F3} / {1:F3}", airFlowCurrent, airFlowRequired);
					}

					return intakeAirString;
				}
			);

		#endregion

		#region Crew

		private static int crewCount;
		private static int crewCapacity;

		private static void crewCountPreForEach(object sender)
		{
			crewCount = 0;
			crewCapacity = 0;
		}

		private static void crewCountPerPart(object sender, VOIDForEachPartArgs args)
		{
			Part part = args.Data;

			crewCount += part.protoModuleCrew.Count;
			crewCapacity += part.CrewCapacity;
		}

		public static readonly VOID_IntValue vesselCrewCount =
			new VOID_IntValue(
				"Crew Onboard",
				delegate()
				{
					return crewCount;
				},
				""
			);

		public static readonly VOID_IntValue vesselCrewCapacity =
			new VOID_IntValue(
				"Crew Capacity",
				delegate()
				{
					return crewCapacity;
				},
				""
			);

		#endregion

		#endregion

		#region Location

		public const double kscLongitude = 285.442323427289 * Math.PI / 180d;
		public const double kscLatitude = -0.0972112860655246 * Math.PI / 180d;

		public static readonly VOID_DoubleValue downrangeDistance =
			new VOID_DoubleValue(
				"Downrange Distance",
				delegate()
				{

					if (Core.Vessel == null ||
					    Planetarium.fetch == null ||
					    Core.Vessel.mainBody != Planetarium.fetch.Home)
					{
						return double.NaN;
					}

					double vesselLongitude = Core.Vessel.longitude * Math.PI / 180d;
					double vesselLatitude = Core.Vessel.latitude * Math.PI / 180d;

					double diffLon = Math.Abs(vesselLongitude - kscLongitude);

					double cosVesselLatitude = Math.Cos(vesselLatitude);
					double sinDiffLon = Math.Sin(diffLon);

					double term1 = cosVesselLatitude * sinDiffLon;

					double cosKSCLatitude = Math.Cos(kscLatitude);
					double sinVesselLatitude = Math.Sin(vesselLatitude);
					double sinKSCLatitude = Math.Sin(kscLatitude);
					double cosDiffLon = Math.Cos(diffLon);

					double term2 = cosKSCLatitude * sinVesselLatitude - sinKSCLatitude * cosVesselLatitude * cosDiffLon;

					double term3 = sinKSCLatitude * sinVesselLatitude + cosKSCLatitude * cosVesselLatitude * cosDiffLon;

					double arc = Math.Atan2(Math.Sqrt(term1 * term1 + term2 * term2), term3);

					return arc * Core.Vessel.mainBody.Radius;
				},
				"m"
			);

		public static readonly VOID_StrValue surfLatitudeString =
			new VOID_StrValue(
				"Latitude",
				new Func<string>(() => VOID_Tools.GetLatitudeString(Core.Vessel))
			);

		public static readonly VOID_DoubleValue surfLatitude =
			new VOID_DoubleValue(
				"Latitude",
				delegate()
				{
					if (CoreInitialized && Core.Vessel != null)
					{
						return Core.Vessel.latitude;
					}
					return double.NaN;
				},
				"°"
			);

		public static readonly VOID_StrValue surfLongitudeString =
			new VOID_StrValue(
				"Longitude",
				new Func<string>(() => VOID_Tools.GetLongitudeString(Core.Vessel))
			);

		public static readonly VOID_DoubleValue surfLongitude =
			new VOID_DoubleValue(
				"Longitude",
				delegate()
				{
					if (CoreInitialized && Core.Vessel != null)
					{
						double longitude = Core.Vessel.longitude;

						longitude = VOID_Tools.FixDegreeDomain(longitude);

						if (longitude < -180d)
						{
							longitude += 360d;
						}
						if (longitude >= 180)
						{
							longitude -= 360d;
						}

						return longitude;
					}
					return double.NaN;
				},
				"°"
			);

		public static readonly VOID_DoubleValue trueAltitude =
			new VOID_DoubleValue(
				"Altitude (true)",
				delegate()
				{
					double alt_true = Core.Vessel.orbit.altitude - Core.Vessel.terrainAltitude;
					// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m,
					// and water covers the whole surface at 0 m.
					if (Core.Vessel.terrainAltitude < 0 && Core.Vessel.mainBody.ocean)
						alt_true = Core.Vessel.orbit.altitude;
					return alt_true;
				},
				"m"
			);

		#endregion

		#region Kinematics

		public static readonly VOID_DoubleValue geeForce =
			new VOID_DoubleValue(
				"G-force",
				new Func<double>(() => Core.Vessel.geeForce),
				"gees"
			);

		public static readonly VOID_DoubleValue horzVelocity =
			new VOID_DoubleValue(
				"Horizontal speed",
				delegate
				{
					return Core.Vessel.horizontalSrfSpeed;
				},
				"m/s"
			);

		public static readonly VOID_DoubleValue surfVelocity =
			new VOID_DoubleValue(
				"Surface velocity",
				new Func<double>(() => Core.Vessel.srf_velocity.magnitude),
				"m/s"
			);

		public static readonly VOID_DoubleValue vertVelocity =
			new VOID_DoubleValue(
				"Vertical speed",
				new Func<double>(() => Core.Vessel.verticalSpeed),
				"m/s"
			);

		public static readonly VOID_DoubleValue vesselAccel =
			new VOID_DoubleValue(
				"Acceleration",
				() => geeForce * KerbinGee,
				"m/s²"
			);

		public static readonly VOID_DoubleValue vesselAngularVelocity =
			new VOID_DoubleValue(
				"Angular Velocity",
				delegate()
				{
					if (Core.Vessel != null)
					{
						return Core.Vessel.angularVelocity.magnitude;
					}
					else
					{
						return double.NaN;
					}
				},
				"rad/s"
			);

		#endregion

		#region Navigation

		public static int upcomingManeuverNodes
		{
			get
			{
				if (Core == null ||
                    Core.Vessel == null ||
				    Core.Vessel.patchedConicSolver == null ||
				    Core.Vessel.patchedConicSolver.maneuverNodes == null)
				{
					return 0;
				}

				return Core.Vessel.patchedConicSolver.maneuverNodes.Count;
			}
		}

		public static readonly VOID_StrValue burnTimeDoneAtNode =
			new VOID_StrValue(
				"Full burn time to be half done at node",
				delegate()
				{
					if (Core.LastStage == null && upcomingManeuverNodes < 1)
					{
						return "N/A";
					}

					ManeuverNode node = Core.Vessel.patchedConicSolver.maneuverNodes[0];

					if ((node.UT - Planetarium.GetUniversalTime()) < 0)
					{
						return string.Empty;
					}

					double interval = (node.UT - currentNodeBurnDuration) - Planetarium.GetUniversalTime();

					if (double.IsNaN(interval))
					{
						return string.Intern("NaN");
					}

					int sign = Math.Sign(interval);
					interval = Math.Abs(interval);

					string format;

					if (sign >= 0)
					{
						format = string.Intern("T - {0}");
					}
					else
					{
						format = string.Intern("T + {0}");
					}

					return string.Format(format, VOID_Tools.FormatInterval(interval));
				}
			);

		public static readonly VOID_StrValue burnTimeHalfDoneAtNode =
			new VOID_StrValue(
				"Full burn time to be half done at node",
				delegate()
				{
					if (Core.LastStage == null && upcomingManeuverNodes < 1)
					{
						return "N/A";
					}

					ManeuverNode node = Core.Vessel.patchedConicSolver.maneuverNodes[0];

					if ((node.UT - Planetarium.GetUniversalTime()) < 0)
					{
						return string.Empty;
					}

					double interval = (node.UT - currentNodeHalfBurnDuration) - Planetarium.GetUniversalTime();

					if (double.IsNaN(interval))
					{
						return string.Intern("NaN");
					}

					int sign = Math.Sign(interval);
					interval = Math.Abs(interval);

					string format;

					if (sign >= 0)
					{
						format = string.Intern("T - {0}");
					}
					else
					{
						format = string.Intern("T + {0}");
					}

					return string.Format(format, VOID_Tools.FormatInterval(interval));
				}
			);

		public static readonly VOID_DoubleValue currManeuverDeltaV =
			new VOID_DoubleValue(
				"Current Maneuver Delta-V",
				delegate()
				{
					if (upcomingManeuverNodes > 0)
					{
						return Core.Vessel.patchedConicSolver.maneuverNodes[0].DeltaV.magnitude;
					}
					else
					{
						return double.NaN;
					}
				},
				"m/s"
			);

		public static readonly VOID_DoubleValue currManeuverDVRemaining =
			new VOID_DoubleValue(
				"Remaining Maneuver Delta-V",
				delegate()
				{
					if (upcomingManeuverNodes > 0)
					{
						return Core.Vessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(Core.Vessel.orbit).magnitude;
					}
					else
					{
						return double.NaN;
					}
				},
				"m/s"
			);

		public static readonly VOID_DoubleValue currentNodeBurnDuration =
			new VOID_DoubleValue(
				"Total Burn Time",
				delegate()
				{
					if (currManeuverDeltaV.Value == double.NaN)
					{
						return double.NaN;
					}

					return realVesselBurnTime(currManeuverDeltaV.Value);
				},
				"s"
			);

		public static readonly VOID_DoubleValue currentNodeBurnRemaining =
			new VOID_DoubleValue(
				"Burn Time Remaining",
				delegate()
				{
					if (currManeuverDVRemaining.Value == double.NaN)
					{
						return double.NaN;
					}

					return realVesselBurnTime(currManeuverDVRemaining.Value);
				},
				"s"
			);

		public static readonly VOID_DoubleValue currentNodeHalfBurnDuration =
			new VOID_DoubleValue(
				"Half Burn Time",
				delegate()
				{
					if (currManeuverDeltaV.Value == double.NaN)
					{
						return double.NaN;
					}

					return realVesselBurnTime(currManeuverDeltaV.Value / 2d);
				},
				"s"
			);

		public static readonly VOID_DoubleValue nextManeuverDeltaV =
			new VOID_DoubleValue(
				"Current Maneuver Delta-V",
				delegate()
				{
					if (upcomingManeuverNodes > 1)
					{
						return Core.Vessel.patchedConicSolver.maneuverNodes[1].DeltaV.magnitude;
					}
					else
					{
						return double.NaN;
					}
				},
				"m/s"
			);

		#endregion

		#region Orbits

		public static readonly VOID_StrValue primaryName =
			new VOID_StrValue(
				VOID_Localization.void_primary,
				delegate()
				{
					if (Core.Vessel == null)
					{
						return string.Empty;
					}
					return Core.Vessel.mainBody.name;
				}
			);

		public static readonly VOID_DoubleValue orbitAltitude =
			new VOID_DoubleValue(
				"Altitude (ASL)",
				new Func<double>(() => Core.Vessel.orbit.altitude),
				"m"
			);

		public static readonly VOID_DoubleValue orbitVelocity =
			new VOID_DoubleValue(
				VOID_Localization.void_velocity,
				new Func<double>(() => Core.Vessel.orbit.vel.magnitude),
				"m/s"
			);

		public static readonly VOID_DoubleValue orbitApoAlt =
			new VOID_DoubleValue(
				VOID_Localization.void_apoapsis,
				new Func<double>(() => Core.Vessel.orbit.ApA),
				"m"
			);

		public static readonly VOID_DoubleValue oribtPeriAlt =
			new VOID_DoubleValue(
				VOID_Localization.void_periapsis,
				new Func<double>(() => Core.Vessel.orbit.PeA),
				"m"
			);

		public static readonly VOID_StrValue timeToApo =
			new VOID_StrValue(
				"Time to Apoapsis",
				new Func<string>(() => VOID_Tools.FormatInterval(Core.Vessel.orbit.timeToAp))
			);

		public static readonly VOID_StrValue timeToPeri =
			new VOID_StrValue(
				"Time to Periapsis",
				new Func<string>(() => VOID_Tools.FormatInterval(Core.Vessel.orbit.timeToPe))
			);

		public static readonly VOID_DoubleValue orbitInclination =
			new VOID_DoubleValue(
				"Inclination",
				new Func<double>(() => Core.Vessel.orbit.inclination),
				"°"
			);

		public static readonly VOID_DoubleValue gravityAccel =
			new VOID_DoubleValue(
				"Gravity",
				delegate()
				{
					double orbitRadius = Core.Vessel.mainBody.Radius +
					                     Core.Vessel.mainBody.GetAltitude(Core.Vessel.CoM);
					return (VOIDCore.Constant_G * Core.Vessel.mainBody.Mass) /
					(orbitRadius * orbitRadius);
				},
				"m/s²"
			);

		public static readonly VOID_StrValue orbitPeriod =
			new VOID_StrValue(
				"Period",
				new Func<string>(() => VOID_Tools.FormatInterval(Core.Vessel.orbit.period))
			);

		public static readonly VOID_DoubleValue semiMajorAxis =
			new VOID_DoubleValue(
				"Semi-Major Axis",
				new Func<double>(() => Core.Vessel.orbit.semiMajorAxis),
				"m"
			);

		public static readonly VOID_DoubleValue eccentricity =
			new VOID_DoubleValue(
				"Eccentricity",
				new Func<double>(() => Core.Vessel.orbit.eccentricity),
				""
			);

		public static readonly VOID_DoubleValue meanAnomaly =
			new VOID_DoubleValue(
				"Mean Anomaly",
				new Func<double>(() => Core.Vessel.orbit.meanAnomaly * 180d / Math.PI),
				"°"
			);

		public static readonly VOID_DoubleValue trueAnomaly = 
			new VOID_DoubleValue(
				"True Anomaly",
				new Func<double>(() => Core.Vessel.orbit.trueAnomaly * 180d / Math.PI),
				"°"
			);

		public static readonly VOID_DoubleValue eccAnomaly =
			new VOID_DoubleValue(
				"Eccentric Anomaly",
				new Func<double>(() => Core.Vessel.orbit.eccentricAnomaly * 180d / Math.PI),
				"°"
			);

		public static readonly VOID_DoubleValue longitudeAscNode =
			new VOID_DoubleValue(
				"Long. Ascending Node",
				new Func<double>(() => Core.Vessel.orbit.LAN),
				"°"
			);

		public static readonly VOID_DoubleValue argumentPeriapsis =
			new VOID_DoubleValue(
				"Argument of Periapsis",
				new Func<double>(() => Core.Vessel.orbit.argumentOfPeriapsis),
				"°"
			);

		public static readonly VOID_StrValue timeToAscendingNode =
			new VOID_StrValue(
				"Time to Ascending Node",
				delegate()
				{
					double trueAnomalyAscNode = 360d - argumentPeriapsis;
					double dTAscNode = Core.Vessel.orbit.GetDTforTrueAnomaly(
						                   trueAnomalyAscNode * Mathf.Deg2Rad,
						                   Core.Vessel.orbit.period
					                   );

					dTAscNode %= Core.Vessel.orbit.period;

					if (dTAscNode < 0d)
					{
						dTAscNode += Core.Vessel.orbit.period;
					}

					return VOID_Tools.FormatInterval(dTAscNode);
				}
			);

		public static readonly VOID_StrValue timeToDescendingNode =
			new VOID_StrValue(
				"Time to Descending Node",
				delegate()
				{
					double trueAnomalyAscNode = 180d - argumentPeriapsis;
					double dTDescNode = Core.Vessel.orbit.GetDTforTrueAnomaly(
						                    trueAnomalyAscNode * Mathf.Deg2Rad,
						                    Core.Vessel.orbit.period
					                    );

					dTDescNode %= Core.Vessel.orbit.period;

					if (dTDescNode < 0d)
					{
						dTDescNode += Core.Vessel.orbit.period;
					}

					return VOID_Tools.FormatInterval(dTDescNode);
				}
			);

		public static readonly VOID_DoubleValue localSiderealLongitude =
			new VOID_DoubleValue(
				"Local Sidereal Longitude",
				new Func<double>(() => VOID_Tools.FixDegreeDomain(
					Core.Vessel.longitude + Core.Vessel.orbit.referenceBody.rotationAngle)),
				"°"
			);

		#endregion

		#region Science

		public static readonly VOID_StrValue expSituation =
			new VOID_StrValue(
				"Situation",
				new Func<string>(() => Core.Vessel.GetExperimentSituation().HumanString())
			);

		public static readonly VOID_StrValue currBiome =
			new VOID_StrValue(
				"Biome",
				delegate()
				{
					if (Core.Vessel.landedAt == string.Empty)
					{
						return VOID_Tools.GetBiome(Core.Vessel).name;
					}
					else
					{
						return Core.Vessel.landedAt;
					}
				}
			);

		#endregion

		#region Surface

		public static readonly VOID_DoubleValue terrainElevation =
			new VOID_DoubleValue(
				"Terrain elevation",
				new Func<double>(() => Core.Vessel.terrainAltitude),
				"m"
			);

		#endregion

		private static double burnTime(double deltaV, double initialMass, double massFlow, double thrust)
		{
			Logging.PostDebugMessage(typeof(VOID_Data), "calculating burnTime from:\n" +
			"\tdeltaV: {0}\n" +
			"\tinitialMass: {1}\n" +
			"\tmassFlow: {2}\n" +
			"\tthrust: {3}\n",
				deltaV,
				initialMass,
				massFlow,
				thrust
			);
			return initialMass / massFlow * (1d - Math.Exp(-deltaV * massFlow / thrust));
		}

		private static double dVfromBurnTime(double time, double initialMass, double massFlow, double thrust)
		{
			return -thrust / massFlow * Math.Log(1d - time * massFlow / initialMass);
		}

		private static double realVesselBurnTime(double deltaV)
		{
			if (Core.Stages == null || Core.Stages.Length < 1)
			{
				return double.NaN;
			}

			double burntime = 0d;
			double dVRemaining = deltaV;

			int stageIdx = Core.Stages.Length - 1;

			while (dVRemaining > double.Epsilon)
			{
				if (stageIdx < 0)
				{
					return double.PositiveInfinity;
				}

				Stage stage = Core.Stages[stageIdx];

				if (stage.deltaV > 0)
				{
					double stageDVUsed = Math.Min(stage.deltaV, dVRemaining);

					burntime += burnTime(stageDVUsed, stage.totalMass, stage.MassFlow(), stage.NominalThrust());
					dVRemaining -= stageDVUsed;
				}

				stageIdx--;
			}

			return burntime;
		}

		private static void onFlightModulesLoaded(object sender)
		{
			if (sender is VOIDCore_Flight)
			{
				VOIDCore_Flight flightCore = sender as VOIDCore_Flight;

				flightCore.onPreForEach += thrustOffsetPreForEach;
				flightCore.onForEachModule += thrustOffSetPerModule;

				flightCore.onPreForEach += intakeAirPreForEach;
				flightCore.onForEachModule += intakeAirForEachModule;

				flightCore.onPreForEach += crewCountPreForEach;
				flightCore.onForEachPart += crewCountPerPart;
			}
		}
        
		private static void onFlightModulesDestroyed(object sender)
		{
			if (sender is VOIDCore_Flight)
			{
				VOIDCore_Flight flightCore = sender as VOIDCore_Flight;

				flightCore.onPreForEach -= thrustOffsetPreForEach;
				flightCore.onForEachModule -= thrustOffSetPerModule;

				flightCore.onPreForEach -= intakeAirPreForEach;
				flightCore.onForEachModule -= intakeAirForEachModule;

				flightCore.onPreForEach -= crewCountPreForEach;
				flightCore.onForEachPart -= crewCountPerPart;
			}
		}

		static VOID_Data()
		{
			VOIDCore_Flight.onModulesLoaded += onFlightModulesLoaded;
			VOIDCore_Flight.onModulesDestroyed += onFlightModulesDestroyed;
		}
	}
}
