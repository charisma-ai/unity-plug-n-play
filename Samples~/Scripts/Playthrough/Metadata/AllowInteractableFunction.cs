
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class AllowInteractableFunction : MetadataFunction
    {
        public override string MetadataId => "allow-interact";

        public override void Execute(string metadataValue)
        {
            var entity = Dependencies.PlaythroughEntities.GetEntityByName(metadataValue);

            if (entity != default
                && entity is CharismaInteractableEntity interactable)
            {
                interactable.SetEnabled(true);
            }
            else
            {
                Debug.LogWarning($"Interactable with name {metadataValue} not found. Cannot enable interaction.");
            }
        }
    }
}
