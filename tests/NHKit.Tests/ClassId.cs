using System;

namespace NHKit.Tests
{
    // An immutable class that can be used as a unique identifier.
    public class ClassId : IComparable
    {
        public readonly int Id;

        public ClassId(int id)
        {
            Id = id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is ClassId other)
            {
                return Id.CompareTo(other.Id);
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClassId other)
            {
                return other.Id == Id;
            }

            return false;
        }
    }
}
