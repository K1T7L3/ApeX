using UnityEngine;

using ApeX.Input;

namespace ApeX
{
    [AddComponentMenu("ApeX/Player", 3)]
    public class ApeXPlayer : MonoBehaviour
    {
        public Rigidbody playerRigidbody;
        public CapsuleCollider bodyCollider;
        public SphereCollider headCollider;

        [Header("Hands")]
        [Header("Left Hand")]

        public Transform leftHandController;
        public ConfigurableJoint leftHandJoint;

        [Header("Right Hand")]

        public Transform rightHandController;
        public ConfigurableJoint rightHandJoint;

        [Header("Strength Settings")]
        [Space]
        public SerializedJointDrive handDrive = new SerializedJointDrive()
        {
            spring = 2500,
            damper = 5,
            useAcceleration = false
        };

        [Header("Drag & Mass")]
        public AnimationCurve dragCurve;
        public AnimationCurve massCurve;
        [Space]
        public float dragMultiplier = 0.2f;
        public float massMultiplier = 0.3f;
        [Space]
        public float dragLength = 3;
        public float massLength = 3;
        [Space]
        public float startingDrag = 0.1f;
        public float startingMass = 2f;

        Rigidbody leftRb;
        Rigidbody rightRb;

        private void Start()
        {
            leftRb = leftHandJoint.GetComponent<Rigidbody>();
            rightRb = rightHandJoint.GetComponent<Rigidbody>();

            SetDrives(leftHandJoint);
            SetDrives(rightHandJoint);

            ApeXInputs.CacheInputDevice(UnityEngine.XR.XRNode.LeftHand);
            ApeXInputs.CacheInputDevice(UnityEngine.XR.XRNode.RightHand);
        }

        void FixedUpdate()
        {
            bodyCollider.transform.eulerAngles = new Vector3(0, headCollider.transform.eulerAngles.y, 0);

            HandleDrag(leftRb);
            HandleDrag(rightRb);

            MapJoint(leftHandJoint, leftHandController);
            MapJoint(rightHandJoint, rightHandController);
        }

        void HandleDrag(Rigidbody rb)
        {
            rb.mass = startingMass + (massCurve.Evaluate(rb.linearVelocity.magnitude / massLength) * massMultiplier);
            rb.linearDamping = startingDrag + (dragCurve.Evaluate(rb.linearVelocity.magnitude / dragLength) * dragMultiplier);
        }

        void MapJoint(ConfigurableJoint joint, Transform target)
        {
            joint.targetPosition = target.localPosition;
            joint.targetRotation = target.localRotation;
        }

        public void SetDrives(ConfigurableJoint joint)
        {
            joint.xDrive = handDrive;
            joint.yDrive = handDrive;
            joint.zDrive = handDrive;
            joint.slerpDrive = handDrive;
        }
    }
}

[System.Serializable]
public struct SerializedJointDrive
{
    public float spring;
    public float damper;
    public bool useAcceleration;

    public readonly JointDrive ToDrive()
    {
        JointDrive tempDrive = new()
        {
            maximumForce = Mathf.Infinity,
            positionSpring = spring,
            positionDamper = damper,
            useAcceleration = useAcceleration
        };

        return tempDrive;
    }

    public static implicit operator JointDrive(SerializedJointDrive drive) => drive.ToDrive();
}

public enum TurningMode
{
    none = 0,
    snap = 1,
    smooth = 2,
}