using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    internal class SimpleReplyInputField : MonoBehaviour
    {
        [SerializeField]
        private GameObject _fadedBG;

        [SerializeField]
        private GameObject _unfadedBG;

        [SerializeField]
        private GameObject _parent;

        [SerializeField]
        private InputField _inputField;

        private void Start()
        {
            _inputField.onValidateInput += delegate (string input, int charIndex, char addedChar)
            {
                return DisableSpecialInputs(addedChar);
            };
        }

        internal void SetFaded(bool v)
        {
            _fadedBG.SetActive(!v);
            _unfadedBG.SetActive(v);
        }

        internal void SetHidden(bool v)
        {
            _parent.SetActive(!v);
            _inputField.text = "";
        }

        internal void Focus(bool v)
        {
            if (v)
            {
                _inputField.ActivateInputField();
            }
            else
            {
                _inputField.DeactivateInputField();
            }
        }

        internal bool IsFocused()
        {
            return _inputField.isFocused;
        }

        internal void SetText(string recognizedText)
        {
            _inputField.text = recognizedText;
        }

        private char DisableSpecialInputs(char charToValidate)
        {
            if (charToValidate == '\t')
            {
                charToValidate = '\0';
            }

            if (charToValidate == '\n')
            {
                charToValidate = '\0';
            }

            return charToValidate;
        }
    }
}
