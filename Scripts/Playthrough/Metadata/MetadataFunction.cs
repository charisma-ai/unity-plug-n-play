using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public abstract class MetadataFunction : ScriptableObject
    {
        public IMetadataDependencies Dependencies => _metadataDependencies;

        private IMetadataDependencies _metadataDependencies;

        public abstract string MetadataId { get; }

        public void AssignDependencies(IMetadataDependencies metadataDependencies)
        {
            _metadataDependencies = metadataDependencies;
        }

        public abstract void Execute(string metadataValue);
    }
}
