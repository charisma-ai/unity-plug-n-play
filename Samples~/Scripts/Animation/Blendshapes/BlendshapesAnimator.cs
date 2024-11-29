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

            public bool IsAnimating => _motion != BlendshapeMotion.Inactive && _motion != BlendshapeMotion.HoldIntensity;

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
                _targetWeight = _baselineWeight;
                _currentWeight = _baselineWeight;
                _motion = BlendshapeMotion.Inactive;
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
            internal void ResetAnimate(float speed)
            {
                _motion = BlendshapeMotion.ReturnToBaseValue;
                _targetWeight = _baselineWeight;
                _maxblendingSpeed = speed;
                _holdTimer = 0;
            }

            internal void Update()
            {
                switch (_motion)
                {
                    case BlendshapeMotion.BlendToIntensity:
                        if (BlendToTarget())
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
                            _targetWeight = _baselineWeight;
                            _blendingVelocity = 0f;
                            _motion = BlendshapeMotion.ReturnToBaseValue;
                        }
                        break;
                    case BlendshapeMotion.ReturnToBaseValue:
                        if (BlendToTarget())
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
            private bool BlendToTarget()
            {
                var resultWeight = Mathf.SmoothDamp(_currentWeight, _targetWeight,
                    ref _blendingVelocity, 0.3f,
                    _maxblendingSpeed, deltaTime: Time.deltaTime);
                
                // increment by 1, smoothdamp can take ages due to smoothening and floating point error
                _currentWeight = resultWeight;
                
                // if no longer moving, we've reached our destination
                if (Mathf.Abs(_currentWeight-_targetWeight) < 0.05f)
                {
                    return true;
                }

                return false;
            }
            
        }

        private Dictionary<int, BlendshapeControl> _blendshapeControls = new Dictionary<int, BlendshapeControl>();

        // Speed of animation in MS per frame
        private const float ANIMATION_SPEED_MS = 150f;

        private const float MINIMUM_EXPRESSION_DURATION = 3f;

        private SkinnedMeshRenderer _smr;

        // Since animating for a single mesh
        // store the collection of possible blendshapes here
        private FacialExpressionBlendshapeCollection _blendshapeCollection;

        // Empty container, used to prevent new dicts created at runtime
        private Dictionary<int, float> _emptyDictionary = new Dictionary<int, float>();
        private List<int> _lipsyncBlendShapes = new List<int>();

        public BlendshapesAnimator(SkinnedMeshRenderer smr, List<string> lipsyncBlendshapeNames)
        {
            _smr = smr;
            _blendshapeCollection = new FacialExpressionBlendshapeCollection(_smr.sharedMesh);

            var blendShapeDictionary = BlendShapeUtils.GetBlendshapesDic(smr.sharedMesh);
            foreach (var entry in blendShapeDictionary)
            {
                var baselineWeight = smr.GetBlendShapeWeight(entry.Value);
                _blendshapeControls.Add(entry.Value, new BlendshapeControl(baselineWeight));
            }

            foreach (var lipsyncBlendshapeName in lipsyncBlendshapeNames)
            {
                if (blendShapeDictionary.TryGetValue(lipsyncBlendshapeName, out var index))
                {
                    _lipsyncBlendShapes.Add(index);
                }
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
            bool removeFacialExpression = multiplier == 0 || facialExpression == default;

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
                    if (newTargets.TryGetValue(blend.Key, out var target))
                    {
                        var weight = target * multiplier;
                        blend.Value.StartAnimating(weight, ANIMATION_SPEED_MS, (duration/1000f) + MINIMUM_EXPRESSION_DURATION);
                    }
                    else
                    {
                        blend.Value.ResetAnimate(ANIMATION_SPEED_MS);
                    }
                }
            }
            else
            {
                foreach (var blendshape in newTargets)
                {
                    _blendshapeControls[blendshape.Key].ForceAnimate(blendshape.Value * multiplier, duration + MINIMUM_EXPRESSION_DURATION);
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
                _blendshapeCollection.Register(facialAnimation, _smr);
            }
            
            newTargets = _blendshapeCollection.GetBlendshapeTargets(facialAnimation);

            foreach (var lipsyncBlendShape in _lipsyncBlendShapes)
            {
                if (newTargets.ContainsKey(lipsyncBlendShape))
                {
                    newTargets.Remove(lipsyncBlendShape);
                }
            }
        }

        /// <summary>
        /// direct blendshape setter for eyeblink.
        /// </summary>
        public void SetBlendshapesForEyeBlink(string blendShapeName, float weight)
        {
            var blendShapeDictionary = BlendShapeUtils.GetBlendshapesDic(_smr.sharedMesh);
            _smr.SetBlendShapeWeight(blendShapeDictionary[blendShapeName], weight);
        }

        public void UpdateShapeWeight()
        {
            foreach (var indexWeight in _blendshapeControls)
            {
                TrySetBlendShapeWeight(indexWeight);
            }
        }
        
        /// <summary>
        /// Tries to set the currentWeight of an animation blendshape
        /// </summary>
        /// <param name="indexWeight">Dictionary pairing formed of the blendshape index and the control object</param>
        /// <param name="dampen">Wether to dampen the facial expression or not. Lowers intensity by 0.75 currently</param>
        private void TrySetBlendShapeWeight(KeyValuePair<int, BlendshapeControl> indexWeight)
        {
            indexWeight.Value.Update();

            if (!indexWeight.Value.IsAnimating)
            {
                return;
            }
            
            _smr.SetBlendShapeWeight(indexWeight.Key, indexWeight.Value.CurrentWeight);
        }
    }
}
