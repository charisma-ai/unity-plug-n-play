using System.Collections.Generic;
using UnityEngine;


namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Comprehensive animator class
    /// Used to animate Facial Expressions and Blinking
    /// </summary>
    public class BlendshapesAnimator
    {
        // Keeps track of individual Blendshapes
        // And gives them a concrete lifetime thru several phases
        private class BlendshapeControl
        {
            // Various states the Blendshape can be in
            private enum BlendshapeMotion
            {
                BlendToIntensity,
                HoldIntensity,
                ReturnToBaseValue,
                Inactive
            }

            // Current weight to be applied to this Blendshape
            public float CurrentWeight => _currentWeight;

            public bool IsAnimating => _motion != BlendshapeMotion.Inactive
                && _motion != BlendshapeMotion.HoldIntensity;

            // Current Motion state
            private BlendshapeMotion _motion = BlendshapeMotion.Inactive;

            private float _baselineWeight;
            private float _currentWeight;
            private float _targetWeight;

            private float _maxblendingSpeed;
            private float _blendingVelocity;
            private float _holdTimer;

            internal BlendshapeControl(float baselineWeight)
            {
                _baselineWeight = baselineWeight;
                Reset();
            }

            /// <summary>
            /// Start animation the blendshape, by blending to the desired intensity
            /// </summary>
            /// <param name="weight">Weight to blend to</param>
            /// <param name="speed">How fast the blending should be</param>
            /// <param name="duration">How long to hold the Target Weight</param>
            internal void StartAnimating(float weight, float speed, float duration)
            {
                _motion = BlendshapeMotion.BlendToIntensity;
                _targetWeight = weight;
                _maxblendingSpeed = speed;
                _holdTimer = duration;
            }

            /// <summary>
            /// Forces the blendshape immediately to the hold position
            /// </summary>
            /// <param name="weight">Target weight to apply</param>
            /// <param name="duration">How long to hold the Target Weight</param>
            internal void ForceAnimate(float weight, float duration)
            {
                _targetWeight = weight;
                _currentWeight = weight;
                _holdTimer = duration;
                _motion = BlendshapeMotion.HoldIntensity;
            }

            /// <summary>
            /// Reset to baseline weights
            /// And return Blendshape to inactive
            /// </summary>
            internal void Reset()
            {
                _targetWeight = _baselineWeight;
                _currentWeight = _baselineWeight;
                _motion = BlendshapeMotion.Inactive;
            }

            internal void Update()
            {
                switch (_motion)
                {
                    case BlendshapeMotion.BlendToIntensity:
                        if (BlendToTarget(_targetWeight))
                        {
                            _blendingVelocity = 0f;
                            _motion = BlendshapeMotion.HoldIntensity;
                        }
                        break;
                    case BlendshapeMotion.HoldIntensity:
                        if (_holdTimer > 0)
                        {
                            _holdTimer -= Time.deltaTime;
                        }
                        else
                        {
                            _blendingVelocity = 0f;
                            _motion = BlendshapeMotion.ReturnToBaseValue;
                        }
                        break;
                    case BlendshapeMotion.ReturnToBaseValue:
                        if (BlendToBase(_baselineWeight))
                        {
                            _blendingVelocity = 0f;
                            _motion = BlendshapeMotion.Inactive;
                        }
                        break;
                    case BlendshapeMotion.Inactive:

                        break;
                }
            }

            /// <summary>
            /// Blend to target value
            /// </summary>
            /// <param name="target">Target value to blend to</param>
            /// <returns>Returns whether we have reached the desired target</returns>
            private bool BlendToTarget(float target)
            {
                var resultWeight = Mathf.SmoothDamp(_currentWeight, target,
                    ref _blendingVelocity, 0.3f,
                    _maxblendingSpeed, deltaTime: Time.deltaTime);

                // increment by 1, smoothdamp can take ages due to smoothening and floating point error
                _currentWeight = resultWeight + 1;

                // if no longer moving, we've reached our destination
                if (_currentWeight >= _targetWeight)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Blend to base value
            /// goes in the opposite direction and is handled slightly differently
            /// </summary>
            /// <param name="target">Target value to blend to</param>
            /// <returns>Returns whether we have reached the desired target</returns>
            private bool BlendToBase(float baseValue)
            {
                _currentWeight -= Time.deltaTime * (_maxblendingSpeed / 3);

                if (_currentWeight <= baseValue)
                {
                    return true;
                }

                return false;
            }
        }

        private Dictionary<int, BlendshapeControl> _blendshapeControls = new Dictionary<int, BlendshapeControl>();

        // Speed of animation in MS per frame
        // TODO: magic number - clarify what this is
        private const float MIN_ANIMATION_SPEED = 80;
        private const float MAX_ANIMATION_SPEED = 200;

        private const float MINIMUM_EXPRESSION_DURATION = 5f;

        private SkinnedMeshRenderer _smr;

        // Since animating for a single mesh
        // store the collection of possible blendshapes here
        private FacialExpressionBlendshapeCollection _blendshapeCollection;

        // Empty container, used to prevent new dicts created at runtime
        private Dictionary<int, float> _emptyDictionary = new Dictionary<int, float>();

        public BlendshapesAnimator(SkinnedMeshRenderer smr)
        {
            _smr = smr;
            _blendshapeCollection = new FacialExpressionBlendshapeCollection(_smr.sharedMesh);

            var blendShapeDictionary = BlendShapeUtils.GetBlendshapesDic(smr.sharedMesh);
            foreach (var entry in blendShapeDictionary)
            {
                var baselineWeight = smr.GetBlendShapeWeight(entry.Value);
                _blendshapeControls.Add(entry.Value, new BlendshapeControl(baselineWeight));
            }
        }

        /// <summary>
        /// Applies facial expression blendshapes
        /// </summary>
        /// <param name="facialExpression">Target expression</param>
        /// <param name="multiplier">Multiplier to be applied to the facial expression value. Recommend values from 0.0 to 1.0. This is used to create partial expressions (smile at 1.0 => half smile at 0.5)</param>
        /// <param name="duration">Expected duration this expression should hold</param>
        /// <param name="instant">If the expression should be applied instantly</param>
        public void SetBlendshapesFromFacialExpression(NpcFacialExpression facialExpression, float multiplier = 1, float duration = 0.0f, bool instant = false)
        {
            bool removeFacialExpression = false;
            if (multiplier == 0 || facialExpression == default)
            {
                removeFacialExpression = true;
            }

            Dictionary<int, float> newTargets = _emptyDictionary;
            if (!removeFacialExpression)
            {
                GetBlendshapesForFacialAnimation(facialExpression, out newTargets);
            }

            if (!instant)
            {
                // Reset targets
                foreach (var blend in _blendshapeControls)
                {
                    blend.Value.Reset();
                }

                // Set new blendshape targets
                foreach (var blendshape in newTargets)
                {
                    var weight = blendshape.Value * multiplier;
                    var speed = Random.Range(MIN_ANIMATION_SPEED, MAX_ANIMATION_SPEED);
                    // Minimum duration clamp, to prevent awkward facial animation
                    if (duration < MINIMUM_EXPRESSION_DURATION)
                    {
                        duration = MINIMUM_EXPRESSION_DURATION;
                    }
                    _blendshapeControls[blendshape.Key].StartAnimating(weight, speed, duration);
                }
            }
            else
            {
                foreach (var blendshape in newTargets)
                {
                    _blendshapeControls[blendshape.Key].ForceAnimate(blendshape.Value * multiplier, MINIMUM_EXPRESSION_DURATION);
                }

                _smr.ResetBlendWeights();
                foreach (var blendshape in newTargets)
                {
                    _smr.SetBlendShapeWeight(blendshape.Key, _blendshapeControls[blendshape.Key].CurrentWeight);
                }

            }
        }

        private void GetBlendshapesForFacialAnimation(NpcFacialExpression facialAnimation, out Dictionary<int, float> newTargets)
        {
            if (!_blendshapeCollection.Contains(facialAnimation))
            {
                _blendshapeCollection.Register(facialAnimation);
            }
            newTargets = _blendshapeCollection.GetBlendshapeTargets(facialAnimation);
        }

        /// <summary>
        /// direct blendshape setter for eyeblink.
        /// TODO: this is too external - implementation should be moved in here.
        /// </summary>
        public void SetBlendshapesForEyeBlink(string blendShapeName, float weight)
        {
            var blendShapeDictionary = BlendShapeUtils.GetBlendshapesDic(_smr.sharedMesh);
            _smr.SetBlendShapeWeight(blendShapeDictionary[blendShapeName], weight);
        }

        public void Update(bool dampen)
        {
            foreach (var indexWeight in _blendshapeControls)
            {
                TrySetBlendShapeWeight(indexWeight, dampen);
            }
        }
        
        /// <summary>
        /// Tries to set the currentWeight of an animation blendshape
        /// </summary>
        /// <param name="indexWeight">Dictionary pairing formed of the blendshape index and the control object</param>
        /// <param name="dampen">Wether to dampen the facial expression or not. Lowers intensity by 0.75 currently</param>
        private void TrySetBlendShapeWeight(KeyValuePair<int, BlendshapeControl> indexWeight, bool dampen)
        {
            indexWeight.Value.Update();

            if (!indexWeight.Value.IsAnimating)
            {
                return;
            }

            var targetWeight = dampen ? indexWeight.Value.CurrentWeight * 0.75f
                : indexWeight.Value.CurrentWeight;

            _smr.SetBlendShapeWeight(indexWeight.Key, targetWeight);
        }
    }
}
