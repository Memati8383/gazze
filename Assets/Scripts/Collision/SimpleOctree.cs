using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Gazze.Collision
{
    /// <summary>
    /// Basit ve optimize edilmiş 3D Octree yapısı.
    /// Uzay bölme algoritması kullanarak çarpışma kontrollerini hızlandırır.
    /// </summary>
    public class SimpleOctree
    {
        /// <summary>Bu dugumun merkez noktasi.</summary>
        private float3 center;
        /// <summary>Bu dugumun kupe benzeri kapsama boyutu.</summary>
        private float size;
        /// <summary>Kalan bolunme derinligi.</summary>
        private int maxDepth;
        private List<CollisionEntity> objects;
        private SimpleOctree[] children;

        /// <summary>
        /// Octree icinde saklanan carpism a aday nesne verisi.
        /// </summary>
        public struct CollisionEntity
        {
            public int id;
            public float3 position;
            public float3 extents;
            public float radius;
            public CollisionType type;
        }

        /// <summary>Desteklenen carpism a hacim tipleri.</summary>
        public enum CollisionType { AABB, OBB, Sphere }

        public SimpleOctree(float3 center, float size, int maxDepth)
        {
            this.center = center;
            this.size = size;
            this.maxDepth = maxDepth;
            this.objects = new List<CollisionEntity>();
        }

        public void Insert(CollisionEntity entity)
        {
            if (maxDepth <= 0 || size < 1f)
            {
                objects.Add(entity);
                return;
            }

            if (children == null) Split();

            foreach (var child in children)
            {
                if (child.Contains(entity))
                {
                    child.Insert(entity);
                    return;
                }
            }

            objects.Add(entity);
        }

        private void Split()
        {
            children = new SimpleOctree[8];
            float childSize = size / 2f;
            float offset = childSize / 2f;

            for (int i = 0; i < 8; i++)
            {
                float3 childCenter = center;
                childCenter.x += ((i & 1) == 0 ? -1 : 1) * offset;
                childCenter.y += ((i & 2) == 0 ? -1 : 1) * offset;
                childCenter.z += ((i & 4) == 0 ? -1 : 1) * offset;
                children[i] = new SimpleOctree(childCenter, childSize, maxDepth - 1);
            }
        }

        private bool Contains(CollisionEntity entity)
        {
            float3 min = center - (size / 2f);
            float3 max = center + (size / 2f);
            float3 eMin = entity.position - entity.extents;
            float3 eMax = entity.position + entity.extents;

            return (eMin.x >= min.x && eMax.x <= max.x) &&
                   (eMin.y >= min.y && eMax.y <= max.y) &&
                   (eMin.z >= min.z && eMax.z <= max.z);
        }

        public void GetPotentialCollisions(CollisionEntity entity, List<CollisionEntity> results)
        {
            results.AddRange(objects);

            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child.Overlaps(entity))
                    {
                        child.GetPotentialCollisions(entity, results);
                    }
                }
            }
        }

        private bool Overlaps(CollisionEntity entity)
        {
            float3 min = center - (size / 2f);
            float3 max = center + (size / 2f);
            float3 eMin = entity.position - entity.extents;
            float3 eMax = entity.position + entity.extents;

            return (min.x <= eMax.x && max.x >= eMin.x) &&
                   (min.y <= eMax.y && max.y >= eMin.y) &&
                   (min.z <= eMax.z && max.z >= eMin.z);
        }
    }
}
