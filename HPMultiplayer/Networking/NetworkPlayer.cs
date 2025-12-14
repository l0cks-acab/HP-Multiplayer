using UnityEngine;
using MelonLoader;
using HPMultiplayer.Synchronization;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// Represents a remote player in the game
    /// </summary>
    public class NetworkPlayer
    {
        public int PlayerId { get; private set; }
        private GameObject playerObject;
        private Vector3 targetPosition;
        private Vector3 targetRotation;
        private bool needsUpdate = false;
        
        public NetworkPlayer(int playerId)
        {
            PlayerId = playerId;
            CreatePlayerObject();
            CreateNameTag();
        }

        private void CreatePlayerObject()
        {
            // Create remote player model (uses simple capsule to avoid input interference)
            // We don't clone the local player model to prevent affecting local player input/camera
            playerObject = PlayerModelFinder.CreateRemotePlayerModel(PlayerId, null);
            
            if (playerObject != null)
            {
                // Start at zero - will be updated when first position update arrives
                playerObject.transform.position = Vector3.zero;
                playerObject.transform.rotation = Quaternion.identity;
                targetPosition = Vector3.zero;
                targetRotation = Vector3.zero;
                MelonLogger.Msg($"Created remote player object for Player {PlayerId} at {playerObject.transform.position}");
            }
            else
            {
                MelonLogger.Error($"Failed to create remote player object for Player {PlayerId}!");
            }
        }

        private void CreateNameTag()
        {
            // Nametags will be drawn using OnGUI in the mod's OnGUI method
            // This method is kept for potential future 3D nametag implementation
            // For now, nametags are rendered as 2D labels in screen space
            MelonLogger.Msg($"NameTag will be rendered for Player {PlayerId}");
        }
        
        /// <summary>
        /// Gets the screen position of the player for nametag rendering
        /// </summary>
        public Vector3 GetScreenPosition()
        {
            if (playerObject == null) return Vector3.zero;
            
            Camera mainCam = Camera.main ?? Camera.current;
            if (mainCam == null) return Vector3.zero;
            
            // Get world position 2 units above player
            Vector3 worldPos = playerObject.transform.position + new Vector3(0, 2f, 0);
            return mainCam.WorldToScreenPoint(worldPos);
        }
        
        /// <summary>
        /// Checks if the player is visible on screen
        /// </summary>
        public bool IsVisibleOnScreen()
        {
            if (playerObject == null) return false;
            
            Camera mainCam = Camera.main ?? Camera.current;
            if (mainCam == null) return false;
            
            Vector3 screenPos = GetScreenPosition();
            return screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width && 
                   screenPos.y >= 0 && screenPos.y <= Screen.height;
        }

        public void UpdatePosition(Vector3 position, Vector3 rotation)
        {
            targetPosition = position;
            targetRotation = rotation;
            needsUpdate = true;
            
            // Always immediately update position if playerObject exists (smooth interpolation happens in Update)
            if (playerObject != null)
            {
                // If we're at zero and received a non-zero position, immediately snap to it
                if (playerObject.transform.position == Vector3.zero && position != Vector3.zero)
                {
                    playerObject.transform.position = position;
                    playerObject.transform.eulerAngles = rotation;
                    MelonLogger.Msg($"Set initial position for Player {PlayerId} to {position}");
                }
            }
        }

        public void Update()
        {
            if (playerObject != null)
            {
                // Always update position smoothly
                if (needsUpdate || Vector3.Distance(playerObject.transform.position, targetPosition) > 0.01f)
                {
                    // Smoothly interpolate to target position
                    playerObject.transform.position = Vector3.Lerp(
                        playerObject.transform.position,
                        targetPosition,
                        Time.deltaTime * 10f
                    );
                    
                    playerObject.transform.eulerAngles = Vector3.Lerp(
                        playerObject.transform.eulerAngles,
                        targetRotation,
                        Time.deltaTime * 10f
                    );

                    if (needsUpdate)
                    {
                        needsUpdate = false;
                    }
                }
            }
        }

        public void Destroy()
        {
            if (playerObject != null)
            {
                Object.Destroy(playerObject);
                playerObject = null;
            }
        }

        /// <summary>
        /// Called when scene changes - cleanup and recreate objects
        /// </summary>
        public void OnSceneChanged()
        {
            // Store current position/rotation before destroying
            Vector3 lastPos = playerObject != null ? playerObject.transform.position : targetPosition;
            Vector3 lastRot = playerObject != null ? playerObject.transform.eulerAngles : targetRotation;
            
            // Destroy old objects
            Destroy();
            
            // Recreate in new scene
            CreatePlayerObject();
            
            // Restore position
            if (playerObject != null)
            {
                playerObject.transform.position = lastPos;
                playerObject.transform.eulerAngles = lastRot;
                targetPosition = lastPos;
                targetRotation = lastRot;
            }
        }
    }
}

