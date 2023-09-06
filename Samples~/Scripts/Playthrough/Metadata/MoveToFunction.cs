using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    internal class MoveToFunction : MetadataFunction
    {
        public override string MetadataId => "move-to";

        public override void Execute(string metadataValue)
        {
            string[] subjectTarget = metadataValue.Split(',');
            if (subjectTarget.Length != 2)
            {
                Debug.LogError($"[MoveToFunction] Could not parse lookat subject and target from '{metadataValue}'");
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
                var targetObject = Dependencies.PlaythroughEntities.GetEntityByName(target);
                if (targetObject != default)
                {
                    Tasking.SetNextTaskPriority(NPCTask.TaskPriority.High);

                    // request Move To
                    if (targetObject is CharismaMoveToEntity moveTo)
                    {
                        // ping moveTo entity for information regarding Stopping Distance
                        humanoid.AddMetaDataTask(new MoveToLocationTask(new MoveToParameters(targetObject.transform, moveTo.StoppingDistance)));
                    }
                    else
                    {
                        // otherwise, use default
                        humanoid.AddMetaDataTask(new MoveToLocationTask(new MoveToParameters(targetObject.transform)));
                    }
                }
                else
                {
                    Debug.Log($"[MoveToFunction] Could not find Target Destination with id {target}");
                }
            }
            else
            {
                Debug.Log($"[MoveToFunction] Could not find NPC with id {character}");
            }
        }
    }
}
