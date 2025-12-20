using UnityEngine;

using ApeX.Input;

namespace ApeX
{
    public class ApeXTurning : MonoBehaviour
    {
        [Header("Player")]
        public SphereCollider headCollider;

        [Header("Turning")]

        public int turnDegrees = 45;
        public float turnTime = .1f;
        public float turnThreshold = .1f;

        public TurningMode turningMode = TurningMode.smooth;

        float lastTimeTurned;

        public void Update()
        {
            float turnValue = ApeXInputs.Thumbstick(false).x;
            Turn(turningMode, turnDegrees, turnValue);
        }

        void Turn(TurningMode mode, float degrees, float turnVal)
        {
            switch (mode)
            {
                case TurningMode.none:
                    break;

                case TurningMode.smooth:
                    if (Mathf.Abs(turnVal) > turnThreshold)
                        transform.RotateAround(headCollider.transform.position, transform.up, degrees * turnVal);
                    break;

                case TurningMode.snap:
                    if (Mathf.Abs(turnVal) > turnThreshold && Time.realtimeSinceStartup - lastTimeTurned >= turnTime)
                    {
                        transform.RotateAround(headCollider.transform.position, transform.up, turnVal > 0 ? degrees : -degrees);
                        lastTimeTurned = Time.realtimeSinceStartup;
                    }
                    break;
            }
        }
    }
}
