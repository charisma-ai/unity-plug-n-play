using UnityEngine;


namespace CharismaSDK.PlugNPlay
{
    internal class SetLookAtFunction : MetadataFunction
    {
        public override string MetadataId => "set-look-at";

        public override void Execute(string metadataValue)
        {
            string[] subjectTarget = metadataValue.Split(',');
            if (subjectTarget.Length == 1 && subjectTarget[0] == "player")
            {
                foreach (var actor in Dependencies.PlaythroughEntities.Actors)
                {
                    if (actor is CharismaHumanoidActor npc)
                    {
                        var player = Dependencies.PlaythroughEntities.GetPlayerEntity();

                        if (player != default)
                        {
                            npc.HumanoidCharacter?.LookAtObject(player.gameObject, -1);
                        }
                    }
                }
            }

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
                // find target
                var targetObject = Dependencies.PlaythroughEntities.GetGameObjectByID(target);
                if (targetObject != default)
                {
                    // request look at
                    humanoid.SetLookAtTarget(targetObject.transform);
                }
            }
        }
    }
}
