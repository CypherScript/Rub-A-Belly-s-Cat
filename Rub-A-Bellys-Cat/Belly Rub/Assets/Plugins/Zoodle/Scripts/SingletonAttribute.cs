using System;

namespace Zoodle
{
    public class SingletonAttribute : Attribute
    {

        /// <summary>
        /// Method to call when the singleton is initialized.
        /// </summary>
        public string InitMethod { get; }

        /// <summary>
        /// Property to store the return value of the initialize method.
        /// </summary>
        public string InstanceProperty { get; }

        /// <summary>
        /// Determines the order that the singleton is initialized. Higher values are initialized first.
        /// </summary>
        public int Priority { get; }

        public SingletonAttribute(string initMethod = "Initialize", string instanceProperty = "Instance", int priority = 0)
        {
            InitMethod = initMethod;
            InstanceProperty = instanceProperty;
            Priority = priority;
        }
    }
}