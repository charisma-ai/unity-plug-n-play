using UnityEngine;
using static CharismaSDK.PlugNPlay.NPCTask;

namespace CharismaSDK.PlugNPlay
{
    public static class Tasking
    {
        private static TaskPriority _nextPriority;

        public static void SetNextTaskPriority(TaskPriority priority)
        {
            _nextPriority = priority;
        }

        public static void GoToObject(this HumanoidNPCCharacterController humanoidNPC, GameObject target, float distance = 1.5f)
        {
            var task = new MoveToLocationTask(new MoveToParameters(target.transform, distance));
            humanoidNPC.AddTaskToNPC(task);
        }

        public static void LookAtObject(this HumanoidNPCCharacterController humanoidNPC, GameObject target, float durationS = -1)
        {
            var task = new LookAtObjectTask(new LookAtObjectParameters(target, durationS));
            humanoidNPC.AddTaskToNPC(task);
        }

        public static void TurnToAndLookAtObject(this HumanoidNPCCharacterController humanoidNPC, GameObject target, float durationS = -1)
        {
            var task = new TurnToAndLookAtObjectTask(new TurnToAndLookAtObjectParameters(target, durationS));
            humanoidNPC.AddTaskToNPC(task);
        }
        public static void PlayAnimation(this HumanoidNPCCharacterController humanoidNPC, string animation, bool force = false)
        {
            var task = new PlayAnimationTask(new PlayAnimationParameters(animation, force));
            humanoidNPC.AddTaskToNPC(task);
        }

        private static void AddTaskToNPC(this HumanoidNPCCharacterController humanoidNPC, NPCTask task)
        {
            task.Priority = _nextPriority;
            humanoidNPC.AddTask(task);
            // Clear next priority
            _nextPriority = TaskPriority.Low;
        }
    }
}
