using Unity.Mathematics;
using Unity.Burst;

namespace Gazze.Collision
{
    [BurstCompile]
    public static class CollisionMath
    {
        /// <summary>
        /// Sphere - Sphere çarpışma kontrolü.
        /// </summary>
        public static bool CheckSphereSphere(float3 posA, float radiusA, float3 posB, float radiusB)
        {
            float distSq = math.distancesq(posA, posB);
            float radiusSum = radiusA + radiusB;
            return distSq <= (radiusSum * radiusSum);
        }

        /// <summary>
        /// AABB - AABB çarpışma kontrolü.
        /// </summary>
        public static bool CheckAABBAABB(float3 minA, float3 maxA, float3 minB, float3 maxB)
        {
            return (minA.x <= maxB.x && maxA.x >= minB.x) &&
                   (minA.y <= maxB.y && maxA.y >= minB.y) &&
                   (minA.z <= maxB.z && maxA.z >= minB.z);
        }

        /// <summary>
        /// OBB - OBB çarpışma kontrolü (Separating Axis Theorem - SAT).
        /// </summary>
        public static bool CheckOBBOBB(
            float3 centerA, float3 extentsA, quaternion rotA,
            float3 centerB, float3 extentsB, quaternion rotB)
        {
            // Basitleştirilmiş OBB (Şu anlık hızlı test için AABB'ye fallback, 
            // ancak gerçek implementasyonda SAT eksenleri döngüyle dönülmeli)
            float3 minA = centerA - extentsA;
            float3 maxA = centerA + extentsA;
            float3 minB = centerB - extentsB;
            float3 maxB = centerB + extentsB;
            return CheckAABBAABB(minA, maxA, minB, maxB);
        }
    }
}
