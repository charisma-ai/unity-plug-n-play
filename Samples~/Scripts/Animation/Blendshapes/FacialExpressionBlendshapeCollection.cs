using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Collection of facial expressions blendshaping maps associated with a target Mesh
    /// </summary>
    public class FacialExpressionBlendshapeCollection
    {
        private Mesh _mesh;

        // Dictionary of facial expressions and their respective 
        private Dictionary<NpcFacialExpression, Dictionary<int, float>> _facialExpressionMap = new Dictionary<NpcFacialExpression, Dictionary<int, float>>();

        public FacialExpressionBlendshapeCollection(Mesh mesh)
        {
            _mesh = mesh;
        }

        internal bool Contains(NpcFacialExpression facialAnimation)
        {
            return _facialExpressionMap.ContainsKey(facialAnimation);
        }

        /// <summary>
        /// Registers facial expression to this Collection
        /// Generates the various target blendshapes and intensities associated with the facial animation
        /// </summary>
        internal void Register(NpcFacialExpression facialAnimation)
        {
            var blendshapesDictionary = _mesh.GetBlendshapesDic();

            var indexedBlendshapes = new Dictionary<int, float>();

            foreach (var entry in blendshapesDictionary)
            {
                float intensity = 0.0f;

                foreach (var blendshape in facialAnimation.Blendshapes)
                {
                    if (blendshape.BlendName == entry.Key)
                    {
                        intensity = blendshape.Intensity;
                        break;
                    }
                }

                if (intensity > 0)
                {
                    indexedBlendshapes.Add(entry.Value, intensity);
                }
            }

            _facialExpressionMap.Add(facialAnimation, indexedBlendshapes);
        }

        /// <summary>
        /// Returns the target blendshape indices and expected intensity/weight
        /// </summary>
        /// <param name="facialExpression">Target facial expression</param>
        internal Dictionary<int, float> GetBlendshapeTargets(NpcFacialExpression facialExpression)
        {
            return _facialExpressionMap[facialExpression];
        }
    }
}
