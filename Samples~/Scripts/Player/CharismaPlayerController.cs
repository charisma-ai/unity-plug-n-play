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
        private float _movementRate = 10;

        [SerializeField]
        private float _rotationSpeedDegreesPerSecondHorizontal = 80;
        [SerializeField]
        private float _rotationSpeedDegreesPerSecondVertical = 80;

        [SerializeField]
        private float _lookLimit = 85f;

        private PlayerTextFieldController _playerTextController;

        private enum PlayerState
        {
            Movement,
            Replying
        }

        private SimpleStateMachine<PlayerState> _fsm;
        private Action<string> _sendReply;

        public override event StartSpeechRecognitionDelegate StartVoiceRecognition;
        public override event StartSpeechRecognitionDelegate StopVoiceRecognition;

        // Update is called once per frame
        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            // move initially to snap to floor
            _characterController.Move(Vector3.zero);

            _playerTextController = new PlayerTextFieldController(_playerUI);
            _playerTextController.OnReplySentDelegate += OnReplySent;
            _playerTextController.OnSpeechRecognitionDelegate += OnSpeechRecognition;

            InitialiseStateMachine();
        }

        private void Update()
        {
            _fsm.Update();
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
            HandleMovingAndLooking();

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
            HandleMovingAndLooking();

            _playerTextController.Update();
        }

        private void HandleMovingAndLooking()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                Vector2 lookInput = Vector2.zero;
                lookInput.x = Input.GetAxis("Mouse X");
                lookInput.y = Input.GetAxis("Mouse Y");

                Look(lookInput);
            }

            if (!_playerUI.IsUserWritingReply())
            {
                Vector2 moveInput = Vector2.zero;
                moveInput.x = Input.GetAxis("Horizontal");
                moveInput.y = Input.GetAxis("Vertical");

                Move(moveInput);
            }

        }

        private void Look(Vector2 lookAtInput)
        {
            float rotationH = lookAtInput.x * _rotationSpeedDegreesPerSecondHorizontal * Time.deltaTime;
            float rotationV = lookAtInput.y * _rotationSpeedDegreesPerSecondVertical * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationH);
            _camera.transform.Rotate(Vector3.left, rotationV);

            Vector3 currentRotation = _camera.transform.localRotation.eulerAngles;

            _camera.transform.localRotation = Quaternion.Euler(
                currentRotation.x > 180f
                    ? Mathf.Clamp(currentRotation.x, 360f - _lookLimit, 360f)
                    : Mathf.Clamp(currentRotation.x, 0, _lookLimit),
                0,
                0);
        }

        private void Move(Vector2 moveInput)
        {
            Vector3 movementVector = Vector3.zero;
            movementVector += this.transform.right * moveInput.x * _movementRate * Time.deltaTime;
            movementVector += this.transform.forward * moveInput.y * _movementRate * Time.deltaTime;

            _characterController.Move(movementVector);
        }

        private void OnReplySent(string resultInput)
        {
            _sendReply?.Invoke(resultInput);
            _fsm.SetState(PlayerState.Movement);
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

    }
}
