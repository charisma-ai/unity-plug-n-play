using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class AnimationMetadata
    {
        public List<AnimationLayerData> AnimationLayers => _animationLayers;
        public List<AnimationParameter> AnimationParameters => _animationParameters;

        [SerializeField]
        private List<AnimationLayerData> _animationLayers;

        [SerializeField]
        private List<AnimationParameter> _animationParameters;

        public bool ContainsAnimation(string animationId, out AnimationLayerData result, out string internalAnimationName)
        {
            result = default;
            internalAnimationName = string.Empty;

            foreach (var layer in _animationLayers)
            {
                foreach (var animation in layer.AnimationNodeData)
                {
                    if (string.Equals(animation.AnimationNodeName, animationId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = layer;
                        internalAnimationName = animation.AnimationNodeName;
                        return true;
                    }
                }
            }

            return false;
        }

        public AnimationParameter FindParameterWithLabel(string v)
        {
            foreach (var param in _animationParameters)
            {
                if (param.Name == v)
                {
                    return param;
                }
            }

            return default;
        }

#if UNITY_EDITOR
        internal void CreateAnimationData(AnimatorController animControllerReference)
        {
            AddAllParameters(animControllerReference);
            AddAllAnimationLayers(animControllerReference);
        }

        private void AddAllAnimationLayers(AnimatorController animControllerReference)
        {
            var existingLayerCount = _animationLayers.Count;

            if (existingLayerCount != 0)
            {
                for (int i = 0; i < existingLayerCount; i++)
                {
                    var layer = animControllerReference.layers.FirstOrDefault
                        (x => x.name == _animationLayers[i].LayerName
                         && i == _animationLayers[i].LayerId);

                    // animator has layer, its already been registered
                    if (layer != default)
                    {
                        UpdateLayer(i, layer);
                        continue;
                    }
                    // animator does not have registered layer
                    else
                    {
                        _animationLayers.RemoveAt(i);
                        i--;
                        existingLayerCount--;
                        continue;
                    }

                }

            }

            int index = 0;
            foreach (var layer in animControllerReference.layers)
            {
                if (!_animationLayers.Any(x => x.LayerName == layer.name
                    && x.LayerId == index))
                {
                    var animationLayer = new AnimationLayerData();
                    animationLayer.LayerName = layer.name;
                    animationLayer.LayerId = index;

                    LoadAnimationsFromStateMachine(ref animationLayer, layer.stateMachine);

                    _animationLayers.Add(animationLayer);
                }
                index++;
            }
        }

        private void UpdateLayer(int i, AnimatorControllerLayer layer)
        {
            var animationLayer = _animationLayers[i];
            var existingNodeCount = _animationLayers[i].AnimationNodeData.Count;

            for (int x = 0; x < existingNodeCount; x++)
            {
                if (!DoesStateMachineContainNode(_animationLayers[i].AnimationNodeData[x], layer.stateMachine))
                {
                    _animationLayers[i].AnimationNodeData.RemoveAt(x);
                    x--;
                    existingNodeCount--;
                }
            }

            LoadAnimationsFromStateMachine(ref animationLayer, layer.stateMachine);
            _animationLayers[i] = animationLayer;
        }

        private bool DoesStateMachineContainNode(AnimationPlayableAnimationNode animationPlayableAnimationNode, AnimatorStateMachine stateMachine)
        {
            foreach (var node in stateMachine.states)
            {
                if (node.state.motion != default
                    && node.state.name == animationPlayableAnimationNode.AnimationNodeName)
                {
                    return true;
                }
            }

            foreach (var sub in stateMachine.stateMachines)
            {
                if (DoesStateMachineContainNode(animationPlayableAnimationNode, sub.stateMachine))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddAllParameters(AnimatorController animControllerReference)
        {
            var existingParameterCount = _animationParameters.Count;

            // just add everything if its an empty list
            if (existingParameterCount != 0)
            {
                for (int i = 0; i < existingParameterCount; i++)
                {
                    // animator has param, its already been registered
                    if (animControllerReference.parameters.Any(x => x.name == _animationParameters[i].Name
                         && x.type == _animationParameters[i].ParamType))
                    {
                        continue;
                    }
                    // animator does not have registered param
                    else
                    {
                        _animationParameters.RemoveAt(i);
                        i--;
                        existingParameterCount--;
                        continue;
                    }
                }
            }

            // add anything that isn't already there
            foreach (var param in animControllerReference.parameters)
            {
                if (!_animationParameters.Any(x => x.Name == param.name
                        && x.ParamType == param.type))
                {
                    _animationParameters.Add(new AnimationParameter(param));
                }
            }

        }

        private void LoadAnimationsFromStateMachine(ref AnimationLayerData animationLayer, AnimatorStateMachine stateMachine)
        {
            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                AddAnimationFromStateMachineEntry(ref animationLayer, subStateMachine);
            }

            foreach (var state in stateMachine.states)
            {
                if (state.state.motion != default)
                {
                    AddMotionsToLayer(ref animationLayer, state.state.name);
                }
            }
        }

        private void AddMotionsToLayer(ref AnimationLayerData animationLayer, string name)
        {
            animationLayer.TryAddNodeName(name);
        }

        private void AddAnimationFromStateMachineEntry(ref AnimationLayerData animationLayer, ChildAnimatorStateMachine subStateMachine)
        {
            LoadAnimationsFromStateMachine(ref animationLayer, subStateMachine.stateMachine);
        }

#endif

    }
}
