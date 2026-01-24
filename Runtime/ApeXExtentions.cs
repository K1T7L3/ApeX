using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ApeX.Extentions.Runtime
{
    public static class ApeXRuntimeExt
    {
        /// <summary>
        /// Searches through objects to the corresponding component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <returns></returns>
        public static T GetComponentInAll<T>(this Component c) where T : Component
        {
            T found = c.GetComponent<T>();
            if (found != null)
                return found;

            found = c.GetComponentInChildren<T>();
            if (found != null)
                return found;

            found = c.GetComponentInParent<T>();
            return found;
        }
    }
}

#if UNITY_EDITOR
namespace ApeX.Extentions.Editor
{
    public static class ApeXEditorExt
    {
        // Adapted from S_Darkwell on unity fourms
        // https://discussions.unity.com/t/adding-layer-by-script/407882/16
        /// <summary>
        /// Create a layer at the next available index. Returns silently if layer already exists.
        /// </summary>
        /// <param name="name">Name of the layer to create</param>
        public static void CreateLayer(string name, int index)
        {
            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentNullException(nameof(name), "New layer name is null or empty.");

            if (index < 8 || index >= 32)
            {
                Debug.LogError("Layer index must be between 8 and 31 (inclusive). Unity reserves 0–7.");
                return;
            }

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
            );

            var layersProp = tagManager.FindProperty("layers");

            if (layersProp == null || index >= layersProp.arraySize)
            {
                Debug.LogError("Could not find layers property.");
                return;
            }

            var targetLayer = layersProp.GetArrayElementAtIndex(index);

            if (targetLayer.stringValue == name)
                return; // already set

            if (!string.IsNullOrEmpty(targetLayer.stringValue))
            {
                Debug.LogWarning($"Overwriting existing layer '{targetLayer.stringValue}' at index {index} with '{name}'.");
            }

            targetLayer.stringValue = name;
            tagManager.ApplyModifiedProperties();

            Debug.Log($"Layer '{name}' created at index {index}.");
        }

        /// <summary>
        /// Checks through the LayerManager to find if the layer is occupied.
        /// </summary>
        /// <param name="name">Name of the layer to create</param>
        public static bool LayerExists(int index)
        {
            if (index < 0 || index >= 32)
            {
                Debug.LogWarning("Layer index out of range (0–31).");
                return false;
            }

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
            );

            var layersProp = tagManager.FindProperty("layers");

            if (layersProp == null || index >= layersProp.arraySize)
                return false;

            var layerProp = layersProp.GetArrayElementAtIndex(index);

            return !string.IsNullOrEmpty(layerProp.stringValue);
        }
    }
}
#endif
