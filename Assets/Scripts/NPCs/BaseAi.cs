using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs
{
    public abstract class BaseAi : MonoBehaviour
    {
        [HideInInspector] public NavMeshAgent agent;
        protected Animator Animator;

        [Header("Chasing")]
        public bool chase = true;
        public Transform targetTransform;

        [Header("Responding")]
        public Vector3 responsePoint;
    
        [Header("Searching")]
        public SphereCollider searchBounds;
        [Tooltip("After hearing a noise, how close should I search to that noise?")]
        public float noiseSearchRadius = 15.0f;

        private Vector3 searchBoundsCenter;
        private float searchBoundsRadius;
    
        [Header("Waiting")]
        public float waitDurationSec = 1.0f;
        protected bool IsWaiting = false;

        protected float WaitTimer;

        protected enum ActionStates
        {
            Chasing,
            Responding,
            Searching,
            Patrolling,
        }
    
        [Header("Question Mark Icon")]
        public GameObject questionMark;
    
        [Header("For Debugging")]
        [SerializeField] protected ActionStates currentState = ActionStates.Responding;
        [SerializeField] protected ActionStates defaultState = ActionStates.Searching;

        public delegate void CollideWithPlayerAction();
        public event CollideWithPlayerAction OnCollideWithPlayer;

        private void Awake()
        {
            Animator = Utils.GetRequiredComponentInChildren<Animator>(this);
        }

        public void Start()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);

            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            agent.autoBraking = false;
        
            WaitTimer = waitDurationSec;

            // If transform is set, then we can use it's position, otherwise we use responsePointPosition (used in missions)
            agent.SetDestination(responsePoint);

            if (!searchBounds) return;
        
            searchBoundsCenter = searchBounds.center;
            searchBoundsRadius = searchBounds.radius;
        }
    
        protected abstract void Update();
    
        public void InitializeResponderParameters(Vector3 newResponsePoint, Vector3 wanderPoint, float wanderRadius)
        {
            SetState(ActionStates.Responding);
            responsePoint = newResponsePoint;
            searchBoundsCenter = wanderPoint;
            searchBoundsRadius = wanderRadius;
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (chase && numTargetsInRange > 0 && currentState != ActionStates.Chasing)
            {
                if (!agent.enabled) return;

                ChaseTarget();
                SetState(ActionStates.Chasing);
            }
            else if (numTargetsInRange == 0 && currentState == ActionStates.Chasing)
            {
                SetState(defaultState);
            }
        }
    
        protected bool WaitComplete()
        {
            WaitTimer -= Time.deltaTime;
            if (WaitTimer > 0)
            {
                IsWaiting = true;
                agent.enabled = false;
                return false;
            }

            WaitTimer = waitDurationSec;
            agent.enabled = true;
            IsWaiting = false;
            return true;
        }
    
        protected void GotoNextSearchPoint()
        {
            Vector3 randomPoint = searchBoundsCenter + Random.insideUnitSphere * searchBoundsRadius;
            NavMeshHit navHit;

            if (!NavMesh.SamplePosition(randomPoint, out navHit, searchBoundsRadius, NavMesh.AllAreas))
            {
                navHit.position = searchBoundsCenter;
            }

            Animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);

            agent.SetDestination(navHit.position);
        }

        protected void ChaseTarget()
        {
            Animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);

            Vector3 targetPosition = targetTransform.position;
            Vector3 thisPosition = transform.position;
            Vector3 dirToTarget = thisPosition - targetPosition;
            Vector3 newPos = thisPosition - dirToTarget;

            agent.SetDestination(newPos);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
            {
                OnCollideWithPlayer?.Invoke();
            }
        }
    
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Noise") && agent.enabled) return;
        
            SetState(ActionStates.Responding);
        
            searchBoundsCenter = other.gameObject.transform.position;
            searchBoundsRadius = noiseSearchRadius;
            agent.SetDestination(searchBoundsCenter);

            WaitTimer = waitDurationSec;
        }

        protected void SetState(ActionStates newState)
        {
            // Ensures that question mark is properly shown/hidden
            switch (newState)
            {
                case ActionStates.Searching:
                    ShowQuestionMark();
                    break;
                case ActionStates.Responding:
                    ShowQuestionMark();
                    break;
                case ActionStates.Chasing:
                    HideQuestionMark();
                    break;
                case ActionStates.Patrolling:
                    HideQuestionMark();
                    break;
                default:
                    HideQuestionMark();
                    break;
            }
        
            currentState = newState;
        }
    
        protected void ShowQuestionMark()
        {
            questionMark.SetActive(true);
        }

        protected void HideQuestionMark()
        {
            questionMark.SetActive(false);
        }
    }
}
