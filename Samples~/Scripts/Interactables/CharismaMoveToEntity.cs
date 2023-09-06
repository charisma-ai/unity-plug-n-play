using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Move-To entity
    /// Target object for move-to metadata commands
    /// Actors will try to move to these entities when given the prompt to
    /// </summary>
    public class CharismaMoveToEntity : CharismaPlaythroughEntity
    {
        /// <summary>
        /// Distance from the MoveToEntity where the Actor will consider having reached the destination
        /// </summary>
        public float StoppingDistance => _stoppingDistance;

        [SerializeField]
        private float _stoppingDistance = 0.25f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Debug.DrawLine(this.transform.position, this.transform.position + Vector3.up, Color.blue);
        }

#endif

    }
}
