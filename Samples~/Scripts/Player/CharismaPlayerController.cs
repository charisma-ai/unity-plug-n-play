using CharismaSDK.StateMachine;
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [RequireComponent(typeof(CharacterController))]
    public class CharismaPlayerController : CharismaPlayer
    {
        private CharacterController _characterController;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private SimplePlayerUIComponent _playerUI;

        [SerializeField]
        private CharismaInteractableDetector _interactionDetector;

        [SerializeField]
        private float _maximumMovementVelocity = 10;

        [Header("Speeds")]
        [SerializeField]
        [Range(0, 1)]
        private float _acceleration = 0.75f;
        [SerializeField]
        private Vector2 _moveSpeed = new Vector2(2.75f, 2.75f);

        [SerializeField]
        private float _gravity = 9.8f;

        [Header("Looking")]
        [SerializeField]
        private float _lookSensitivity = 5;

        [SerializeField]
        private bool _doSmoothing;
        [Range(0, 1)]
        [SerializeField]
        private float _smoothing = 0.6f;

        private PlayerTextFieldController _playerTextController;

        private Vector3 _startPosition;
        private Vector3 _startRotation;
        private Vector2 _lastFrameInput;

        private enum PlayerState
        {
            Movement,
            Replying
        }

        private SimpleStateMachine<PlayerState> _fsm;
        private Action<string> _sendReply;

        private Vector3 _moveDirection;
        private Vector2 _actualDiffMouse;

        private float _cameraVerticalRotation;

        private bool _skipMovement;

        public bool IsTalking => _isTalking;

        private bool _isTalking;

        public override event StartSpeechRecognitionDelegate StartVoiceRecognition;
        public override event StartSpeechRecognitionDelegate StopVoiceRecognition;

        public static CharismaPlayerController Instance;

        // Update is called once per frame
        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _characterController = GetComponent<CharacterController>();

            // move initially to snap to floor
            _characterController.Move(Vector3.zero);
            _characterController.enabled = false;

            _startPosition = this.gameObject.transform.position;
            _startRotation = this.gameObject.transform.eulerAngles;

            _playerTextController = new PlayerTextFieldController(_playerUI);
            _playerTextController.OnReplySentDelegate += OnReplySent;
            _playerTextController.OnSpeechRecognitionDelegate += OnSpeechRecognition;

            InitialiseStateMachine();
        }

        private void Start()
        {
            _characterController.enabled = true;

            this.gameObject.transform.position = _startPosition;
            this.gameObject.transform.eulerAngles = _startRotation;
        }

        private void Update()
        {
            _fsm.Update();

            if (!_playerUI.IsUserWritingReply() && !Input.GetKey(KeyCode.LeftShift))
            {
                OnLookUpdate();
            }

            if (!_playerUI.IsUserWritingReply())
            {
                OnMoveUpdate();
            }
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

        private void InitialiseStateMachine()
        {
            _fsm = new SimpleStateMachine<PlayerState>();
            _fsm.AddState(PlayerState.Movement)
                .OnEntry(OnMovementEnter)
                .OnUpdate(OnMovementUpdate);

            _fsm.AddState(PlayerState.Replying)
                .OnEntry(OnReplyEnter)
                .OnUpdate(OnReplyUpdate);
        }

        private void OnMovementEnter()
        {

        }

        private void OnMovementUpdate()
        {
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
        }

        private void OnReplyEnter()
        {
            _playerUI.SetPlayerInputFieldActive(true);
        }

        private void OnReplyUpdate()
        {
            _playerTextController.Update();

            if (_playerUI.IsUserWritingReply())
            {
                _isTalking = true;
            }
        }

        private void OnLookUpdate()
        {
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
            var inputX = Input.GetAxis("Horizontal");
            var inputY = Input.GetAxis("Vertical");

            if (_skipMovement)
            {
                if (!(inputX > 0
                    || inputX < 0
                    || inputY > 0
                    || inputY < 0))
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

        private void OnReplySent(string resultInput)
        {
            _sendReply?.Invoke(resultInput);
            _fsm.SetState(PlayerState.Movement);
            _isTalking = false;
        }

        private void OnSpeechRecognition(bool started)
        {
            if (started)
            {
                StartVoiceRecognition.Invoke(true);
                _isTalking = true;
            }
            else
            {
                StopVoiceRecognition.Invoke(false);
                _isTalking = false;
            }
        }

    }
}
