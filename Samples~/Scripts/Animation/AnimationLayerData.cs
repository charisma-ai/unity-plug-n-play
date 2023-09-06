using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CharismaSDK.PlugNPlay
{
    public enum LayerMasking
    {
        Fullbody,
        Head,
        Upperbody
    }

    [Serializable]
    public class AnimationLayerData
    {
        [ReadOnly]
        [SerializeField]
        public string LayerName;

        [ReadOnly]
        [SerializeField]
        public int LayerId;

        public LayerMasking LayerMask => _layerMaskType;

        public List<AnimationPlayableAnimationNode> AnimationNodeData => _animationNodeData;

        [SerializeField]
        private LayerMasking _layerMaskType = LayerMasking.Fullbody;

        [ReadOnly]
        [SerializeField]
        private List<AnimationPlayableAnimationNode> _animationNodeData = new List<AnimationPlayableAnimationNode>();

#if UNITY_EDITOR
        internal bool TryAddNodeName(string name)
        {
            if (!_animationNodeData.Any(x => x.AnimationNodeName == name))
            {
                _animationNodeData.Add(new AnimationPlayableAnimationNode(name));
                return true;
            }
            else
            {
                return false;
            }
        }
#endif

        internal AnimationPlayableAnimationNode GetRandomAnimationNodeWithTag(AnimationFlags tag, string feelingToApply)
        {
            AnimationPlayableAnimationNode lastNode = default;
            AnimationPlayableAnimationNode genericNode = default;
            foreach (var node in _animationNodeData)
            {
                var needsFeeling = string.Equals(node.AssociatedCharismaEmotion, feelingToApply, StringComparison.InvariantCultureIgnoreCase)
                    || feelingToApply == "";

                var hasTag = node.Tags.HasFlag(tag);

                if (hasTag && needsFeeling)
                {
                    lastNode = node;

                    // will leave early if it random chance permits
                    if (UnityEngine.Random.value > 0.75f)
                    {
                        return lastNode;
                    }
                }

                if (hasTag && node.AssociatedCharismaEmotion == "")
                {
                    genericNode = node;
                }
            }

            // assign any generic node we find, may as well give an animation even if its not associated with an emotion
            if (lastNode == default)
            {
                lastNode = genericNode;
            }

            return lastNode;
        }
    }

    [Serializable]
    public class AnimationPlayableAnimationNode
    {
        public string AnimationNodeName => _animationNodeName;

        public AnimationFlags Tags => _animationTags;

        public string AssociatedCharismaEmotion => _associatedCharismaEmotion;

        [ReadOnly]
        [SerializeField]
        private string _animationNodeName;

        [SerializeField]
        private AnimationFlags _animationTags;

        [SerializeField]
        private string _associatedCharismaEmotion;

        public AnimationPlayableAnimationNode(string name)
        {
            _animationNodeName = name;
        }
    }
}
