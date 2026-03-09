using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gazze.Collision
{
    public class HighPerformanceCollisionManager : MonoBehaviour
    {
        public static HighPerformanceCollisionManager Instance;

        public struct EntityData
        {
            public int id;
            public float3 position;
            public float3 extents;
            public quaternion rotation;
            public float radius;
            public CollisionType type;
            public int layer;
        }

        public enum CollisionType { AABB, OBB, Sphere }

        private List<EntityData> dynamicEntities = new List<EntityData>();
        private NativeList<EntityData> nativeEntities;
        private NativeList<int2> collisionResults;
        
        // Spatial Hashing
        private const float CellSize = 10f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            nativeEntities = new NativeList<EntityData>(2048, Allocator.Persistent);
            collisionResults = new NativeList<int2>(2048, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (nativeEntities.IsCreated) nativeEntities.Dispose();
            if (collisionResults.IsCreated) collisionResults.Dispose();
        }

        public void RegisterEntity(EntityData entity)
        {
            dynamicEntities.Add(entity);
        }

        public void ClearEntities()
        {
            dynamicEntities.Clear();
        }

        private void LateUpdate()
        {
            if (dynamicEntities.Count < 2) 
            {
                // Note: If only Player is registered, we don't need to check.
                // But we should still clear for next frame.
                dynamicEntities.Clear();
                return;
            }

            nativeEntities.Clear();
            for (int i = 0; i < dynamicEntities.Count; i++) nativeEntities.Add(dynamicEntities[i]);
            dynamicEntities.Clear();

            collisionResults.Clear();

            // 1. Build Spatial Hash
            NativeParallelMultiHashMap<int3, int> spatialHash = new NativeParallelMultiHashMap<int3, int>(nativeEntities.Length, Allocator.TempJob);
            
            var hashJob = new BuildSpatialHashJob
            {
                entities = nativeEntities.AsArray(),
                cellSize = CellSize,
                spatialHash = spatialHash.AsParallelWriter()
            };
            JobHandle hashHandle = hashJob.Schedule(nativeEntities.Length, 64);

            // 2. Collision Check Job
            var collisionJob = new CollisionCheckJob
            {
                entities = nativeEntities.AsArray(),
                spatialHash = spatialHash,
                cellSize = CellSize,
                results = collisionResults.AsParallelWriter()
            };
            JobHandle collisionHandle = collisionJob.Schedule(nativeEntities.Length, 32, hashHandle);
            collisionHandle.Complete();

            // 3. Process Results
            ProcessCollisions();

            spatialHash.Dispose();
        }

        private void ProcessCollisions()
        {
            for (int i = 0; i < collisionResults.Length; i++)
            {
                int2 pair = collisionResults[i];
                // Player is always ID 0 in our logic
                if (pair.x == 0 || pair.y == 0)
                {
                        if (PlayerController.Instance != null)
                        {
                            // Ölüm yerine hasar sistemini tetikleyerek can sistemine entegre ediyoruz
                            PlayerController.Instance.TakeDamage(25f);
                        }
                }
            }
        }

        [BurstCompile]
        struct BuildSpatialHashJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<EntityData> entities;
            public float cellSize;
            public NativeParallelMultiHashMap<int3, int>.ParallelWriter spatialHash;

            public void Execute(int index)
            {
                int3 gridPos = (int3)math.floor(entities[index].position / cellSize);
                spatialHash.Add(gridPos, index);
            }
        }

        [BurstCompile]
        struct CollisionCheckJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<EntityData> entities;
            [ReadOnly] public NativeParallelMultiHashMap<int3, int> spatialHash;
            public float cellSize;
            public NativeList<int2>.ParallelWriter results;

            public void Execute(int index)
            {
                EntityData entityA = entities[index];
                int3 gridPos = (int3)math.floor(entityA.position / cellSize);

                // Check 3x3x3 neighboring cells
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            int3 neighborPos = gridPos + new int3(x, y, z);
                            if (spatialHash.TryGetFirstValue(neighborPos, out int otherIndex, out var it))
                            {
                                do
                                {
                                    if (index >= otherIndex) continue; // Avoid duplicate checks

                                    EntityData entityB = entities[otherIndex];
                                    
                                    // Layer filtering (Player vs TrafficCar)
                                    if (entityA.layer == entityB.layer) continue;

                                    if (CheckCollision(entityA, entityB))
                                    {
                                        results.AddNoResize(new int2(entityA.id, entityB.id));
                                    }
                                } while (spatialHash.TryGetNextValue(out otherIndex, ref it));
                            }
                        }
                    }
                }
            }

            bool CheckCollision(EntityData a, EntityData b)
            {
                // Simple AABB for performance benchmark
                float3 minA = a.position - a.extents;
                float3 maxA = a.position + a.extents;
                float3 minB = b.position - b.extents;
                float3 maxB = b.position + b.extents;

                return (minA.x <= maxB.x && maxA.x >= minB.x) &&
                       (minA.y <= maxB.y && maxA.y >= minB.y) &&
                       (minA.z <= maxB.z && maxA.z >= minB.z);
            }
        }
    }
}
