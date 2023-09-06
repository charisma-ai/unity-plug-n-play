
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Abstract Lipsync "Helper" class
    /// Used to implement the various possible Lipsync implementations
    /// Inherit from this class and write your own custom Lipsync depending on their requirements
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public abstract class LipsyncHelper : MonoBehaviour
    {
        public SkinnedMeshRenderer HeadMeshRenderer => _headMeshRenderer;

        /// <summary>
        /// Provide list of string VisemeToBlendTargets, as that seems to be the most common lipsync implementation
        /// This should cover most cases
        /// </summary>
        public List<string> VisemeToBlendTargets => _visemeToBlendTargets;

        [SerializeField]
        [Tooltip("Reference head mesh, used for lipsync.")]
        private SkinnedMeshRenderer _headMeshRenderer;

        [SerializeField]
        [Tooltip("List of Viseme aimed for blending, which can be passed on into the expected implementation. NOTE: Needs some cleanup depending on implementation, as at time of writing this grabs all blendshapes available on the headMeshRenderer.")]
        private List<string> _visemeToBlendTargets;

        public abstract void CreateLipsyncImplementation();

        public abstract void ResetBlendshapes();

        public void GetAllBlendTargets()
        {
            _visemeToBlendTargets.Clear();
            var listOfBlendshapes = _headMeshRenderer.GetBlendShapes(true);
            foreach (var blendshape in listOfBlendshapes)
            {
                _visemeToBlendTargets.Add(blendshape.BlendName);
            }
        }
    }
}
