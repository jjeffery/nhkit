using System;

namespace NHKit.Internal
{
    internal static class Tools
    {
        public static object[] CloneArray(object[] source)
        {
            if (source == null)
            {
                source = Array.Empty<object>();
            }

            var dest = new object[source.Length];
            Array.Copy(source, dest, source.Length);
            return dest;
        }

        public static bool AreObjectsEqual(object obj1, object obj2)
        {
            if (obj1 == null)
            {
                return obj2 == null;
            }

            return obj1.Equals(obj2);
        }
    }
}
