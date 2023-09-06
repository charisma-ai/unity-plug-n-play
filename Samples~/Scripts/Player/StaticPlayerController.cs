using CharismaSDK.StateMachine;
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class StaticPlayerController : CharismaPlayer
    {
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private SimplePlayerUIComponent _playerUI;

        private enum PlayerState
        {
            NotReplying,
            Replying
        }

        private SimpleStateMachine<PlayerState> _fsm;
        private Action<string> _sendReply;

        public override event StartSpeechRecognitionDelegate StartVoiceRecognition;
        public override event StartSpeechRecognitionDelegate StopVoiceRecognition;

        private bool _isListening;
        private bool _isWriting;

        private PlayerTextFieldController _playerTextField;

        // Update is called once per frame
        void Awake()
        {
            InitialiseStateMachine();

            _playerTextField = new PlayerTextFieldController(_playerUI);
            _playerTextField.OnReplySentDelegate += OnReplySent;
            _playerTextField.OnSpeechRecognitionDelegate += OnSpeechRecognition;
        }

        private void OnReplySent(string resultInput)
        {
            _sendReply?.Invoke(resultInput);
            _fsm.SetState(PlayerState.NotReplying);
        }

        private void OnSpeechRecognition(bool started)
        {
            if (started)
            {
                StartVoiceRecognition.Invoke(true);
            }
            else
            {
                StopVoiceRecognition.Invoke(false);
            }
        }

        private void InitialiseStateMachine()
        {
            _fsm = new SimpleStateMachine<PlayerState>();
            _fsm.AddState(PlayerState.NotReplying)
                .OnEntry(OnMovementEnter)
                .OnUpdate(OnMovementUpdate);

            _fsm.AddState(PlayerState.Replying)
                .OnEntry(OnReplyEnter)
                .OnUpdate(OnReplyUpdate);
        }

        private void Update()
        {
            _fsm.Update();
        }


        private void OnMovementEnter()
        {

        }
        private void OnMovementUpdate()
        {
        }

        private void OnReplyEnter()
        {
            _playerUI.SetPlayerInputFieldActive(true);
        }

        private void OnReplyUpdate()
        {
            _playerTextField.Update();
        }

        public override void SetOnReplyCallback(Action<string> sendReply)
        {
            _sendReply = sendReply;
        }

        public override void SetReadyToReply()
        {
            _fsm.SetState(PlayerState.Replying);
        }

        public override void SendSpeechResult(string recognizedText)
        {
            _playerUI.SetReplyText(recognizedText);
        }
    }
}
