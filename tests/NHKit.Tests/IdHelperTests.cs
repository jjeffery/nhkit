#region License

// Copyright 2004-2020 John Jeffery
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
using NHKit.Internal;
using Shouldly;
using Xunit;

namespace NHKit.Tests
{
    public class IdHelperTests
    {
        [Fact]
        public void It_handles_Int16_identifiers()
        {
            var helper = new IdHelperForValueType<short>();

            helper.IsNull(0).ShouldBeFalse();
            helper.IsDefaultValue(0).ShouldBeTrue();
            helper.IsDefaultValue(1).ShouldBeFalse();
            helper.AreEqual(0, 0).ShouldBeTrue();
            helper.AreEqual(23, 23).ShouldBeTrue();
            helper.Compare(0, 1).ShouldBe(-1);
            helper.Compare(0, -1).ShouldBe(1);
            helper.Compare(0, 0).ShouldBe(0);
            helper.Compare(23, 23).ShouldBe(0);
            helper.GetHashCode(0).ShouldBe(((short)0).GetHashCode());
            helper.GetHashCode(23).ShouldBe(((short)23).GetHashCode());
        }

        [Fact]
        public void It_handles_Int64_identifiers()
        {
            var helper = new IdHelperForInt64();

            helper.IsNull(0).ShouldBeFalse();
            helper.IsDefaultValue(0).ShouldBeTrue();
            helper.IsDefaultValue(1).ShouldBeFalse();
            helper.AreEqual(0, 0).ShouldBeTrue();
            helper.AreEqual(23, 23).ShouldBeTrue();
            helper.Compare(0, 1).ShouldBe(-1);
            helper.Compare(0, -1).ShouldBe(1);
            helper.Compare(0, 0).ShouldBe(0);
            helper.Compare(23, 23).ShouldBe(0);
            helper.GetHashCode(0).ShouldBe(((long)0).GetHashCode());
            helper.GetHashCode(23).ShouldBe(((long)23).GetHashCode());
        }

        [Fact]
        public void It_handles_Int32_identifiers()
        {
            var helper = new IdHelperForInt32();

            helper.IsNull(0).ShouldBeFalse();
            helper.IsDefaultValue(0).ShouldBeTrue();
            helper.IsDefaultValue(1).ShouldBeFalse();
            helper.AreEqual(0, 0).ShouldBeTrue();
            helper.AreEqual(23, 23).ShouldBeTrue();
            helper.Compare(0, 1).ShouldBe(-1);
            helper.Compare(0, -1).ShouldBe(1);
            helper.Compare(0, 0).ShouldBe(0);
            helper.Compare(23, 23).ShouldBe(0);
            helper.GetHashCode(0).ShouldBe(0.GetHashCode());
            helper.GetHashCode(23).ShouldBe(23.GetHashCode());
        }

        [Fact]
        public void It_handles_Guid_identifiers()
        {
            var helper = new IdHelperForValueType<Guid>();

            var guid1 = new Guid("005faa88-a529-4065-9d04-72a96316988b");
            var guid2 = new Guid("64b9dd7a-f46e-448b-b6e5-ddd3f83ec508");


            helper.IsNull(Guid.Empty).ShouldBeFalse();
            helper.IsDefaultValue(Guid.Empty).ShouldBeTrue();
            helper.IsDefaultValue(guid1).ShouldBeFalse();
            helper.AreEqual(Guid.Empty, Guid.Empty).ShouldBeTrue();
            helper.AreEqual(guid1, guid1).ShouldBeTrue();
            helper.Compare(Guid.Empty, guid1).ShouldBe(-1);
            helper.Compare(guid2, Guid.Empty).ShouldBe(1);
            helper.Compare(Guid.Empty, Guid.Empty).ShouldBe(0);
            helper.Compare(guid2, guid2).ShouldBe(0);
            helper.GetHashCode(Guid.Empty).ShouldBe(Guid.Empty.GetHashCode());
            helper.GetHashCode(guid1).ShouldBe(guid1.GetHashCode());
        }

        [Fact]
        public void It_handles_String_identifiers()
        {
            var helper = new IdHelperForString();
            string defaultValue = default;

            // ReSharper disable StringLiteralTypo
            // ReSharper disable ExpressionIsAlwaysNull
            helper.IsNull(defaultValue).ShouldBeTrue();
            helper.IsDefaultValue(defaultValue).ShouldBeTrue();
            helper.IsDefaultValue("XX").ShouldBeFalse();
            helper.AreEqual(defaultValue, defaultValue).ShouldBeTrue();
            helper.AreEqual(defaultValue, "X").ShouldBeFalse();
            helper.AreEqual("X", defaultValue).ShouldBeFalse();
            helper.AreEqual("ABCD", "abcd").ShouldBeTrue();
            helper.AreEqual("ABCDE", "ABCDE").ShouldBeTrue();
            helper.Compare(defaultValue, "AAA").ShouldBe(-1);
            helper.Compare("BBB", defaultValue).ShouldBe(1);
            helper.Compare(defaultValue, defaultValue).ShouldBe(0);
            helper.Compare("X", "x").ShouldBe(0);
            helper.GetHashCode(string.Empty).ShouldBe(string.Empty.GetHashCode());
            helper.GetHashCode(defaultValue).ShouldBe(0);
            helper.GetHashCode("XXX").ShouldBe("XXX".GetHashCode());
            helper.GetHashCode("XXX").ShouldBe(helper.GetHashCode("xxx"));
            // ReSharper restore ExpressionIsAlwaysNull
            // ReSharper restore StringLiteralTypo
        }

        [Fact]
        public void It_handles_class_identifiers()
        {
            var helper = new IdHelperForClassType<ClassId>();
            ClassId idDefault = default;
            var id1 = new ClassId(1);
            var id2 = new ClassId(2);

            // ReSharper disable ExpressionIsAlwaysNull
            helper.IsNull(idDefault).ShouldBeTrue();
            helper.IsDefaultValue(idDefault).ShouldBeTrue();
            helper.IsDefaultValue(id1).ShouldBeFalse();
            helper.AreEqual(idDefault, idDefault).ShouldBeTrue();
            helper.AreEqual(id2, id2).ShouldBeTrue();
            helper.AreEqual(idDefault, id1).ShouldBeFalse();
            helper.Compare(idDefault, id1).ShouldBe(-1);
            helper.Compare(id1, idDefault).ShouldBe(1);
            helper.Compare(idDefault, idDefault).ShouldBe(0);
            helper.Compare(id1, id2).ShouldBe(-1);
            helper.Compare(id2, id1).ShouldBe(1);
            helper.Compare(id2, id2).ShouldBe(0);
            helper.GetHashCode(idDefault).ShouldBe(0);
            helper.GetHashCode(id1).ShouldBe(id1.GetHashCode());
            helper.GetHashCode(id2).ShouldBe(id2.GetHashCode());
            // ReSharper restore ExpressionIsAlwaysNull
        }
    }
}
