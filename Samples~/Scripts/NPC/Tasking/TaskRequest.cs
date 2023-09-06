
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    [Serializable]
    public class TaskRequest
    {
        [SerializeField]
        public NPCTask TaskToUse;

        [SerializeReference, TaskParameterPicker(typeof(TaskRequest), "TaskToUse")]
        public TaskParameters ObjectParameters;
    }
}
