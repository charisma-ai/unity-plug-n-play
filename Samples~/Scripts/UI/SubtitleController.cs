using System;
using System.Collections;
using System.Collections.Generic;
using CharismaSDK.Events;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class SubtitleController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private Color _characterNameColor;
        [SerializeField] private float _additionalDuration = 1f;
        
        private Coroutine _subtitleCoroutine;
        
        private void Start()
        {
            var playthroughInstance = FindObjectOfType<PnpPlaythroughInstance>();
            playthroughInstance.AddOnMessageCallback(OnMessageReceived);
            _subtitleText.text = "";
        }

        private void OnMessageReceived(MessageEvent messageEvent)
        {
            if (messageEvent.messageType != MessageType.character)
            {
                return;
            }

            if (messageEvent.message.speech == null || messageEvent.message.character == null)
            {
                return;
            }

            var freezeSubtitle = messageEvent.message.metadata.ContainsKey("freeze-subtitles");
            var subtitleDuration = (messageEvent.message.speech.duration / 1000f) + _additionalDuration;
            ShowSubtitle(messageEvent.message.character.name, messageEvent.message.text, subtitleDuration, freezeSubtitle);
        }

        private void ShowSubtitle(string characterName, string text, float duration, bool freezeSubtitle)
        {
            if (_subtitleCoroutine != null)
            {
                StopCoroutine(_subtitleCoroutine);
                _subtitleText.text = "";
            }
            
            _subtitleCoroutine = StartCoroutine(SubtitleSequence(characterName, text, duration, freezeSubtitle));
        }

        private IEnumerator SubtitleSequence(string characterName, string text, float duration, bool freezeSubtitle)
        {
            _subtitleText.text = $"<color=#{_characterNameColor.ToHexString()}>{characterName}: </color>{text}";

            var currentDurationRemaining = duration;
            
            while (currentDurationRemaining > 0 || freezeSubtitle)
            {
                yield return null;
                currentDurationRemaining -= Time.deltaTime;
            }
            
            _subtitleText.text = "";
        }
    }
}