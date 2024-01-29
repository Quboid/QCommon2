﻿using Unity.Entities;

namespace QCommonLib.QAccessor
{
    public interface IQLookupContainer
    {
        void Init(SystemBase system);
        void Update(SystemBase system);
    }

    public struct QLookup : IQLookupContainer
    {
        public static QLookup m_Lookup;
        private static bool _Initialized;

        public static void Reset()
        {
            _Initialized = false;
        }

        public static ref QLookup Get(SystemBase system)
        {
            if (!_Initialized)
            {
                m_Lookup = new();
                m_Lookup.Init(system);
                _Initialized = true;
            }

            return ref m_Lookup;
        }


        internal BufferLookup<Game.Areas.Node> gaNode;
        internal BufferLookup<Game.Net.ConnectedEdge> gnConnectedEdge;
        internal ComponentLookup<Game.Areas.Geometry> gaGeometry;
        internal ComponentLookup<Game.Common.Updated> gcUpdated;
        internal ComponentLookup<Game.Net.Curve> gnCurve;
        internal ComponentLookup<Game.Net.Edge> gnEdge;
        internal ComponentLookup<Game.Net.EdgeGeometry> gnEdgeGeometry;
        internal ComponentLookup<Game.Net.EndNodeGeometry> gnEndNodeGeometry;
        internal ComponentLookup<Game.Net.Node> gnNode;
        internal ComponentLookup<Game.Net.NodeGeometry> gnNodeGeometry;
        internal ComponentLookup<Game.Net.StartNodeGeometry> gnStartNodeGeometry;
        internal ComponentLookup<Game.Objects.Transform> goTransform;
        internal ComponentLookup<Game.Prefabs.ObjectGeometryData> gpObjectGeometryData;
        internal ComponentLookup<Game.Rendering.CullingInfo> grCullingInfo;

        internal int test;

        public void Init(SystemBase system)
        {
            gaNode = system.GetBufferLookup<Game.Areas.Node>();
            gnConnectedEdge = system.GetBufferLookup<Game.Net.ConnectedEdge>();
            gaGeometry = system.GetComponentLookup<Game.Areas.Geometry>();
            gcUpdated = system.GetComponentLookup<Game.Common.Updated>();
            gnCurve = system.GetComponentLookup<Game.Net.Curve>();
            gnEdge = system.GetComponentLookup<Game.Net.Edge>();
            gnEdgeGeometry = system.GetComponentLookup<Game.Net.EdgeGeometry>();
            gnEndNodeGeometry = system.GetComponentLookup<Game.Net.EndNodeGeometry>();
            gnNode = system.GetComponentLookup<Game.Net.Node>();
            gnNodeGeometry = system.GetComponentLookup<Game.Net.NodeGeometry>();
            gnStartNodeGeometry = system.GetComponentLookup<Game.Net.StartNodeGeometry>();
            goTransform = system.GetComponentLookup<Game.Objects.Transform>();
            gpObjectGeometryData = system.GetComponentLookup<Game.Prefabs.ObjectGeometryData>();
            grCullingInfo = system.GetComponentLookup<Game.Rendering.CullingInfo>();
        }

        public void Update(SystemBase system)
        {
            gaNode.Update(system);
            gnConnectedEdge.Update(system);
            gaGeometry.Update(system);
            gcUpdated.Update(system);
            gnCurve.Update(system);
            gnEdge.Update(system);
            gnEdgeGeometry.Update(system);
            gnEndNodeGeometry.Update(system);
            gnNode.Update(system);
            gnNodeGeometry.Update(system);
            gnStartNodeGeometry.Update(system);
            goTransform.Update(system);
            gpObjectGeometryData.Update(system);
            grCullingInfo.Update(system);
        }
    }
}
