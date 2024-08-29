using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class BlendshapeGroup
    {
        public List<Blendshape> Blendshapes => _blendshapes;

        [SerializeField] private List<Blendshape> _blendshapes;

        public BlendshapeGroup(List<Blendshape> blendshapes)
        {
            _blendshapes = blendshapes;
        }

        public void ReinitBlendshapes(List<Blendshape> blendshapes)
        {
            _blendshapes = blendshapes;
        }
    }
}