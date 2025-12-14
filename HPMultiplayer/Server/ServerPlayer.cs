using System;
using System.Net;
using HPMultiplayer.Networking;

namespace HPMultiplayer.Server
{
    /// <summary>
    /// Represents a connected player on the server
    /// </summary>
    public class ServerPlayer
    {
        public int PlayerId { get; private set; }
        public string PlayerName { get; set; }
        public IPEndPoint EndPoint { get; private set; }
        public Vector3F Position { get; set; }
        public Vector3F Rotation { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime LastHeartbeatTime { get; set; }
        public bool IsConnected { get; set; }

        public ServerPlayer(int playerId, IPEndPoint endPoint, string playerName = "Player")
        {
            PlayerId = playerId;
            EndPoint = endPoint;
            PlayerName = playerName;
            Position = Vector3F.Zero;
            Rotation = Vector3F.Zero;
            LastUpdateTime = DateTime.UtcNow;
            LastHeartbeatTime = DateTime.UtcNow;
            IsConnected = true;
        }

        public void UpdatePosition(Vector3F position, Vector3F rotation)
        {
            Position = position;
            Rotation = rotation;
            LastUpdateTime = DateTime.UtcNow;
        }
    }
}

