using UnityEngine;

using ApeX.Input;

namespace ApeX.Grabbing
{
    [AddComponentMenu("ApeX/Grabbing/Grabber")]
    public class Grabber : MonoBehaviour
    {
        public bool editorInput;
        public float radius;
        public LayerMask grabLayer;
        public LayerMask handLayer;
        public Transform controller;
        public Hand hand;

        bool grabInputActive;
        bool isGrabbing;

        Collider[] returnedColliders;

        GrabObjectBase grabbedObject;
        FixedJoint _joint;

        void Update()
        {
            grabInputActive = ApeXInputs.GripDown(hand == Hand.left) || editorInput;

            if (grabInputActive && !isGrabbing)
            {
                returnedColliders = Physics.OverlapSphere(transform.position, radius, grabLayer);

                if (returnedColliders[0])
                {
                    if (returnedColliders[0].GetComponent<GrabObjectBase>())
                        grabbedObject = returnedColliders[0].GetComponent<GrabObjectBase>();
                    else
                        grabbedObject = returnedColliders[0].GetComponentInParent<GrabObjectBase>();

                    _joint = gameObject.AddComponent<FixedJoint>();
                    isGrabbing = true;

                    if (grabbedObject)
                    {
                        foreach (Collider c in grabbedObject.excludeColliders)
                        {
                            c.excludeLayers = handLayer;
                        }

                        if (grabbedObject.grabAnchor)
                        {
                            Vector3 betweenPos = grabbedObject.grabAnchor.position - transform.position;
                            grabbedObject.grabbed = true;
                            grabbedObject.grabbedHand = hand;

                            if (grabbedObject.goToHand)
                                grabbedObject.transform.position -= betweenPos;
                            else
                            {
                                transform.position += betweenPos;
                                controller.position += betweenPos;
                            }

                            _joint.anchor = grabbedObject.grabAnchor.position;
                        }
                    }

                    if (returnedColliders[0].attachedRigidbody)
                    {
                        _joint.connectedBody = returnedColliders[0].attachedRigidbody;

                        Debug.Log("Grabbed Dynamic object (It has a Rigidbody attached)");
                    }

                    else
                        Debug.Log("Grabbed Static object");
                }
            }

            else if (!grabInputActive && isGrabbing)
            {
                breakJoint();
            }
        }

        void breakJoint()
        {
            isGrabbing = false;
            Destroy(_joint);

            if (grabbedObject)
            {
                foreach (Collider c in grabbedObject.excludeColliders)
                {
                    c.excludeLayers = default;
                }

                grabbedObject.grabbedHand = Hand.none;
                grabbedObject.grabbed = false;
                grabbedObject = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
