using NPCs.Components;
using UnityEngine;

namespace NPCs
{
    [RequireComponent(typeof(Chaser))]
    public class PureChaser : BaseAi
    {
        public float startChaseRadius = 100f;
        
        public Chaser chaser;
        private ChildRootMotionController rootMotionController;
        private bool shouldChase = false;

        public const float ReportReachedDistance = 2f;
        

        public delegate void ReachedDestinationAction();
        public event ReachedDestinationAction OnReachDestination;

        private new void Awake()
        {
            base.Awake();
            rootMotionController = Utils.GetRequiredComponentInChildren<ChildRootMotionController>(this);
        }
        
        private new void Start()
        {
            base.Start();
            
            chaser = Utils.GetRequiredComponent<Chaser>(this);
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
            if (shouldChase && chaser.targetTransform)
            {
                SetDestination(chaser.targetTransform.position);
            }
        }

        public override void SetState(string newState)
        {
            throw new System.NotImplementedException();
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
            if (!chaser.targetTransform || agent.speed <= 0f)
            {
                SetMoving(false);
                return;
            }
            else if (shouldChase)
            {
                SetMoving(true);
            }

            if (!shouldChase && Vector3.Distance(transform.position, chaser.targetTransform.position) <= startChaseRadius)
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
