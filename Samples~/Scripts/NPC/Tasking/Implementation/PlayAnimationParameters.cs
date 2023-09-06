using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class PlayAnimationParameters : TaskParameters
    {
        public string AnimationName => _animationName;

        [SerializeField]
        private string _animationName;

        public bool Force => _force;

        [SerializeField]
        private bool _force;

        // Need empty constructor for serialization
        public PlayAnimationParameters()
        {

        }
        public PlayAnimationParameters(string animationName, bool force = false)
        {
            _animationName = animationName;
            _force = force;
        }
    }
}
