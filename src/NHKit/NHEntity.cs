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
using System.Reflection;
using NHKit.Internal;

namespace NHKit
{
    /// <summary>
    /// Base class for a class that has NHibernate semantics for <see cref="Equals(object)"/> and <see cref="CompareTo"/>
    /// </summary>
    /// <typeparam name="TEntity">
    /// Type of the entity, used for implementing <see cref="IEquatable{TEntity}"/> and
    /// and <see cref="IComparable{TEntity}"/>.
    /// </typeparam>
    /// <typeparam name="TId">
    /// Type of the identifier that uniquely identifies the entity. This is usually one of
    /// <see cref="Int32"/>, <see cref="Int64"/>, <see cref="String"/> or perhaps <see cref="Guid"/>.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// The main job of this base class is to provide the required equality semantics
    /// as required by NHibernate.
    /// </para>
    /// </remarks>
    public abstract class NHEntity<TEntity, TId> : IEquatable<TEntity>, IComparable<TEntity>
        where TEntity : NHEntity<TEntity, TId>
        where TId : IComparable
    {
#pragma warning disable 649
        // This field gets set via reflection.
        // ReSharper disable once UnassignedReadonlyField
        protected static readonly IIdHelper<TId> IdHelper;
#pragma warning restore 649

        /// <summary>
        /// Return the entity unique ID.
        /// </summary>
        /// <remarks>
        /// <returns>
        /// Returns the entity's unique ID, or the default value if the entity is transient.
        /// (I.e. has not been persisted to the database).
        /// </returns>
        /// <para>
        /// The implementing method must guarantee that, once it returns a non-default
        /// value, that value will always be returned in subsequent calls.
        /// </para>
        /// </remarks>
        protected abstract TId GetId();

        static NHEntity()
        {
            if (IdHelper == null)
            {
                var type = typeof(NHEntity<TEntity, TId>);
                var field = type.GetField(nameof(IdHelper), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                // field will not be null, so disable warnings about a possible NRE
                // ReSharper disable PossibleNullReferenceException

                var idType = typeof(TId);
                if (idType == typeof(string))
                {
                    // Special case for string, as identifiers are compared
                    // ordinal case-insensitive.
                    var helper = new IdHelperForString();
                    field.SetValue(null, helper);
                }
                else if (idType == typeof(int))
                {
                    // Special case for int as many identifiers are this type.
                    // This class avoids boxing altogether.
                    var helper = new IdHelperForInt32();
                    field.SetValue(null, helper);
                }
                else if (idType == typeof(long))
                {
                    // Special case for long as some identifiers are this type.
                    // This class avoids boxing altogether.
                    var helper = new IdHelperForInt64();
                    field.SetValue(null, helper);
                }
                else if (idType.IsValueType)
                {
                    // Value types get an ID helper that does not involve boxing.
                    var helperType = typeof(IdHelperForValueType<>);
                    var genericType = helperType.MakeGenericType(new[] { typeof(TId) });
                    var helper = Activator.CreateInstance(genericType, null);
                    field.SetValue(null, helper);
                }
                else
                {
                    // ID helper type for classes that can have null value
                    var helperType = typeof(IdHelperForClassType<>);
                    var genericType = helperType.MakeGenericType(new[] { typeof(TId) });
                    var helper = Activator.CreateInstance(genericType, null);
                    field.SetValue(null, helper);
                }
                // ReSharper restore PossibleNullReferenceException
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as TEntity;
            return Equals(other);
        }

        // Used to ensure that the value of Id does not change after the first call to GetHashCode(),
        // even if the entity Id changes.
        private int? _hashCode;

        // Set to true if the entity is transient the first time that GetHashCode() is called.
        private bool _isTransient;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Resharper does not like us referencing a non-readonly variable inside
            // GetHashCode(). This is quite clever, but in this case we are justified,
            // because once set for the first time, the variables will not change.
            // ReSharper disable NonReadonlyFieldInGetHashCode
            if (_hashCode == null)
            {
                // Obtain the entity ID. Note that it is assumed that once GetId returns a 
                // non-default value, that value will not change in subsequent calls to GetId.
                // So if, for example, GetId() returns 1 and then returns 2, this code will break.
                var id = GetId();

                // If the Id is the default value (0 in most cases), this means that the entity has not been
                // persisted. If this is the case, default to reference equality.
                _isTransient = IdHelper.IsDefaultValue(id);

                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                _hashCode = _isTransient ? base.GetHashCode() : IdHelper.GetHashCode(id);
            }

            return _hashCode.Value;
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        /// <summary>
        /// Test for null without triggering a fetch
        /// </summary>
        /// <remarks>
        /// Remembering that entity is probably an NHibernate object,
        /// convert to object and test for null. If entity is an
        /// NHibernate proxy, we want to be careful not to trigger
        /// an unnecessary fetch from the database.
        /// </remarks>
        private static bool IsNull(object entity)
        {
            return entity == null;
        }

        /// <inheritdoc/>
        public virtual bool Equals(TEntity other)
        {
            if (IsNull(other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (_isTransient)
            {
                // The entity was transient when GetHashCode was first called,
                // so it needs to stick to reference equality.
                return false;
            }

            var thisId = GetId();
            // Because IsNull returned false, we know that other is not null.
            // ReSharper disable once PossibleNullReferenceException
            var otherId = other.GetId();

            if (IdHelper.IsDefaultValue(thisId) || IdHelper.IsDefaultValue(otherId))
            {
                // If two different objects have the same default ID value, they
                // are considered different.
                return false;
            }

            return IdHelper.AreEqual(thisId, otherId);
        }

        public static bool operator ==(NHEntity<TEntity, TId> entity1, NHEntity<TEntity, TId> entity2)
        {
            return AreEqual(entity1, entity2);
        }

        public static bool operator !=(NHEntity<TEntity, TId> entity1, NHEntity<TEntity, TId> entity2)
        {
            return !AreEqual(entity1, entity2);
        }

        /// <inheritdoc/>
        public virtual int CompareTo(TEntity other)
        {
            if (IsNull(other))
            {
                return 1;
            }

            var thisId = _isTransient ? default : GetId();
            var otherId = other._isTransient ? default : other.GetId();

            return IdHelper.Compare(thisId, otherId);
        }

        private static bool AreEqual(NHEntity<TEntity, TId> entity1, NHEntity<TEntity, TId> entity2)
        {
            // Perform 'safe' tests for null first. Safe means that an NHibernate
            // proxy will not cause a fetch from the database. We want to get null
            // tests out of the way first.
            if (IsNull(entity1))
            {
                // null == null
                return IsNull(entity2);
            }

            if (IsNull(entity2))
            {
                return false;
            }

            if (ReferenceEquals(entity1, entity2))
            {
                return true;
            }

            // Resharper thinks that there is a possible null exception, because
            // it does not know about IsNull()
            // ReSharper disable PossibleNullReferenceException

            if (entity1._isTransient || entity2._isTransient)
            {
                // One or both of the entities were transient the first time that
                // GetHashCode() was called, so use reference equality only.
                return false;
            }

            var id1 = entity1.GetId();
            var id2 = entity2.GetId();
            // ReSharper restore PossibleNullReferenceException

            if (IdHelper.IsDefaultValue(id1) || IdHelper.IsDefaultValue(id2))
            {
                // If two different objects have the same default ID value, they
                // are considered non-equal.
                return false;
            }

            return IdHelper.AreEqual(id1, id2);
        }
    }
}
