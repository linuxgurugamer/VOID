// VOID © 2015 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/
using System;

namespace VOID
{
	public enum VOID_TimeScale : UInt32
	{
		KERBIN_TIME = 1, // Earth if 0
		SOLAR_DAY = 4, // Sidereal if 0
		ROUNDED_SCALE = 1024 // Real values if 0
	}

	public enum IconState : UInt32
	{
		PowerOff = 1,
		PowerOn = 2,
		Inactive = 4,
		Active = 8
	}
}

