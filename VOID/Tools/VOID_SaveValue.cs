// VOID
//
// VOID_SaveValue.cs
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

using KSP;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public struct VOID_SaveValue<T> : IVOID_SaveValue
	{
		private T _value;
		private Type _type;

		private VOIDCore Core
		{
			get
			{
				return VOID_Data.Core;
			}
		}

		object IVOID_SaveValue.value
		{
			get
			{
				return this.value;
			}
		}

		public T value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (this.Core != null && !System.Object.Equals(this._value, value))
				{
					ToadicusTools.Logging.PostDebugMessage (string.Format (
						"VOID: Dirtying config for type {0}." +
						"\n\t Old Value: {1}, New Value: {2}" +
						"\n\t Object.Equals(New, Old): {3}\n" ,
						this._type,
						this._value,
						value,
						System.Object.Equals(this._value, value)
					));
					this.Core.configDirty = true;
				}
				this._value = value;
			}
		}

		public Type type
		{
			get
			{
				if (this._type == null && this._value != null)
				{
					this._type = this._value.GetType ();
				}
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}

		public void SetValue(object v)
		{
			this.value = (T)v;
		}

		public static implicit operator T(VOID_SaveValue<T> v)
		{
			return (T)v.value;
		}

		public static explicit operator VOID_SaveValue<T>(T v)
		{
			VOID_SaveValue<T> r = new VOID_SaveValue<T>();
			r.type = v.GetType();
			r.value = v;

			return r;
		}

		public override string ToString()
		{
			return this.value.ToString();
		}
	}
}

