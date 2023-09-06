
namespace CharismaSDK.PlugNPlay
{
    public abstract class NPCTask
    {
        public enum TaskPriority
        {
            Low,
            High
        }

        private enum TaskState
        {
            Inactive,
            JustStarted,
            Running,
            Completed
        }

        private TaskState _state;

        /// <summary>
        /// Returns whether the task has started updating
        /// </summary>
        public bool IsRunning => _state == TaskState.Running || _state == TaskState.JustStarted;

        /// <summary>
        /// Returns whether the task has ran the NPC thru its tasking and succesfully Finished
        /// </summary>
        public bool HasCompleted => _state == TaskState.Completed;

        public TaskPriority Priority { get; set; }

        /// <summary>
        /// Starts the task, given the instruction
        /// </summary>
        internal virtual void TaskStart(HumanoidNPCCharacterController humanoidNPC)
        {
            _state = TaskState.JustStarted;
        }

        /// <summary>
        /// Stops the task, at the current point of execution
        /// </summary>
        /// <param name="force">Used in case an immediate return to the state amchine behaviour is needed</param>
        internal virtual void TaskStop(HumanoidNPCCharacterController humanoidNPC, bool force = false)
        {
            _state = TaskState.Completed;
        }

        /// <summary>
        /// Returns whether can perform this task given the current state and context of the NPCStatemachine
        /// </summary>
        internal abstract bool CanPerform(HumanoidNPCCharacterController humanoidNPC);

        /// <summary>
        /// Update call for tasks lifetime. Runs until task finishes
        /// </summary>
        internal virtual bool TaskUpdate(HumanoidNPCCharacterController humanoidNPC, float timeStep)
        {
            _state = TaskState.Running;
            return false;
        }
    }
}
