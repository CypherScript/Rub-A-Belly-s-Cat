using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zoodle
{
    /// <summary>
    /// Attribute to be used in conjunction with DebugUtility.DisplayDebugWindow method. Fields and properties with this attribute can be displayed in a debug window. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisplayInDebugWindowAttribute : Attribute
    {
        /// <summary>
        /// String that overrides the default label for a displayed field or property.
        /// </summary>
        public string Label { get; }

        public DisplayInDebugWindowAttribute(string label = null)
        {
            Label = label;
        }
    }
}