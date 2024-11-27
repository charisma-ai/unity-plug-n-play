using CharismaSDK.Events;
using CharismaSDK.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static CharismaSDK.Playthrough;

namespace CharismaSDK.PlugNPlay
{
    public class PnpPlaythroughInstance : PlaythroughInstanceBase
    {
        public delegate void SetListeningDelegate(bool listening);
        
        [SerializeField]
        [Tooltip("Toggle to output messages as audible Speech.")]
        private bool _useSpeechOutput = true;
        
        [Header("PnP Data")]
        [SerializeField]
        [Tooltip("Collection of metadata functions that will be registered to this playthrough.")]
        private List<MetadataFunction> _metadataFunctions;

        [ReadOnly]
        [SerializeField]
        [Tooltip("Collection of CharismaEntities that will be registered to this playthrough.")]
        private PlaythroughEntities _entities;

        [Header("Debug")]
        [SerializeField]
        private ConnectionStateDisplay _connectionState;
        
        [SerializeField]
        private UnityEvent _onStoryEnd;

        [SerializeField] 
        private CharismaPlaythroughActor _currentSpeaker;
        
        // STT
        private List<string> _recognizedSpeechTextList = new List<string>();
        private string _currentRecognizedText;
        
        private string CurrentRecognizedText 
        {
            get
            {
                var text = new StringBuilder();

                foreach (var speechLine in _recognizedSpeechTextList)
                {
                    text.Append(speechLine + " ");
                }

                text.Append(_currentRecognizedText);
                
                return text.ToString();
            }
        }
        
        private void Awake()
        {
            _entities.FindAllValidPlaythroughEntities();
        }
        
        protected override void Start()
        {
            base.Start();
            
            MetadataDependencies dependencies = new MetadataDependencies(_entities);
            
            foreach (var metadata in _metadataFunctions)
            {
                metadata.AssignDependencies(dependencies);
            }

            var player = _entities.GetPlayerEntity();

            // Hook up send-reply callback, to bind the function to the external player.
            player?.SetOnReplyCallback(SendReply);

            LoadPlaythrough();
        }

        private void OnApplicationQuit()
        {
            if (_playthrough != default)
            {
                _playthrough.OnConnectionStateChange -= UpdateConnectionState;
                _playthrough.OnSpeechRecognitionResult -= OnSpeechRecognitionResult;
                _playthrough.OnMessage -= OnMessageReceived;

                _playthrough.Disconnect();

                var player = _entities.GetPlayerEntity();

                if (player != default)
                {
                    player.StartVoiceRecognition -= SetPlaythroughToListening;
                    player.StopVoiceRecognition -= SetPlaythroughToListening;
                }
            }
        }

        #region Public Functions

        protected override void OnPlaythroughLoaded(CreatePlaythroughTokenResponse tokenResponse, string conversationUuid)
        {
            _entities.SetAllEntitiesLive(true);

            _playthrough.OnConnectionStateChange += UpdateConnectionState;
            _playthrough.OnSpeechRecognitionResult += OnSpeechRecognitionResult;

            var player = _entities.GetPlayerEntity();

            if (player != default)
            {
                player.StartVoiceRecognition += SetPlaythroughToListening;
                player.StopVoiceRecognition += SetPlaythroughToListening;
            }
            
            StartPlaythrough();
        }
        
        /// <summary>
        /// Sends a local reply to the Charisma playthrough session.
        /// The playthrough will attempt to send a message in return.
        /// </summary>
        /// <param name="reply"></param>
        public void SendReply(string reply)
        {
            if (!IsPlaythroughLoaded())
            {
                Logger.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // Send a reply to our current conversation.
            _playthrough.Reply(_conversationUuid, reply);
        }

        /// <summary>
        /// Sends an action to the current playthrough.
        /// </summary>
        /// <param name="reply"></param>
        public void SetMemory(string recallValue, string saveValue)
        {
            if (!IsPlaythroughLoaded())
            {
                Logger.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
                return;
            }

            // Send a memory to our current conversation.
            StartCoroutine(CharismaAPI.SetMemory(_playthrough.Token, recallValue, saveValue));
        }
        
        #endregion

        #region Private functions

        private void OnSpeechRecognitionResult(SpeechRecognitionResult message)
        {
            _currentRecognizedText = message.text;

            var player = _entities.GetPlayerEntity();
            
            if (message.isFinal)
            {
                _recognizedSpeechTextList.Add(_currentRecognizedText);
                _currentRecognizedText = "";
            }
            
            if (player != default)
            {
                player.SendSpeechResult(CurrentRecognizedText);
            }
        }

        private void SetPlaythroughToListening(bool listening)
        {
            if (listening)
            {
                _recognizedSpeechTextList.Clear();
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
            _entities.SetAllEntitiesLive(connectionState == ConnectionState.Connected);
        }

        protected override void OnMessageReceived(MessageEvent message)
        {
            Logger.Log(message);
            
            _onMessageCallback?.Invoke(message);
            
            // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
            if (message.messageType == MessageType.panel)
            {
                return;
            }
            
            // We can't generate speech or access character & text data so we return after we have checked if this is the end of the story.
            if (message.endStory)
            {
                _onStoryEnd?.Invoke();
                _playthrough.Disconnect();
                return;
            }
        
            // Go through all the metadata handlers and try to execute the callback
            foreach (var metadata in _metadataFunctions)
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
            foreach (var actor in _entities.Actors)
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
                _currentSpeaker = actor;
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
                var currentSpeaker = _entities.GetActorByName(message.message.character.name);

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
        
        /// <summary>
        /// Sets the callback to execute on succesfully receiving a message from the playthrough.
        /// Should be set before starting the Playthrough.
        /// </summary>
        public void AddOnMessageCallback(MessageDelegate callback)
        {
            _onMessageCallback += callback;
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
                        CharismaAudio.GetAudioClip(message.message.speech.encoding, message.message.speech.audio, clip => SendAudio(actor, clip));
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
