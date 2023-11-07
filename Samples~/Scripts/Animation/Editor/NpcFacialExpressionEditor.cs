
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [CustomEditor(typeof(NpcFacialExpression))]
    public class NpcFacialExpressionEditor : Editor
    {
        private SkinnedMeshRenderer _smr;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _smr = EditorGUILayout.ObjectField("Reference Mesh", _smr, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

            if (_smr != default)
            {
                NpcFacialExpression expression = (NpcFacialExpression)target;

                if (GUILayout.Button("Get Current Facial Expression From Selection"))
                {
                    expression.GetCurrentFacialExpressionFromSelection(_smr);
                    EditorUtility.SetDirty(expression);
                }
                if (GUILayout.Button("Apply Facial Expression To Selection"))
                {
                    expression.ApplyFacialExpressionToSelection(_smr);
                }
                if (GUILayout.Button("Reset Selected Facial Expression"))
                {
                    expression.ResetSelectedFacialExpression(_smr);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
