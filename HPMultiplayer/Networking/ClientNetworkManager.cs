using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MelonLoader;
using UnityEngine;
using HPMultiplayer.Synchronization;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// Client-side network manager - connects to dedicated server
    /// </summary>
    public class ClientNetworkManager
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private bool isConnected = false;
        private int localPlayerId = 0; // Assigned by server
        private string playerName = "Player";
        
        private Dictionary<int, NetworkPlayer> remotePlayers = new Dictionary<int, NetworkPlayer>();
        private float lastSendTime = 0f;
        private const float SEND_INTERVAL = 0.033f; // ~30 updates per second
        
        // Connection state
        private bool connectionConfirmed = false;
        private float lastConnectionAttempt = 0f;
        private const float CONNECTION_RETRY_INTERVAL = 1f;
        private const int MAX_CONNECTION_RETRIES = 5;
        private int connectionRetries = 0;
        
        // Thread-safe queue for main thread operations
        private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        public event Action<int> OnPlayerConnected;
        public event Action<int> OnPlayerDisconnected;
        public event Action<byte[]> OnGameStateReceived;
        
        public bool IsConnected => isConnected;
        public int LocalPlayerId => localPlayerId;
        public string ServerIP => serverEndPoint?.Address?.ToString() ?? "N/A";
        public int ServerPort => serverEndPoint?.Port ?? 0;
        public int ConnectedPlayerCount => remotePlayers.Count;
        
        /// <summary>
        /// Gets all remote players for nametag rendering
        /// </summary>
        public IEnumerable<NetworkPlayer> GetRemotePlayers()
        {
            return remotePlayers.Values;
        }

        public ClientNetworkManager()
        {
            MelonLogger.Msg("ClientNetworkManager initialized");
        }

        /// <summary>
        /// Connect to a dedicated server
        /// </summary>
        public bool ConnectToServer(string serverAddress, int port = 7777, string playerName = "Player")
        {
            try
            {
                this.playerName = playerName;
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), port);
                
                // Use any available local port
                udpClient = new UdpClient(0);
                udpClient.BeginReceive(OnReceive, null);
                
                isConnected = false;
                connectionConfirmed = false;
                connectionRetries = 0;
                lastConnectionAttempt = Time.time;
                
                // Send initial connection message
                SendConnectionMessage();
                
                MelonLogger.Msg($"Attempting to connect to server at {serverAddress}:{port}");
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to connect: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update called every frame
        /// </summary>
        public void Update()
        {
            // Process queued main thread operations
            while (mainThreadQueue.TryDequeue(out Action action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Error executing main thread action: {e.Message}");
                }
            }
            
            // Handle connection retry
            if (!isConnected && serverEndPoint != null && !connectionConfirmed)
            {
                if (Time.time - lastConnectionAttempt >= CONNECTION_RETRY_INTERVAL)
                {
                    if (connectionRetries < MAX_CONNECTION_RETRIES)
                    {
                        SendConnectionMessage();
                        connectionRetries++;
                        lastConnectionAttempt = Time.time;
                        MelonLogger.Msg($"Retrying connection... ({connectionRetries}/{MAX_CONNECTION_RETRIES})");
                    }
                    else
                    {
                        MelonLogger.Error("Failed to connect: Max retries reached");
                        Shutdown();
                        return;
                    }
                }
            }

            if (!isConnected) return;

            // Update all remote players
            foreach (var player in remotePlayers.Values)
            {
                player.Update();
            }

            // Send player updates at fixed interval
            if (Time.time - lastSendTime >= SEND_INTERVAL)
            {
                SendPlayerUpdate();
                lastSendTime = Time.time;
            }
        }

        /// <summary>
        /// Called when the scene changes - recreates player objects in the new scene
        /// </summary>
        public void OnSceneChanged()
        {
            MelonLogger.Msg("Handling scene change for network players...");
            
            foreach (var player in remotePlayers.Values)
            {
                player.OnSceneChanged();
            }
        }

        /// <summary>
        /// Shutdown network connections
        /// </summary>
        public void Shutdown()
        {
            if (isConnected)
            {
                SendDisconnectMessage();
            }
            
            udpClient?.Close();
            udpClient = null;
            isConnected = false;
            connectionConfirmed = false;
            connectionRetries = 0;
            serverEndPoint = null;
            remotePlayers.Clear();
            
            MelonLogger.Msg("Network shutdown complete");
        }

        private void OnReceive(IAsyncResult result)
        {
            try
            {
                if (udpClient == null) return;
                
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.EndReceive(result, ref sender);
                
                // Verify sender is the server
                if (serverEndPoint != null && sender.Address.Equals(serverEndPoint.Address) && sender.Port == serverEndPoint.Port)
                {
                    ProcessMessage(data);
                }
                
                // Continue receiving
                udpClient.BeginReceive(OnReceive, null);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error receiving data: {e.Message}");
            }
        }

        private void ProcessMessage(byte[] data)
        {
            if (data.Length < 1) return;

            MessageType type = (MessageType)data[0];

            switch (type)
            {
                case MessageType.ConnectionAccepted:
                    HandleConnectionAccepted(data);
                    break;
                case MessageType.ConnectionRejected:
                    HandleConnectionRejected(data);
                    break;
                case MessageType.PlayerUpdate:
                    HandlePlayerUpdate(data);
                    break;
                case MessageType.PlayerJoined:
                    HandlePlayerJoined(data);
                    break;
                case MessageType.PlayerLeft:
                    HandlePlayerLeft(data);
                    break;
                case MessageType.Disconnect:
                    HandleDisconnect(data);
                    break;
                case MessageType.GameState:
                    HandleGameState(data);
                    break;
            }
        }

        private void HandleConnectionAccepted(byte[] data)
        {
            if (data.Length < 5) return;
            
            int assignedPlayerId = BitConverter.ToInt32(data, 1);
            
            mainThreadQueue.Enqueue(() =>
            {
                if (!connectionConfirmed)
                {
                    localPlayerId = assignedPlayerId;
                    connectionConfirmed = true;
                    isConnected = true;
                    MelonLogger.Msg($"Connection confirmed! Assigned Player ID: {localPlayerId}");
                    OnPlayerConnected?.Invoke(localPlayerId);
                }
            });
        }

        private void HandleConnectionRejected(byte[] data)
        {
            if (data.Length < 5) return;
            
            int reasonLength = BitConverter.ToInt32(data, 1);
            if (data.Length < 5 + reasonLength) return;
            
            string reason = System.Text.Encoding.UTF8.GetString(data, 5, reasonLength);
            
            mainThreadQueue.Enqueue(() =>
            {
                MelonLogger.Error($"Connection rejected: {reason}");
                Shutdown();
            });
        }

        private void HandlePlayerUpdate(byte[] data)
        {
            if (!NetworkProtocol.ParsePlayerUpdateMessage(data, out int playerId, out Vector3F position, out Vector3F rotation))
                return;
            
            // Don't process our own updates
            if (playerId == localPlayerId) return;
            
            mainThreadQueue.Enqueue(() =>
            {
                if (!remotePlayers.ContainsKey(playerId))
                {
                    remotePlayers[playerId] = new NetworkPlayer(playerId);
                    MelonLogger.Msg($"Created NetworkPlayer for remote Player {playerId}");
                    OnPlayerConnected?.Invoke(playerId);
                }

                // Convert Vector3F to Unity Vector3
                Vector3 pos = new Vector3(position.x, position.y, position.z);
                Vector3 rot = new Vector3(rotation.x, rotation.y, rotation.z);
                remotePlayers[playerId].UpdatePosition(pos, rot);
            });
        }

        private void HandlePlayerJoined(byte[] data)
        {
            if (data.Length < 6) return;
            
            int playerId = BitConverter.ToInt32(data, 1);
            int nameLength = BitConverter.ToInt32(data, 5);
            
            if (data.Length < 9 + nameLength) return;
            
            string playerName = System.Text.Encoding.UTF8.GetString(data, 9, nameLength);
            
            mainThreadQueue.Enqueue(() =>
            {
                if (playerId != localPlayerId && !remotePlayers.ContainsKey(playerId))
                {
                    MelonLogger.Msg($"Player {playerId} ({playerName}) joined the game");
                    remotePlayers[playerId] = new NetworkPlayer(playerId);
                    OnPlayerConnected?.Invoke(playerId);
                }
            });
        }

        private void HandlePlayerLeft(byte[] data)
        {
            if (data.Length < 5) return;
            
            int playerId = BitConverter.ToInt32(data, 1);
            
            mainThreadQueue.Enqueue(() =>
            {
                MelonLogger.Msg($"Player {playerId} left the game");
                
                if (remotePlayers.ContainsKey(playerId))
                {
                    remotePlayers[playerId].Destroy();
                    remotePlayers.Remove(playerId);
                    OnPlayerDisconnected?.Invoke(playerId);
                }
            });
        }

        private void HandleDisconnect(byte[] data)
        {
            mainThreadQueue.Enqueue(() =>
            {
                MelonLogger.Msg("Disconnected from server");
                Shutdown();
            });
        }

        private void HandleGameState(byte[] data)
        {
            if (!NetworkProtocol.ParseGameStateMessage(data, out byte[] stateData))
                return;
            
            mainThreadQueue.Enqueue(() =>
            {
                OnGameStateReceived?.Invoke(stateData);
            });
        }

        private void SendConnectionMessage()
        {
            if (serverEndPoint == null) return;

            byte[] message = NetworkProtocol.CreateConnectionMessage(0, playerName); // 0 = request ID assignment
            SendMessage(message);
        }

        private void SendPlayerUpdate()
        {
            if (serverEndPoint == null || !isConnected) return;

            // Get local player position/rotation
            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            
            GameObject playerObject = PlayerModelFinder.FindPlayerModel();
            
            if (playerObject != null)
            {
                position = playerObject.transform.position;
                rotation = playerObject.transform.eulerAngles;
            }
            else if (Camera.main != null)
            {
                position = Camera.main.transform.position;
                rotation = Camera.main.transform.eulerAngles;
            }

            Vector3F pos = new Vector3F(position.x, position.y, position.z);
            Vector3F rot = new Vector3F(rotation.x, rotation.y, rotation.z);
            
            byte[] message = NetworkProtocol.CreatePlayerUpdateMessage(localPlayerId, pos, rot);
            SendMessage(message);
        }

        private void SendDisconnectMessage()
        {
            if (serverEndPoint == null) return;

            byte[] message = NetworkProtocol.CreateDisconnectMessage(localPlayerId);
            SendMessage(message);
        }

        private void SendMessage(byte[] data)
        {
            try
            {
                if (serverEndPoint != null && udpClient != null)
                {
                    udpClient.Send(data, data.Length, serverEndPoint);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error sending message: {e.Message}");
            }
        }
    }
}

