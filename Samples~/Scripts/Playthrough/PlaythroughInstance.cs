using CharismaSDK.Events;
using CharismaSDK.Sound;
using System;
using UnityEngine;
using UnityEngine.Events;
using static CharismaSDK.Playthrough;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Main Playthrough monobehaviour
    /// TRies to automate as much of the process as possible by parsing messages comprehensively
    /// </summary>
    public class PlaythroughInstance : MonoBehaviour
    {
        public delegate void SetListeningDelegate(bool listening);

        [SerializeField]
        [Tooltip("Activate internal Charisma-specific logging.")]
        private bool _enableLogging;

        [Header(header: "Charisma")]
        [SerializeField]
        [Tooltip("Playthrough parameters - these are collected on Scene start currently.")]
        private PlaythroughParameters _parameters;

        [Header(header: "Settings")]
        [SerializeField]
        [Tooltip("Toggle to output messages as audible Speech.")]
        private bool _useSpeechOutput = true;

        [SerializeField]
        [Tooltip("Configuration node of the Speech output.")]
        private SpeechOptions _speechOptions;

        [Header(header: "Debug")]
        [SerializeField]
        private ConnectionStateDisplay _connectionState;

        private string _conversationUuid;

        private Playthrough _playthrough;

        private Action _onLoadCallback;
        private MessageDelegate _onMessageCallback;
        private string _recognizedText;

        [SerializeField]
        private UnityEvent _onStoryEnd;

        private void Awake()
        {
            _parameters.Initialise();
        }

        private void Start()
        {
            MetadataDependencies dependencies = new MetadataDependencies(_parameters.Entities);
            foreach (var metadata in _parameters.MetadataFunctions)
            {
                metadata.AssignDependencies(dependencies);
            }

            var player = _parameters.Entities.GetPlayerEntity();

            // Hook up send-reply callback, to bind the function to the external player.
            player?.SetOnReplyCallback(SendReply);

            // Set up default-behaviour callbacks
            // Once load is complete, start the playthrough.
            SetOnLoadCallback(StartPlaythrough);
            // Hook up standard handle message callback.
            SetOnMessageCallback(OnMessageReceived);
        }

        private void OnApplicationQuit()
        {
            if (_playthrough != default)
            {
                _playthrough.OnConnectionStateChange -= UpdateConnectionState;
                _playthrough.OnSpeechRecognitionResult -= OnSpeechRecognitionResult;
                _playthrough.OnMessage -= HandleMessage;

                _playthrough.Disconnect();

                var player = _parameters.Entities.GetPlayerEntity();

                if (player != default)
                {
                    player.StartVoiceRecognition -= SetPlaythroughToListening;
                    player.StopVoiceRecognition -= SetPlaythroughToListening;
                }
            }

        }

        #region Public Functions
        /// <summary>
        /// Starts the loading process of the Playthrough, by requesting a token and creating a conversation
        /// Will execute OnLoadCallback when complete. 
        /// The OnLoadCallback can be set via the SetLoadCallback function.
        /// </summary>
        public void LoadPlaythrough()
        {
            // We use these settings to create a play-through token.
            StartCoroutine(CharismaAPI.CreatePlaythroughToken(_parameters.PlaythroughToken, callback: (tokenResponse) =>
            {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(token: tokenResponse.Token, callback: conversationUuid =>
                {
                // We'll cache our conversation Id since we need this to send replies and other events to Charisma.
                _conversationUuid = conversationUuid;

                // We can now create a new charisma object and pass it our token.
                _playthrough = new Playthrough(
                        token: tokenResponse.Token,
                        playthroughUuid: tokenResponse.PlaythroughUuid,
                        _speechOptions
                    );

                    _parameters.Entities.SetAllEntitiesLive(true);

                    _playthrough.OnConnectionStateChange += UpdateConnectionState;
                    _playthrough.OnSpeechRecognitionResult += OnSpeechRecognitionResult;

                    var player = _parameters.Entities.GetPlayerEntity();

                    if (player != default)
                    {
                        player.StartVoiceRecognition += SetPlaythroughToListening;
                        player.StopVoiceRecognition += SetPlaythroughToListening;
                    }

                // Invoke any custom user defined action.
                _onLoadCallback?.Invoke();
                }));
            }));
        }

        /// <summary>
        /// Starts the playthrough.
        /// This will begin the relevant active substory, and start sending messages back to the local Unity session.
        /// This message callback can be set via the SetOnMessageCallback function.
        /// </summary>
        public void StartPlaythrough()
        {
            if (!IsLoaded())
            {
                Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
            _playthrough.Connect(() =>
            {
                Debug.Log("Ready!");

            // In the start function, we pass the conversationId we cached earlier. 
            _playthrough.Start(_conversationUuid, startGraphReferenceId: _parameters.StartGraphReferenceId);
            });

            // On message callback needs to be assigned.
            if (_onMessageCallback != default)
            {
                // We can now subscribe to message events from charisma.
                _playthrough.OnMessage += HandleMessage;
            }
        }

        /// <summary>
        /// Sends a local reply to the Charisma playthrough session.
        /// The playthrough will attempt to send a message in return.
        /// </summary>
        /// <param name="reply"></param>
        public void SendReply(string reply)
        {
            if (!IsLoaded())
            {
                Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // Send a reply to our current conversation.
            _playthrough.Reply(_conversationUuid, reply);
        }

        /// <summary>
        /// Sends an action to the current playthrough.
        /// </summary>
        /// <param name="reply"></param>
        public void SetAction(string action)
        {
            if (!IsLoaded())
            {
                Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // Send an action to our current conversation.
            _playthrough.Action(_conversationUuid, action);
        }

        /// <summary>
        /// Sends an action to the current playthrough.
        /// </summary>
        /// <param name="reply"></param>
        public void SetMemory(string recallValue, string saveValue)
        {
            if (!IsLoaded())
            {
                Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // Send a memory to our current conversation.
            StartCoroutine(CharismaAPI.SetMemory(_playthrough.Token, recallValue, saveValue));
        }

        /// <summary>
        /// Returns whether the Charisma playthrough has succesfully been loaded
        /// </summary>
        public bool IsLoaded()
        {
            return _playthrough != default;
        }

        /// <summary>
        /// Sets the callback to execute on succesfully loading the Playthrough.
        /// Should be set before loading the Playthrough.
        /// </summary>
        public void SetOnLoadCallback(Action callback)
        {
            _onLoadCallback = callback;
        }

        /// <summary>
        /// Sets the callback to execute on succesfully receiving a message from the playthrough.
        /// Should be set before starting the Playthrough.
        /// </summary>
        public void SetOnMessageCallback(MessageDelegate callback)
        {
            _onMessageCallback = callback;
        }

        #endregion

        #region Private functions

        private void OnSpeechRecognitionResult(SpeechRecognitionResult message)
        {
            _recognizedText = message.text;

            var player = _parameters.Entities.GetPlayerEntity();
            if (player != default)
            {
                player.SendSpeechResult(_recognizedText);
            }
        }

        private void SetPlaythroughToListening(bool listening)
        {
            if (listening)
            {
                _playthrough.StartSpeechRecognition(this.gameObject);
            }
            else
            {
                _playthrough.StopSpeechRecognition();
            }
        }


        /// <summary>
        /// Update connection debug with updated status
        /// </summary>
        /// <param name="connectionState">New Connection State</param>
        private void UpdateConnectionState(ConnectionState connectionState)
        {
            _connectionState?.SetResultState(connectionState);

            // change entity behaviour depending on connection state
            _parameters.Entities.SetAllEntitiesLive(connectionState == ConnectionState.Connected);
        }

        /// <summary>
        /// Standard Message handling behaviour, catching panels and triggering disconnects on endStory.
        /// </summary>
        /// <param name="message">Message Event received from currently active Playthrough.</param>
        private void HandleMessage(MessageEvent message)
        {
            Debug.Log(message);

            // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
            if (message.messageType == MessageType.panel)
            {
                Debug.Log("This is a panel node");
            }
            else
            {
                _onMessageCallback?.Invoke(message);
            }

            // If this is the end of the story, we disconnect from Charisma.
            if (message.endStory)
            {
                _onStoryEnd.Invoke();
                _playthrough.Disconnect();
            }
        }

        private void OnMessageReceived(MessageEvent message)
        {
            // Go thru all the metadata handlers and try to execute the callback
            foreach (var metadata in _parameters.MetadataFunctions)
            {
                string metadataValue;
                if (message.message.metadata.TryGetValue(metadata.MetadataId, out metadataValue))
                {
                    metadata.Execute(metadataValue);
                }
            }

            if (message.message.character == null)
            {
                return;
            }

            // Inform subscribers that a message has been received.
            foreach (var actor in _parameters.Entities.Actors)
            {
                TrySendDataToActor(actor, message);
            }

        }

        private void TrySendDataToActor(CharismaPlaythroughActor actor, MessageEvent message)
        {
            var partialSend = actor.ListensForAllCharacterMessages;
            var fullSend = PlaythroughHelpers.DoesMessageBelongToCharacter(message.message, actor);

            // Send all relevant message information dependent on context
            // some information may be necessary even if that Subscriber does not own the message
            if (fullSend)
            {
                FullSend(actor, message);
            }
            else if (partialSend)
            {
                PartialSend(actor, message);
            }
        }

        /// <summary>
        /// partial sends are meant for Non-speakers
        /// this is essentially a way to get information about a message
        /// Example: provide look at coordinates for who is talking
        /// </summary>
        private void PartialSend(CharismaPlaythroughActor actor, MessageEvent message)
        {
            if (actor.HasTextOutput)
            {
                actor.SendPlaythroughMessage(message.message);
            }

            if (actor.HasCurrentSpeakerRequirement)
            {
                var currentSpeaker = _parameters.Entities.GetActorByName(message.message.character.name);

                if (currentSpeaker != default)
                {
                    actor.SetCurrentSpeaker(currentSpeaker);
                }
            }

            actor.Resolve();
        }

        /// <summary>
        /// Full sends are meant for speakers
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="message"></param>
        private void FullSend(CharismaPlaythroughActor actor, MessageEvent message)
        {
            Action tryResolveOnComplete = () => { actor.Resolve(); };

            if (actor.HasTextOutput)
            {
                actor.SendPlaythroughMessage(message.message);
            }

            if (actor.HasCharacterData)
            {
                HandleEmotions(actor, message);
            }

            if (actor.HasAudioPlayback)
            {
                TrySendAudioOutput(actor, message);
                // clear the resolve on complete, as its handled elsewhere
                // audio must be delivered before we can resolve
                tryResolveOnComplete = default;
            }

            tryResolveOnComplete?.Invoke();
        }

        private void HandleEmotions(CharismaPlaythroughActor actor, MessageEvent message)
        {
            // Send Emotions
            foreach (Emotion emotion in message.emotions)
            {
                if (emotion.activeEffects.Length > 0)
                {
                    actor.AddCharacterEmotion(emotion);
                }
            }
        }

        private void TrySendAudioOutput(CharismaPlaythroughActor actor, MessageEvent message)
        {
            if (_useSpeechOutput)
            {
                if (message.message.speech != null)
                {
                    if (actor.HasAudioPlayback)
                    {
                        Audio.GetAudioClip(message.message.speech.encoding, message.message.speech.audio, clip => SendAudio(actor, clip));
                    }
                }
            }
        }

        private void SendAudio(CharismaPlaythroughActor actor, AudioClip audioClip)
        {
            actor.SendAudioClip(audioClip);
            actor.Resolve();
        }

        #endregion
    }
}
