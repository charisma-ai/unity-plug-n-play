using CharismaSDK.StateMachine;
using CharismaSDK.Events;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class HumanoidNPCCharacterController : MonoBehaviour
    {
        internal enum NPCState
        {
            Stand, // => Sit and Walk
            Walk, // => Stand
            Sit // => Stand
        }

        internal HumanoidNPCAnimationController AnimationController => _animationController;

        [SerializeField]
        private HumanoidNPCAnimationController _animationController;

        internal NavmeshComponent NavmeshComponent => _navmeshComponent;

        [SerializeField]
        private NavmeshComponent _navmeshComponent;

        internal NPCState CurrentState => _fsm.CurrentState;

        [SerializeField]
        private AudioSource _audioSource;

        private Dictionary<int, float> _emotionIdDuration = new Dictionary<int, float>();

        [ReadOnly]
        [SerializeField]
        private List<NPCTask> _currentTasks = new List<NPCTask>();

        [ReadOnly]
        [SerializeField]
        private List<NPCTask> _pendingRemoval = new List<NPCTask>();


        private SimpleStateMachine<NPCState> _fsm = new SimpleStateMachine<NPCState>();

        private Transform _pendingGoToTarget;
        private float _stoppingDistance;

        private AnimationParameter _walkingAnimationParam;
        private AnimationParameter _talkingAnimationParam;

        private string _currentEmotion;
        private float _emotionIntensity;

        private const float MINIMUM_MANNERISM_THRESHOLD = 5f;
        private const float MAXIMUM_MANNERISM_THRESHOLD = 20f;

        private const float MINIMUM_RNG_TIMER_THRESHOLD = 5f;
        private const float MAXIMUM_RNG_TIMER_THRESHOLD = 15f;
        private const float TALKING_ANIM_TIME_OUT = 1f;


        private float _mannerismTimer;

        private bool _requestManerism;
        private bool _resetRng;
        private float _rngTimer;


        private float _talkingAnimTimer;

        private void Start()
        {
            InitialiseParameters();
            InitialiseFiniteStateMachine();

            this.LookAtObject(Camera.main.gameObject);
        }

        private void Update()
        {
            _fsm.Update();

            UpdateCurrentTasks(Time.deltaTime);
            UpdateTalkingAnimationParameters();
        }

        #region Public functions

        public void SetEmotion(List<Emotion> emotions, float messageDuration)
        {
            // reset current emotion
            _emotionIntensity = 0.0f;
            _currentEmotion = string.Empty;

            if (emotions == default)
            {
                return;
            }

            if (emotions.Count == 0)
            {
                return;
            }

            foreach (var emotion in emotions)
            {
                SortMostIntenseEmotion(emotion);
            }

            _animationController.SetFacialExpression(_currentEmotion, _emotionIntensity, messageDuration);

        }

        private void SortMostIntenseEmotion(Emotion emotion)
        {
            foreach (var effect in emotion.activeEffects)
            {
                float intensity = effect.intensity;

                //Adjust the intensity of the emotion based on the remaining duration
                if (_emotionIdDuration.TryGetValue(effect.id, out float initialDuration))
                {
                    intensity *= effect.durationRemaining / (float)initialDuration;

                    if (effect.durationRemaining == 1)
                    {
                        _emotionIdDuration.Remove(effect.id);
                    }
                }
                else if (effect.durationRemaining > 1)
                {
                    _emotionIdDuration.Add(effect.id, effect.durationRemaining + 5.0f);
                }

                if (intensity > _emotionIntensity)
                {
                    _emotionIntensity = intensity;
                    _currentEmotion = effect.feeling;
                }
            }
        }

        public void StopTalking()
        {
            _talkingAnimTimer = 0.0f;
        }

        public void ReplyTo(AudioClip audioClip)
        {
            StartIdleRNGTimer();
            StartManerismTimer();

            _resetRng = false;
            _requestManerism = false;

            if (!IsAnimationTaskPending())
            {
                PlayAppropriateTalkingAnimation();
            }

            _audioSource.clip = audioClip;
            _audioSource.Play();
        }

        // TODO: find a better solution to resolve task + other animation collisions
        private bool IsWalkingTaskPending()
        {
            bool result = false;
            foreach(var task in _currentTasks)
            {
                if(task is MoveToLocationTask)
                {
                    result = true;
                }
            }

            return result;
        }

        // TODO: find a better solution to resolve task + other animation collisions
        private bool IsAnimationTaskPending()
        {
            bool result = false;
            foreach (var task in _currentTasks)
            {
                if (task is PlayAnimationTask)
                {
                    result = true;
                }
            }

            return result;
        }

        private void PlayAppropriateIdleAnimation()
        {
            var idleAnimation = AnimationFlags.Idle;

            if (!IsWalkingTaskPending())
            {
                AddAnimationFlagBasedOnState(ref idleAnimation);
                _animationController.RequestAnimationWithFlagsAndEmotion(idleAnimation, GetCurrentEmotion());
            }
        }

        private void PlayAppropriateWalkingAnimation()
        {
            var walkingAnimation = AnimationFlags.Walking;

            _animationController.RequestAnimationWithFlags(walkingAnimation, true);
        }

        private void PlayAppropriateMannerismAnimation()
        {
            var mannerismAnimation = AnimationFlags.Mannerism;

            if (!IsWalkingTaskPending())
            {
                AddAnimationFlagBasedOnState(ref mannerismAnimation);
                _animationController.RequestAnimationWithFlagsAndEmotion(mannerismAnimation, GetCurrentEmotion());
                _talkingAnimTimer = TALKING_ANIM_TIME_OUT;
            }
        }

        private void PlayAppropriateTalkingAnimation()
        {
            var talkingAnimationFlags = AnimationFlags.Talking;

            if (!IsWalkingTaskPending() && _talkingAnimTimer <= 0)
            {
                AddAnimationFlagBasedOnState(ref talkingAnimationFlags);
                _animationController.RequestAnimationWithFlagsAndEmotion(talkingAnimationFlags, GetCurrentEmotion());
            }
        }

        private string GetCurrentEmotion()
        {
            return _currentEmotion;
        }

        private void AddAnimationFlagBasedOnState(ref AnimationFlags talkingAnimationFlags)
        {
            if (_fsm.CurrentState == NPCState.Stand
                || _fsm.CurrentState == NPCState.Walk)
            {
                talkingAnimationFlags |= AnimationFlags.Standing;
            }
        }

        public void AddTask(NPCTask task)
        {
            Debug.Log($"[AddTask] - {this.gameObject.name} - {task.GetType()}");

            _currentTasks.Add(task);
        }

        private void UpdateCurrentTasks(float timeStep)
        {
            foreach (var task in _currentTasks)
            {
                if (task.IsRunning)
                {
                    if (task.TaskUpdate(this, timeStep))
                    {
                        task.TaskStop(this, false);
                    }

                    if (task.HasCompleted)
                    {
                        _pendingRemoval.Add(task);
                    }
                }
                else
                {
                    if (task.CanPerform(this))
                    {
                        task.TaskStart(this);
                    }
                }
            }

            foreach (var toRemove in _pendingRemoval)
            {
                _currentTasks.Remove(toRemove);
            }

            _pendingRemoval.Clear();
        }

        public void ClearCurrentTasks(bool force = false)
        {
            Debug.Log($"[ClearCurrentTask] - {this.gameObject.name}, force: {force}");

            foreach (var task in _currentTasks)
            {
                if (task.Priority == NPCTask.TaskPriority.Low || force == true)
                {
                    task.TaskStop(this, force);
                }
            }

            _currentTasks.Clear();
        }

        internal void ClearGoToTarget()
        {
            _navmeshComponent.ClearGoToTarget();
        }

        internal void SetGoToTarget(Transform target, float stoppingDistance)
        {
            _pendingGoToTarget = target;
            _stoppingDistance = stoppingDistance;
        }

        internal bool CanMove()
        {
            return true;
        }

        #endregion

        #region private functions
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawDebugLines();
        }

        private void DrawDebugLines()
        {
            // this objects forward
            var yOffset = new Vector3(0, 0.5f, 0);
            var objectPosition = this.transform.position + yOffset;
            Debug.DrawLine(objectPosition, (objectPosition + this.transform.forward), Color.green);

            // the animators forward
            Debug.DrawLine(objectPosition, objectPosition + _animationController.transform.forward, Color.red);
            if (_pendingGoToTarget != default)
            {
                Debug.DrawLine(objectPosition, _pendingGoToTarget.transform.position - objectPosition, Color.blue);
            }
        }
#endif


        private void InitialiseParameters()
        {
            _walkingAnimationParam = _animationController.Configuration.AnimationMetadata.FindParameterWithLabel("Walking");
            _talkingAnimationParam = _animationController.Configuration.AnimationMetadata.FindParameterWithLabel("Talking");
        }

        private void UpdateTalkingAnimationParameters()
        {
            if (_audioSource.isPlaying && _audioSource.clip != default)
            {
                _animationController.SetParameter(_talkingAnimationParam, true);
            }
            else
            {
                if (_talkingAnimTimer > 0.0f)
                {
                    _talkingAnimTimer -= Time.deltaTime;
                }
                else
                {
                    _animationController.SetParameter(_talkingAnimationParam, false);
                }
            }
        }


        private bool IsLookingAtTarget()
        {
            return _animationController.IsLookingAtTarget();

        }

        private void StartManerismTimer()
        {
            _mannerismTimer = UnityEngine.Random.Range(MINIMUM_MANNERISM_THRESHOLD, MAXIMUM_MANNERISM_THRESHOLD);
        }

        private void StartIdleRNGTimer()
        {
            _rngTimer = UnityEngine.Random.Range(MINIMUM_RNG_TIMER_THRESHOLD, MAXIMUM_RNG_TIMER_THRESHOLD);
        }

        private bool IsFacingTargetGoTo()
        {
            if (_pendingGoToTarget == default)
            {
                return false;
            }

            var targetPosition = _pendingGoToTarget.position;
            targetPosition.y = 0;
            var npcPosition = this.transform.position;
            npcPosition.y = 0;

            var angle = Vector3.Angle(this.transform.forward, targetPosition - npcPosition);

            return angle < 45;
        }

        private void InitialiseFiniteStateMachine()
        {
            _fsm.AddState(NPCState.Stand)
                .OnEntry(StandOnEntry)
                .OnUpdate(StandOnUpdate)
                .OnExit(StandOnExit);

            _fsm.AddState(NPCState.Walk)
                .OnEntry(WalkOnEntry)
                .OnUpdate(WalkOnUpdate)
                .OnExit(WalkOnExit);
        }

        private void StandOnEntry()
        {
            _animationController.ClearTurnTo();
            StartIdleRNGTimer();
        }

        private void StandOnUpdate()
        {
            UpdateIdleRngTimer();
            UpdateManerismTimer();

            if (_pendingGoToTarget != default)
            {
                if (!_animationController.HasRequestedAnimationFinished())
                {
                    return;
                }

                // ignore go to requests that lead to the same area
                if(Vector3.Distance(this.gameObject.transform.position, _pendingGoToTarget.position) < _stoppingDistance)
                {
                    _pendingGoToTarget = default;
                    return;
                }

                if (IsFacingTargetGoTo())
                {
                    PlayAppropriateWalkingAnimation();
                    _fsm.SetState(NPCState.Walk);
                    return;
                }
                else
                {
                    _animationController.TurnToTarget(_pendingGoToTarget);
                }
            }

            if (_animationController.GetAnimatorClipHasEnded())
            {
                if (_requestManerism)
                {
                    _requestManerism = false;
                    PlayAppropriateMannerismAnimation();
                }

                if (_resetRng)
                {
                    StartManerismTimer();
                    PlayAppropriateIdleAnimation();
                    _resetRng = false;
                }
            }
        }

        private void UpdateIdleRngTimer()
        {
            if (_rngTimer >= 0)
            {
                _rngTimer -= Time.deltaTime;
            }
            else
            {
                _resetRng = true;
                //_currentIdleRNG = UnityEngine.Random.Range(0f, 5f) / 5;
                StartIdleRNGTimer();
            }
        }

        private void UpdateManerismTimer()
        {
            if (_mannerismTimer >= 0)
            {
                _mannerismTimer -= Time.deltaTime;
            }
            else
            {
                StartManerismTimer();
                _requestManerism = true;
            }

        }

        private void StandOnExit()
        {

        }

        private void WalkOnEntry()
        {
        }

        private void WalkOnUpdate()
        {
            if (_pendingGoToTarget != default)
            {
                if (IsFacingTargetGoTo())
                {
                    _animationController.SetParameter(_walkingAnimationParam, true);
                    _navmeshComponent.SetNavMeshTarget(_pendingGoToTarget);
                    _navmeshComponent.SetStoppingDistance(_stoppingDistance);
                    _animationController.ClearTurnTo();
                    _pendingGoToTarget = default;
                    _stoppingDistance = 0.0f;
                }
                else
                {
                    _animationController.TurnToTarget(_pendingGoToTarget);
                }
            }
            else
            {
                if (!_navmeshComponent.IsPathing)
                {
                    _fsm.SetState(NPCState.Stand);
                }
            }
        }

        private void WalkOnExit()
        {
            _animationController.SetParameter(_walkingAnimationParam, false);
            _animationController.ClearTurnTo();
        }


        #endregion


    }
}
