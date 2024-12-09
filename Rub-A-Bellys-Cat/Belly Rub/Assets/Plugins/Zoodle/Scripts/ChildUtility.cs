using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zoodle
{
    /// <summary>
    /// Static class that provides helpful functions for dealing with child GameObjects.
    /// </summary>
    public static class ChildUtility
    {
        /// <summary>
        /// Instantiates a template GameObject for each source item, and then runs the specified initialization method.
        /// For example, spawning multiple GameObjects with a LevelDisplay component, from a list of LevelData objects.
        /// </summary>
        /// <typeparam name="TSource">Type of the source item that will be used as input data.</typeparam>
        /// <param name="parent">RectTransform to act as parent for all instantiated GameObjects.</param>
        /// <param name="templateObject">GameObject to use as a template.</param>
        /// <param name="source">IEnumerable of the specified type.</param>
        /// <param name="initMethod">Method that takes an instantiated GameObject, and initializes it from a source item.</param>
        /// <param name="isSetActive">Whether to set the instantiated GameObject active after initialization.</param>
        public static void Populate<TSource>(Transform parent, GameObject templateObject, IEnumerable<TSource> source, Action<GameObject, TSource> initMethod, bool isSetActive = true)
        {
            foreach(TSource sourceItem in source)
            {
                GameObject newObject = UnityEngine.Object.Instantiate(templateObject, parent);
                newObject.SetActive(isSetActive);
                initMethod.Invoke(newObject, sourceItem);
            }
        }
        /// <summary>
        /// Instantiates a template GameObject for a source item, and then runs the specified initialization method.
        /// For example, spawning a GameObject with a LevelDisplay component, using a LevelData object.
        /// </summary>
        /// <typeparam name="TSource">Type of the source item that will be used as input data.</typeparam>
        /// <param name="parent">RectTransform to act as parent for all instantiated GameObjects.</param>
        /// <param name="templateObject">GameObject to use as a template.</param>
        /// <param name="sourceItem">Single instance of the specified type.</param>
        /// <param name="initMethod">Method that takes an instantiated GameObject, and initializes it from a source item.</param>
        /// <param name="isSetActive">Whether to set the instantiated GameObject active after initialization.</param>
        public static void Populate<TSource>(Transform parent, GameObject templateObject, TSource sourceItem, Action<GameObject, TSource> initMethod, bool isSetActive = true)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(templateObject, parent);
            newObject.SetActive(isSetActive);
            initMethod.Invoke(newObject, sourceItem);
        }

        /// <summary>
        /// Destroys all child GameObjects except the specified protected objects.
        /// </summary>
        /// <param name="parent">RectTransform to have its children cleared.</param>
        /// <param name="protectedObjects">List of child GameObjects that will not be destroyed.
        /// </param>
        public static void Clear(Transform parent, params GameObject[] protectedObjects)
        {
            GameObject child;
            if (protectedObjects == null || protectedObjects.Length == 0)
            {
                for (int i = parent.childCount-1; i >= 0; i--)
                {
                    child = parent.GetChild(i).gameObject;
                    UnityEngine.Object.Destroy(child);
                }
            }
            else
            {
                for (int i = parent.childCount-1; i >= 0; i--)
                {
                    child = parent.GetChild(i).gameObject;
                    if (!protectedObjects.Contains(child))
                        UnityEngine.Object.Destroy(child);
                }
            }
        }
    }
}
