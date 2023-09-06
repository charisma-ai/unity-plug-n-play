using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class MoveToParameters : TaskParameters
    {
        internal Transform Target => _target;

        [SerializeField]
        private Transform _target;

        internal float StoppingDistance => _stoppingDistance;

        [SerializeField]
        private float _stoppingDistance = 0.25f;

        // Need empty constructor for serialization
        public MoveToParameters()
        {

        }

        public MoveToParameters(Transform target, float stoppingDistance = 0.25f)
        {
            _target = target;
            _stoppingDistance = stoppingDistance;
        }
    }
}
