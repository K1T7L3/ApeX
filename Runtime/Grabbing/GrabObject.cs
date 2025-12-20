using UnityEngine;
using UnityEngine.Events;

public class GrabObject : MonoBehaviour
{
    public UnityEvent onGrab;
    public UnityEvent onLetGo;
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
