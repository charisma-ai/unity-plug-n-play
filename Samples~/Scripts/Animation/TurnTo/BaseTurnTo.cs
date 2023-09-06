using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Base turn-to helper class - used to turn the NPC to face a particular target position
    /// This has been abstracted in case of different implementations
    /// </summary>
    public abstract class BaseTurnTo
    {
        /// <summary>
        /// Generic update function
        /// </summary>

        public abstract void Update();

        /// <summary>
        /// Assigns a turn-to target
        /// This is done via Unity's gameobject, for concrete tracking of its moving transform
        /// </summary>
        /// <param name="gameobject">Target Object</param>
        /// <param name="instant">If the look-at should snap instantly into position</param>
        public abstract void SetTurnToTarget(GameObject gameObject);

        /// <summary>
        /// Assigns a turn-to target
        /// This is done via Unity's gameobject, for concrete tracking of its moving transform
        /// </summary>
        /// <param name="gameobject">Target Object</param>
        /// <param name="instant">If the look-at should snap instantly into position</param>
        public abstract void SetTurnToTarget(Vector3 position);

        /// <summary>
        /// Removes the turn to target
        /// Both positional and GameObject target.
        /// </summary>
        public abstract void ClearTurnTo();
    }
}
