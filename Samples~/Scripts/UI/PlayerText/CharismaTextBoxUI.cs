using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    internal class CharismaTextBoxUI : MonoBehaviour
    {
        public RectTransform TextParent => _textParent;

        private float _lifetimeDuration;

        [SerializeField]
        private Text _textOutput;

        [SerializeField]
        private RectTransform _textParent;

        [SerializeField]
        private Text _nameField;

        [SerializeField]
        private Image _arrowBGSprite;
        [SerializeField]
        private Image _arrowlessBGSprite;

        [SerializeField]
        private float _minimumDistance = 5.0f;

        [SerializeField]
        private float _maximumDistance = 15f;

        [SerializeField]
        private float _minimumScale = 1.0f;

        [SerializeField]
        private float _maximumScale = 0.65f;

        private void Start()
        {
            _textParent?.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateLifetime();
        }

        private void UpdateLifetime()
        {
            if (_lifetimeDuration > 0)
            {
                _lifetimeDuration -= Time.deltaTime;
            }
            else
            {
                _textParent.gameObject.SetActive(false);
            }
        }

        public void SetActorName(string name)
        {
            // apply first character upper case to name
            _nameField.text = string.Concat(name[0].ToString().ToUpper(), name.Substring(1).ToLower());
        }

        internal void PrintTextMessage(string messageBody, float durationMs)
        {
            if (durationMs < 500)
            {
                durationMs = 2500;
            }

            _textOutput.text = messageBody;
            _textParent.gameObject.SetActive(true);

            _lifetimeDuration = (float)durationMs / 1000 + 4f;
        }

        internal void SetIsOnScreen(bool onScreen)
        {
            if (onScreen)
            {
                _arrowBGSprite.gameObject.SetActive(true);
                _arrowlessBGSprite.gameObject.SetActive(false);
            }
            else
            {
                _arrowBGSprite.gameObject.SetActive(false);
                _arrowlessBGSprite.gameObject.SetActive(true);
            }
        }

        internal void SetDistance(float distance)
        {
            var resultDistance = Math.Clamp(distance, _minimumDistance, _maximumDistance);

            var scale = _minimumScale;

            if(distance <= _minimumDistance)
            {
                scale = _minimumScale;
            }
            else if (distance > _minimumDistance && distance < _maximumDistance)
            {
                // reduce the step to base 0
                var step = resultDistance - _minimumDistance;
                // and convert it to a 0-1 ratio
                step /= (_maximumDistance - _minimumDistance);

                // use step as a way to lerp between min scale and max scale
                scale = Mathf.Lerp(_minimumScale, _maximumScale, step);
            }
            else if (distance >= _maximumDistance)
            {
                scale = _maximumScale;
            }

            this.gameObject.transform.localScale = new Vector3(scale, scale);
        }
    }
}
