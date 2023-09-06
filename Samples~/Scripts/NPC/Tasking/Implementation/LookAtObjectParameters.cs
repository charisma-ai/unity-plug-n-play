using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class LookAtObjectParameters : TaskParameters
    {
        public GameObject Target => _target;

        [SerializeField]
        private GameObject _target;

        public float DurationMS => _durationMS;

        [SerializeField]
        private float _durationMS;

        // Need empty constructor for serialization
        public LookAtObjectParameters()
        {

        }

        public LookAtObjectParameters(GameObject target, float durationMs)
        {
            _target = target;
            _durationMS = durationMs;
        }
    }
}
