
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class ResetInteractableFunction : MetadataFunction
    {
        public override string MetadataId => "reset-interact";

        public override void Execute(string metadataValue)
        {
            string[] subjectTargets = metadataValue.Split(',');

            foreach (var target in subjectTargets)
            {
                var entity = Dependencies.PlaythroughEntities.GetEntityByName(target);

                if (entity != default && entity is CharismaInteractableEntity interactable)
                {
                    interactable.ResetUse();
                }
                else
                {
                    Debug.LogWarning($"Interactable with name {target} not found. Cannot reset use.");
                }
            }
        }
    }
}
