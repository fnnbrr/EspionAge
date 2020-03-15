using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Searcher : MonoBehaviour
    {
        [Tooltip("After hearing a noise, how close should I search to that noise?")]
        public float noiseSearchRadius = 15.0f;

        private Vector3 searchBoundsCenter;
        private float searchBoundsRadius;

        private NavMeshAgent agent;
        private bool isSearching = false;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }

        private void Start()
        {
            searchBoundsRadius = noiseSearchRadius;
        }

        protected void GotoNextSearchPoint()
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