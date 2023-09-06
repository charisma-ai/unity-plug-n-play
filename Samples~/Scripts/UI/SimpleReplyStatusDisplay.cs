using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    internal class SimpleReplyStatusDisplay : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bubbleParent;

        [SerializeField]
        private GameObject _listeningText;

        [SerializeField]
        private GameObject _replyText;

        [SerializeField]
        private GameObject _timerParent;

        [SerializeField]
        private Image _gradient;

        internal void SetHidden(bool v)
        {
            _bubbleParent.SetActive(!v);
            _listeningText.SetActive(!v);
            _replyText.SetActive(!v);

            _timerParent.SetActive(!v);
        }

        internal void SetWaitingReply()
        {
            _bubbleParent.SetActive(true);
            _listeningText.SetActive(false);
            _replyText.SetActive(true);
            SetTimeToSubmitBar(false);

        }

        internal void SetListening()
        {
            _bubbleParent.SetActive(true);
            _listeningText.SetActive(true);
            _replyText.SetActive(false);
            SetTimeToSubmitBar(false);
        }

        internal void SetTimeToSubmitBar(bool v)
        {
            _timerParent.SetActive(v);
            _gradient.fillAmount = 1;

        }

        internal void SetTimeToSubmit(float filledAmount, float totalAmount)
        {
            _gradient.fillAmount = filledAmount / totalAmount;
        }
    }
}
