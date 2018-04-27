// VOID
//
// VOID_DataValue.cs
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
using ToadicusTools.MuMechTools;
using ToadicusTools.Text;
using UnityEngine;

namespace VOID
{
	public class VOID_DataValue<T> : IVOID_DataValue
	{
		/*
		 * Static Members
		 * */
		public static implicit operator T(VOID_DataValue<T> v)
		{
			return (T)v.Value;
		}

		/*
		 * Instance Members
		 * */
		/*
		* Fields
		 * */
		protected T cache;
		protected Func<T> ValueFunc;
		protected float lastUpdate;

		/* 
		 * Properties
		 * */
		public string Label { get; protected set; }
		public string Units { get; protected set; }

		object IVOID_DataValue.Value
		{
			get
			{
				return this.Value;
			}
		}

		public T Value
		{
			get
			{
				if (
					(VOID_Data.Core.UpdateTimer - this.lastUpdate > VOID_Data.Core.UpdatePeriod) ||
					(this.lastUpdate > VOID_Data.Core.UpdateTimer)
				)
				{
					this.Refresh();
				}
				return (T)this.cache;
			}
		}

		/*
		 * Methods
		 * */
		public VOID_DataValue(string Label, Func<T> ValueFunc, string Units = "")
		{
			this.Label = Label;
			this.Units = Units;
			this.ValueFunc = ValueFunc;
			this.lastUpdate = 0;

			VOID_Data.DataValues[this.GetHashCode()] = this;
		}

		public void Refresh()
		{
			this.cache = this.ValueFunc.Invoke ();
			this.lastUpdate = VOID_Data.Core.UpdateTimer;
		}

		public T GetFreshValue()
		{
			this.Refresh ();
			return (T)this.cache;
		}

		public virtual string ValueUnitString()
		{
			if (this.Value == null || this.Units == null)
			{
				using (PooledStringBuilder sb = PooledStringBuilder.Get())
				{
					if (this.Value == null)
					{
						sb.AppendFormat("{0}: Value is null during ValueUnitString\n", this.Label);
					}
					if (this.Units == null)
					{
						sb.AppendFormat("{0}: Units is null during ValueUnitString\n", this.Label);
					}

					ToadicusTools.Logging.PostErrorMessage(sb.ToString());
				}
			}

			return string.Format("{0}{1}", this.Value == null ? "NULL" : this.Value.ToString(), this.Units);
		}

		public virtual void DoGUIHorizontal()
		{
			GUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
			GUILayout.Label (this.Label + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (this.ValueUnitString(), GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal ();
		}

		public override int GetHashCode()
		{
			int hash;
			unchecked
			{
				hash = 79999;

				hash = hash * 104399 + this.Label.GetHashCode();
				hash = hash * 104399 + this.ValueFunc.GetHashCode();
				hash = hash * 104399 + this.Units.GetHashCode();
			}

			return hash;
		}

		public override string ToString()
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (),
				this.Units
			);
		}
	}

	public abstract class VOID_NumValue<T> : VOID_DataValue<T>, IFormattable
		where T : IFormattable, IConvertible, IComparable
	{
		public static IFormatProvider formatProvider = SIFormatProvider.SIFormatter;

		public static implicit operator Double(VOID_NumValue<T> v)
		{
			return v.ToDouble();
		}

		public static implicit operator Int32(VOID_NumValue<T> v)
		{
			return v.ToInt32();
		}

		public static implicit operator Single(VOID_NumValue<T> v)
		{
			return v.ToSingle();
		}

		public VOID_NumValue(string Label, Func<T> ValueFunc, string Units = "") : base(Label, ValueFunc, Units)
		{

		}

		public virtual double ToDouble(IFormatProvider provider)
		{
			return this.Value.ToDouble(provider);
		}

		public virtual double ToDouble()
		{
			return this.ToDouble(formatProvider);
		}

		public virtual int ToInt32(IFormatProvider provider)
		{
			return this.Value.ToInt32(provider);
		}

		public virtual int ToInt32()
		{
			return this.ToInt32(formatProvider);
		}

		public virtual float ToSingle(IFormatProvider provider)
		{
			return this.Value.ToSingle(provider);
		}

		public virtual float ToSingle()
		{
			return this.ToSingle(formatProvider);
		}

		public virtual string ToString(string format)
		{
			return this.ToString(format, formatProvider);
		}

		public virtual string ToString(string format, IFormatProvider provider)
		{
			return string.Format (
				"{0}{1}",
				this.Value.ToString(format, provider),
				this.Units
			);
		}

		public virtual string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue)
		{
			return string.Format (
				"{0}{1}",
				MuMechTools.MuMech_ToSI (this, digits, MinMagnitude, MaxMagnitude),
				this.Units
			);
		}

		public virtual string ValueUnitString(string format)
		{
			return this.Value.ToString(format, formatProvider) + this.Units;
		}
		
		public virtual string ValueUnitString(int digits) {
			return string.Format("{0}{1}", SIFormatProvider.ToSI(this, digits), Units);
		}

		public virtual string ValueUnitString(int digits, int MinMagnitude, int MaxMagnitude)
		{
			return MuMechTools.MuMech_ToSI(this, digits, MinMagnitude, MaxMagnitude) + this.Units;
		}

		public virtual void DoGUIHorizontal(string format)
		{
			GUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
			GUILayout.Label (this.Label + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (this.ValueUnitString(format), GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal ();
		}

		public virtual int DoGUIHorizontal(int digits, bool precisionButton = true)
		{
			if (precisionButton)
			{
				return this.DoGUIHorizontalPrec(digits);
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + ":", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			GUILayout.Label(this.ValueUnitString(digits), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			return digits;
		}

		public virtual int DoGUIHorizontalPrec(int digits)
		{
			if (digits < 0 || digits > 8)
			{
				digits = 5;
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + "ⁱ:", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();

			GUILayout.Label(this.ValueUnitString(digits), GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.mouseUp)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					ToadicusTools.Logging.PostDebugMessage(string.Format("{0}: Changing digits from {1}",
						this.GetType().Name,
						digits
					));

					if (Event.current.button == 0)
					{
						digits = (digits + 3) % 9;
					}
					else if (Event.current.button == 1)
					{
						digits = (digits - 3) % 9;
					}

					if (digits < 0)
					{
						digits += 9;
					}

					ToadicusTools.Logging.PostDebugMessage(string.Format("{0}: Changed digits to {1}.",
						this.GetType().Name,
						digits
					));
				}
			}

			return digits;
		}
	}

	public class VOID_DoubleValue : VOID_NumValue<double>
	{
		public VOID_DoubleValue(string Label, Func<double> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_FloatValue : VOID_NumValue<float>
	{
		public VOID_FloatValue(string Label, Func<float> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_IntValue : VOID_NumValue<int>
	{
		public VOID_IntValue(string Label, Func<int> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_StrValue : VOID_DataValue<string>
	{
		public VOID_StrValue(string Label, Func<string> ValueFunc) : base(Label, ValueFunc, "") {}
	}

	public class VOID_Vector3Value : VOID_DataValue<Vector3>
	{
		public VOID_Vector3Value(string Label, Func<Vector3> ValueFunc, string Units)
			: base(Label, ValueFunc, Units)
		{}

		public string ToString(string format)
		{
			return string.Format("{0}: {1}{2}",
				this.Label,
				this.Value.ToString(format),
				this.Units
			);
		}

		public string ValueUnitString(string format) {
			return this.Value.ToString(format) + this.Units;
		}
	}
}

