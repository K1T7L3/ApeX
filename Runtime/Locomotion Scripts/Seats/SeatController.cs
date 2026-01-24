using UnityEngine;

namespace ApeX
{
    [AddComponentMenu("ApeX/Seats/Seat Controller")]
    public class SeatController : MonoBehaviour
    {
        public Seat currentSeat;
        public Vector3 bounds;
        public LayerMask mask;

        ApeXPlayer player;
        Seat targetSeat;

        private void Start() => player = GetComponent<ApeXPlayer>();

        void Update()
        {
            Collider[] colliders = Physics.OverlapBox(player.transform.position, bounds / 2, Quaternion.identity, mask);
            targetSeat = colliders[0].gameObject.GetComponent<Seat>();
        }
    }
}
