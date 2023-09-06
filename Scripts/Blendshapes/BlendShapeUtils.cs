using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public static class BlendShapeUtils
    {
        /// <summary>
        /// Dictionary used to store all the blendshape strings and their respective IDs within the blendshape data
        /// These are organised on a per mesh basis, so each mesh has its own dictionary of IDs and strings once referenced
        ///
        /// NOTE: A bit cumbersome, would do good with some streamlining
        /// </summary>
        private static readonly Dictionary<Mesh, Dictionary<string, int>> _blendShapeCache = new Dictionary<Mesh, Dictionary<string, int>>();

        #region Helper Methods

        /// <summary>
        /// Returns the string to integer dictionary of a particular mesh
        /// If not already determined, will generate that data and store it in the static dictionary _blendshapeCache
        /// </summary>
        public static Dictionary<string, int> GetBlendshapesDic(this Mesh mesh)
        {
            if (!_blendShapeCache.TryGetValue(mesh, out Dictionary<string, int> dic))
            {
                dic = new Dictionary<string, int>();

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    dic.Add(mesh.GetBlendShapeName(i), i);
                }

                _blendShapeCache.Add(mesh, dic);
            }

            return dic;
        }

        /// <summary>
        /// Resets all blendshapes of this particular skinnedmeshrenderer
        /// </summary>
        public static void ResetBlendWeights(this SkinnedMeshRenderer smr)
        {
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                smr.SetBlendShapeWeight(i, 0);
            }
        }

        /// <summary>
        /// Returns all the blendshape weights currently applied to the skinnedmeshrenderer smr
        /// Can optionally skip zero values
        /// TODO: creating a list like this allocates memory, could do with some storage?
        /// </summary>
        /// <param name="smr">Referenced mesh</param>
        /// <param name="nonZeroOnly">Toggle to skip zero values, as they may not be important</param>
        /// <returns>Complete list of blendshapes</returns>
        public static List<Blendshape> GetBlendShapes(this SkinnedMeshRenderer smr, bool nonZeroOnly)
        {
            var dictionary = GetBlendshapesDic(smr.sharedMesh);
            List<Blendshape> blendShapes = new List<Blendshape>();

            foreach (var entry in dictionary)
            {
                var intensity = smr.GetBlendShapeWeight(entry.Value);

                if (nonZeroOnly || intensity > 0)
                {
                    blendShapes.Add(new Blendshape
                    {
                        BlendName = entry.Key,
                        Intensity = smr.GetBlendShapeWeight(entry.Value)
                    });
                }
            }

            return blendShapes;
        }

        /// <summary>
        /// Sets all blendshape weights on this mesh
        /// Applies a multiplier to the intensity before applying 
        /// </summary>
        /// <param name="smr">Referenced mesh</param>
        /// <param name="values">Blendshapes to be applied</param>
        /// <param name="multiplier">Multiplier to change intensity within Blendshape</param>
        /// <param name="nonZeroOnly">Skip zero values</param>
        public static void SetBlendShapeWeights(this SkinnedMeshRenderer smr,
            List<Blendshape> values, float multiplier, bool nonZeroOnly)
        {
            var blendShapeDictionary = GetBlendshapesDic(smr.sharedMesh);

            foreach (var blendValue in values)
            {
                if (!nonZeroOnly || blendValue.Intensity != 0)
                {
                    smr.SetBlendShapeWeight(blendShapeDictionary[blendValue.BlendName], blendValue.Intensity * multiplier);
                }
            }
        }

        #endregion
    }
}
