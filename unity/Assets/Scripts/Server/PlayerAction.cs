using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Server {
    /// <summary>
    /// Manages player behaiviours on the server side
    /// </summary>
    public class PlayerAction : NetworkBehaviour {
        [SerializeField] [Tooltip("How hard to kick the ball")]
        float kickForce = 20f;

        [SerializeField] [Tooltip("Distance the paddle can kick from")]
        float kickDistance = 1.5f;

        [SerializeField] [Tooltip("How far down to the bottom to kick. 2f is the bottom.")]
        float kickAngle = 2.7f;

        BoxCollider box;

        // --- Messages ---

        /// <summary>
        /// Gab teh rigidbody and box collider
        /// </summary>
        void Start() { box = GetComponent<BoxCollider>(); }

        // --- Functions ---

        /// <summary>
        /// Command: Kicks the ball
        /// </summary>
        [Command]
        public void CmdKickBall() {
            Debug.Log("Command: Attempting to kick the ball!");
            var diff = new Vector3(0, box.size.y / kickAngle, 0);
            var origin = transform.position - transform.TransformVector(diff);

            RaycastHit hit;
            if (Physics.Raycast(origin, transform.forward, out hit, kickDistance)) {
                if (hit.collider.name == Ball.Name) {
                    var crb = hit.collider.GetComponent<Rigidbody>();
                    var force = -kickForce * hit.normal;
                    crb.AddForceAtPosition(force, hit.point, ForceMode.Impulse);
                }
            }
        }
    }
}
