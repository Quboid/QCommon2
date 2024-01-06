﻿using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Entities;
using System.Reflection;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QReferenceBufferType
    {
        /// <summary>
        /// The type of the buffer component
        /// </summary>
        public Type tParentComponent;
        /// <summary>
        /// The FieldInfo of the buffer component's Entity field
        /// </summary>
        public FieldInfo m_FieldInfo;
    }

    public struct QObject : IDisposable
    {
        internal readonly EntityManager EM => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        public QEntity m_Parent;
        internal QLookup m_Lookup;
        internal NativeArray<QEntity> m_Children;

        internal QObject(Entity e, SystemBase system)
        {
            if (e == Entity.Null) throw new ArgumentNullException("Creating QObject with null entity");

            m_Lookup = QLookup.Get(system);
            m_Entity = e;
            m_Parent = new(e, system);
            var subEntities = GetSubEntities(e);

            if (subEntities.Count > 0)
            {
                m_Children = new(subEntities.Count, Allocator.Persistent);
                for (int i = 0; i < subEntities.Count; i++)
                {
                    if (subEntities[i] == Entity.Null) throw new NullReferenceException($"Creating child for {e.D()} with null entity");
                    m_Children[i] = new(subEntities[i], system);
                }
            }
            else
            {
                m_Children = new();
            }
        }

        public void Dispose()
        {
            m_Children.Dispose();
        }

        #region Transforming
        public void Transform(float3 position, float angle)
        {
            MoveTo(position);
            RotateTo(angle);
        }

        public void MoveBy(float3 delta)
        {
            m_Parent.MoveBy(delta);
            
            for (int i = 0; i < m_Children.Length; i++)
            {
                m_Children[i].MoveBy(delta);
            }
        }

        public void MoveTo(float3 newPosition)
        {
            MoveBy(newPosition - m_Parent.Position);
        }

        public void RotateTo(float newAngle)
        {
            float delta = newAngle - m_Parent.Angle;
            float3 origin = m_Parent.Position;
            GetMatrix(delta, origin, out Matrix4x4 matrix);

            m_Parent.RotateTo(newAngle, ref matrix, origin);

            if (m_Children.Length > 0)
            {
                for (int i = 0; i < m_Children.Length; i++)
                {
                    m_Children[i].RotateBy(delta, ref matrix, origin);
                }
            }
        }

        private readonly void GetMatrix(float delta, float3 origin, out Matrix4x4 matrix)
        {
            matrix = default;
            matrix.SetTRS(origin, Quaternion.Euler(0f, delta, 0f), Vector3.one);
        }
        #endregion

        #region Load children
        private static List<Entity> GetSubEntities(Entity e)
        {
            List<Entity> entities = IterateSubEntities(e, e, 0);

            return entities;
        }

        private static List<Entity> IterateSubEntities(Entity top, Entity e, int depth)
        {
            if (depth > 3) throw new Exception($"Moveable.IterateSubEntities depth ({depth}) too deep for {top.D()}/{e.D()}");
            depth++;

            List<Entity> entities = new();

            foreach (QReferenceBufferType type in GetReferenceTypes())
            {
                if (QByType.HasBuffer(type.tParentComponent, e))
                {
                    QByType.GetRefBufferComponents(type.tParentComponent, e, out List<IBufferElementData> buffer, true);
                    foreach (IBufferElementData element in buffer)
                    {
                        Entity sub = (Entity)type.m_FieldInfo.GetValue(element);
                        if (!entities.Contains(sub))
                        {
                            entities.Add(sub);
                            entities.AddRange(IterateSubEntities(top, sub, depth));
                        }
                        else
                        {
                            QLog.Debug($"Duplicate subEntity found: {sub.D()}");
                        }
                    }
                }
            }

            return entities;
        }

        private static List<QReferenceBufferType> GetReferenceTypes()
        {
            return new()
            {
                new() { tParentComponent = typeof(Game.Areas.SubArea),      m_FieldInfo = GetEntityReferenceField(typeof(Game.Areas.SubArea)) },
                new() { tParentComponent = typeof(Game.Net.SubNet),         m_FieldInfo = GetEntityReferenceField(typeof(Game.Net.SubNet)) },
                new() { tParentComponent = typeof(Game.Net.SubLane),        m_FieldInfo = GetEntityReferenceField(typeof(Game.Net.SubLane)) },
                new() { tParentComponent = typeof(Game.Objects.SubObject),  m_FieldInfo = GetEntityReferenceField(typeof(Game.Objects.SubObject)) },
            };
        }
        #endregion

        #region Low level entity access
        /// <summary>
        /// Get the field in an IBufferElementData reference buffer component that holds the actual entity reference
        /// For example for Game.Areas.SubArea.m_area, it returns m_area
        /// </summary>
        /// <param name="type">The IBufferElementData struct type to search</param>
        /// <param name="index">How many entity fields to skip over</param>
        /// <returns>FieldInfo of this field</returns>
        /// <exception cref="Exception">If no such field is found</exception>
        public static FieldInfo GetEntityReferenceField(Type type, int index = 0)
        {
            int c = 0;
            FieldInfo field = null;
            foreach (FieldInfo f in type.GetFields())
            {
                if (f.FieldType == typeof(Entity))
                {
                    if (c == index)
                    {
                        field = f;
                        break;
                    }
                    else
                    {
                        c++;
                    }
                }
            }
            if (field == null) throw new Exception($"Entity field not found for type {type}");
            return field;
        }
        #endregion

        public readonly override string ToString()
        {
            return "Parent:" + m_Entity.D() + ", children: " + (m_Children.IsCreated ? m_Children.Length : "Not Created!");
        }

        internal void DebugDumpAll()
        {
            StringBuilder sb = new("Parent:" + m_Entity.D() + ", children: " + (m_Children.IsCreated ? m_Children.Length : "Not Created!"));

            if (m_Children.IsCreated)
            {
                for (int i = 0; i < m_Children.Length; i++)
                {
                    sb.AppendFormat("\n    {0}: \"{1}\"",
                        m_Children[i].m_Entity.D(),
                        QCommon.GetPrefabName(EM, m_Children[i].m_Entity)
                    );
                }
            }

            QLog.Debug(sb.ToString());
        }
    }
}
