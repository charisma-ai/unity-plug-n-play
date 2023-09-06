
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class ResetInteractableFunction : MetadataFunction
    {
        public override string MetadataId => "reset-interact";

        public override void Execute(string metadataValue)
        {
            var entity = Dependencies.PlaythroughEntities.GetEntityByName(metadataValue);

            if (entity != default
                && entity is CharismaInteractableEntity interactable)
            {
                interactable.ResetUse();
            }
            else
            {
                Debug.LogWarning($"Interactable with name {metadataValue} not found. Cannot enable interaction.");
            }
        }
    }
}
