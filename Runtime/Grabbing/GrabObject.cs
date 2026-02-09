using UnityEngine;

namespace ApeX.Grabbing
{
    [AddComponentMenu("ApeX/Grabbing/Grab Object Base")]
    public class GrabObjectBase : MonoBehaviour
    {
        public Transform grabAnchor;
        public bool goToHand = true;
        public Collider[] excludeColliders;

        [HideInInspector]
        public Hand grabbedHand;

        [HideInInspector]
        public bool grabbed = false;
    }

    public enum Hand
    {
        left,
        right,
        none
    }
}