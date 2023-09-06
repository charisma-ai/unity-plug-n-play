using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class PlayAnimationFunction : MetadataFunction
    {
        public override string MetadataId => "play-animation";

        public override void Execute(string metadataValue)
        {
            string[] subjectTarget = metadataValue.Split(',');
            if (subjectTarget.Length != 2)
            {
                Debug.LogError($"Could not parse lookat subject and target from '{metadataValue}' - resulting split: {subjectTarget}");
                return;
            }

            var character = subjectTarget[0];
            var target = subjectTarget[1];

            // find NPC
            var npcCharacter = Dependencies.PlaythroughEntities.GetActorByName(character);
            if (npcCharacter != default
                && npcCharacter is CharismaHumanoidActor humanoid)
            {
                // Play animation
                humanoid.AddMetaDataTask(new PlayAnimationTask(new PlayAnimationParameters(target, true)));
            }
        }
    }
}
