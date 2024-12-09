using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Zoodle
{
    /// <summary>
    /// Static class that provides helpful functions for showing debug information.
    /// </summary>
    public static class DebugUtility
    {
        /// <summary>
        /// To be called from a MonoBehaviour's OnGUI method. Creates an IMGUI panel that displays all fields and properties with the DisplayInDebugWindow attribute.
        /// </summary>
        /// <param name="monoBehaviour">MonoBehaviour to retrieve displayed fields and properties from.</param>
        public static void DisplayDebugWindow(this MonoBehaviour monoBehaviour)
        {
            Type type = monoBehaviour.GetType();
            List<MemberInfo> displayMembers = GetDisplayMembers(type);

            int count = displayMembers.Count;
            float windowHeight = count * 20f + 40f;

            GUILayout.BeginArea(new Rect(10f, 10f, 300f, windowHeight), GUI.skin.box);
            GUILayout.Label($"{type.Name} - Debug Window");

            DisplayInDebugWindowAttribute displayAttribute;
            string label;
            object value;

            foreach (var member in displayMembers)
            {
                displayAttribute = (DisplayInDebugWindowAttribute)Attribute.GetCustomAttribute(member, typeof(DisplayInDebugWindowAttribute));
                label = GetMemberLabel(member, displayAttribute);
                value = GetMemberValue(member, monoBehaviour);
                GUILayout.Label($"{label}: {value}");
            }

            GUILayout.EndArea();
        }

        private static List<MemberInfo> GetDisplayMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(member => Attribute.IsDefined(member, typeof(DisplayInDebugWindowAttribute)))
                .ToList();
        }

        private static object GetMemberValue(MemberInfo member, MonoBehaviour monoBehaviour)
        {
            if (member is FieldInfo field)
                return field.GetValue(monoBehaviour);

            if (member is PropertyInfo property)
                return property.GetValue(monoBehaviour);

            return null;
        }

        private static string GetMemberLabel(MemberInfo member, DisplayInDebugWindowAttribute displayAttribute)
        {
            return displayAttribute?.Label ?? member.Name;
        }
    }
}