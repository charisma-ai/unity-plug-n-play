
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    [CreateAssetMenu(fileName = "NPC Facial Expression", menuName = "Charisma/Config/NPC Facial Expression", order = 0),
 Tooltip(
     "Stores the blendshapes for a facial expression, and provides methods to get them from and set them to a currently selected skinned mesh renderer")]
    public class NpcFacialExpression : ScriptableObject
    {
        public string AssociatedCharismaEmotion => _associatedCharismaEmotion;

        [SerializeField]
        private string _associatedCharismaEmotion;

        public List<Blendshape> Blendshapes => _blendshapes;

        [SerializeField]
        private List<Blendshape> _blendshapes;

#if UNITY_EDITOR

        public void GetCurrentFacialExpressionFromSelection(SkinnedMeshRenderer smr)
        {
            _blendshapes = smr.GetBlendShapes(false);
        }

        public void ApplyFacialExpressionToSelection(SkinnedMeshRenderer smr)
        {
            smr.ResetBlendWeights();
            smr.SetBlendShapeWeights(_blendshapes, 1, false);
        }

        public void ResetSelectedFacialExpression(SkinnedMeshRenderer smr)
        {
            smr.ResetBlendWeights();
        }

#endif
    }
}
