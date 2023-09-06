using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class SimplePlayerUIComponent : MonoBehaviour
    {
        [SerializeField]
        private SimpleInteractPrompt _interactPrompt;

        [SerializeField]
        private SimpleReplyController _charismaReplyUI;

        public void SetPlayerInputFieldActive(bool flag)
        {
            _charismaReplyUI.SetInputFieldActive(flag);
        }

        public void SetDisplayInteractPrompt(bool display)
        {
            _interactPrompt.gameObject.SetActive(display);
        }

        internal string GetReplyText()
        {
            return _charismaReplyUI.GetReplyText();
        }

        internal bool IsUserWritingReply()
        {
            return _charismaReplyUI.IsWritingReply();
        }

        internal bool HasReplyBeenSubmitted()
        {
            return _charismaReplyUI.HasReplied();
        }

        internal void EditText()
        {
            _charismaReplyUI.FocusInputField(true, false);
        }

        internal void EditViaSpeech()
        {
            _charismaReplyUI.FocusInputField(false, true);
        }

        internal void SetReplyText(string replyText)
        {
            _charismaReplyUI.SetReplyText(replyText);
        }
    }
}
