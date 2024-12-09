using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Zoodle
{
    /// <summary>
    /// Static class that provides helpful functions for instantiating singletons.
    /// </summary>
    public static class SingletonUtility
    {
        /// <summary>
        /// True after all singletons have finished initializing.
        /// </summary>
        public static bool IsInitialized { get; private set; } = false;

        public static List<Type> SingletonTypes { get; private set; } = new();

        public static void Register<T>() where T : MonoBehaviour => Register(typeof(T));

        public static void RegisterSO<T>() where T : ScriptableObject => Register(typeof(T));

        private static void Register(Type T)
        {
            SingletonTypes.Add(T);
        }

        /// <summary>
        /// Instantiates a GameObject with the specified singleton MonoBehaviour.
        /// </summary>
        /// <typeparam name="T">Type of the singleton MonoBehaviour.</typeparam>
        /// <param name="name">Name of the GameObject.</param>
        /// <param name="doNotDestroyOnLoad">If true, the GameObject's DoNotDestroyOnLoad flag is set.</param>
        /// <returns></returns>
        public static T Instantiate<T>(string name, bool doNotDestroyOnLoad = true) where T : MonoBehaviour
        {
            GameObject instance = new GameObject(name, typeof(T));
            if(doNotDestroyOnLoad)
                UnityEngine.Object.DontDestroyOnLoad(instance);

            Debug.Log($"{name} initialized.", instance);
            return instance.GetComponent<T>();
        }

        public static T InstantiateFromResources<T>(string name, bool doNotDestroyOnLoad = true) where T : MonoBehaviour
        {
            GameObject instance = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(name));
            instance.name = name;
            if (doNotDestroyOnLoad)
                UnityEngine.Object.DontDestroyOnLoad(instance);

            Debug.Log($"{name} initialized.", instance);
            return instance.GetComponent<T>();
        }

        /// <summary>
        /// Creates a singleton ScriptableObject.
        /// </summary>
        /// <typeparam name="T">Type of the ScriptableObject.</typeparam>
        /// <param name="name">Name of the ScriptableObject.</param>
        /// <returns></returns>
        public static T InstantiateSO<T>(string name) where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            Debug.Log($"{name} initialized.", instance);
            return instance;
        }

        /// <summary>
        /// At the start of the game, calls the initialize methods for all classes that have the Singleton attribute.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            var singletonTypes = SingletonTypes
            .Where(type => type.GetCustomAttribute<SingletonAttribute>() != null)
            .OrderByDescending(type => type.GetCustomAttribute<SingletonAttribute>().Priority);

            float initStartTime = Time.realtimeSinceStartup;
            Debug.Log("Started initializing all singletons.");

            foreach (Type type in singletonTypes)
            {
                SingletonAttribute singletonAttr = type.GetCustomAttribute<SingletonAttribute>();

                if (singletonAttr == null)
                    continue;

                if (singletonAttr.InitMethod != null)
                {
                    MethodInfo initMethod = type.GetMethod(singletonAttr.InitMethod, BindingFlags.Public | BindingFlags.Static);

                    if (initMethod == null)
                    {
                        Debug.LogError($"Initialization method named {singletonAttr.InitMethod} not found in class {type.Name}.");
                        continue;
                    }

                    if (singletonAttr.InstanceProperty != null)
                        SetInstanceProperty(type, singletonAttr.InstanceProperty, initMethod);
                    else
                        initMethod.Invoke(null, null);
                }
            }

            IsInitialized = true;
            Debug.Log($"Initialization time: {(Time.realtimeSinceStartup - initStartTime) * 1000f}ms");
        }

        static void SetInstanceProperty(Type type, string propertyName, MethodInfo initMethod)
        {
            PropertyInfo instanceProperty = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);

            if (instanceProperty == null)
            {
                Debug.LogError($"Instance property named {propertyName} not found in class {type.Name}.");
                return;
            }

            instanceProperty.SetValue(null, initMethod.Invoke(null, null));
        }
    }
}