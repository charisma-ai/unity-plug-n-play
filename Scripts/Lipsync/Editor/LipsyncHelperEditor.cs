#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Custom Editor for displaying various helper functions
    /// </summary>
    [CustomEditor(typeof(LipsyncHelper), true)]
    class LipsyncHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LipsyncHelper lipsyncHelper = (LipsyncHelper)target;

            if (GUILayout.Button("Reset Blendshapes To Preset"))
            {
                lipsyncHelper.ResetBlendshapes();
            }

            if (GUILayout.Button("Get All Blendshapes"))
            {
                lipsyncHelper.GetAllBlendTargets();
            }

            if (GUILayout.Button("Add Lipsync Components"))
            {
                lipsyncHelper.CreateLipsyncImplementation();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}

#endif
