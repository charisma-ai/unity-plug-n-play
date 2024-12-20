using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Flags]
    public enum AnimationFlags
    {
        None = 0,
        Talking = 1,
        Standing = 2,
        Idle = 4,
        Mannerism = 8,
        Walking = 16,
        Thinking = 32
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NPC Animation Config", menuName = "Charisma/Config/NPC Animation Config", order = 0),
     Tooltip(
         "Manages settings for NPC animations, including a database of all animation clips and their labels, as well as facial expressions and their associated emotion")]
    public class HumanoidNPCAnimationConfig : ScriptableObject
    {
        public AnimationCurve BlinkTime => _blinkTime;
        public AnimationCurve BlinkInterval => _blinkInterval;

        public List<Blendshape> BlinkBlendshapes => _blinkBlendshapes;
        public List<NpcFacialExpression> FacialExpressions => _facialExpressions;
        public AnimationMetadata AnimationMetadata => _animationMetadata;

        [Header("Eyes")]
        [SerializeField] private AnimationCurve _blinkTime;
        [SerializeField] private AnimationCurve _blinkInterval;
        [SerializeField] private List<Blendshape> _blinkBlendshapes = new List<Blendshape>();


        [Header("Animator Data")]
        [SerializeField]
        private AnimationMetadata _animationMetadata;

#if UNITY_EDITOR
        [SerializeField]
        private AnimatorController _referenceAnimationController;
#endif

        [Header("Facial Expressions")]
        [SerializeField]
        private List<NpcFacialExpression> _facialExpressions;

#if UNITY_EDITOR

        public void RefreshExpressionDatabase()
        {
            InitialiseFacialExpressions();
            EditorUtility.SetDirty(this);
        }

        public void CreateAnimationDataFromController()
        {
            if (_referenceAnimationController != default)
            {
                _animationMetadata.CreateAnimationData(_referenceAnimationController);
                EditorUtility.SetDirty(this);
            }
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();

            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        private void InitialiseFacialExpressions()
        {
            //Refresh facial expressions
            _facialExpressions = FindAssetsByType<NpcFacialExpression>();
        }

#endif

        public bool GetExpression(string expressionName, out NpcFacialExpression expressionOut)
        {
            foreach (var expression in _facialExpressions)
            {
                if (expression.AssociatedCharismaEmotion == expressionName)
                {
                    expressionOut = expression;
                    return true;
                }
            }

            expressionOut = default;
            return false;
        }
    }
}
