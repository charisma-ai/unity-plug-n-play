using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    internal class CharismaTextBoxLocation : MonoBehaviour
    {
        public string ActorName => _actorName;

        private string _actorName;

        private CharismaTextBoxUI _textbox;

        private Vector3 _startingPosition;
        private Vector3 _offset;

        [SerializeField]
        private float _minimumDistance = 3f;

        [SerializeField]
        private float _maximumDistance = 25f;

        [SerializeField]
        private float _minimumOffset = 0f;

        [SerializeField]
        private float _maximumOffset = 1f;

        private void Awake()
        {
            _startingPosition = this.gameObject.transform.localPosition;
        }

        internal void AddUIComponent(CharismaTextBoxUI charismaTextBoxUI)
        {
            _textbox = charismaTextBoxUI;
            _textbox.SetActorName(_actorName);
        }

        internal void SetActorName(string actorName)
        {
            _actorName = actorName;
        }

        internal void PrintTextMessage(string messageBody, float durationMs = 0)
        {
            if(_textbox == default)
            {
                return;
            }

            _textbox.PrintTextMessage(messageBody, durationMs);
        }

        internal void SetDistance(float distance)
        {
            var resultDistance = Math.Clamp(distance, _minimumDistance, _maximumDistance);

            _offset = Vector3.zero;

            if (distance <= _minimumDistance)
            {
                _offset = new Vector3(0, _minimumOffset);
            }
            else if (distance > _minimumDistance && distance < _maximumDistance)
            {
                // reduce the step to base 0
                var step = resultDistance - _minimumDistance;
                // and convert it to a 0-1 ratio
                step /= (_maximumDistance - _minimumDistance);

                // use step as a way to lerp between min scale and max scale
                _offset = new Vector3(0, Mathf.Lerp(_minimumOffset, _maximumOffset, step));
            }
            else if (distance >= _maximumDistance)
            {
                _offset = new Vector3(0, _maximumOffset);
            }

            this.gameObject.transform.localPosition = _startingPosition + _offset;
        }
    }
}
