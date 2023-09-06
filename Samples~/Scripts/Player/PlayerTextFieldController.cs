using CharismaSDK.StateMachine;
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class PlayerTextFieldController
    {
        private bool _isListening;
        private bool _isWriting;

        private SimplePlayerUIComponent _playerUI;

        public delegate void OnReplySent(string resultInput);
        public delegate void OnSpeechRecognition(bool started);

        public event OnReplySent OnReplySentDelegate;
        public event OnSpeechRecognition OnSpeechRecognitionDelegate;

        public PlayerTextFieldController(SimplePlayerUIComponent playerUI)
        {
            _playerUI = playerUI;
        }

        public void Update()
        {
            UpdateSpeechRecognition();

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                _playerUI.EditText();
                _isWriting = true;
            }

            UpdateReplySubmission();
        }

        private void UpdateReplySubmission()
        {
            if (_isListening)
            {
                return;
            }

            var resultInput = _playerUI.GetReplyText();

            var canSubmitReply = resultInput != ""
                && resultInput != null;

            if ((Input.GetKey(KeyCode.Return) && canSubmitReply)
                || _playerUI.HasReplyBeenSubmitted())
            {
                _playerUI.SetPlayerInputFieldActive(false);
                _isWriting = false;
                OnReplySentDelegate.Invoke(resultInput);
            }
        }

        private void UpdateSpeechRecognition()
        {
            if (_isWriting)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (!_isListening)
                {
                    OnSpeechRecognitionDelegate.Invoke(true);
                    _playerUI.EditViaSpeech();
                    Debug.Log("[StartVoiceRegonition]");
                    _isListening = true;
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (_isListening)
                {
                    OnSpeechRecognitionDelegate.Invoke(false);
                    Debug.Log("[StopVoiceRecognition]");
                    _isListening = false;
                }
            }
        }
    }
}
