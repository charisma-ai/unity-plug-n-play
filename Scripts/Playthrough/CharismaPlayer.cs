using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Abstract base class for the Player entity registered to the playthrough
    /// Can handle replying as both
    /// </summary>
    public abstract class CharismaPlayer : MonoBehaviour
    {

        // Delegate used to inform if speech recognition has started or stopped
        public delegate void StartSpeechRecognitionDelegate(bool listening);

        // Event for when VoiceRecognition starts
        // should be hooked up when that behaviour is expected to inform the playthrough
        public abstract event StartSpeechRecognitionDelegate StartVoiceRecognition;

        // Event for when VoiceRecognition end
        // should be hooked up when that behaviour is expected to inform the playthrough
        public abstract event StartSpeechRecognitionDelegate StopVoiceRecognition;

        /// <summary>
        /// Sets a local reply callback
        /// Make sure to Invoke this when a reply is sent
        /// </summary>
        /// <param name="sendReply">Reply callback, sent from the playthrough</param>
        public abstract void SetOnReplyCallback(Action<string> sendReply);

        /// <summary>
        /// Called when the Player is ready to reply
        /// Current example has it set up to the "set-player-speak" metadata which is the standard use-case
        /// </summary>
        public abstract void SetReadyToReply();

        /// <summary>
        /// Function, used to inform the Player of what the speech recognition result came back from the Playthrough
        /// </summary>
        /// <param name="recognizedText"></param>
        public abstract void SendSpeechResult(string recognizedText);
    }
}
