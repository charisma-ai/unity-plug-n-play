using UnityEngine;

/// <summary>
/// Data structure used to define a blendshape
/// Currently designed with only one blendshape ID/name in mind and target intensity
/// For more complex, multi-blendshape expressions consider expanding from this structure.
/// </summary>
[System.Serializable]
public struct Blendshape
{
    [SerializeField]
    public string BlendName;

    [SerializeField]
    public float Intensity;
}
