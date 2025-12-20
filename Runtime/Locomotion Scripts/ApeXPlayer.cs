using UnityEngine;

namespace ApeX
{
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
            joint.xDrive = handDrive.drive();
            joint.yDrive = handDrive.drive();
            joint.zDrive = handDrive.drive();
            joint.slerpDrive = handDrive.drive();
        }
    }
}

[System.Serializable]
public class SerializedJointDrive
{
    public float spring;
    public float damper;
    public bool useAcceleration;

    public JointDrive drive()
    {
        JointDrive tempDrive = new();

        tempDrive.maximumForce = Mathf.Infinity;
        tempDrive.positionSpring = spring;
        tempDrive.positionDamper = damper;
        tempDrive.useAcceleration = useAcceleration;

        return tempDrive;
    }
}

public enum TurningMode
{
    none = 0,
    snap = 1,
    smooth = 2,
}