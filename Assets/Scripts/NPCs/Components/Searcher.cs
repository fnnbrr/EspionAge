using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Searcher : MonoBehaviour
    {
        public float movementSpeed = 5.0f;
        public Vector3 searchBoundsCenter;
        public float searchBoundsRadius = 15.0f;

        private BaseAi baseAi;
        private NavMeshAgent agent;
        private bool isSearching = false;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        public void GotoNextSearchPoint()
        {
            Vector3 randomPoint = searchBoundsCenter + Random.insideUnitSphere * searchBoundsRadius;
            NavMeshHit navHit;

            if (!NavMesh.SamplePosition(randomPoint, out navHit, searchBoundsRadius, NavMesh.AllAreas))
            {
                navHit.position = searchBoundsCenter;
            }

            agent.SetDestination(navHit.position);
            isSearching = true;
        }
    }
}