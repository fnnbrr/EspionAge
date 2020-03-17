using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseNavAi))]
    public class Searcher : MonoBehaviour
    {
        public float movementSpeed = 5.0f;
        public Vector3 searchBoundsCenter;
        public float searchBoundsRadius = 15.0f;

        private BaseNavAi baseNavAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseNavAi = Utils.GetRequiredComponent<BaseNavAi>(this);
            agent = baseNavAi.agent;
        }

        public void GotoNextSearchPoint()
        {
            Vector3 randomPoint = searchBoundsCenter + Random.insideUnitSphere * searchBoundsRadius;

            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit navHit, searchBoundsRadius, NavMesh.AllAreas))
            {
                navHit.position = searchBoundsCenter;
            }

            agent.SetDestination(navHit.position);
        }

        public void ResetSearchPoint()
        {
            searchBoundsCenter = agent.destination;
        }
    }
}