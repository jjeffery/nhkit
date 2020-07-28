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

namespace NHKit.Internal
{
	/// <summary>
	/// Interface that provides helper methods for NHibernate identity values.
	/// </summary>
	/// <typeparam name="TId">
	/// Type of entity identifer, usually <c>int</c> or <c>string</c>.
	/// </typeparam>
	public interface IIdHelper<in TId>
	{
		bool IsDefaultValue(TId id);
		bool IsNull(TId id);
		int Compare(TId id1, TId id2);
		bool AreEqual(TId id1, TId id2);
		int GetHashCode(TId id);
	}
}
