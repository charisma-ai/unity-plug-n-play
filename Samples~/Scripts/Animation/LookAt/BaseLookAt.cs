using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Base look-at helper class - used to move the head of an NPC to face a particular position
    /// This has been abstracted in case of different implementations
    /// In the PnP, we make use of Unity's in-built look-at IK system
    /// </summary>
    public abstract class BaseLookAt
    {
        /// <summary>
        /// Generic update function
        /// May need to be called in Unity's UpdateIK for IK use-cases
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Assigns a look-at target
        /// This is done via Unity's gameobject, for concrete tracking of its moving transform
        /// </summary>
        /// <param name="gameobject">Target Object</param>
        /// <param name="instant">If the look-at should snap instantly into position</param>
        public abstract void SetLookAtTarget(GameObject gameobject, bool instant);


        /// <summary>
        /// Assigns a look-at target
        /// This is done via a direct position, with no tracking
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="instant">If the look-at should snap instantly into position</param>
        public abstract void SetLookAtTarget(Vector3 position, bool instant);

        /// <summary>
        /// Removes look at target.
        /// Clears both Object and Positional targets.
        /// </summary>
        public abstract void ClearLookAt();

        /// <summary>
        /// Returns if a lookAt Target has been assigned
        /// </summary>
        public abstract bool HasLookAtTarget();

        /// <summary>
        /// Returns if the "head" is facing the target. 
        /// This means we've reached the correct orientation for the head relative to the target.
        /// </summary>
        public abstract bool IsFacingTarget();

        /// <summary>
        /// Pauses look at update and snaps the head back into neutral position
        /// Currently used for not overlaying head animation with look-at IK
        /// </summary>
        public abstract void DisableLookAt();
    }
}
