// VOID
//
// VOID_Transfer.cs
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
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_Transfer : VOID_WindowModule
	{
		protected List<CelestialBody> selectedBodies = new List<CelestialBody>();

		public VOID_Transfer() : base()
		{
			this.Name = "Transfer Angle Information";

			this.WindowPos.x = 475;
			this.WindowPos.y = 85;
			this.defWidth = 315;
		}

		public override void ModuleWindow(int id)
		{
			CelestialBody body;

			GUILayout.BeginVertical();

			if (Vessel.mainBody.name == "Sun")  //Vessel is orbiting the Sun
			{
			    for (int idx = 0; idx < Vessel.mainBody.orbitingBodies.Count; idx++)
			    {
					body = Vessel.mainBody.orbitingBodies[idx];

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					if (GUILayout.Button(body.bodyName))
					{
						//add or remove this body to this list of bodies to display more info on
						if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
						else selectedBodies.Add(body);
					}
					GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					if (selectedBodies.Contains(body))
					{
						VOID_Tools.display_transfer_angles_SUN2PLANET(body, Vessel);  //show phase angles for each selected body
						tad_targeting(body);    //display Set/Unset Target button for each selected body
					}
			    }
			}
			else if (Vessel.mainBody.referenceBody.name == "Sun")	//Vessel is orbiting a planet
			{
			    for (int idx = 0; idx < Vessel.mainBody.referenceBody.orbitingBodies.Count; idx++)
				{
					body = Vessel.mainBody.referenceBody.orbitingBodies[idx];
			        if (body.name != Vessel.mainBody.name)	// show other planets
			        {
			            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			            if (GUILayout.Button(body.bodyName))
			            {
			                //add or remove this body to this list of bodies to display more info on
			                if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
			                else selectedBodies.Add(body);
			            }
			            GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			            GUILayout.EndHorizontal();

			            if (selectedBodies.Contains(body))
			            {
			                VOID_Tools.display_transfer_angles_PLANET2PLANET(body, Vessel);
			                tad_targeting(body);    //display Set/Unset Target button
			            }
			        }
			    }
			    for (int moonIdx = 0; moonIdx < Vessel.mainBody.orbitingBodies.Count; moonIdx++)
			    {
					body = Vessel.mainBody.orbitingBodies[moonIdx];

			        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			        if (GUILayout.Button(body.bodyName))
			        {
			            //add or remove this body to this list of bodies to display more info on
			            if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
			            else selectedBodies.Add(body);
			        }
			        GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			        GUILayout.EndHorizontal();

			        if (selectedBodies.Contains(body))
			        {
			            VOID_Tools.display_transfer_angles_PLANET2MOON(body, Vessel);
			            tad_targeting(body);    //display Set/Unset Target button
			        }
			    }
			}
			else if (Vessel.mainBody.referenceBody.referenceBody.name == "Sun")	// Vessel is orbiting a moon
			{
			    for (int idx = 0; idx < Vessel.mainBody.referenceBody.orbitingBodies.Count; idx++)
			    {
					body = Vessel.mainBody.referenceBody.orbitingBodies[idx];

					if (body.name != Vessel.mainBody.name)	// show other moons
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						if (GUILayout.Button(body.bodyName))
						{
							//add or remove this body to this list of bodies to display more info on
							if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
							else selectedBodies.Add(body);
						}
						GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						if (selectedBodies.Contains(body))
						{
							VOID_Tools.display_transfer_angles_MOON2MOON(body, Vessel);
							tad_targeting(body);    //display Set/Unset Target button
						}
					}
				}
			}
			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}

		private void tad_targeting(CelestialBody body)
		{
		    //Target Set/Unset buttons
		    if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() == null))
		    {
		        //No TGT set or TGT is a Body
		        if ((CelestialBody)FlightGlobals.fetch.VesselTarget != body)
		        {
		            if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
		            {
		                FlightGlobals.fetch.SetVesselTarget(body);
						Logging.PostDebugMessage("[VOID] KSP Target set to CelestialBody " + body.bodyName);
		            }
		        }
		        else if ((CelestialBody)FlightGlobals.fetch.VesselTarget == body)
		        {
		            if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
		            {
		                FlightGlobals.fetch.SetVesselTarget(null);
		                Logging.PostDebugMessage("[VOID] KSP Target set to null");
		            }
		        }
		    }
		    else if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != null))
		    {
		        //No TGT or TGT is a vessel
		        if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
		        {
		            FlightGlobals.fetch.SetVesselTarget(body);
		            Logging.PostDebugMessage("[VOID] KSP Target set to CelestialBody " + body.bodyName);
		        }
		    }
		}
	}
}

