using CharismaSDK;
using CharismaSDK.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    internal class SimpleTextActor : CharismaPlaythroughActor
    {
        [SerializeField]
        [Tooltip("Text source, used to print the synthesized speech on screen.")]
        private Text _textOutput;

        public override bool HasAudioPlayback => false;
        public override bool HasTextOutput => true;
        public override bool HasCharacterData => false;
        public override bool HasCurrentSpeakerRequirement => false;
        public override bool IsTalking => false;
        public override bool ListensForAllCharacterMessages => true;

        public override void SendPlaythroughMessage(Message message)
        {
            if (_textOutput == default)
            {
                Debug.LogWarning("Text output has not been assigned. Please set _textOutput field.");
            }
            else
            {
                _textOutput.text = ($"{message.character.name}: {message.text}");
            }
        }
    }
}
