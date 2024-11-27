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

        public Action<string> OnTextUpdate;
        
        private bool _isReplying = false;
        
        public bool IsActive { get; private set; }
        
        public void SetPlayerInputFieldActive(bool flag)
        {
            _charismaReplyUI.SetInputFieldActive(flag);
            IsActive = flag;
        }

        public void SetDisplayInteractPrompt(bool display)
        {
            _interactPrompt.gameObject.SetActive(display);
        }
        
        public bool IsUserWritingReply()
        {
            return _charismaReplyUI.IsWritingReply();
        }

        public bool HasReplyBeenSubmitted()
        {
            return _charismaReplyUI.HasReplied();
        }

        public void EditText(bool focus)
        {
            _charismaReplyUI.FocusInputField(focus, false);
        }

        public void EditViaSpeech(bool focus)
        {
            _charismaReplyUI.FocusInputField(focus, true);
        }

        public void SetReplyText(string replyText)
        {
            _charismaReplyUI.SetReplyText(replyText);
        }
        
        public string GetReplyText()
        {
            return _charismaReplyUI.GetReplyText();
        }
    }
}
