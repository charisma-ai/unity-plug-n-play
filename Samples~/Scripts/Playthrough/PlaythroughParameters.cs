using CharismaSDK;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class PlaythroughParameters
    {
        public CreatePlaythroughTokenParams PlaythroughToken => _tokenParameters;
        public string StartGraphReferenceId => _startGraphReferenceId;
        public List<MetadataFunction> MetadataFunctions => _metadataFunctions;
        public PlaythroughEntities Entities => _entities;

        [SerializeField]
        private CreatePlaythroughTokenParams _tokenParameters;

        [SerializeField]
        [Tooltip("Specificies the node with which the Playthrough graph will start with. Can be found in the Charisma story.")]
        private string _startGraphReferenceId;

        [SerializeField]
        [Tooltip("Collection of metadata functions that will be registered to this playthrough.")]
        private List<MetadataFunction> _metadataFunctions;

        [ReadOnly]
        [SerializeField]
        [Tooltip("Collection of CharismaEntities that will be registered to this playthrough.")]
        private PlaythroughEntities _entities;

        public void Initialise()
        {
            _entities.FindAllValidPlaythroughEntities();
        }
    }
}
