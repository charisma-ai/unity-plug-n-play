using CharismaSDK.Events;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Playthrough layer of the NPC Actor
    /// Used to interpret various playthrough messages, commands and metadata
    /// This information is then passed on to various concrete objects like the humanoid controller
    /// </summary>
    public class CharismaHumanoidActor : CharismaPlaythroughActor
    {
        public HumanoidNPCCharacterController HumanoidCharacter => _characterComponent;

        public Transform HeadLocation => _headLocation;

        [SerializeField]
        private Transform _headLocation;

        [SerializeField]
        private HumanoidNPCCharacterController _characterComponent;

        [SerializeField]
        private CharismaTextBoxLocation _textRender;

        public override bool HasAudioPlayback => true;
        public override bool HasTextOutput => true;
        public override bool HasCharacterData => true;
        public override bool HasCurrentSpeakerRequirement => true;
        public override bool ListensForAllCharacterMessages => true;

        // Resolve parameters
        // these should be assigned and referenced later within the Resolve() function
        private Transform _resolveParamLookAtTarget;
        private AudioClip _resolveAudioClip;
        private List<Emotion> _resolveEmotions = new List<Emotion>();
        private Message _resolveMessage;
        private List<NPCTask> _pendingTasks = new List<NPCTask>();

        public void Awake()
        {
            _textRender.SetActorName(CharacterId);
        }

        #region Internal functions - these should be called via Playthrough
        public override void SetCurrentSpeaker(CharismaPlaythroughActor speaker)
        {
            if (speaker is CharismaHumanoidActor npc)
            {
                if (_resolveParamLookAtTarget == default)
                {
                    _resolveParamLookAtTarget = npc.HeadLocation;
                }
            }
        }

        public void SetLookAtTarget(Transform lookAtTarget)
        {
            if (_resolveParamLookAtTarget == default)
            {
                _resolveParamLookAtTarget = lookAtTarget;
            }
        }

        public override void SendAudioClip(AudioClip audioClip)
        {
            _resolveAudioClip = audioClip;

        }
        public override void AddCharacterEmotion(Emotion emotion)
        {
            _resolveEmotions.Add(emotion);
        }

        public override void SendPlaythroughMessage(Message message)
        {
            _resolveMessage = message;
        }

        public void AddMetaDataTask(NPCTask task)
        {
            _pendingTasks.Add(task);
        }

        public override void Resolve()
        {
            if (_resolveParamLookAtTarget == default)
            {
                _resolveParamLookAtTarget = Camera.main.transform;
            }

            // Assign NPC tasks that came thru from metadata
            _characterComponent?.ClearCurrentTasks();
            foreach (var task in _pendingTasks)
            {
                _characterComponent.AddTask(task);
            }

            // Store message duration, as it may be used for emotion display
            var messageDuration = 0.0f;

            // if the message belongs to the character, play/print the text and look at the player
            if (PlaythroughHelpers.DoesMessageBelongToCharacter(_resolveMessage, this))
            {
                if (_textRender != default)
                {
                    _textRender.PrintTextMessage(_resolveMessage.text, _resolveMessage.speech.duration);
                }

                _characterComponent.LookAtObject(_resolveParamLookAtTarget.gameObject, -1);
                _characterComponent.ReplyTo(_resolveAudioClip);
                messageDuration = _resolveMessage.speech.duration;
            }
            else
            {
                _characterComponent.LookAtObject(_resolveParamLookAtTarget.gameObject, -1);

                // if not, try to look at whoever is speaking
                if (_resolveMessage != default)
                {
                    _characterComponent.StopTalking();
                }
            }

            // if an emotion is available send it to the NPC
            if (_resolveEmotions != default)
            {
                _characterComponent.SetEmotion(_resolveEmotions, messageDuration);
            }

            ClearParameters();
        }

        private void ClearParameters()
        {
            _resolveParamLookAtTarget = default;
            _resolveAudioClip = default;
            _resolveEmotions.Clear();
            _resolveMessage = default;
            _pendingTasks.Clear();
        }

        #endregion
    }
}
