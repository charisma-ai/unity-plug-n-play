
namespace CharismaSDK.PlugNPlay
{
    public class MoveToLocationTask : NPCTask
    {
        private MoveToParameters _parameters;

        public MoveToLocationTask(MoveToParameters parameters)
        {
            _parameters = parameters;
        }

        internal override bool CanPerform(HumanoidNPCCharacterController humanoidNPC)
        {
            // need to query FSM state internally
            return humanoidNPC.CanMove();
        }

        internal override void TaskStart(HumanoidNPCCharacterController humanoidNPC)
        {
            // base at the start
            base.TaskStart(humanoidNPC);

            humanoidNPC.SetGoToTarget(_parameters.Target, _parameters.StoppingDistance);
        }

        internal override void TaskStop(HumanoidNPCCharacterController humanoidNPC, bool force = false)
        {
            humanoidNPC.ClearGoToTarget();

            // base at the end
            base.TaskStop(humanoidNPC, force);
        }

        internal override bool TaskUpdate(HumanoidNPCCharacterController humanoidNPC, float timeStep)
        {
            base.TaskUpdate(humanoidNPC, timeStep);

            if (humanoidNPC.NavmeshComponent.HasReachedTarget)
            {
                return true;
            }

            return false;
        }
    }
}
