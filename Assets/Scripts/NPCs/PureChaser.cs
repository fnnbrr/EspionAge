using UnityEngine;
using UnityEngine.AI;

namespace NPCs
{
    public class PureChaser : BaseAi
    {
        public float startChaseRadius = 100f;

        private ChildRootMotionController rootMotionController;
        private bool shouldChase = false;

        public const float ReportReachedDistance = 2f;

        public delegate void ReachedDestinationAction();
        public event ReachedDestinationAction OnReachDestination;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
            rootMotionController = Utils.GetRequiredComponentInChildren<ChildRootMotionController>(this);
        }

        public void SetSpeed(float speed)
        {
            agent.speed = speed;
        }

        public void SetAnimationSpeed(float speed)
        {
            rootMotionController.SetAnimationSpeed(speed);
        }

        public void SetDestination(Vector3 position)
        {
            agent.SetDestination(position);
        }

        private new void ChaseTarget()
        {
            if (shouldChase && targetTransform)
            {
                SetDestination(targetTransform.position);
            }
        }

        protected override void Update()
        {
            if (!agent.isOnNavMesh)
            {
                SetMoving(false);
                return;
            }

            CheckRemainingDistance();

            // below are all about using targetTransform
            if (!targetTransform || agent.speed <= 0f)
            {
                SetMoving(false);
                return;
            }
            else if (shouldChase)
            {
                SetMoving(true);
            }

            if (!shouldChase && Vector3.Distance(transform.position, targetTransform.position) <= startChaseRadius)
            {
                shouldChase = true;
            }

            ChaseTarget();
        }

        private void SetMoving(bool isMoving)
        {
            rootMotionController.SetBool(Constants.ANIMATION_STEVE_MOVING, isMoving);
        }

        private void CheckRemainingDistance()
        {
            if (Vector3.Distance(transform.position, agent.destination) <= ReportReachedDistance)
            {
                OnReachDestination?.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, startChaseRadius);
        }
    }
}
