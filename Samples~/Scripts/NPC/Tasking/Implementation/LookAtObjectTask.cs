
namespace CharismaSDK.PlugNPlay
{
    public class LookAtObjectTask : NPCTask
    {
        private LookAtObjectParameters _parameters;

        private float _timer;

        public LookAtObjectTask(LookAtObjectParameters parameters)
        {
            _parameters = parameters;
        }

        internal override bool CanPerform(HumanoidNPCCharacterController humanoidNPC)
        {
            return true;

        }

        internal override void TaskStart(HumanoidNPCCharacterController humanoidNPC)
        {
            // base at the start
            base.TaskStart(humanoidNPC);
            humanoidNPC.AnimationController.SetLookAtTarget(_parameters.Target);
            _timer = _parameters.DurationMS / 1000;
        }

        internal override void TaskStop(HumanoidNPCCharacterController humanoidNPC, bool force = false)
        {
            if (_parameters.DurationMS > -1)
            {
                humanoidNPC.AnimationController.ClearLookAt();
            }

            // base at the end
            base.TaskStop(humanoidNPC, force);
        }

        internal override bool TaskUpdate(HumanoidNPCCharacterController humanoidNPC, float timeStep)
        {
            base.TaskUpdate(humanoidNPC, timeStep);

            // Duration is infinite
            if (_parameters.DurationMS < 0.0f)
            {
                return true;
            }

            _timer -= timeStep;
            if (_timer <= 0.0f)
            {
                return true;
            }

            return false;

        }
    }
}
