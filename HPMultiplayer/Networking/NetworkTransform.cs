using UnityEngine;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// Component to sync transform data over network
    /// </summary>
    public class NetworkTransform : MonoBehaviour
    {
        public int NetworkId { get; set; }
        public bool IsLocalPlayer { get; set; }
        
        private float syncRate = 30f;
        private float lastSyncTime = 0f;

        private void Update()
        {
            if (IsLocalPlayer)
            {
                // Local player - send updates
                if (Time.time - lastSyncTime >= 1f / syncRate)
                {
                    // NetworkManager will handle sending
                    lastSyncTime = Time.time;
                }
            }
        }

        public Vector3 GetPosition() => transform.position;
        public Vector3 GetRotation() => transform.eulerAngles;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetRotation(Vector3 rotation) => transform.eulerAngles = rotation;
    }
}

