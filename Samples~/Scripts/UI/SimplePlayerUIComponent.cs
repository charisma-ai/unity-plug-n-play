using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class SimplePlayerUIComponent : MonoBehaviour
    {
        [SerializeField]
        private SimpleReplyController _charismaReplyUI;
        
        [SerializeField]
        private SimpleInteractPrompt _interactPrompt;
        
        [SerializeField]
        private SimpleTapContinuePrompt _tapContinuePrompt;
        
        public Action<string> OnTextUpdate;
        
        private bool _isReplying = false;
        
        public bool IsActive { get; private set; }

        public bool IsTapToContinuePromptActive => _tapContinuePrompt.IsPromptActive;
        
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

        public void SetTapContinuePromptActive(bool active)
        {
            _tapContinuePrompt.SetPromptActive(active);
        }
    }
}
