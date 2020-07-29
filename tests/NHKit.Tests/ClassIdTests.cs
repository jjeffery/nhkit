using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace NHKit.Tests
{
    public class ClassIdTests
    {
        private readonly ClassId _id0 = new ClassId(0);
        private readonly ClassId _id1 = new ClassId(1);
        private readonly ClassId _id2 = new ClassId(2);

        [Fact]
        public void It_is_comparable()
        {
            _id0.CompareTo(_id1).ShouldBeLessThan(0);
            _id2.CompareTo(_id1).ShouldBeGreaterThan(0);
            _id1.CompareTo(new ClassId(1)).ShouldBe(0);
            _id0.CompareTo(null).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void It_is_equatable()
        {
            // ReSharper disable EqualExpressionComparison
            _id0.Equals(_id0).ShouldBeTrue();
            _id0.Equals(new ClassId(0)).ShouldBeTrue();
            _id2.Equals(_id1).ShouldBeFalse();
            _id1.Equals(null).ShouldBeFalse();
            // ReSharper restore EqualExpressionComparison
        }
    }
}
