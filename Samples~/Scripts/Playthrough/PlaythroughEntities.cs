using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Implementation of Playthrough entities
    /// Contains all playthrough objects and provides them to Playthrough systems
    /// Will seek these out using Unity's in built object finder.
    /// </summary>
    [Serializable]
    public class PlaythroughEntities : IPlaythroughEntities
    {
        public List<CharismaPlaythroughActor> Actors => _actors;

        [ReadOnly]
        [SerializeField]
        [Tooltip("Player entity, used to control the the passage of the playthrough via various functions and replies")]
        private CharismaPlayer _player;

        [ReadOnly]
        [SerializeField]
        [Tooltip("Actors/characters that listen for Playthrough messages and act accordingly based on emotional state.")]
        private List<CharismaPlaythroughActor> _actors;

        [ReadOnly]
        [SerializeField]
        [Tooltip("Interactable objects in the scene, which feature custom callbacks to the playthrough.")]
        private List<CharismaInteractableEntity> _interactables;

        [ReadOnly]
        [Tooltip("Go-to destinations defined within the playthrough and given 3d context within the Unity scene.")]
        [SerializeField]
        private List<CharismaMoveToEntity> _gotos;

        internal void FindAllValidPlaythroughEntities()
        {
            _player = GameObject.FindObjectOfType<CharismaPlayer>();
            _actors = GameObject.FindObjectsOfType<CharismaPlaythroughActor>().ToList();
            _interactables = GameObject.FindObjectsOfType<CharismaInteractableEntity>().ToList();
            _gotos = GameObject.FindObjectsOfType<CharismaMoveToEntity>().ToList();
        }

        public CharismaPlayer GetPlayerEntity()
        {
            return _player;
        }

        public CharismaPlaythroughActor GetActorByName(string id)
        {
            foreach (var actor in _actors)
            {
                if (string.Equals(actor.CharacterId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                     return actor;
                }
            }

            return default;
        }

        public CharismaPlaythroughEntity GetEntityByName(string id)
        {
            foreach (var gotoEntity in _gotos)
            {
                if (string.Equals(gotoEntity.EntityId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                    return gotoEntity;
                }
            }

            foreach (var interactable in _interactables)
            {
                if (string.Equals(interactable.EntityId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                    return interactable;
                }
            }

            return default;
        }

        public GameObject GetGameObjectByID(string id)
        {
            if (id == "player")
            {
                return _player?.gameObject;
            }

            foreach (var character in _actors)
            {
                if (string.Equals(character.CharacterId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                    return character.gameObject;
                }
            }

            foreach (var gotoEntity in _gotos)
            {
                if (string.Equals(gotoEntity.EntityId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                    return gotoEntity.gameObject;
                }
            }

            foreach (var interactable in _interactables)
            {
                if (string.Equals(interactable.EntityId, id, StringComparison.InvariantCultureIgnoreCase))
                {
                    return interactable.gameObject;
                }
            }

            return default;
        }

        public void SetAllEntitiesLive(bool flag)
        {
            foreach (var gotoEntity in _gotos)
            {
                gotoEntity.SetLive(flag);
            }

            foreach (var interactable in _interactables)
            {
                interactable.SetLive(flag);
            }
        }

    }
}
