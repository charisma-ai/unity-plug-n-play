
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [CustomEditor(typeof(HumanoidNPCAnimationConfig))]
    public class NpcAnimationConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            HumanoidNPCAnimationConfig animationConfig = (HumanoidNPCAnimationConfig)target;

            if (GUILayout.Button("Get Animation Data From Controller"))
            {
                animationConfig.CreateAnimationDataFromController();
            }
            if (GUILayout.Button("Populate Expression Database"))
            {
                animationConfig.RefreshExpressionDatabase();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
