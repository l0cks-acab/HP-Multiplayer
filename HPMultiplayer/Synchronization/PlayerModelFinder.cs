using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace HPMultiplayer.Synchronization
{
    /// <summary>
    /// Finds and manages player models in House Party
    /// </summary>
    public static class PlayerModelFinder
    {
        /// <summary>
        /// Attempts to find the player GameObject in the scene
        /// </summary>
        public static GameObject FindPlayerModel()
        {
            GameObject player = null;
            
            // Try common player object names
            string[] playerNames = {
                "Player",
                "PlayerController",
                "PlayerCharacter",
                "Character",
                "MainPlayer",
                "LocalPlayer",
                "FPSController",
                "FirstPersonCharacter"
            };
            
            foreach (string name in playerNames)
            {
                player = GameObject.Find(name);
                if (player != null)
                {
                    MelonLogger.Msg($"Found player by name: {name}");
                    return player;
                }
            }
            
            // Try finding by tag
            try
            {
                GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
                if (playerByTag != null)
                {
                    MelonLogger.Msg("Found player by tag: Player");
                    return playerByTag;
                }
            }
            catch { }
            
            // Try finding Camera and check its parent/hierarchy
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // Check parent
                if (mainCam.transform.parent != null)
                {
                    player = mainCam.transform.parent.gameObject;
                    MelonLogger.Msg("Found player as camera parent");
                    return player;
                }
                
                // Check if camera itself is the player (sometimes camera IS the player object)
                if (mainCam.gameObject.name.Contains("Player") || mainCam.gameObject.name.Contains("Character"))
                {
                    player = mainCam.gameObject;
                    MelonLogger.Msg("Found player as camera object");
                    return player;
                }
            }
            
            // Search all GameObjects for player-like names
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string objName = obj.name.ToLower();
                if (objName.Contains("player") || objName.Contains("character") || objName.Contains("fps"))
                {
                    // Check if it has typical player components
                    bool hasCamera = obj.GetComponent<Camera>() != null;
                    bool hasChildren = obj.transform.childCount > 0;
                    
                    // If it matches name patterns and has camera or children, it's likely the player
                    if (hasCamera || hasChildren)
                    {
                        MelonLogger.Msg($"Found potential player: {obj.name}");
                        return obj;
                    }
                }
            }
            
            MelonLogger.Warning("Could not find player model - using fallback");
            return null;
        }
        
        /// <summary>
        /// Finds an NPC or character model to use as a template for remote players
        /// </summary>
        private static GameObject FindNPCModelTemplate()
        {
            // Try to find NPCs/characters in the scene that we can clone
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            GameObject bestCandidate = null;
            
            foreach (GameObject obj in allObjects)
            {
                string objName = obj.name.ToLower();
                
                // Look for NPC/character models (but not the player)
                if ((objName.Contains("npc") || 
                     (objName.Contains("character") && !objName.Contains("player")) ||
                     objName.Contains("guest") ||
                     objName.Contains("person")) &&
                    !objName.Contains("remote") &&
                    obj.activeInHierarchy)
                {
                    // Prefer objects with renderers (visible models)
                    if (obj.GetComponent<Renderer>() != null || obj.GetComponentInChildren<Renderer>() != null)
                    {
                        bestCandidate = obj;
                        MelonLogger.Msg($"Found NPC model candidate: {obj.name}");
                        break; // Use the first valid one we find
                    }
                }
            }
            
            return bestCandidate;
        }

        /// <summary>
        /// Creates a player representation for remote players
        /// Tries to clone the actual player model, falls back to NPC or capsule if not found
        /// </summary>
        public static GameObject CreateRemotePlayerModel(int playerId, GameObject localPlayerModel = null)
        {
            GameObject remotePlayer = null;
            
            // First, try to find the actual player model
            GameObject playerModel = FindPlayerModel();
            if (playerModel != null)
            {
                try
                {
                    // Clone the player model
                    remotePlayer = Object.Instantiate(playerModel);
                    remotePlayer.name = $"RemotePlayer_{playerId}";
                    remotePlayer.transform.position = Vector3.zero;
                    
                    // Remove all MonoBehaviour components that could interfere with local player
                    // This includes cameras, controllers, input handlers, AI scripts, etc.
                    MonoBehaviour[] allComponents = remotePlayer.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (MonoBehaviour mb in allComponents)
                    {
                        if (mb != null)
                        {
                            Object.Destroy(mb);
                        }
                    }
                    
                    // Remove cameras (we don't want remote players to have cameras)
                    Camera[] cameras = remotePlayer.GetComponentsInChildren<Camera>(true);
                    foreach (Camera cam in cameras)
                    {
                        if (cam != null)
                        {
                            Object.Destroy(cam);
                        }
                    }
                    
                    // Note: AudioListener and CharacterController removal is handled by MonoBehaviour removal above
                    // Removing all MonoBehaviours should catch most problematic components
                    // If specific issues occur with audio or physics, they can be addressed separately
                    
                    // Ensure the object is active
                    remotePlayer.SetActive(true);
                    
                    MelonLogger.Msg($"Created remote player from actual player model '{playerModel.name}' for Player {playerId}");
                    return remotePlayer;
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"Failed to clone player model: {e.Message}. Trying NPC model...");
                    // Fall through to NPC model
                }
            }
            
            // Second, try to find and clone an NPC model
            GameObject npcTemplate = FindNPCModelTemplate();
            if (npcTemplate != null)
            {
                try
                {
                    // Clone the NPC model
                    remotePlayer = Object.Instantiate(npcTemplate);
                    remotePlayer.name = $"RemotePlayer_{playerId}";
                    remotePlayer.transform.position = Vector3.zero;
                    
                    // Remove all MonoBehaviour components that could interfere
                    MonoBehaviour[] allComponents = remotePlayer.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (MonoBehaviour mb in allComponents)
                    {
                        if (mb != null)
                        {
                            Object.Destroy(mb);
                        }
                    }
                    
                    // Remove cameras
                    Camera[] cameras = remotePlayer.GetComponentsInChildren<Camera>(true);
                    foreach (Camera cam in cameras)
                    {
                        if (cam != null)
                        {
                            Object.Destroy(cam);
                        }
                    }
                    
                    remotePlayer.SetActive(true);
                    
                    MelonLogger.Msg($"Created remote player from NPC model '{npcTemplate.name}' for Player {playerId}");
                    return remotePlayer;
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"Failed to clone NPC model: {e.Message}. Falling back to capsule.");
                    // Fall through to capsule creation
                }
            }
            
            // Fallback: Create a capsule representation
            remotePlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            remotePlayer.name = $"RemotePlayer_{playerId}";
            remotePlayer.transform.position = Vector3.zero;
            
            // Color the capsule to distinguish players
            var renderer = remotePlayer.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material playerMaterial = new Material(renderer.material);
                playerMaterial.color = playerId == 2 ? new Color(0f, 0.5f, 1f, 1f) : new Color(0f, 1f, 0.5f, 1f);
                renderer.material = playerMaterial;
            }
            
            remotePlayer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            
            MelonLogger.Msg($"Created remote player capsule for Player {playerId} (fallback)");
            return remotePlayer;
        }
        
        /// <summary>
        /// Finds NPCs and interactive objects that should be synced
        /// </summary>
        public static List<GameObject> FindSyncableObjects()
        {
            List<GameObject> syncableObjects = new List<GameObject>();
            
            // Find NPCs (common names)
            string[] npcNames = { "NPC", "Character", "Person", "Guest", "AI" };
            foreach (string name in npcNames)
            {
                GameObject[] npcs = GameObject.FindGameObjectsWithTag(name);
                syncableObjects.AddRange(npcs);
            }
            
            // Find interactive objects (doors, items, etc.)
            // This is very game-specific and will need customization
            
            return syncableObjects;
        }
    }
}

