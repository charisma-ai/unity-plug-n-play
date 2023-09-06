using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public interface IPlaythroughEntities
    {
        public List<CharismaPlaythroughActor> Actors { get; }

        public CharismaPlayer GetPlayerEntity();
        public CharismaPlaythroughActor GetActorByName(string id);
        public CharismaPlaythroughEntity GetEntityByName(string id);

        public GameObject GetGameObjectByID(string id);
        public void SetAllEntitiesLive(bool flag);
    }
}
