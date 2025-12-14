using UnityEngine;
using MelonLoader;
using System.Collections.Generic;

namespace HPMultiplayer.Synchronization
{
    /// <summary>
    /// Finds NPCs and interactive objects that need to be synchronized
    /// </summary>
    public static class GameObjectFinder
    {
        private static float lastScanTime = 0f;
        private const float SCAN_INTERVAL = 2f; // Scan for new objects every 2 seconds
        private static HashSet<string> foundObjects = new HashSet<string>();

        /// <summary>
        /// Find all NPCs in the scene that should be synchronized
        /// </summary>
        public static List<GameObject> FindNPCs()
        {
            List<GameObject> npcs = new List<GameObject>();
            
            // Try to find NPCs by common naming patterns
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string objName = obj.name.ToLower();
                
                // Look for NPC-like names
                if (objName.Contains("npc") || 
                    objName.Contains("character") && !objName.Contains("player") ||
                    objName.Contains("guest") ||
                    objName.Contains("person"))
                {
                    // Make sure it's not a player or UI element
                    if (!objName.Contains("remote") && 
                        !objName.Contains("ui") && 
                        !objName.Contains("canvas") &&
                        obj.activeInHierarchy)
                    {
                        npcs.Add(obj);
                    }
                }
            }
            
            return npcs;
        }

        /// <summary>
        /// Find interactive objects (doors, items, etc.) that should be synchronized
        /// </summary>
        public static List<GameObject> FindInteractiveObjects()
        {
            List<GameObject> interactiveObjects = new List<GameObject>();
            
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string objName = obj.name.ToLower();
                
                // Look for interactive object patterns
                if (objName.Contains("door") ||
                    objName.Contains("item") ||
                    objName.Contains("pickup") ||
                    objName.Contains("interactive") ||
                    objName.Contains("trigger"))
                {
                    if (obj.activeInHierarchy)
                    {
                        interactiveObjects.Add(obj);
                    }
                }
            }
            
            return interactiveObjects;
        }

        /// <summary>
        /// Register all syncable objects with GameStateSync
        /// </summary>
        public static void RegisterSyncableObjects(GameStateSync gameStateSync, bool isHost)
        {
            if (gameStateSync == null) return;
            
            // Only scan periodically to avoid performance issues
            if (Time.time - lastScanTime < SCAN_INTERVAL) return;
            lastScanTime = Time.time;

            try
            {
                // Only actively sync objects if we're the host (host is authoritative for game state)
                // Clients will receive state updates and apply them
                if (isHost)
                {
                    // Find and register NPCs
                    List<GameObject> npcs = FindNPCs();
                    foreach (GameObject npc in npcs)
                    {
                        if (npc == null) continue;
                        string objectId = $"npc_{npc.name}_{npc.GetInstanceID()}";
                        if (!foundObjects.Contains(objectId))
                        {
                            gameStateSync.RegisterObject(objectId, npc, SyncType.PositionAndRotation);
                            foundObjects.Add(objectId);
                            MelonLogger.Msg($"Registered NPC for sync: {npc.name}");
                        }
                    }

                    // Find and register interactive objects
                    List<GameObject> interactiveObjects = FindInteractiveObjects();
                    foreach (GameObject obj in interactiveObjects)
                    {
                        if (obj == null) continue;
                        string objectId = $"interactive_{obj.name}_{obj.GetInstanceID()}";
                        if (!foundObjects.Contains(objectId))
                        {
                            gameStateSync.RegisterObject(objectId, obj, SyncType.PositionAndRotation);
                            foundObjects.Add(objectId);
                            MelonLogger.Msg($"Registered interactive object for sync: {obj.name}");
                        }
                    }
                }
                else
                {
                    // Client: Register objects so we can receive and apply state updates
                    // But don't actively sync them (host is authoritative)
                    List<GameObject> allSyncable = new List<GameObject>();
                    allSyncable.AddRange(FindNPCs());
                    allSyncable.AddRange(FindInteractiveObjects());
                    
                    foreach (GameObject obj in allSyncable)
                    {
                        if (obj == null) continue;
                        string objectId = obj.name.ToLower().Contains("npc") || obj.name.ToLower().Contains("character") ? 
                            $"npc_{obj.name}_{obj.GetInstanceID()}" : 
                            $"interactive_{obj.name}_{obj.GetInstanceID()}";
                        if (!foundObjects.Contains(objectId))
                        {
                            gameStateSync.RegisterObject(objectId, obj, SyncType.PositionAndRotation);
                            foundObjects.Add(objectId);
                        }
                    }
                }

                // Clean up destroyed objects from our tracking
                foundObjects.RemoveWhere(id => {
                    // Objects are removed when they're destroyed - GameStateSync will handle unregistering
                    return false; // Keep all for now, let GameStateSync handle cleanup
                });
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error registering syncable objects: {e.Message}");
            }
        }
    }
}

