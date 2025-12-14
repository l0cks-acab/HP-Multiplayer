using System;
using System.Collections.Generic;
using System.Text;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// Shared network protocol definitions (no Unity dependencies)
    /// Used by both client and server
    /// </summary>
    public enum MessageType : byte
    {
        Connection = 1,
        ConnectionAccepted = 2,
        ConnectionRejected = 3,
        PlayerUpdate = 4,
        PlayerJoined = 5,
        PlayerLeft = 6,
        Disconnect = 7,
        GameState = 8,
        ServerInfo = 9
    }

    /// <summary>
    /// Simple Vector3 replacement for server-side (no Unity)
    /// </summary>
    public struct Vector3F
    {
        public float x;
        public float y;
        public float z;

        public Vector3F(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3F Zero => new Vector3F(0, 0, 0);
    }

    /// <summary>
    /// Network protocol message serialization/deserialization
    /// </summary>
    public static class NetworkProtocol
    {
        /// <summary>
        /// Create a connection request message
        /// </summary>
        public static byte[] CreateConnectionMessage(int playerId, string playerName = "Player")
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.Connection);
            data.AddRange(BitConverter.GetBytes(playerId));
            
            byte[] nameBytes = Encoding.UTF8.GetBytes(playerName);
            data.AddRange(BitConverter.GetBytes(nameBytes.Length));
            data.AddRange(nameBytes);
            
            return data.ToArray();
        }

        /// <summary>
        /// Create a connection accepted message
        /// </summary>
        public static byte[] CreateConnectionAcceptedMessage(int assignedPlayerId)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.ConnectionAccepted);
            data.AddRange(BitConverter.GetBytes(assignedPlayerId));
            return data.ToArray();
        }

        /// <summary>
        /// Create a connection rejected message
        /// </summary>
        public static byte[] CreateConnectionRejectedMessage(string reason)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.ConnectionRejected);
            
            byte[] reasonBytes = Encoding.UTF8.GetBytes(reason);
            data.AddRange(BitConverter.GetBytes(reasonBytes.Length));
            data.AddRange(reasonBytes);
            
            return data.ToArray();
        }

        /// <summary>
        /// Create a player update message
        /// </summary>
        public static byte[] CreatePlayerUpdateMessage(int playerId, Vector3F position, Vector3F rotation)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.PlayerUpdate);
            data.AddRange(BitConverter.GetBytes(playerId));
            data.AddRange(BitConverter.GetBytes(position.x));
            data.AddRange(BitConverter.GetBytes(position.y));
            data.AddRange(BitConverter.GetBytes(position.z));
            data.AddRange(BitConverter.GetBytes(rotation.x));
            data.AddRange(BitConverter.GetBytes(rotation.y));
            data.AddRange(BitConverter.GetBytes(rotation.z));
            return data.ToArray();
        }

        /// <summary>
        /// Create a player joined message
        /// </summary>
        public static byte[] CreatePlayerJoinedMessage(int playerId, string playerName)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.PlayerJoined);
            data.AddRange(BitConverter.GetBytes(playerId));
            
            byte[] nameBytes = Encoding.UTF8.GetBytes(playerName);
            data.AddRange(BitConverter.GetBytes(nameBytes.Length));
            data.AddRange(nameBytes);
            
            return data.ToArray();
        }

        /// <summary>
        /// Create a player left message
        /// </summary>
        public static byte[] CreatePlayerLeftMessage(int playerId)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.PlayerLeft);
            data.AddRange(BitConverter.GetBytes(playerId));
            return data.ToArray();
        }

        /// <summary>
        /// Create a disconnect message
        /// </summary>
        public static byte[] CreateDisconnectMessage(int playerId)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.Disconnect);
            data.AddRange(BitConverter.GetBytes(playerId));
            return data.ToArray();
        }

        /// <summary>
        /// Create a game state message
        /// </summary>
        public static byte[] CreateGameStateMessage(byte[] stateData)
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageType.GameState);
            data.AddRange(BitConverter.GetBytes(stateData.Length));
            data.AddRange(stateData);
            return data.ToArray();
        }

        /// <summary>
        /// Parse connection message
        /// </summary>
        public static bool ParseConnectionMessage(byte[] data, out int playerId, out string playerName)
        {
            playerId = 0;
            playerName = "";
            
            // Minimum size: 1 byte (message type) + 4 bytes (playerId) + 4 bytes (nameLength) = 9 bytes
            if (data == null || data.Length < 9)
            {
                return false;
            }
            
            try
            {
                playerId = BitConverter.ToInt32(data, 1);
                int nameLength = BitConverter.ToInt32(data, 5);
                
                // Validate nameLength is reasonable (prevent negative or extremely large values)
                if (nameLength < 0 || nameLength > 256)
                {
                    return false;
                }
                
                // Check we have enough bytes: 1 (type) + 4 (playerId) + 4 (nameLength) + nameLength bytes
                if (data.Length < 9 + nameLength)
                {
                    return false;
                }
                
                // Ensure we're not trying to read past the array bounds
                if (9 + nameLength > data.Length)
                {
                    return false;
                }
                
                // Extract player name
                playerName = Encoding.UTF8.GetString(data, 9, nameLength);
                return true;
            }
            catch (Exception)
            {
                // Catch any index out of range or other parsing errors
                return false;
            }
        }

        /// <summary>
        /// Parse player update message
        /// </summary>
        public static bool ParsePlayerUpdateMessage(byte[] data, out int playerId, out Vector3F position, out Vector3F rotation)
        {
            playerId = 0;
            position = Vector3F.Zero;
            rotation = Vector3F.Zero;
            
            // Message format: [MessageType(1)] [PlayerId(4)] [Position(12)] [Rotation(12)] = 29 bytes
            if (data.Length < 29) return false;
            
            playerId = BitConverter.ToInt32(data, 1);
            position.x = BitConverter.ToSingle(data, 5);
            position.y = BitConverter.ToSingle(data, 9);
            position.z = BitConverter.ToSingle(data, 13);
            rotation.x = BitConverter.ToSingle(data, 17);
            rotation.y = BitConverter.ToSingle(data, 21);
            rotation.z = BitConverter.ToSingle(data, 25);
            
            return true;
        }

        /// <summary>
        /// Parse game state message
        /// </summary>
        public static bool ParseGameStateMessage(byte[] data, out byte[] stateData)
        {
            stateData = null;
            
            if (data.Length < 5) return false;
            
            int stateLength = BitConverter.ToInt32(data, 1);
            if (data.Length < 5 + stateLength) return false;
            
            stateData = new byte[stateLength];
            Array.Copy(data, 5, stateData, 0, stateLength);
            
            return true;
        }
    }
}

