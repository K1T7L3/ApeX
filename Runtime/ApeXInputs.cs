using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace ApeX.Input
{
    public static class ApeXInputs 
    {
        #region Types

        private readonly struct XRInputKey : IEquatable<XRInputKey>
        {
            public readonly XRNode Node;
            public readonly string FeatureName;

            public XRInputKey(XRNode node, string featureName)
            {
                Node = node;
                FeatureName = featureName;
            }

            public bool Equals(XRInputKey other) =>
                Node == other.Node && FeatureName == other.FeatureName;

            public override bool Equals(object obj) =>
                obj is XRInputKey other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine((int)Node, FeatureName);
        }

        #endregion

        #region Subscription

        private static readonly Dictionary<XRInputKey, SubscriptionBase> subscriptions = new();
        private static XRInputRunner runner;

        private abstract class SubscriptionBase
        {
            public abstract bool HasSubscribers { get; }
            public abstract void Tick();
        }

        private sealed class Subscription<T> : SubscriptionBase
        {
            private readonly Func<T> read;
            private readonly Func<T, T, bool> fireCondition;

            private T lastValue;
            private bool initialized;

            private event Action<T> callbacks;
            private int count;

            public Subscription(Func<T> read, Func<T, T, bool> fireCondition)
            {
                this.read = read;
                this.fireCondition = fireCondition;
            }

            public void Add(Action<T> cb)
            {
                callbacks += cb;
                count++;
            }

            public void Remove(Action<T> cb)
            {
                callbacks -= cb;
                count--;
            }

            public override bool HasSubscribers => count > 0;

            public override void Tick()
            {
                T current = read();

                if (initialized && fireCondition(lastValue, current))
                    callbacks?.Invoke(current);

                lastValue = current;
                initialized = true;
            }
        }

        private sealed class XRInputRunner : MonoBehaviour
        {
            private void Update()
            {
                foreach(var sub in subscriptions.Values)
                    sub.Tick();
            }
        }

        private static void EnsureRunner()
        {
            if (runner != null) return;

            GameObject go = new("[XRInputRunner]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<XRInputRunner>();
        }

        private static void DisposeRunner()
        {
            if (runner == null) return;

            UnityEngine.Object.Destroy(runner.gameObject);
            runner = null;
        }

        public static void Subscribe(InputFeatureUsage<bool> feature, bool leftHand, Action callback) =>
            SubscribeInternal(leftHand, feature.name, () => GetValue(feature, Controller(leftHand)), (last, current) => !last && current, _ => callback());
        public static void Subscribe(InputFeatureUsage<float> feature, bool leftHand, Action<float> callback, float threshold = 0.5f) =>
            SubscribeInternal(leftHand, feature.name, () => GetValue(feature, Controller(leftHand)), (last, current) => Mathf.Abs(current - last) >= threshold, callback);
        public static void Subscribe(InputFeatureUsage<Vector2> feature, bool leftHand, Action<Vector2> callback, float threshold = 0.5f) =>
            SubscribeInternal(leftHand, feature.name, () => GetValue(feature, Controller(leftHand)), (last, current) => Vector2.Distance(current, last) >= threshold, callback);
        public static void Subscribe(InputFeatureUsage<Vector3> feature, bool leftHand, Action<Vector3> callback, float threshold = 0.5f) =>
            SubscribeInternal(leftHand, feature.name, () => GetValue(feature, Controller(leftHand)), (last, current) => Vector3.Distance(current, last) >= threshold, callback);

        private static void SubscribeInternal<T>(bool leftHand, string featureName, Func<T> reader, Func<T, T, bool> fireCondition, Action<T> callback) where T : struct
        {
            XRNode node = Controller(leftHand);
            var key = new XRInputKey(node, featureName);

            if (!subscriptions.TryGetValue(key, out var baseSub))
            {
                var sub = new Subscription<T>(reader, fireCondition);
                subscriptions[key] = sub;
                baseSub = sub;
            }

            ((Subscription<T>)baseSub).Add(callback);
            EnsureRunner();
        }

        public static void Unsubscribe<T>(InputFeatureUsage<T> feature, bool leftHand, Action<T> callback) where T : struct
        {
            XRNode node = Controller(leftHand);
            var key = new XRInputKey(node, feature.name);

            if (!subscriptions.TryGetValue(key, out var baseSub))
                return;

            var sub = (Subscription<T>)baseSub;
            sub.Remove(callback);

            if (!sub.HasSubscribers)
                subscriptions.Remove(key);

            if (subscriptions.Count == 0)
                DisposeRunner();
        }

        #endregion

        private static readonly Dictionary<XRNode, InputDevice> nodeMap = new();
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

        public static Vector2 ThumbstickValue(bool leftHand) => GetValue(CommonUsages.primary2DAxis, Controller(leftHand));
        public static bool ThumbstickDown(bool leftHand) => GetValue(CommonUsages.primary2DAxisClick, Controller(leftHand));
        public static bool ThumbstickTouch(bool leftHand) => GetValue(CommonUsages.primary2DAxisTouch, Controller(leftHand));

        public static bool MenuButton(bool leftHand) => GetValue(CommonUsages.menuButton, Controller(leftHand));

        public static Vector3 HandPosition(bool leftHand) => GetValue(CommonUsages.devicePosition, Controller(leftHand));
        public static Quaternion HandRotation(bool leftHand) => GetValue(CommonUsages.deviceRotation, Controller(leftHand));
        public static Vector3 HandAcceleration(bool leftHand) => GetValue(CommonUsages.deviceAcceleration, Controller(leftHand));
        public static Vector3 HandVelocity(bool leftHand) => GetValue(CommonUsages.deviceVelocity, Controller(leftHand));
        public static Vector3 HandAngularVelocity(bool leftHand) => GetValue(CommonUsages.deviceAngularVelocity, Controller(leftHand));
        public static Vector3 HandAngularAcceleration(bool leftHand) => GetValue(CommonUsages.deviceAngularAcceleration, Controller(leftHand));

        public static bool IsTracked(bool leftHand) => GetValue(CommonUsages.isTracked, Controller(leftHand));
        public static float BatteryLevel(bool leftHand) => GetValue(CommonUsages.batteryLevel, Controller(leftHand));
        public static InputTrackingState TrackingState(bool leftHand) => GetValue(CommonUsages.trackingState, Controller(leftHand));

        public static void VibrateController(bool leftHand, float amplitude, float duration) => TriggerVibro(amplitude, duration, Controller(leftHand));

        #region Value Types

        /// <summary>
        /// Gets a float value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0.0f).</returns>
        public static float GetValue(InputFeatureUsage<float> feature, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
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
            if(!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) && value;
        }

        /// <summary>
        /// Gets a Vector2 value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0, 0).</returns>
        public static Vector2 GetValue(InputFeatureUsage<Vector2> feature, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        /// <summary>
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0, 0, 0).</returns>
        public static Vector3 GetValue(InputFeatureUsage<Vector3> feature, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        /// <summary>
        /// Gets a Quaternion value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (0, 0, 0, 1).</returns>
        public static Quaternion GetValue(InputFeatureUsage<Quaternion> feature, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        /// <summary>
        /// Gets a Tracking State value from an xr device.
        /// </summary>
        /// <param name="feature">The feature you want to read.</param>
        /// <param name="node">The node you want to read from.</param>
        /// <returns>The feature's value. If value or feature is invalid, it will return the default value (None = 0u).</returns>
        public static InputTrackingState GetValue(InputFeatureUsage<InputTrackingState> feature, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
                return default;

            return device.TryGetFeatureValue(feature, out var value) ? value : default;
        }

        public static void TriggerVibro(float amplitude, float duration, XRNode node)
        {
            if(!TryGetDevice(node, out var device))
                return;

            if(device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0;
                device.SendHapticImpulse(channel, amplitude, duration);
            }
        }

        #endregion

        #region Input

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
            if(nodeDevice == null)
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