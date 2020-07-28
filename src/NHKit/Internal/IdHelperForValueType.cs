#region License

// Copyright 2004-2020 John Jeffery <john@jeffery.id.au>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;

namespace NHKit.Internal
{
	/// <summary>
	/// Implementation of <see cref="IIdHelper{T}"/> that works for any value type.
	/// </summary>
	/// <typeparam name="TId">
	/// Type of entity identifier.
	/// </typeparam>
	public class IdHelperForValueType<TId> : IIdHelper<TId> where TId : struct, IComparable
	{
		// ReSharper disable StaticFieldInGenericType
		private static readonly object BoxedDefaultType = default(TId);
		// ReSharper restore StaticFieldInGenericType

		public bool IsDefaultValue(TId id)
		{
			return id.Equals(BoxedDefaultType);
		}

		public bool IsNull(TId id)
		{
			return false;
		}

		public virtual int Compare(TId id1, TId id2)
		{
			return id1.CompareTo(id2);
		}

		public bool AreEqual(TId id1, TId id2)
		{
			return id1.Equals(id2);
		}

		public int GetHashCode(TId id)
		{
			var x = id.GetHashCode();
			return x;
		}
	}
}
