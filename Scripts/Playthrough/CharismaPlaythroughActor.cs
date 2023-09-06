using CharismaSDK.Events;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Abstract base Playthrough Actor class
    /// Used as a modular way to give various functionality
    /// </summary>
    public abstract class CharismaPlaythroughActor : MonoBehaviour
    {
        public string CharacterId => _characterId;

        [SerializeField]
        private string _characterId;

        public abstract bool ListensForAllCharacterMessages { get; }
        public abstract bool HasAudioPlayback { get; }
        public abstract bool HasTextOutput { get; }
        public abstract bool HasCharacterData { get; }
        public abstract bool HasCurrentSpeakerRequirement { get; }

        /// <summary>
        /// Send the message body that needs to be printed
        /// </summary>
        public virtual void SendPlaythroughMessage(Message message) { }

        /// <summary>
        /// Used to send generated Audioclips around
        /// Mainly separated to avoid generating the same AudioClip multiple times
        /// </summary>
        public virtual void SendAudioClip(AudioClip audioClip) { }

        /// <summary>
        /// Send all messageEvent data received from the current Playthrough 
        /// </summary>
        public virtual void AddCharacterEmotion(Emotion emotion) { }

        /// <summary>
        /// Sends the current speaker that has succesfully received dialogue.
        /// </summary>
        /// <param name="currentSpeaker"></param>
        public virtual void SetCurrentSpeaker(CharismaPlaythroughActor actor) { }

        /// <summary>
        /// Used to resolve all the contents within the message
        /// Mainly to manage complex parsing of multiple metadata calls
        /// </summary>
        public virtual void Resolve() { }

    }
}
