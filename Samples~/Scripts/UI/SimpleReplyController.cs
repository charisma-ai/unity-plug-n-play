using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class SimpleReplyController : MonoBehaviour
    {
        private enum PlayerInputState
        {
            WaitingForMessage,
            PromptToReply,
            Listening,
            Pending,
            SendReply
        }

        private enum InputMode
        {
            None,
            Writing,
            SpeechToText
        }

        [SerializeField]
        private SimpleReplyInputField _playerInputField;

        [SerializeField]
        private GameObject _promptParent;

        [SerializeField]
        private SimpleReplyStatusDisplay _display;

        private string _pendingMessage = "";

        private const float MESSAGE_COMPLETE_BUFFER = 5.0f;
        private const float TIME_UNTIL_REPLY_IS_SENT = 2.5f;
        private bool _startTimer;
        private float _pendingTimer;

        [SerializeField]
        private PlayerInputState _state;

        [SerializeField]
        private bool _readyToReply;

        [SerializeField]
        private InputMode _input = InputMode.None;

        private void Start()
        {
            Cursor.visible = false;
        }

        public void Update()
        {
            var isPendingMessageNull = string.IsNullOrEmpty(_pendingMessage);

            switch (_state)
            {
                case PlayerInputState.WaitingForMessage:
                    _playerInputField.SetHidden(true);
                    _promptParent.SetActive(false);
                    _display.SetHidden(true);
                    if (_readyToReply)
                    {
                        SetState(PlayerInputState.PromptToReply);
                        _playerInputField.SetHidden(false);
                        _playerInputField.SetFaded(true);
                        _promptParent.SetActive(true);
                        _display.SetHidden(false);
                        _display.SetWaitingReply();
                        _readyToReply = false;
                    }
                    break;
                case PlayerInputState.PromptToReply:
                    if (_input != InputMode.None)
                    {
                        _playerInputField.SetFaded(false);
                        _display.SetListening();
                        _startTimer = false;
                        SetState(PlayerInputState.Listening);
                    }
                    break;
                case PlayerInputState.Listening:

                    // reset timer if we're fidgeting around with arrows
                    if (Input.GetKeyDown(KeyCode.LeftArrow)
                        || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
                    }

                    if (_startTimer)
                    {
                        _pendingTimer -= Time.deltaTime;
                        if (_pendingTimer <= 0.0f && !isPendingMessageNull)
                        {
                            _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
                            _display.SetTimeToSubmitBar(true);
                            SetState(PlayerInputState.Pending);
                        }
                    }
                    else
                    {
                        if (!isPendingMessageNull)
                        {
                            _startTimer = true;
                            _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
                        }
                    }

                    break;
                case PlayerInputState.Pending:

                    // reset timer if we're fidgeting around with arrows
                    if (Input.GetKeyDown(KeyCode.LeftArrow)
                        || Input.GetKeyDown(KeyCode.RightArrow)
                        || isPendingMessageNull)
                    {
                        _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
                        _display.SetTimeToSubmitBar(false);
                        SetState(PlayerInputState.Listening);
                    }

                    _pendingTimer -= Time.deltaTime;
                    _display.SetTimeToSubmit(_pendingTimer, TIME_UNTIL_REPLY_IS_SENT);
                    if (_pendingTimer <= 0.0f)
                    {
                        _display.SetTimeToSubmitBar(false);
                        SetState(PlayerInputState.SendReply);
                    }
                    break;
                case PlayerInputState.SendReply:
                    SetState(PlayerInputState.WaitingForMessage);
                    _playerInputField.Focus(false);
                    _pendingMessage = default;
                    _input = InputMode.None;
                    break;
            }
        }

        internal void SetReplyText(string recognizedText)
        {
            if (_input == InputMode.SpeechToText)
            {
                _playerInputField.SetText(recognizedText);
                _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
            }
        }

        internal void FocusInputField(bool focusInputField, bool isSpeech)
        {
            _display.SetTimeToSubmitBar(false);
            _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;

            if (isSpeech)
            {
                _input = InputMode.SpeechToText;

                if (!focusInputField)
                {
                    _playerInputField.Focus(focusInputField);
                }
            }
            else
            {
                _input = InputMode.Writing;
                _playerInputField.Focus(focusInputField);
            }
        }

        internal bool HasReplied()
        {
            return _state == PlayerInputState.SendReply && !string.IsNullOrEmpty(_pendingMessage);
        }

        internal bool IsWritingReply()
        {
            return _state == PlayerInputState.Listening || _state == PlayerInputState.Pending;
        }

        public void SetPendingReplyString(string message)
        {
            SetPendingMessage(message);
        }

        public void SendReplyString(string message)
        {
            SetPendingMessage(message);
        }

        public void SetInputFieldActive(bool flag)
        {
            _readyToReply = flag;
            if (!_readyToReply)
            {
                _display.SetTimeToSubmitBar(false);
                SetState(PlayerInputState.SendReply);
            }
            else
            {
                _pendingMessage = default;
                SetState(PlayerInputState.WaitingForMessage);
            }
        }

        private void SetState(PlayerInputState state)
        {
            _state = state;
        }

        internal string GetReplyText()
        {
            return _pendingMessage;
        }

        private void SetPendingMessage(string message)
        {
            _pendingMessage = message;

            if (_state == PlayerInputState.Listening)
            {
                _pendingTimer = MESSAGE_COMPLETE_BUFFER;
            }
            else if (_state == PlayerInputState.Pending)
            {
                _pendingTimer = TIME_UNTIL_REPLY_IS_SENT;
            }
        }

    }
}
