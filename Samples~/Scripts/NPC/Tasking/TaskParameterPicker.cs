
using System;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Custom code built specifically for TaskRequest
    /// Picker Attribute used to grab the correct TaskParameters given a NPCTask
    /// Needs the type of the current parent class and property name of the NPCTask
    /// Which is used as reference for geting the correct taskParam implementation
    /// </summary>
    public class TaskParameterPicker : PropertyAttribute
    {
        public Type MyType;
        public string PropertyName;

        public TaskParameterPicker(Type type, string property)
        {
            MyType = type;
            PropertyName = property;
        }
    }
}
