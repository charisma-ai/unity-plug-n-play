
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class PlayAnimationTask : NPCTask
    {
        private PlayAnimationParameters _parameters;
        private AnimationLayerData _layer;
        private string _correctedAnimationName;

        public PlayAnimationTask(PlayAnimationParameters parameters)
        {
            _parameters = parameters;
        }

        internal override bool CanPerform(HumanoidNPCCharacterController humanoidNPC)
        {
            // need to query FSM state internally
            var animationConfig = humanoidNPC.AnimationController.Configuration;

            if (animationConfig.AnimationMetadata.ContainsAnimation(_parameters.AnimationName, out var layer, out var internalAnimName))
            {
                _layer = layer;
                _correctedAnimationName = internalAnimName;
                return true;
            }

            Debug.LogError($"Cannot execute Play Animation Task - animation {_parameters.AnimationName} is not found in the config file.");

            return false;

        }

        internal override void TaskStart(HumanoidNPCCharacterController humanoidNPC)
        {
            // base at the start
            base.TaskStart(humanoidNPC);

            humanoidNPC.AnimationController.PlayAnimationFromLayer(_correctedAnimationName, _layer, _parameters.Force);
        }

        internal override void TaskStop(HumanoidNPCCharacterController humanoidNPC, bool force = false)
        {
            // base at the end
            base.TaskStop(humanoidNPC, force);
        }

        internal override bool TaskUpdate(HumanoidNPCCharacterController humanoidNPC, float timeStep)
        {
            base.TaskUpdate(humanoidNPC, timeStep);

            return humanoidNPC.AnimationController.HasRequestedAnimationFinished();
        }
    }
}
