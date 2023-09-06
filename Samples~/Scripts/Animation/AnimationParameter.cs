using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class AnimationParameter
    {
        public AnimatorControllerParameterType ParamType => _type;

        [ReadOnly]
        [SerializeField]
        private AnimatorControllerParameterType _type;

        public string Name => _name;

        [ReadOnly]
        [SerializeField]
        private string _name;

        public AnimationParameter(AnimatorControllerParameter param)
        {
            _name = param.name;
            _type = param.type;
        }
    }
}
