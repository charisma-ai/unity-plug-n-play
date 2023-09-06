using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Abstract base Playthrough Entity class
    /// All PlaythroughInstance objects inherit from this base class
    /// Any custom objects should inherit from this or in the case of Actors, from the CharismaPlaythroughActor
    /// </summary>
    public abstract class CharismaPlaythroughEntity : MonoBehaviour
    {
        public string EntityId => _entityId;
        public bool Live => _live;

        [SerializeField]
        [Tooltip("Unique ID of this Playthrough Entity.")]
        private string _entityId;

        [SerializeField]
        [Tooltip("Whether this Entity starts Live(i.e. perform functions within the context of Unity, even with the Playthrough not active)")]
        private bool _startLive = false;

        [SerializeField]
        [ReadOnly]
        [Tooltip("Returns if this Entity is currently Live (Active within the context of the Playthrough)")]
        private bool _live;


        public virtual void Awake()
        {
            _live = _startLive;
        }

        public void SetLive(bool flag)
        {
            _live = flag;
        }
    }
}
