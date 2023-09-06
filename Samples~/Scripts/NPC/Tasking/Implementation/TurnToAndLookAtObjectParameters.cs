
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class TurnToAndLookAtObjectParameters : TaskParameters
    {
        public GameObject Target => _target;

        [SerializeField]
        private GameObject _target;

        public float DurationMS => _durationMS;

        [SerializeField]
        private float _durationMS;

        // Need empty constructor for serialization
        public TurnToAndLookAtObjectParameters()
        {

        }

        public TurnToAndLookAtObjectParameters(GameObject target, float durationMs)
        {
            _target = target;
            _durationMS = durationMs;
        }
    }
}
