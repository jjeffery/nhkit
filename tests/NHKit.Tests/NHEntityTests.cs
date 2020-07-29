#region License

// Copyright 2004-2014 John Jeffery
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
using System.Collections.Generic;
using Shouldly;
using Xunit;
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
namespace NHKit.Tests
{
	public class NHEntityTests
	{
        public class Int32 : CommonTestCases<int>
        {
            public Int32()
            {
                Id1 = 1;
                Id2 = 2;
            }
		}

        public class Int64 : CommonTestCases<long>
        {
            public Int64()
            {
                Id1 = 1L;
                Id2 = 2L;
            }
        }

        public class Guid : CommonTestCases<System.Guid>
        {
            public Guid()
            {
                Id1 = new System.Guid("005faa88-a529-4065-9d04-72a96316988b");
                Id2 = new System.Guid("64b9dd7a-f46e-448b-b6e5-ddd3f83ec508");
			}
		}

        public class String : CommonTestCases<string>
        {
            public String()
            {
                Id1 = "Alpha";
                Id2 = "Bravo";
            }
        }

        public class ValueType : CommonTestCases<EnumType>
        {
            public ValueType()
            {
                Id1 = EnumType.Enum1;
                Id2 = EnumType.Enum2;
            }
        }

        public class ClassType : CommonTestCases<ClassId>
        {
            public ClassType()
            {
                Id1 = new ClassId(20);
                Id2 = new ClassId(81);
            }
        }

        public abstract class CommonTestCases<TId> where TId : IComparable
        {
            public readonly TId DefaultId = default;
            public TId Id1;
            public TId Id2;

			[Fact]
			public void Persistent_entities_with_same_id_are_equal()
			{
				var id1A = new Entity<TId>(Id1);
				var id1B = new Entity<TId>(Id1);
				id1A.ShouldBe(id1B);
				(id1A != id1B).ShouldBeFalse();
				(id1A == id1B).ShouldBeTrue();
                id1A.Equals(id1B).ShouldBeTrue();
                id1B.Equals(id1A).ShouldBeTrue();
            }

            [Fact]
			public void Transient_entities_are_not_equal()
			{
				// default values mean entities are not equal
				var id0A = new Entity<TId>(DefaultId);
				var id0B = new Entity<TId>(DefaultId);
				id0A.ShouldNotBe(id0B);
				(id0A != id0B).ShouldBeTrue();
				(id0A == id0B).ShouldBeFalse();
                id0A.Equals(id0B).ShouldBeFalse();
                id0B.Equals(id0A).ShouldBeFalse();
            }

			[Fact]
			public void Persistent_entities_with_different_id_are_not_equal()
			{
				var id1 = new Entity<TId>(Id1);
				var id2 = new Entity<TId>(Id2);
				id1.ShouldNotBe(id2);
				(id1 != id2).ShouldBeTrue();
				(id1 == id2).ShouldBeFalse();
			}

			[Fact]
			public void Persistent_entities_are_comparable_by_id()
			{
				var id1 = new Entity<TId>(Id1);
				var id2 = new Entity<TId>(Id2);

				id1.CompareTo(id2).ShouldBeLessThan(0);
				id2.CompareTo(id1).ShouldBeGreaterThan(0);
				id1.CompareTo(id1).ShouldBe(0);
			}

			[Fact]
			public void Entities_do_not_change_hash_code()
			{
				var id1 = new Entity<TId>(DefaultId);
				var id2 = new Entity<TId>(Id1);

				id1.Equals(id2).ShouldBeFalse();

				// this will call GetHashCode()
				var set = new HashSet<Entity<TId>> { id1, id2 };

				// now make id1 persistent
				id1.SetId(Id1);

				set.ShouldContain(id1);
				set.ShouldContain(id2);
				id1.Equals(id2).ShouldBeFalse();
                (id1 == id2).ShouldBeFalse();
                (id2 == id1).ShouldBeFalse();
                (id1 != id2).ShouldBeTrue();
                (id2 != id1).ShouldBeTrue();
                id1.CompareTo(id2).ShouldNotBe(0);
                id2.CompareTo(id1).ShouldNotBe(0);
            }

            [Fact]
            public void Entities_use_reference_equality_if_they_change_id()
            {
                var id1a = new Entity<TId>(Id1);
                var id1b = new Entity<TId>(Id1);
                var id2 = new Entity<TId>(Id2);

                id1a.Equals(id1b).ShouldBeTrue();

                // this will call GetHashCode()
                var set = new HashSet<Entity<TId>> { id1a, id2 };

                set.ShouldContain(id1b);

                // now make id1a transient
                id1a.SetId(DefaultId);

                set.ShouldContain(id1a);
                set.ShouldNotContain(id1b);
                id1a.Equals(id1b).ShouldBeFalse();
            }

            [Fact]
            public void Different_classes_with_the_same_id_are_not_equal()
            {
                var entity1 = new Entity<TId>(Id1);
                var entity2 = new DifferentEntity<TId>(Id1);
                // ReSharper disable SuspiciousTypeConversion.Global
                entity1.Equals(entity2).ShouldBeFalse();
                entity2.Equals(entity1).ShouldBeFalse();
                // ReSharper restore SuspiciousTypeConversion.Global
            }

            [Fact]
            public void Entities_can_be_compared_with_null()
            {
                var id1 = new Entity<TId>(Id1);
                object nullObject = null;
                // ReSharper disable once ExpressionIsAlwaysNull
                id1.Equals(nullObject).ShouldBe(false);
                id1.Equals(null).ShouldBe(false);

                Entity<TId> nullEntity = null;

                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                (id1 == nullEntity).ShouldBeFalse();
                (nullEntity == id1).ShouldBeFalse();
                (id1 != nullEntity).ShouldBeTrue();
                (nullEntity != id1).ShouldBeTrue();
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                // ReSharper disable once ExpressionIsAlwaysNull
                id1.CompareTo(nullEntity).ShouldBe(1);
            }

            [Fact]
            public void Entities_are_compared_by_reference()
            {
                var id1 = new Entity<TId>(Id1);
                var id2 = id1;

                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                (id1 == id2).ShouldBeTrue();
                (id2 == id1).ShouldBeTrue();
                (id1 != id2).ShouldBeFalse();
                (id2 != id1).ShouldBeFalse();
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                // Now change Id. Because id1 and id2 are the same
                // object, they will still be equal.
                id1.SetId(Id2);

                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                (id1 == id2).ShouldBeTrue();
                (id2 == id1).ShouldBeTrue();
                (id1 != id2).ShouldBeFalse();
                (id2 != id1).ShouldBeFalse();
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            }
        }


		public class Entity<TId> : NHEntity<Entity<TId>, TId> where TId : IComparable
        {
            public TId Id { get; private set; }

            public Entity(TId id)
            {
                Id = id;
            }

            protected override TId GetId()
            {
                return Id;
            }

            public void SetId(TId id)
            {
                Id = id;
            }
        }

        public class DifferentEntity<TId> : NHEntity<DifferentEntity<TId>, TId> where TId : IComparable
        {
            public TId Id { get; private set; }

            public DifferentEntity(TId id)
            {
                Id = id;
            }

            protected override TId GetId()
            {
                return Id;
            }
        }


        public enum EnumType
		{
			Enum1 = 1,
			Enum2 = 2,
		}
	}
}
