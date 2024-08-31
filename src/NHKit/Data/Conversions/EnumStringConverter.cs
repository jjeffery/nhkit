using NHKit.Data.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace NHKit.Data.Conversions
{
    public static class EnumStringConverter<T>
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly int MaxLength;
        public static readonly ReadOnlyCollection<string> StringValues;
        private static readonly IDictionary<T, string> EnumToString;
        private static readonly IDictionary<string, T> StringToEnum;
        private static readonly T NullValue;
        private static readonly bool HasNullValue;
        private static readonly Type EnumType;
        // ReSharper restore StaticMemberInGenericType

        public static string ConvertToString(T enumValue)
        {
            if (HasNullValue && NullValue.Equals(enumValue))
                return null;
            try
            {
                return EnumToString[enumValue];
            }
            catch (KeyNotFoundException)
            {
                string message = $"Unknown enum value for type {EnumType}: {enumValue}";
                throw new ApplicationException(message);
            }
        }

        public static T ConvertToEnum(string stringValue)
        {
            if (stringValue == null)
            {
                if (HasNullValue)
                {
                    return NullValue;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                try
                {
                    return StringToEnum[stringValue];
                }
                catch (KeyNotFoundException)
                {
                    // This condition can happen if a web client sends an invalid value for an enum value.
                    // For this reason, raise a BadRequestException, which will result in a 400.
                    // Don't include .NET type information.
                    throw new ArgumentException($"Invalid value for enumeration {typeof(T).FullName}: \"{stringValue}\"");
                }
            }
        }

        private class EqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return CaseInsensitiveComparer.DefaultInvariant.Compare(x, y) == 0;
            }

            public int GetHashCode(string s)
            {
                return s.ToUpperInvariant().GetHashCode();
            }
        }

        static EnumStringConverter()
        {
            EnumToString = new Dictionary<T, string>();
            StringToEnum = new Dictionary<string, T>(new EqualityComparer());
            var stringValues = new List<string>();

            EnumType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (!EnumType.IsEnum)
            {
                string message = "Type is not an enum: " + EnumType;
                throw new ApplicationException(message);
            }

            foreach (T enumValue in Enum.GetValues(EnumType))
            {
                string stringValue = FindStringValueForEnum(enumValue);

                if (stringValue == null)
                {
                    if (HasNullValue)
                    {
                        string message = $"Two {EnumType} fields have null StringValue: {NullValue}, {enumValue}";
                        throw new ApplicationException(message);
                    }
                    else
                    {
                        HasNullValue = true;
                        NullValue = enumValue;
                    }
                }
                else
                {
                    if (stringValue.Length > MaxLength)
                    {
                        MaxLength = stringValue.Length;
                    }
                    try
                    {
                        StringToEnum.Add(stringValue, enumValue);
                    }
                    catch (ArgumentException)
                    {
                        string message = $"Two {EnumType} fields have the same StringValue: {StringToEnum[stringValue]}, {enumValue}";
                        throw new ApplicationException(message);
                    }

                    EnumToString.Add(enumValue, stringValue);
                    stringValues.Add(stringValue);
                }
            }

            StringValues = new ReadOnlyCollection<string>(stringValues);
        }

        private static string FindStringValueForEnum(T enumValue)
        {
            string fieldName = enumValue.ToString();
            FieldInfo fieldInfo = EnumType.GetField(fieldName!);

            var stringValueAttribute = fieldInfo!.GetCustomAttribute<StringValueAttribute>();
            if (stringValueAttribute != null) {
                // Return whether null or not
                return stringValueAttribute.Value;
            }
            // If we can't find the StringValue attribute, look for a EnumMemberAttribute, which is defined in System.Runtime.Serialization.
            // This has the advantage of being compatible with JSON.NET JSON serialization for enums.
            var enumMemberAttribute =  fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMemberAttribute?.Value != null) {
                return enumMemberAttribute.Value;
            }
            return fieldInfo.Name;
        }
    }
}
