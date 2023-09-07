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
        [Tooltip("Unique ID of the story that you want to play.")]
        private int _storyId;

        [SerializeField]
        [Tooltip("The version of the story you want to play. If set to 0, will load the latest published version. If set to -1, will load the current draft version. The draft also requires the API key to be set")]
        private int _storyVersion;

        [SerializeField]
        [Tooltip("Used for loading the draft version of the story.")]
        private string _apiKey;

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

        private CreatePlaythroughTokenParams _tokenParameters;

        public void Initialise()
        {
            _entities.FindAllValidPlaythroughEntities();
            _tokenParameters = new CreatePlaythroughTokenParams(_storyId, _storyVersion, _apiKey);
        }
    }
}
