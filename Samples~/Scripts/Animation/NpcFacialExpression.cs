using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    [CreateAssetMenu(fileName = "NPC Facial Expression", menuName = "Charisma/Config/NPC Facial Expression", order = 0),
     Tooltip("Stores the blendshapes for a facial expression, and provides methods to get them from and set them to a currently selected skinned mesh renderer")]
    public class NpcFacialExpression : ScriptableObject
    {
        public string AssociatedCharismaEmotion => _associatedCharismaEmotion;

        [SerializeField]
        private string _associatedCharismaEmotion;

        public SerializedDictionary<string, BlendshapeGroup> BlendshapeGroups => _blendShapeGroups;

        [SerializeField] 
        private SerializedDictionary<string, BlendshapeGroup> _blendShapeGroups;
        
#if UNITY_EDITOR

        public void GetCurrentFacialExpressionFromSelection(SkinnedMeshRenderer smr)
        {
            var blendshapes = smr.GetBlendShapes(false);
            
            if (!_blendShapeGroups.ContainsKey(smr.name))
            {
                _blendShapeGroups.Add(smr.name, new BlendshapeGroup(blendshapes));
                return;
            }
            
            _blendShapeGroups[smr.name].ReinitBlendshapes(blendshapes);
        }

        public void ApplyFacialExpressionToSelection(SkinnedMeshRenderer smr)
        {
            smr.ResetBlendWeights();
            smr.SetBlendShapeWeights(_blendShapeGroups[smr.name].Blendshapes, 1, false);
        }

        public void ResetSelectedFacialExpression(SkinnedMeshRenderer smr)
        {
            smr.ResetBlendWeights();
        }

#endif
    }
}