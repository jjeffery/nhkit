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
	/// Implementation of <see cref="IIdHelper{T}"/> that is optimised for <see cref="Int64"/>.
	/// </summary>
	public class IdHelperForInt64 : IIdHelper<long>
	{
		public bool IsDefaultValue(long id)
		{
			return id == 0;
		}

		public bool IsNull(long id)
		{
			return false;
		}

		public virtual int Compare(long id1, long id2)
		{
			return id1.CompareTo(id2);
		}

		public bool AreEqual(long id1, long id2)
		{
			return id1 == id2;
		}

		public int GetHashCode(long id)
		{
			return id.GetHashCode();
		}
	}
}
