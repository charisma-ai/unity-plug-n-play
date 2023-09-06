using UnityEngine;
using UnityEngine.Events;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Interactable Entity used in playthrough
    /// Keeps track of various interactions a player may have w/ entities in the story
    /// </summary>
    public class CharismaInteractableEntity : CharismaPlaythroughEntity
    {
        public bool CanBeInteractedWith => _enabled && !_used && Live;

        // Distance/range of interactable and when the use prompt will trigger
        public float Range => _range;

        // Flag to determine of the interactable starts off as enabled
        // If a Interactable is not enabled, then it cannot be interacted with
        [SerializeField]
        private bool _startEnabled = true;

        [SerializeField]
        private float _range = 0.5f;

        // Flag to determine if the object is enabled and is available for use
        private bool _enabled = false;

        // Flag to determine if the interactable has been used by the player
        // If used, cannot be used again unless specifically reset
        private bool _used = false;

        [SerializeField]
        private UnityEvent _onUse;

        private void Start()
        {
            _enabled = _startEnabled;
        }

        public void Interact()
        {
            _used = true;
            _onUse?.Invoke();
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public bool IsInRange(Vector3 position)
        {
            return Vector3.Distance(position, this.transform.position) <= _range;
        }

        public void ResetUse()
        {
            _used = false;
        }
    }
}
