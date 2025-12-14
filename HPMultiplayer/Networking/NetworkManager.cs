using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MelonLoader;
using UnityEngine;
using HPMultiplayer.Synchronization;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// P2P message types
    /// </summary>
    public enum MessageType : byte
    {
        Connection = 1,
        PlayerUpdate = 2,
        Disconnect = 3,
        GameState = 4
    }

    /// <summary>
    /// Manages P2P network connections using UDP sockets
    /// </summary>
    public class NetworkManager
    {
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private bool isHost = false;
        private bool isConnected = false;
        private int localPort = 7777;
        private int remotePort = 7778;
        
        private Dictionary<int, NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();
        private int localPlayerId = 1;
        private float lastSendTime = 0f;
        private const float SEND_INTERVAL = 0.033f; // ~30 updates per second
        
        // Connection handshake state
        private bool connectionConfirmed = false;
        private float lastConnectionAttempt = 0f;
        private const float CONNECTION_RETRY_INTERVAL = 1f; // Retry connection every second
        private const int MAX_CONNECTION_RETRIES = 5;
        private int connectionRetries = 0;
        
        // Thread-safe queue for main thread operations
        private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        public event Action<int> OnPlayerConnected;
        public event Action<int> OnPlayerDisconnected;
        public bool IsHost => isHost;
        public bool IsConnected => isConnected;
        public int LocalPort => localPort;
        public string LocalIP => GetLocalIPAddress();
        public string RemoteIP => remoteEndPoint?.Address?.ToString() ?? "N/A";
        public int ConnectedPlayerCount => players.Count;
        
        /// <summary>
        /// Gets all remote players for nametag rendering
        /// </summary>
        public IEnumerable<NetworkPlayer> GetRemotePlayers()
        {
            return players.Values;
        }

        public NetworkManager()
        {
            MelonLogger.Msg("NetworkManager initialized");
        }

        /// <summary>
        /// Start hosting a game (server + client)
        /// </summary>
        public bool StartHost(int port = 7777)
        {
            try
            {
                localPort = port;
                remotePort = port + 1;
                localPlayerId = 1;
                
                udpClient = new UdpClient(localPort);
                udpClient.BeginReceive(OnReceive, null);
                
                isHost = true;
                isConnected = true;
                
                MelonLogger.Msg($"Started hosting on port {localPort}");
                OnPlayerConnected?.Invoke(localPlayerId);
                
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to start host: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Connect to a host
        /// </summary>
        public bool ConnectToHost(string hostAddress, int port = 7777)
        {
            try
            {
                remotePort = port;
                localPort = port + 1;
                localPlayerId = 2; // Client gets ID 2
                
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(hostAddress), remotePort);
                udpClient = new UdpClient(localPort);
                udpClient.BeginReceive(OnReceive, null);
                
                isHost = false;
                isConnected = false; // Don't set to true until connection is confirmed
                connectionConfirmed = false;
                connectionRetries = 0;
                lastConnectionAttempt = Time.time;
                
                // Send initial connection message
                SendConnectionMessage();
                
                MelonLogger.Msg($"Attempting to connect to host at {hostAddress}:{port}");
                
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
            // Process queued main thread operations (Unity API calls from async callbacks)
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
            
            // Handle connection retry for clients
            if (!isHost && !isConnected && remoteEndPoint != null && !connectionConfirmed)
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
            foreach (var player in players.Values)
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
            
            // Recreate all player objects in the new scene
            foreach (var player in players.Values)
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
            isHost = false;
            connectionConfirmed = false;
            connectionRetries = 0;
            remoteEndPoint = null;
            players.Clear();
            
            MelonLogger.Msg("Network shutdown complete");
        }

        private void OnReceive(IAsyncResult result)
        {
            try
            {
                if (udpClient == null) return;
                
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.EndReceive(result, ref sender);
                
                ProcessMessage(data, sender);
                
                // Continue receiving
                udpClient.BeginReceive(OnReceive, null);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error receiving data: {e.Message}");
            }
        }

        private void ProcessMessage(byte[] data, IPEndPoint sender)
        {
            if (data.Length < 1) return;

            MessageType type = (MessageType)data[0];

            switch (type)
            {
                case MessageType.Connection:
                    HandleConnectionMessage(data, sender);
                    break;
                case MessageType.PlayerUpdate:
                    HandlePlayerUpdate(data, sender);
                    break;
                case MessageType.Disconnect:
                    HandleDisconnectMessage(data, sender);
                    break;
                case MessageType.GameState:
                    HandleGameStateMessage(data, sender);
                    break;
            }
        }

        private void HandleConnectionMessage(byte[] data, IPEndPoint sender)
        {
            // Capture values needed for main thread (IPEndPoint is not thread-safe to capture directly)
            IPAddress senderAddress = sender.Address;
            int senderPort = sender.Port;
            bool isHostFlag = isHost;
            
            // Queue Unity operations to main thread
            mainThreadQueue.Enqueue(() =>
            {
                IPEndPoint senderEndPoint = new IPEndPoint(senderAddress, senderPort);
                
                // Host receives connection from client
                if (isHostFlag)
                {
                    if (remoteEndPoint == null || !remoteEndPoint.Address.Equals(senderAddress) || remoteEndPoint.Port != senderPort)
                    {
                        remoteEndPoint = senderEndPoint;
                        MelonLogger.Msg($"Client connected from {senderEndPoint}");
                        
                        // Send confirmation back
                        SendConnectionMessage();
                        
                        // Immediately send host's position so client can see host right away
                        SendPlayerUpdate();
                    }
                }
                // Client receives confirmation from host
                else
                {
                    // Verify this is from the expected host
                    if (remoteEndPoint != null && remoteEndPoint.Address.Equals(senderAddress) && remoteEndPoint.Port == senderPort)
                    {
                        if (!connectionConfirmed)
                        {
                            connectionConfirmed = true;
                            isConnected = true;
                            MelonLogger.Msg("Connection confirmed by host!");
                            OnPlayerConnected?.Invoke(localPlayerId);
                        }
                    }
                    else if (remoteEndPoint == null)
                    {
                        // First response from host
                        remoteEndPoint = senderEndPoint;
                        connectionConfirmed = true;
                        isConnected = true;
                        MelonLogger.Msg("Connection confirmed by host!");
                        OnPlayerConnected?.Invoke(localPlayerId);
                    }
                }
            });
        }

        private void HandlePlayerUpdate(byte[] data, IPEndPoint sender)
        {
            // Message format: [MessageType(1)] [PlayerId(4)] [Position(12)] [Rotation(12)] = 29 bytes
            if (data.Length < 29) return; // Minimum size check

            int playerId = BitConverter.ToInt32(data, 1);
            float x = BitConverter.ToSingle(data, 5);
            float y = BitConverter.ToSingle(data, 9);
            float z = BitConverter.ToSingle(data, 13);
            float rotX = BitConverter.ToSingle(data, 17);
            float rotY = BitConverter.ToSingle(data, 21);
            float rotZ = BitConverter.ToSingle(data, 25);

            Vector3 position = new Vector3(x, y, z);
            Vector3 rotation = new Vector3(rotX, rotY, rotZ);

            // Queue Unity operations to main thread (GameObject creation must be on main thread)
            mainThreadQueue.Enqueue(() =>
            {
                // Update remote player
                // Don't create NetworkPlayer for ourselves
                if (playerId != localPlayerId)
                {
                    if (!players.ContainsKey(playerId))
                    {
                        players[playerId] = new NetworkPlayer(playerId);
                        MelonLogger.Msg($"Created NetworkPlayer for remote Player {playerId}");
                        OnPlayerConnected?.Invoke(playerId);
                    }

                    players[playerId].UpdatePosition(position, rotation);
                }
            });
        }

        private void HandleDisconnectMessage(byte[] data, IPEndPoint sender)
        {
            int playerId = BitConverter.ToInt32(data, 1);
            
            // Queue Unity operations to main thread
            mainThreadQueue.Enqueue(() =>
            {
                MelonLogger.Msg($"Player {playerId} disconnected");
                
                if (players.ContainsKey(playerId))
                {
                    players[playerId].Destroy();
                    players.Remove(playerId);
                    OnPlayerDisconnected?.Invoke(playerId);
                }
            });
        }

        private void SendConnectionMessage()
        {
            if (remoteEndPoint == null && !isHost) return;

            List<byte> message = new List<byte>();
            message.Add((byte)MessageType.Connection);
            message.AddRange(BitConverter.GetBytes(localPlayerId));

            SendMessage(message.ToArray());
        }

        private void SendPlayerUpdate()
        {
            if (remoteEndPoint == null || !isConnected) return;

            // Get local player position/rotation using PlayerModelFinder
            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            
            GameObject playerObject = PlayerModelFinder.FindPlayerModel();
            
            if (playerObject != null)
            {
                position = playerObject.transform.position;
                rotation = playerObject.transform.eulerAngles;
            }
            else
            {
                // Fallback: use camera position if player object not found
                if (Camera.main != null)
                {
                    position = Camera.main.transform.position;
                    rotation = Camera.main.transform.eulerAngles;
                }
            }

            List<byte> message = new List<byte>();
            message.Add((byte)MessageType.PlayerUpdate);
            message.AddRange(BitConverter.GetBytes(localPlayerId));
            message.AddRange(BitConverter.GetBytes(position.x));
            message.AddRange(BitConverter.GetBytes(position.y));
            message.AddRange(BitConverter.GetBytes(position.z));
            message.AddRange(BitConverter.GetBytes(rotation.x));
            message.AddRange(BitConverter.GetBytes(rotation.y));
            message.AddRange(BitConverter.GetBytes(rotation.z));

            SendMessage(message.ToArray());
        }

        private void SendDisconnectMessage()
        {
            if (remoteEndPoint == null) return;

            List<byte> message = new List<byte>();
            message.Add((byte)MessageType.Disconnect);
            message.AddRange(BitConverter.GetBytes(localPlayerId));

            SendMessage(message.ToArray());
        }

        private void SendMessage(byte[] data)
        {
            try
            {
                if (isHost && remoteEndPoint != null)
                {
                    udpClient?.Send(data, data.Length, remoteEndPoint);
                }
                else if (!isHost && remoteEndPoint != null)
                {
                    udpClient?.Send(data, data.Length, remoteEndPoint);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error sending message: {e.Message}");
            }
        }
        
        /// <summary>
        /// Send game state update over network
        /// </summary>
        public void SendGameState(byte[] stateData)
        {
            if (remoteEndPoint == null || !isConnected) return;
            
            List<byte> message = new List<byte>();
            message.Add((byte)MessageType.GameState);
            message.AddRange(BitConverter.GetBytes(stateData.Length));
            message.AddRange(stateData);
            
            SendMessage(message.ToArray());
        }
        
        private void HandleGameStateMessage(byte[] data, IPEndPoint sender)
        {
            if (data.Length < 5) return; // Need at least type + length (4 bytes)
            
            int stateDataLength = BitConverter.ToInt32(data, 1);
            if (data.Length < 5 + stateDataLength) return;
            
            byte[] stateData = new byte[stateDataLength];
            Array.Copy(data, 5, stateData, 0, stateDataLength);
            
            // Queue to main thread for processing
            mainThreadQueue.Enqueue(() =>
            {
                OnGameStateReceived?.Invoke(stateData);
            });
        }
        
        public event Action<byte[]> OnGameStateReceived;

        /// <summary>
        /// Get local IP address (for hosting - what others should connect to)
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address?.ToString() ?? "Unknown";
                }
            }
            catch
            {
                try
                {
                    // Fallback: get first IPv4 address
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            return ip.ToString();
                        }
                    }
                }
                catch
                {
                    return "Unknown";
                }
                return "Unknown";
            }
        }
    }
}

