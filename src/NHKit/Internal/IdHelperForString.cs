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
	/// Implementation of <see cref="IIdHelper{T}"/> that works for a string.
	/// </summary>
	/// <remarks>
	/// TODO: String comparisons are invariant culture, case-insensitive. This works
	/// well for databases where string comparison is case-sensitive, but may not be 
	/// appropriate in all cases.
	/// </remarks>
	public class IdHelperForString : IIdHelper<string>
	{
		public bool IsDefaultValue(string id)
		{
			return id == null;
		}

		public bool IsNull(string id)
		{
			return id == null;
		}

		public int Compare(string id1, string id2)
		{
			if (id1 == null)
			{
				if (id2 == null)
				{
					return 0;
				}
				return -1;
			}
			if (id2 == null)
			{
				return 1;
			}

			// Needs to be invariant culture to work with GetHashCode
			return StringComparer.InvariantCultureIgnoreCase.Compare(id1, id2);
		}

		public bool AreEqual(string id1, string id2)
		{
			if (id1 == null)
			{
				if (id2 == null)
				{
					return true;
				}
				return false;
			}
			// Needs to be invariant culture to work with GetHashCode
			return StringComparer.InvariantCultureIgnoreCase.Compare(id1, id2) == 0;
		}

		public int GetHashCode(string id)
		{
			if (id == null)
			{
				return 0;
			}
			return id.ToUpperInvariant().GetHashCode();
		}
	}
}
