using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class HumanoidNPCAnimationController : MonoBehaviour
    {
        // Pending animation structure
        internal struct PendingAnimation
        {
            public AnimationLayerData Layer;
            public AnimationPlayableAnimationNode Animation;

            // Flag if the animation is fullbody or not
            // This is important... As some animations may layer on top of each other if they're not fullbody.
            public bool Fullbody;
        }

        /// <summary>
        /// Animation history for this NPC
        /// TODO: should be a joint history for all NPCs
        /// </summary>
        private class AnimationHistory
        {
            // Animation history, keeping track of requests for each layer of the Unity Animator Controller
            private Dictionary<int, RequestData> _animHistory = new Dictionary<int, RequestData>();

            internal class RequestData
            {
                public string Animation { get; private set; }

                // Animation Hash referenced in the Animator layers
                // This is case sensitive! Make sure you pass in the right names! (this should be automated if you use our Humanoid Animation Config)
                public int AnimationHash { get; private set; }

                public bool Completed = false;

                internal RequestData(string animationNode)
                {
                    Refresh(animationNode);
                }

                internal void Refresh(string animationNode)
                {
                    Animation = animationNode;
                    AnimationHash = Animator.StringToHash(animationNode);
                    Completed = false;
                }

                internal void MarkAsComplete()
                {
                    Completed = true;
                }
            }

            /// <summary>
            /// Gets the last animation request on this particular layer
            /// </summary>
            /// <param name="layerId">Desired layer ID</param>
            internal RequestData GetLastAnimOnLayer(int layerId)
            {
                if (!_animHistory.ContainsKey(layerId))
                {
                    return default;
                }

                return _animHistory[layerId];
            }

            /// <summary>
            /// Adds an animation request to the history
            /// </summary>
            /// <param name="animationNode">animation node Id - should be present in the animator controller</param>
            /// <param name="layer">layer index where the animation node lives</param>
            internal void AddAnimationRequest(string animationNode, int layer)
            {
                if (!_animHistory.ContainsKey(layer))
                {
                    _animHistory.Add(layer, new RequestData(animationNode));
                }
                else
                {
                    _animHistory[layer].Refresh(animationNode);
                }
            }

            /// <summary>
            /// Goes thru all the current requests and confirms that they've all completed
            /// this is done by deferring to the animator and checking the current hash on each layer
            /// </summary>
            internal bool HaveRequestsFinished(Animator animator)
            {
                bool result = true;
                foreach (var entry in _animHistory)
                {
                    if (entry.Value.Completed)
                    {
                        continue;
                    }

                    var stateInfo = animator.GetCurrentAnimatorStateInfo(entry.Key);

                    if (stateInfo.shortNameHash == entry.Value.AnimationHash)
                    {
                        result = false;
                        break;
                    }

                    if(result == true)
                    {
                        entry.Value.MarkAsComplete();
                    }
                }

                return result;
            }
        }

        internal HumanoidNPCAnimationConfig Configuration => _npcAnimationConfig;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private SkinnedMeshRenderer _headSkinnedMeshRenderer;

        [SerializeField]
        private HumanoidNPCAnimationConfig _npcAnimationConfig;

        [SerializeField]
        private Transform _headPosition;

        [SerializeField]
        private bool _blinkingEnabled = true;

        private BlendshapesAnimator _blendshapesAnimator;
        private BaseLookAt _lookAt;
        private BaseTurnTo _turnTo;

        private AnimationHistory _lastRequestedAnims = new AnimationHistory();

        private List<PendingAnimation> _pendingAnims = new List<PendingAnimation>();

        private int _headLayerIndex;

        private float _blinkStartTime;
        private float _blinkEndTime;

        private float _intervalEndTime;

        private bool _hasBlinked;

        void Start()
        {
            InitialiseBlendshapesAnimator();
            InitialiseLookAtController();
            InitialiseTurnToController();

            foreach(var layer in _npcAnimationConfig.AnimationMetadata.AnimationLayers)
            {
                if(layer.LayerMask == LayerMasking.Head)
                {
                    _headLayerIndex = layer.LayerId;
                    return;
                }
            }
        }

        void Update()
        {
            _blendshapesAnimator?.Update(false);
            _turnTo?.Update();
            BlinkUpdate();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            _lookAt?.Update(); 
        }
        
        #region Public Functions

        #region Facial Expressions
        /// <summary>
        /// Sets the facial expression for this NPC based on the charisma emotion associated with it
        /// </summary>
        /// <param name="emotion">Emotion name</param>
        /// <param name="modifier">Modifier - see SetFacialExpression for more info</param>
        /// <param name="duration">How long to hold expression</param>
        /// <param name="instant">If the expression should be applied instantly</param>
        public void SetFacialExpression(string emotion, float modifier = 1, float duration = 0f, bool instant = false)
        {
            if (!_npcAnimationConfig.GetExpression(emotion, out NpcFacialExpression expression))
            {
                Debug.LogWarning($"[SetFacialExpression] No facial expression with name '{emotion}' found in database");
            }
            else
            {
                SetFacialExpression(expression, modifier, instant: instant);
            }
        }

        /// <summary>
        /// Returns if the requested animations have finished
        /// </summary>
        internal bool HasRequestedAnimationFinished()
        {
            return _lastRequestedAnims.HaveRequestsFinished(_animator);
        }

        /// <summary>
        /// Set npc facial expression by referring the Facial Expression object directly
        /// </summary>
        /// <param name="expression">Expression object</param>
        /// <param name="modifier">Modifier - see SetFacialExpression for more info</param>
        /// <param name="duration">How long to hold expression</param>
        /// <param name="instant">If the expression should be applied instantly</param>
        public void SetFacialExpression(NpcFacialExpression expression, float modifier = 1, float duration = 0f, bool instant = false)
        {
            if (_blendshapesAnimator == default)
            {
                InitialiseBlendshapesAnimator();
            }

            Debug.Log($"[SetFacialExpression] Facial expression set to {expression.name}: {modifier}");
            _blendshapesAnimator?.SetBlendshapesFromFacialExpression(expression, modifier, duration, instant);
        }

        /// <summary>
        /// Clears the current facial expression, reverting all applied blendshapes to base
        /// </summary>
        /// <param name="instant"></param>
        public void ResetFacialExpression(bool instant = false)
        {
            if (_blendshapesAnimator == default)
            {
                InitialiseBlendshapesAnimator();
            }

            Debug.Log($"[ResetFacialExpression] Clearing Facial expresssion.");
            _blendshapesAnimator?.SetBlendshapesFromFacialExpression(null, instant: instant);
        }
        #endregion

        #region Look At And Turn At Target
        public void SetLookAtTarget(GameObject targetToLookAt, bool instant = false)
        {
            if (targetToLookAt == default)
            {
                Debug.LogWarning($"[SetLookAtTarget] LookAt Target is null. Exiting early.");
                return;
            }

            if (_lookAt == default)
            {
                InitialiseLookAtController();
            }

            _lookAt?.SetLookAtTarget(targetToLookAt, instant);
        }

        public void SetLookAtTarget(Transform targetToLookAt, bool instant = false)
        {
            SetLookAtTarget(targetToLookAt.gameObject, instant);
        }

        public void SetLookAtTarget(Vector3 position, bool instant = false)
        {
            if (_lookAt == default)
            {
                InitialiseLookAtController();
            }

            _lookAt?.SetLookAtTarget(position, instant);
        }

        public void ClearLookAt()
        {
            if (_lookAt == default)
            {
                InitialiseLookAtController();
            }

            _lookAt?.ClearLookAt();
        }

        public void SetTurnToTarget(GameObject targetToTurnTo)
        {
            if (targetToTurnTo == default)
            {
                Debug.LogWarning($"[TurnToTarget] LookAt Target is null. Exiting early.");
                return;
            }

            if (_turnTo == default)
            {
                InitialiseTurnToController();
            }

            _turnTo?.SetTurnToTarget(targetToTurnTo);
        }

        public void TurnToTarget(Transform targetToTurnTo)
        {
            SetTurnToTarget(targetToTurnTo.gameObject);
        }

        public void TurnToTarget(Vector3 position)
        {
            if (_turnTo == default)
            {
                InitialiseTurnToController();
            }

            _turnTo?.SetTurnToTarget(position);
        }

        public void ClearTurnTo()
        {
            if (_turnTo == default)
            {
                InitialiseTurnToController();
            }

            _turnTo?.ClearTurnTo();
        }

        #endregion

        #region Animations

        /// <summary>
        /// Play animation request for layer
        /// </summary>
        /// <param name="animationNode">Animation node name</param>
        /// <param name="layer">Which layer the animation node belongs to. This dataset can be found in the humanoidNPCAnimationConfig file</param>
        /// <param name="force">Force apply the animation request</param>
        public void PlayAnimationFromLayer(string animationNode, AnimationLayerData layer, bool force = false)
        {
            _lastRequestedAnims.AddAnimationRequest(animationNode, layer.LayerId);

            if (force)
            {
                _animator.Play(animationNode, layer.LayerId);
            }
            else
            {
                _animator.CrossFade(animationNode, 0.75f, layer.LayerId);
            }

            Debug.Log($"[RequestAnimation] {this.name} - Setting animation {animationNode} on layer {layer.LayerName}.");
        }

        /// <summary>
        /// Tries to request the correct animation featuring the attached flags
        /// </summary>
        /// <param name="flag">Animation flags needed</param>
        /// <param name="force">Applies the animation regardless of any requests are still being fulfilled</param>
        public void RequestAnimationWithFlags(AnimationFlags flag, bool force = false)
        {
            ParseAnimationRequest(flag, "", force);
        }

        /// <summary>
        /// Tries to request the correct animation featuring the attached flags
        /// </summary>
        /// <param name="flag">Animation flags needed</param>
        /// <param name="emotionToApply">Charisma emotion that is required for this animation. Can be null</param>
        /// <param name="force">Applies the animation regardless of any requests are still being fulfilled</param>
        public void RequestAnimationWithFlagsAndEmotion(AnimationFlags flag, string emotionToApply, bool force = false)
        {
            if (!HasRequestedAnimationFinished())
            {
                Debug.LogWarning($"[RequestAnimationWithFlags] Received animation request while another has yet to finish. Skipping request for {flag}, {emotionToApply}");
                return;
            }

            ParseAnimationRequest(flag, emotionToApply, force);
        }

        /// <summary>
        /// Tries to request the correct animation featuring the attached flags
        /// </summary>
        /// <param name="flag">Animation flags needed</param>
        /// <param name="emotionToApply">Charisma emotion that is required for this animation. Can be null</param>
        /// <param name="force">Applies the animation regardless of any requests are still being fulfilled</param>
        private void ParseAnimationRequest(AnimationFlags flag, string emotionToApply, bool force = false)
        {
            _pendingAnims.Clear();

            // look for anims that feature the required flags
            foreach(var layer in _npcAnimationConfig.AnimationMetadata.AnimationLayers)
            {
                // dont play fullbody animations if we're walking
                if (_animator.GetBool("Walking") && layer.LayerMask == LayerMasking.Fullbody)
                {
                    continue;
                }

                var lastAnimPlayed = _lastRequestedAnims.GetLastAnimOnLayer(layer.LayerId);

                foreach (var animation in layer.AnimationNodeData)
                {
                    // don't add recently played anims
                    if (lastAnimPlayed != default &&
                        lastAnimPlayed.Animation == animation.AnimationNodeName)
                    {
                        continue;
                    }

                    if (animation.Tags.HasFlag(flag)
                        && animation.AssociatedCharismaEmotion == emotionToApply)
                    {
                        if (layer.LayerMask == LayerMasking.Fullbody)
                        {
                            _pendingAnims.Add(new PendingAnimation
                            {
                                Animation = animation,
                                Layer = layer,
                                Fullbody = true
                            });;
                        }
                        else
                        {
                            _pendingAnims.Add(new PendingAnimation
                            {
                                Animation = animation,
                                Layer = layer,
                                Fullbody = false
                            });
                        }
                    }
                }
            }

            var pendingAnimsCount = _pendingAnims.Count;

            if (pendingAnimsCount > 0)
            {
                // randomly select an anim
                int i = UnityEngine.Random.Range(0, pendingAnimsCount);
                var potentialAnim = _pendingAnims[i];

                if (potentialAnim.Fullbody)
                {
                    PlayAnimationFromLayer(potentialAnim.Animation.AnimationNodeName, potentialAnim.Layer, force);
                    return;
                }
                else
                {
                    PlayAnimationFromLayer(potentialAnim.Animation.AnimationNodeName, potentialAnim.Layer, force);

                    foreach (var otherPartialAnim in _pendingAnims)
                    {
                        var otherlayerID = otherPartialAnim.Layer.LayerId;

                        if (otherlayerID == potentialAnim.Layer.LayerId)
                        {
                            continue;
                        }

                        if (otherPartialAnim.Fullbody)
                        {
                            continue;
                        }

                        var lastAnimPlayed = _lastRequestedAnims.GetLastAnimOnLayer(otherlayerID);

                        // don't add recently played anims
                        if (lastAnimPlayed != default &&
                            lastAnimPlayed.Animation != "")
                        {
                            continue;
                        }

                        PlayAnimationFromLayer(otherPartialAnim.Animation.AnimationNodeName, otherPartialAnim.Layer, force);
                    }
                }
            }

            // look for non-emotional anims, if an emotion was requested and nothing was found
            if (pendingAnimsCount == 0 && emotionToApply != "")
            {
                Debug.LogWarning($"[PlayRandomAnimation] No animations associated with flag '{flag}' and feeling '{emotionToApply}' found in database");
                ParseAnimationRequest(flag, "", force);
            }
        }

        #endregion

        #endregion

        #region Private Functions

        internal void SetParameter(AnimationParameter param, bool value)
        {
            if (param.ParamType == AnimatorControllerParameterType.Bool)
            {
                _animator.SetBool(param.Name, value);
            }
            else
            {
                Debug.LogError("[NPCAnimationController] - incorrect Animation Parameter type. Expected type bool.");
            }
        }

        internal void SetParameter(AnimationParameter param, float value)
        {
            if (param.ParamType == AnimatorControllerParameterType.Float)
            {
                _animator.SetFloat(param.Name, value);
            }
            else
            {
                Debug.LogError("[NPCAnimationController] - incorrect Animation Parameter type. Expected type float.");
            }
        }

        internal void SetParameter(AnimationParameter param, int value)
        {
            if (param.ParamType == AnimatorControllerParameterType.Int)
            {
                _animator.SetInteger(param.Name, value);
            }
            else
            {
                Debug.LogError("[NPCAnimationController] - incorrect Animation Parameter type. Expected type int.");
            }
        }

        internal bool HasLookAtTarget()
        {
            return _lookAt.HasLookAtTarget();
        }

        internal bool GetAnimatorClipHasEnded()
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);

            return state.normalizedTime % 1 >= 0.95f;
        }

        internal bool IsLookingAtTarget()
        {
            return _lookAt.IsFacingTarget();
        }

        internal void SetParameter(AnimationParameter param)
        {
            if (param.ParamType == AnimatorControllerParameterType.Trigger)
            {
                _animator.SetTrigger(param.Name);
            }
            else
            {
                Debug.LogError("[NPCAnimationController] - incorrect Animation Parameter type. Expected type trigger.");
            }
        }

        private void InitialiseBlendshapesAnimator()
        {
            if (_headSkinnedMeshRenderer == default)
            {
                Debug.LogError($"[InitialiseBlendshapesAnimator] Head SkinnedMeshRenderer not set on {this.name}. Unable to set facial expressions.");
                return;
            }

            _blendshapesAnimator = new BlendshapesAnimator(_headSkinnedMeshRenderer);
        }

        private void InitialiseLookAtController()
        {
            if (_animator == default)
            {
                Debug.LogError($"[InitialiseLookAtController] Animator missing, unable to initialise LookAt functionality.");
                return;
            }

            _lookAt = new HumanoidLookAtIK(_animator, this.transform.parent, _headPosition);
        }

        private void InitialiseTurnToController()
        {
            if (_animator == default)
            {
                Debug.LogError($"[InitialiseLookAtController] Animator missing, unable to initialise LookAt functionality.");
                return;
            }

            _turnTo = new HumanoidTurnTo(_animator, this.transform.parent, _npcAnimationConfig);
        }


        private void BlinkUpdate()
        {
            if (_headSkinnedMeshRenderer == null)
            {
                return;
            }


            if (!_blinkingEnabled)
            {
                return;
            }

            if (_hasBlinked)
            {
                if (Time.time > _intervalEndTime)
                {
                    ResetBlinkTimer();
                    _hasBlinked = false;
                }
            }
            else
            {
                bool blinking = Time.time < _blinkEndTime;
                float weight = blinking
                        ? Mathf.Sin(
                            Mathf.InverseLerp(_blinkStartTime, _blinkEndTime, Time.time)
                            * Mathf.PI) * 100 : 0;

                foreach (var bs in _npcAnimationConfig.BlinkBlendshapes)
                {
                    _blendshapesAnimator.SetBlendshapesForEyeBlink(bs.BlendName, weight);
                }

                // no longer blinking
                if (!blinking)
                {
                    _intervalEndTime = Time.time + _npcAnimationConfig.BlinkInterval.Evaluate(UnityEngine.Random.value);
                    _hasBlinked = true;
                }
            }
        }

        private void ResetBlinkTimer()
        {
            _blinkStartTime = Time.time;
            _blinkEndTime = _blinkStartTime + _npcAnimationConfig.BlinkTime.Evaluate(UnityEngine.Random.value);
        }

        #endregion
    }
}
