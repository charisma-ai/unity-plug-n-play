using CharismaSDK;
using CharismaSDK.Events;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class SimpleAudioActor : CharismaPlaythroughActor
    {
        [SerializeField]
        [Tooltip("Audio source, used to output the synthesized speech. Only active if _useSpeech is toggled to true.")]
        private AudioSource _audioOutput;

        public override bool HasAudioPlayback => true;
        public override bool HasTextOutput => false;
        public override bool HasCharacterData => false;
        public override bool HasCurrentSpeakerRequirement => false;
        public override bool ListensForAllCharacterMessages => true;

        public override void SendAudioClip(AudioClip audioClip)
        {
            if (_audioOutput == default)
            {
                Debug.LogWarning("Audio output has not been assigned. Please set _audioOutput field.");
            }
            else
            {
                _audioOutput.clip = audioClip;
                _audioOutput.Play();
            }
        }
    }
}
