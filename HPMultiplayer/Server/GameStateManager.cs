using System;
using System.Collections.Generic;
using HPMultiplayer.Networking;

namespace HPMultiplayer.Server
{
    /// <summary>
    /// Manages game state on the server (NPCs, objects, etc.)
    /// This is a simplified version - in production you'd sync actual game objects
    /// </summary>
    public class GameStateManager
    {
        private Dictionary<string, SyncedObjectState> syncedObjects = new Dictionary<string, SyncedObjectState>();
        
        public GameStateManager()
        {
        }
        
        /// <summary>
        /// Serialize current game state for network transmission
        /// </summary>
        public byte[] SerializeState()
        {
            // For now, return empty state
            // In production, this would serialize NPC positions, object states, etc.
            List<byte> stateData = new List<byte>();
            
            lock (syncedObjects)
            {
                foreach (var obj in syncedObjects.Values)
                {
                    byte[] objData = obj.Serialize();
                    stateData.AddRange(objData);
                }
            }
            
            return stateData.ToArray();
        }
        
        /// <summary>
        /// Register an object for synchronization
        /// </summary>
        public void RegisterObject(string objectId, Vector3F position, Vector3F rotation)
        {
            lock (syncedObjects)
            {
                if (!syncedObjects.ContainsKey(objectId))
                {
                    syncedObjects[objectId] = new SyncedObjectState(objectId);
                }
                
                syncedObjects[objectId].Position = position;
                syncedObjects[objectId].Rotation = rotation;
            }
        }
        
        /// <summary>
        /// Unregister an object
        /// </summary>
        public void UnregisterObject(string objectId)
        {
            lock (syncedObjects)
            {
                syncedObjects.Remove(objectId);
            }
        }
    }
    
    /// <summary>
    /// Represents a synchronized object state on the server
    /// </summary>
    internal class SyncedObjectState
    {
        public string ObjectId { get; private set; }
        public Vector3F Position { get; set; }
        public Vector3F Rotation { get; set; }
        
        public SyncedObjectState(string objectId)
        {
            ObjectId = objectId;
            Position = Vector3F.Zero;
            Rotation = Vector3F.Zero;
        }
        
        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            
            // Object ID
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(ObjectId);
            data.Add((byte)idBytes.Length);
            data.AddRange(idBytes);
            
            // Position
            data.AddRange(BitConverter.GetBytes(Position.x));
            data.AddRange(BitConverter.GetBytes(Position.y));
            data.AddRange(BitConverter.GetBytes(Position.z));
            
            // Rotation (stored as quaternion in original, simplified to 3 floats here)
            data.AddRange(BitConverter.GetBytes(Rotation.x));
            data.AddRange(BitConverter.GetBytes(Rotation.y));
            data.AddRange(BitConverter.GetBytes(Rotation.z));
            data.AddRange(BitConverter.GetBytes(0f)); // w component (quaternion)
            
            return data.ToArray();
        }
    }
}

