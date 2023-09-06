using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class BlockInteractableFunction : MetadataFunction
    {
        public override string MetadataId => "block-interact";

        public override void Execute(string metadataValue)
        {
            var entity = Dependencies.PlaythroughEntities.GetEntityByName(metadataValue);

            if (entity != default
                && entity is CharismaInteractableEntity interactable)
            {
                interactable.SetEnabled(false);
            }
            else
            {
                Debug.LogWarning($"Interactable with name {metadataValue} not found. Cannot enable interaction.");
            }
        }
    }
}
