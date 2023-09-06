using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    public class SimpleCharismaReplyUI : CharismaPlayer
    {
        [SerializeField]
        [Tooltip("Text source, used to get")]
        private InputField _input;

        [SerializeField]
        [Tooltip("Reply button, triggers the reply event on click.")]
        private Button _replyButton;

        private Action<string> _sendTextEvent;

        public override event StartSpeechRecognitionDelegate StartVoiceRecognition;
        public override event StartSpeechRecognitionDelegate StopVoiceRecognition;

        // Start is called before the first frame update
        void Start()
        {
            _replyButton.onClick.AddListener(SendPlayerMessage);
        }

        private void Update()
        {
            if (Input.GetKeyDown(key: KeyCode.Return))
            {
                SendPlayerMessage();
            }
        }

        #region Public functions
        public override void SetOnReplyCallback(Action<string> sendReply)
        {
            _sendTextEvent = sendReply;
        }

        #endregion

        #region Private functions

        private void SendPlayerMessage()
        {
            if (string.IsNullOrEmpty(value: _input.text))
            {
                return;
            }

            _sendTextEvent?.Invoke(_input.text);

            _input.text = string.Empty;
        }

        public override void SetReadyToReply()
        {

        }

        public override void SendSpeechResult(string recognizedText)
        {


        }

        #endregion
    }
}
