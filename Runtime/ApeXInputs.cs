using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace ApeX.Input
{
    public static class ApeXInputs
    {
        private static readonly Dictionary<XRNode, InputDevice> nodeMap;
        private static XRNode Controller(bool leftHand) => leftHand ? XRNode.LeftHand : XRNode.RightHand;

        public static bool TriggerDown(bool leftHand, float threshold = 0.5f) => GetValue(CommonUsages.trigger, Controller(leftHand)) >= threshold;
        public static bool GripDown(bool leftHand, float threshold = 0.5f) => GetValue(CommonUsages.grip, Controller(leftHand)) >= threshold;

        public static float TriggerValue(bool leftHand) => GetValue(CommonUsages.trigger, Controller(leftHand));
        public static float GripValue(bool leftHand) => GetValue(CommonUsages.grip, Controller(leftHand));

        public static bool TriggerTouch(bool leftHand) => GetValue(CommonUsages.triggerButton, Controller(leftHand));
        public static bool GripTouch(bool leftHand) => GetValue(CommonUsages.gripButton, Controller(leftHand));

        public static bool PrimaryDown(bool leftHand) => GetValue(CommonUsages.primaryButton, Controller(leftHand));
        public static bool SecondaryDown(bool leftHand) => GetValue(CommonUsages.secondaryButton, Controller(leftHand));
        public static bool PrimaryTouch(bool leftHand) => GetValue(CommonUsages.primaryTouch, Controller(leftHand));
        public static bool SecondaryTouch(bool leftHand) => GetValue(CommonUsages.secondaryTouch, Controller(leftHand));

        public static Vector2 Thumbstick(bool leftHand) => GetValue(CommonUsages.primary2DAxis, Controller(leftHand));
        public static bool ThumbstickTouch(bool leftHand) => GetValue(CommonUsages.primary2DAxisTouch, Controller(leftHand));
        public static bool ThumbstickClick(bool leftHand) => GetValue(CommonUsages.primary2DAxisClick, Controller(leftHand));

        #region Value Types

        /// <summary>
        /// Gets a float value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0.0f).</returns>
        public static float GetValue(InputFeatureUsage<float> feature, XRNode node)
        {
            if (!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        /// <summary>
        /// Gets a vector3 value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0.0f, 0.0f).</returns>
        public static Vector2 GetValue(InputFeatureUsage<Vector2> feature, XRNode node)
        {
            if (!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        /// <summary>
        /// Gets a boolean value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (false).</returns>
        public static bool GetValue(InputFeatureUsage<bool> feature, XRNode node)
        {
            if (!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) && value;
        }
        #endregion

        #region Devices 
        public static bool TryGetDevice(XRNode node, out InputDevice device)
        {
            if (!nodeMap.TryGetValue(node, out device))
                device = CacheInputDevice(node);

            if (!device.isValid)
                return false;

            return true;
        }

        public static InputDevice CacheInputDevice(XRNode node)
        {
            InputDevice nodeDevice = InputDevices.GetDeviceAtXRNode(node);
            if (nodeDevice == null)
            {
                Debug.LogWarning("<color=lightblue>[XRInputs]</color> No device found at given XRNode!");
                return new InputDevice();
            }
            nodeMap[node] = nodeDevice;
            return nodeDevice;
        }
        #endregion
    }
}
