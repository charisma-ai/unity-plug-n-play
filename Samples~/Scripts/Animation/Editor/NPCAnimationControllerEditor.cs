
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [CustomEditor(typeof(HumanoidNPCAnimationController))]
    public class NPCAnimationControllerEditor : Editor
    {
        private bool _toggleFacialAnimationDisplay;
        private bool _toggleLookAtDisplay;
        private bool _toggleNodeRequestDebug;

        private string _expressionName;
        private float _modifier = 1.0f;

        private HumanoidNPCAnimationConfig _npcAnimationConfig;

        private Animator _animator;

        private NpcFacialExpression _npcFacialEpxresssion;

        private GameObject _targetToLookAt;

        private bool _continous;

        private int _selection;
        private string _animationNode;
        private int _layer;
        private GameObject _targetTurnTo;
        private bool _toggleTurnToDisplay;

        private void OnEnable()
        {
            _npcAnimationConfig = (HumanoidNPCAnimationConfig)serializedObject.FindProperty("_npcAnimationConfig").objectReferenceValue;
            _animator = (Animator)serializedObject.FindProperty("_animator").objectReferenceValue;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            HumanoidNPCAnimationController animationController = (HumanoidNPCAnimationController)target;

            AddFacialExpressionDebug(animationController);
            AddLookAtDebug(animationController);
            AddTurnToDebug(animationController);
            AddNodeRequestDebug(animationController);

            serializedObject.ApplyModifiedProperties();
        }

        private void AddFacialExpressionDebug(HumanoidNPCAnimationController animationController)
        {
            _toggleFacialAnimationDisplay = EditorGUILayout.BeginFoldoutHeaderGroup(_toggleFacialAnimationDisplay, "Facial Animation Debug (Edittime)");

            if (_toggleFacialAnimationDisplay)
            {
                DrawDropdown();

                _modifier = EditorGUILayout.FloatField("Modifier", _modifier);

                if (GUILayout.Button("Set Facial Animation"))
                {
                    animationController.SetFacialExpression(_npcFacialEpxresssion, _modifier, instant: true);
                }

                if (GUILayout.Button("Reset Facial Expression"))
                {
                    animationController.ResetFacialExpression(instant: true);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        private void AddLookAtDebug(HumanoidNPCAnimationController animationController)
        {
            _toggleLookAtDisplay = EditorGUILayout.BeginFoldoutHeaderGroup(_toggleLookAtDisplay, "Look At Debug (Runtime)");

            if (_toggleLookAtDisplay)
            {
                _targetToLookAt = EditorGUILayout.ObjectField("Target Object", _targetToLookAt, typeof(GameObject), true) as GameObject;

                _continous = EditorGUILayout.Toggle("Continous tracking", _continous);

                if (GUILayout.Button("Look At Target") || _continous)
                {
                    animationController.SetLookAtTarget(_targetToLookAt, instant: true);
                }

                if (GUILayout.Button("Clear Look-At Target"))
                {
                    animationController.ClearLookAt();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }


        private void AddTurnToDebug(HumanoidNPCAnimationController animationController)
        {
            _toggleTurnToDisplay = EditorGUILayout.BeginFoldoutHeaderGroup(_toggleTurnToDisplay, "Turn To Debug (Runtime)");

            if (_toggleTurnToDisplay)
            {
                _targetTurnTo = EditorGUILayout.ObjectField("Target Object", _targetTurnTo, typeof(GameObject), true) as GameObject;

                if (GUILayout.Button("Turn To Target") || _continous)
                {
                    animationController.SetTurnToTarget(_targetTurnTo);
                }

                if (GUILayout.Button("Clear Turn To Target"))
                {
                    animationController.ClearLookAt();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        private void AddNodeRequestDebug(HumanoidNPCAnimationController animationController)
        {
            _toggleNodeRequestDebug = EditorGUILayout.BeginFoldoutHeaderGroup(_toggleNodeRequestDebug, "Animation Request Debug (Runtime)");

            if (_toggleNodeRequestDebug)
            {
                if (GUILayout.Button("Play Random Animation"))
                {
                    animationController.RequestAnimationWithFlagsAndEmotion(AnimationFlags.Talking, "");
                }

                _animationNode = EditorGUILayout.TextField("Animation Node", _animationNode);
                _layer = EditorGUILayout.IntField("Layer", _layer);
                var layerName = _animator.GetLayerName(_layer);

                var animationlayer = new AnimationLayerData();
                animationlayer.LayerName = layerName;
                animationlayer.LayerId = _layer;

                if (GUILayout.Button("Play animation"))
                {
                    animationController.PlayAnimationFromLayer(_animationNode, animationlayer, true);
                }

                int count = _animator.layerCount;
                for (int i = 0; i < count; i++)
                {
                    EditorGUILayout.LabelField($"Animation Layer: {i} - {_animator.GetLayerName(i)}");
                    var clipInfo = _animator.GetCurrentAnimatorClipInfo(i);
                    var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);

                    var wave = Animator.StringToHash("Wave");

                    EditorGUILayout.LabelField($"State name matches: {stateInfo.IsName(_animator.GetLayerName(i) + "." + "Wave")}");
                    EditorGUILayout.LabelField($"State hash: {stateInfo.fullPathHash}, {stateInfo.shortNameHash}, wave hash: {wave}");
                    foreach (var clip in clipInfo)
                    {
                        EditorGUILayout.LabelField($"Playing: {clip.clip.name}");
                    }
                }

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // should probably draw a generic method for this
        // but code duplication is acceptable IMO in the case of editor tools like this
        private void DrawDropdown()
        {
            if (_npcAnimationConfig == default)
            {
                return;
            }

            if (_npcAnimationConfig.FacialExpressions == default)
            {
                return;
            }

            int count = _npcAnimationConfig.FacialExpressions.Count;

            if (count == 0)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            string[] options = new string[count];
            int i = 0;
            foreach (var expression in _npcAnimationConfig.FacialExpressions)
            {
                options[i] = expression.name;
                i++;
            }

            _selection = EditorGUILayout.Popup("Label", _selection, options);

            if (_npcFacialEpxresssion == default)
            {
                _npcFacialEpxresssion = _npcAnimationConfig.FacialExpressions[_selection];
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (_selection < count)
                {
                    _npcFacialEpxresssion = _npcAnimationConfig.FacialExpressions[_selection];
                }
            }
        }
    }
}
#endif
