﻿using Colossal.Entities;
using Game.Prefabs;
using Game.Tools;
using System.Diagnostics;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib
{
    public class QCommon
    {
        public static long ElapsedMilliseconds(long startTime)
        {
            long elapsed = math.abs(Stopwatch.GetTimestamp() - startTime);

            return elapsed / (Stopwatch.Frequency / 1000);
        }

        public static ToolBaseSystem ActiveTool { get => QCommon.ToolSystem.activeTool; }

        public static ToolSystem ToolSystem
        {
            get
            {
                _toolSystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ToolSystem>();
                return _toolSystem;
            }
        }
        private static ToolSystem _toolSystem = null;

        public static DefaultToolSystem DefaultTool
        {
            get
            {
                _defaultTool ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DefaultToolSystem>();
                return _defaultTool;
            }
        }
        private static DefaultToolSystem _defaultTool = null;

        public static PrefabSystem PrefabSystem
        {
            get
            {
                _prefabSystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PrefabSystem>();
                return _prefabSystem;
            }
        }
        private static PrefabSystem _prefabSystem = null;

        public static PrefabBase GetPrefab(EntityManager Manager, Entity e)
        {
            if (Manager.TryGetComponent(e, out PrefabRef prefab))
            {
                return PrefabSystem.GetPrefab<PrefabBase>(prefab);
            }

            return null;
        }

        public static string GetPrefabName(EntityManager Manager, Entity e)
        {
            if (e.Equals(Entity.Null)) return "ENTITY.NULL0";
            if (!Manager.HasComponent<PrefabRef>(e)) return "ENTITY.NULL1";
            PrefabRef prefabRef = Manager.GetComponentData<PrefabRef>(e);
            if (prefabRef.m_Prefab.Equals(Entity.Null)) return "ENTITY.NULL2";

            string name;
            PrefabBase prefabBase = PrefabSystem.GetPrefab<PrefabBase>(prefabRef);
            if (prefabBase != null)
            {
                name = prefabBase.prefab ? prefabBase.prefab.name : prefabBase.name;
            }
            else
            {
                name = Manager.GetName(e);
            }

            return name;
        }

        public static Allocator GetAllocator(object foo)
        {
            FieldInfo field = foo.GetType().GetField("m_AllocatorLabel", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) return Allocator.Invalid;
            return (Allocator)field.GetValue(foo);
        }
    }
}
