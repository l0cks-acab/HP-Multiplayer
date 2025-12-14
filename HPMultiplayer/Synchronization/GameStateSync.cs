using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace HPMultiplayer.Synchronization
{
    /// <summary>
    /// Manages synchronization of game state between players
    /// </summary>
    public class GameStateSync
    {
        private Dictionary<string, SyncedObject> syncedObjects = new Dictionary<string, SyncedObject>();
        private float lastSyncTime = 0f;
        private const float SYNC_INTERVAL = 0.2f; // Sync every 200ms (5 times per second)
        private float lastFullSyncTime = 0f;
        private const float FULL_SYNC_INTERVAL = 1f; // Force sync all objects every 1 second
        
        public event Action<byte[]> OnStateChanged;
        
        public void Update()
        {
            // Periodically sync game state
            if (Time.time - lastSyncTime >= SYNC_INTERVAL)
            {
                SyncGameState();
                lastSyncTime = Time.time;
            }
            
            // Update all synced objects
            foreach (var obj in syncedObjects.Values)
            {
                obj.Update();
            }
        }
        
        /// <summary>
        /// Register a GameObject to be synchronized
        /// </summary>
        public void RegisterObject(string objectId, GameObject gameObject, SyncType syncType)
        {
            if (!syncedObjects.ContainsKey(objectId))
            {
                syncedObjects[objectId] = new SyncedObject(objectId, gameObject, syncType);
                MelonLogger.Msg($"Registered object for sync: {objectId} ({syncType})");
            }
        }
        
        /// <summary>
        /// Unregister a GameObject from synchronization
        /// </summary>
        public void UnregisterObject(string objectId)
        {
            if (syncedObjects.ContainsKey(objectId))
            {
                syncedObjects.Remove(objectId);
                MelonLogger.Msg($"Unregistered object from sync: {objectId}");
            }
        }
        
        /// <summary>
        /// Apply received game state changes
        /// </summary>
        public void ApplyState(byte[] stateData)
        {
            if (stateData == null || stateData.Length == 0) return;
            
            try
            {
                int offset = 0;
                
                // Parse state data (format: [objectIdLength][objectId][pos][rot][objectIdLength]...)
                while (offset < stateData.Length)
                {
                    if (offset + 1 > stateData.Length) break;
                    
                    byte idLength = stateData[offset++];
                    if (offset + idLength > stateData.Length) break;
                    
                    string objectId = System.Text.Encoding.UTF8.GetString(stateData, offset, idLength);
                    offset += idLength;
                    
                    // Read position (12 bytes: 3 floats)
                    if (offset + 12 > stateData.Length) break;
                    float x = System.BitConverter.ToSingle(stateData, offset);
                    float y = System.BitConverter.ToSingle(stateData, offset + 4);
                    float z = System.BitConverter.ToSingle(stateData, offset + 8);
                    offset += 12;
                    
                    // Read rotation (16 bytes: 4 floats)
                    if (offset + 16 > stateData.Length) break;
                    float qx = System.BitConverter.ToSingle(stateData, offset);
                    float qy = System.BitConverter.ToSingle(stateData, offset + 4);
                    float qz = System.BitConverter.ToSingle(stateData, offset + 8);
                    float qw = System.BitConverter.ToSingle(stateData, offset + 12);
                    offset += 16;
                    
                    // Apply state to synced object
                    // First, check if we already have this object registered
                    if (syncedObjects.ContainsKey(objectId))
                    {
                        var syncedObj = syncedObjects[objectId];
                        if (syncedObj != null && syncedObj.GameObject != null)
                        {
                            Vector3 targetPos = new Vector3(x, y, z);
                            Quaternion targetRot = new Quaternion(qx, qy, qz, qw);
                            
                            // Store target position/rotation for interpolation in Update()
                            // This follows Unity best practices: store network state, interpolate in Update()
                            syncedObj.SetNetworkTarget(targetPos, targetRot);
                        }
                    }
                    else
                    {
                        // Object not registered yet - try to find it by parsing the object ID
                        // Object IDs are in format: "npc_Name_InstanceID" or "interactive_Name_InstanceID"
                        string[] parts = objectId.Split('_');
                        if (parts.Length >= 2)
                        {
                            string objectType = parts[0]; // "npc" or "interactive"
                            string objectName = parts.Length > 2 ? string.Join("_", parts, 1, parts.Length - 2) : parts[1];
                            
                            // Try to find the GameObject by name
                            GameObject foundObj = GameObject.Find(objectName);
                            if (foundObj != null)
                            {
                                // Register it now so we can update it
                                RegisterObject(objectId, foundObj, SyncType.PositionAndRotation);
                                MelonLogger.Msg($"Found and registered object from state update: {objectName} ({objectId})");
                                
                                // Apply the state immediately using the SyncedObject's method
                                var syncedObj = syncedObjects[objectId];
                                if (syncedObj != null)
                                {
                                    syncedObj.SetNetworkTarget(new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw));
                                }
                            }
                            else
                            {
                                // MelonLogger.Warning($"Received state for object we can't find: {objectId} (searched for: {objectName})");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Error applying game state: {e.Message}");
            }
        }
        
        private void SyncGameState()
        {
            // Only sync if we have objects registered (host should sync NPCs/interactive objects)
            if (syncedObjects.Count == 0) return;
            
            // Build state snapshot - send ALL registered objects periodically to ensure clients stay in sync
            // This ensures that even if objects haven't moved, clients still receive their current state
            List<byte> stateData = new List<byte>();
            
            int syncedCount = 0;
            // Force sync all objects periodically to ensure clients receive state even for stationary objects
            bool forceFullSync = (Time.time - lastFullSyncTime) >= FULL_SYNC_INTERVAL;
            if (forceFullSync)
            {
                lastFullSyncTime = Time.time;
            }
            
            foreach (var syncedObj in syncedObjects.Values)
            {
                // Always sync objects that exist and are valid
                if (syncedObj != null && syncedObj.GameObject != null)
                {
                    // Check if object changed OR if we should force sync (periodic full sync)
                    bool shouldSync = syncedObj.HasChanged() || forceFullSync;
                    
                    if (shouldSync)
                    {
                        byte[] objData = syncedObj.Serialize();
                        if (objData != null && objData.Length > 0)
                        {
                            stateData.AddRange(objData);
                            syncedCount++;
                        }
                    }
                }
            }
            
            if (stateData.Count > 0)
            {
                OnStateChanged?.Invoke(stateData.ToArray());
                // MelonLogger.Msg($"Synced {syncedCount} objects");
            }
        }
    }
    
    /// <summary>
    /// Represents a synchronized game object
    /// </summary>
    public class SyncedObject
    {
        public string ObjectId { get; private set; }
        public GameObject GameObject { get; private set; }
        public SyncType SyncType { get; private set; }
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private bool hasChanged = false;
        
        // For client-side interpolation
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool hasNetworkUpdate = false;
        
        public SyncedObject(string objectId, GameObject gameObject, SyncType syncType)
        {
            ObjectId = objectId;
            GameObject = gameObject;
            SyncType = syncType;
            
            if (gameObject != null)
            {
                lastPosition = gameObject.transform.position;
                lastRotation = gameObject.transform.rotation;
            }
        }
        
        public bool HasChanged()
        {
            if (GameObject == null) return false;
            
            // Check if position or rotation changed
            bool positionChanged = Vector3.Distance(GameObject.transform.position, lastPosition) > 0.01f;
            bool rotationChanged = Quaternion.Angle(GameObject.transform.rotation, lastRotation) > 1f;
            
            if (positionChanged || rotationChanged)
            {
                hasChanged = true;
                lastPosition = GameObject.transform.position;
                lastRotation = GameObject.transform.rotation;
            }
            
            return hasChanged;
        }
        
        public void MarkSynced()
        {
            hasChanged = false;
        }
        
        public byte[] Serialize()
        {
            if (GameObject == null) return new byte[0];
            
            List<byte> data = new List<byte>();
            
            // Object ID length + ID
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(ObjectId);
            data.Add((byte)idBytes.Length);
            data.AddRange(idBytes);
            
            // Position
            Vector3 pos = GameObject.transform.position;
            data.AddRange(BitConverter.GetBytes(pos.x));
            data.AddRange(BitConverter.GetBytes(pos.y));
            data.AddRange(BitConverter.GetBytes(pos.z));
            
            // Rotation
            Quaternion rot = GameObject.transform.rotation;
            data.AddRange(BitConverter.GetBytes(rot.x));
            data.AddRange(BitConverter.GetBytes(rot.y));
            data.AddRange(BitConverter.GetBytes(rot.z));
            data.AddRange(BitConverter.GetBytes(rot.w));
            
            // Update last position/rotation when serializing so HasChanged() works correctly
            lastPosition = pos;
            lastRotation = rot;
            MarkSynced();
            return data.ToArray();
        }
        
        public void Update()
        {
            // Apply network updates with smooth interpolation on main thread
            if (hasNetworkUpdate && GameObject != null)
            {
                // Smooth interpolation for better visual quality
                // Using fixed interpolation speed for consistent behavior
                float lerpSpeed = 10f;
                GameObject.transform.position = Vector3.Lerp(
                    GameObject.transform.position,
                    targetPosition,
                    Time.deltaTime * lerpSpeed
                );
                GameObject.transform.rotation = Quaternion.Slerp(
                    GameObject.transform.rotation,
                    targetRotation,
                    Time.deltaTime * lerpSpeed
                );
                
                // Reset flag after applying
                hasNetworkUpdate = false;
            }
        }
        
        /// <summary>
        /// Set target position/rotation from network update (called from ApplyState)
        /// </summary>
        public void SetNetworkTarget(Vector3 position, Quaternion rotation)
        {
            targetPosition = position;
            targetRotation = rotation;
            hasNetworkUpdate = true;
        }
    }
    
    public enum SyncType
    {
        Position,
        PositionAndRotation,
        FullState // Includes animations, properties, etc.
    }
}

