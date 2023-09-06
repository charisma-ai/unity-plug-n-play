
#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace CharismaSDK.PlugNPlay
{
    [CustomPropertyDrawer(typeof(TaskParameterPicker))]
    public class TaskParameterPickerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TaskParameterPicker atb = attribute as TaskParameterPicker;
            var taskField = atb.MyType.GetField(atb.PropertyName);

            if (taskField != null)
            {
                var objectFromProperty = GetTargetParentOfProperty(property);

                if (objectFromProperty is TaskRequest request)
                {
                    var resultingType = GetTypeForNPCTask(request.TaskToUse, request.ObjectParameters);

                    if (resultingType != null)
                    {
                        property.managedReferenceValue = resultingType.GetConstructor(Type.EmptyTypes).Invoke(null);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }

                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private Type GetTypeForNPCTask(NPCTask npcTask, TaskParameters parameters)
        {
            if (npcTask is LookAtObjectTask && parameters is not LookAtObjectParameters)
            {
                return typeof(LookAtObjectParameters);
            }

            if (npcTask is PlayAnimationTask && parameters is not PlayAnimationParameters)
            {
                return typeof(PlayAnimationParameters);
            }

            if (npcTask is MoveToLocationTask && parameters is not MoveToParameters)
            {
                return typeof(MoveToParameters);
            }

            return null;
        }

        // Code found online for reflection: https://forum.unity.com/threads/casting-serializedproperty-to-the-desired-type.1169618/
        // TODO: this claims to have open license, but worth double checking
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetParentOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            object parent = obj;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    parent = obj;
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    parent = obj;
                    obj = GetValue_Imp(obj, element);
                }
            }
            return parent;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}
#endif
