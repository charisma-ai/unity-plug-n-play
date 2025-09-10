using CharismaSDK.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [RequireComponent(typeof(CharacterController))]
    public class CharismaPlayerController : CharismaPlayer
    {
        private CharacterController _characterController;

        [SerializeField] private Camera _camera;
        [SerializeField] private SimplePlayerUIComponent _playerUI;
        [SerializeField] private CharismaInteractableDetector _interactionDetector;
        [SerializeField] private float _maximumMovementVelocity = 10;
        [SerializeField] private bool _mouseLookEnabled = true;
        [SerializeField] private bool _keyboardMovementEnabled = true;
        [SerializeField] private bool _interruptionsEnabled = true;
        
        [Header("Speeds")] 
        [SerializeField] [Range(0, 1)]
        private float _acceleration = 0.75f;

        [SerializeField] 
        private Vector2 _moveSpeed = new Vector2(2.75f, 2.75f);

        [SerializeField] 
        private float _gravity = 9.8f;

        [Header("Looking")] [SerializeField] 
        private float _lookSensitivity = 5;

        [SerializeField] 
        private bool _doSmoothing;

        [Range(0, 1)] [SerializeField] 
        private float _smoothing = 0.6f;

        private PnpPlaythroughInstance _playthroughInstance;
        private List<CharismaHumanoidActor> _interruptNPCTargets;
        private Vector3 _startPosition;
        private Vector3 _startRotation;
        private Vector2 _lastFrameInput;
        private string _pendingText;
        private bool _sentReplyThisCycle;
        private bool _isTalking;
        private bool _isWriting;
        private Action<string> _sendReply;
        private Vector3 _moveDirection;
        private Vector2 _actualDiffMouse;
        private float _cameraVerticalRotation;
        private bool _skipMovement;

        public override event SpeechRecognitionStatusDelegate StartVoiceRecognition;
        public override event SpeechRecognitionStatusDelegate StopVoiceRecognition;

        public static CharismaPlayerController Instance;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _playthroughInstance = FindObjectOfType<PnpPlaythroughInstance>();
            _interruptNPCTargets = FindObjectsOfType<CharismaHumanoidActor>().ToList();
            _characterController = GetComponent<CharacterController>();
            _playerUI.OnTextUpdate += OnTextUpdate;

            _playthroughInstance.OnTapContinueRequest += OnTapToContinueRequested;
            
            // move initially to snap to floor
            _characterController.Move(Vector3.zero);
            _characterController.enabled = false;

            _startPosition = this.gameObject.transform.position;
            _startRotation = this.gameObject.transform.eulerAngles;
        }

        private void Start()
        {
            _characterController.enabled = true;

            this.gameObject.transform.position = _startPosition;
            this.gameObject.transform.eulerAngles = _startRotation;
        }

        private void Update()
        {
            OnInteractionUpdate();
            OnLookUpdate();

            if (!_playerUI.IsUserWritingReply())
            {
                OnMoveUpdate();
            }
        }

        private void OnLookUpdate()
        {
            if (!_mouseLookEnabled || _isWriting || _isTalking)
            {
                return;
            }

            _actualDiffMouse = Vector2.zero;
            _actualDiffMouse.x = Input.GetAxis("Mouse X");
            _actualDiffMouse.y = Input.GetAxis("Mouse Y");
            _actualDiffMouse *= _lookSensitivity;

            if (_actualDiffMouse * Time.timeScale != Vector2.zero)
            {
                _cameraVerticalRotation -= _actualDiffMouse.y;
                _cameraVerticalRotation = Mathf.Clamp(_cameraVerticalRotation, -90f, 90f);
                _camera.transform.localEulerAngles = Vector3.right * _cameraVerticalRotation;

                this.gameObject.transform.Rotate(Vector3.up * _actualDiffMouse.x);
            }
        }

        private void OnMoveUpdate()
        {
            if (!_keyboardMovementEnabled || _isWriting || _isTalking)
            {
                return;
            }

            if (_interactionDetector.CurrentInteractable != default)
            {
                _playerUI.SetDisplayInteractPrompt(true);

                if (Input.GetKey(KeyCode.F))
                {
                    _interactionDetector.CurrentInteractable.Interact();
                }
            }
            else
            {
                _playerUI.SetDisplayInteractPrompt(false);
            }

            var inputX = Input.GetAxis("Horizontal");
            var inputY = Input.GetAxis("Vertical");

            if (_skipMovement)
            {
                if (!(inputX > 0 || inputX < 0 || inputY > 0 || inputY < 0))
                {
                    _skipMovement = false;
                }
                else
                {
                    return;
                }
            }

            var input2D = Vector2.zero;
            input2D.x = inputX;
            input2D.y = inputY;

            input2D.x = Mathf.Lerp(_lastFrameInput.x, input2D.x, _acceleration);
            input2D.y = Mathf.Lerp(_lastFrameInput.y, input2D.y, _acceleration);

            _moveDirection = Vector3.zero;
            _moveDirection += this.transform.right * input2D.x * _moveSpeed.x;
            _moveDirection += this.transform.forward * input2D.y * _moveSpeed.y;
            _moveDirection.y -= _gravity;

            //Clamp Movement
            _moveDirection = Vector3.ClampMagnitude(_moveDirection, _maximumMovementVelocity);

            //Apply Movement
            _characterController.Move(_moveDirection * Time.fixedDeltaTime);

            _lastFrameInput = input2D;
        }

        public override void SetOnReplyCallback(Action<string> sendReply)
        {
            _sendReply += sendReply;
        }

        public override void SetPlayerInputFieldActive(bool repliesEnabled)
        {
            _sentReplyThisCycle = false;
            _playerUI.SetPlayerInputFieldActive(repliesEnabled);
        }

        public override void SendSpeechResult(string recognizedText)
        {
            _playerUI.SetReplyText(recognizedText);
        }

        public override bool TryInterrupt()
        {
            if (!_interruptionsEnabled)
            {
                return false;
            }

            var anyNpcInterrupted = false;

            foreach (var npc in _interruptNPCTargets)
            {
                if (npc.IsTalking)
                {
                    anyNpcInterrupted = true;
                    npc.Interrupt();
                }
            }

            if (anyNpcInterrupted)
            {
                _playthroughInstance.SetAction("interrupt");
            }
            
            return anyNpcInterrupted;
        }

        public override void ForceSendLastInterruption()
        {
            // TODO - Not needed for now.
        }
        
        private void OnInteractionUpdate()
        {
            UpdateSpeechRecognition();
            UpdateTapContinue();
            UpdateReplySubmission();
        }

        private void UpdateReplySubmission()
        {
            if (_isTalking)
            {
                return;
            }

            var resultInput = _playerUI.GetReplyText();
            var canSubmitReply = !string.IsNullOrEmpty(resultInput);

            if (Input.GetKeyUp(KeyCode.Return) && !_isWriting)
            {
                _playerUI.EditText(true);
                _isWriting = true;
            }
            else if (Input.GetKeyUp(KeyCode.Return) && _isWriting)
            {
                _isWriting = false;
                _playerUI.EditText(false);

                if (canSubmitReply)
                {
                    _playerUI.SetPlayerInputFieldActive(false);
                    SendReply(resultInput);
                }
            }
        }

        private void UpdateSpeechRecognition()
        {
            if (_isWriting)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && !_isTalking)
            {
                OnSpeechRecognition(true);
                _playerUI.EditViaSpeech(true);
                _isTalking = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) && _isTalking)
            {
                OnSpeechRecognition(false);
                _playerUI.EditViaSpeech(false);
                _isTalking = false;
            }
        }
        
        private void UpdateTapContinue()
        {
            if (_isWriting || _isTalking || !_playerUI.IsTapToContinuePromptActive)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playthroughInstance.Tap();
                _playerUI.SetTapContinuePromptActive(false);
            }
        }

        private void SendReply(string resultInput)
        {
            if (!string.IsNullOrEmpty(_pendingText) && !_sentReplyThisCycle)
            {
                return;
            }

            var npcsInterrupted = TryInterrupt();

            if (!npcsInterrupted)
            {
                _sendReply?.Invoke(resultInput);
            }

            _sentReplyThisCycle = true;
        }

        private void OnSpeechRecognition(bool started)
        {
            if (started)
            {
                SetPendingText("");
                StartVoiceRecognition.Invoke(true);
            }
            else
            {
                StopVoiceRecognition.Invoke(false);
            }
        }

        private void SetPendingText(string text)
        {
            _playerUI.SetReplyText(text);
            _pendingText = text;
        }

        private void OnTextUpdate(string newText)
        {
            _pendingText = newText;
        }
        
        private void OnTapToContinueRequested()
        {
            _playerUI.SetTapContinuePromptActive(true);
        }
    }
}