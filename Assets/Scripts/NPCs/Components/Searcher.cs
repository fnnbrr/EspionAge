using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    public class Searcher : MonoBehaviour
    {
        public float movementSpeed = 5.0f;
        public Vector3 searchPosition;
        public float searchRadius = 15.0f;

        public Vector3 GetNextSearchPoint()
        {
            Vector3 randomPoint = searchPosition + Random.insideUnitSphere * searchRadius;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit navHit, searchRadius, NavMesh.AllAreas))
            {
                return navHit.position;
            }

            return searchPosition;
        }

        public void SetSearchPoint(Vector3 newSearchPosition)
        {
            searchPosition = newSearchPosition;
        }
    }
}