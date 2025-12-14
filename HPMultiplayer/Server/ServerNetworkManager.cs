using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HPMultiplayer.Networking;

namespace HPMultiplayer.Server
{
    /// <summary>
    /// Dedicated server network manager - handles multiple clients
    /// </summary>
    public class ServerNetworkManager
    {
        private UdpClient udpServer;
        private int serverPort;
        private int maxPlayers;
        private bool isRunning = false;
        private Thread receiveThread;
        
        private Dictionary<int, ServerPlayer> players = new Dictionary<int, ServerPlayer>();
        private Dictionary<IPEndPoint, int> endPointToPlayerId = new Dictionary<IPEndPoint, int>();
        private int nextPlayerId = 1;
        private object lockObject = new object();
        
        private GameStateManager gameStateManager;
        
        // Server tick rate
        private const int TICK_RATE = 30; // 30 updates per second
        private const double TICK_INTERVAL = 1.0 / TICK_RATE;
        private DateTime lastTickTime = DateTime.UtcNow;
        
        // Connection timeout
        private const double PLAYER_TIMEOUT = 30.0; // 30 seconds
        
        public bool IsRunning => isRunning;
        public int Port => serverPort;
        public int MaxPlayers => maxPlayers;
        public int ConnectedPlayerCount
        {
            get
            {
                lock (lockObject)
                {
                    return players.Count(p => p.Value.IsConnected);
                }
            }
        }

        public ServerNetworkManager(int port = 7777, int maxPlayers = 16)
        {
            this.serverPort = port;
            this.maxPlayers = maxPlayers;
            this.gameStateManager = new GameStateManager();
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public bool Start()
        {
            try
            {
                udpServer = new UdpClient(serverPort);
                isRunning = true;
                
                // Start receive thread
                receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "ServerReceiveThread"
                };
                receiveThread.Start();
                
                // Start server tick loop
                Thread tickThread = new Thread(ServerTickLoop)
                {
                    IsBackground = true,
                    Name = "ServerTickThread"
                };
                tickThread.Start();
                
                // Start cleanup thread
                Thread cleanupThread = new Thread(CleanupLoop)
                {
                    IsBackground = true,
                    Name = "ServerCleanupThread"
                };
                cleanupThread.Start();
                
                Console.WriteLine($"[Server] Started on port {serverPort}");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Server] Failed to start: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            
            lock (lockObject)
            {
                // Send disconnect to all players
                foreach (var player in players.Values)
                {
                    if (player.IsConnected)
                    {
                        try
                        {
                            byte[] disconnectMsg = NetworkProtocol.CreateDisconnectMessage(player.PlayerId);
                            udpServer.Send(disconnectMsg, disconnectMsg.Length, player.EndPoint);
                        }
                        catch { }
                    }
                }
                
                players.Clear();
                endPointToPlayerId.Clear();
            }
            
            udpServer?.Close();
            udpServer = null;
            
            Console.WriteLine("[Server] Stopped");
        }

        /// <summary>
        /// Main receive loop (runs on separate thread)
        /// </summary>
        private void ReceiveLoop()
        {
            while (isRunning && udpServer != null)
            {
                try
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpServer.Receive(ref remoteEndPoint);
                    
                    ProcessMessage(data, remoteEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    // Server was closed
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Server] Receive error: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Process incoming message
        /// </summary>
        private void ProcessMessage(byte[] data, IPEndPoint sender)
        {
            if (data.Length < 1) return;
            
            MessageType messageType = (MessageType)data[0];
            
            lock (lockObject)
            {
                switch (messageType)
                {
                    case MessageType.Connection:
                        HandleConnection(data, sender);
                        break;
                        
                    case MessageType.PlayerUpdate:
                        HandlePlayerUpdate(data, sender);
                        break;
                        
                    case MessageType.Disconnect:
                        HandleDisconnect(data, sender);
                        break;
                        
                    default:
                        Console.WriteLine($"[Server] Unknown message type: {messageType}");
                        break;
                }
            }
        }

        /// <summary>
        /// Handle connection request
        /// </summary>
        private void HandleConnection(byte[] data, IPEndPoint sender)
        {
            // Check if already connected
            if (endPointToPlayerId.ContainsKey(sender))
            {
                int existingId = endPointToPlayerId[sender];
                // Resend connection accepted
                byte[] acceptMsg = NetworkProtocol.CreateConnectionAcceptedMessage(existingId);
                udpServer.Send(acceptMsg, acceptMsg.Length, sender);
                return;
            }
            
            // Check max players
            if (players.Count >= maxPlayers)
            {
                byte[] rejectMsg = NetworkProtocol.CreateConnectionRejectedMessage("Server is full");
                udpServer.Send(rejectMsg, rejectMsg.Length, sender);
                Console.WriteLine($"[Server] Connection rejected from {sender}: Server full");
                return;
            }
            
            // Parse connection message
            if (!NetworkProtocol.ParseConnectionMessage(data, out int requestedId, out string playerName))
            {
                byte[] rejectMsg = NetworkProtocol.CreateConnectionRejectedMessage("Invalid connection message");
                udpServer.Send(rejectMsg, rejectMsg.Length, sender);
                return;
            }
            
            // Assign player ID
            int playerId = nextPlayerId++;
            ServerPlayer player = new ServerPlayer(playerId, sender, playerName);
            players[playerId] = player;
            endPointToPlayerId[sender] = playerId;
            
            // Send connection accepted
            byte[] acceptMsg2 = NetworkProtocol.CreateConnectionAcceptedMessage(playerId);
            udpServer.Send(acceptMsg2, acceptMsg2.Length, sender);
            
            Console.WriteLine($"[Server] Player {playerId} ({playerName}) connected from {sender}");
            
            // Notify other players
            BroadcastToOthers(playerId, NetworkProtocol.CreatePlayerJoinedMessage(playerId, playerName));
        }

        /// <summary>
        /// Handle player update
        /// </summary>
        private void HandlePlayerUpdate(byte[] data, IPEndPoint sender)
        {
            if (!endPointToPlayerId.TryGetValue(sender, out int playerId))
                return;
            
            if (!players.TryGetValue(playerId, out ServerPlayer player))
                return;
            
            if (!NetworkProtocol.ParsePlayerUpdateMessage(data, out int msgPlayerId, out Vector3F position, out Vector3F rotation))
                return;
            
            // Update player state
            player.UpdatePosition(position, rotation);
            player.LastHeartbeatTime = DateTime.UtcNow;
            
            // Broadcast to all other players
            BroadcastToOthers(playerId, NetworkProtocol.CreatePlayerUpdateMessage(playerId, position, rotation));
        }

        /// <summary>
        /// Handle disconnect
        /// </summary>
        private void HandleDisconnect(byte[] data, IPEndPoint sender)
        {
            if (!endPointToPlayerId.TryGetValue(sender, out int playerId))
                return;
            
            RemovePlayer(playerId);
        }

        /// <summary>
        /// Remove a player
        /// </summary>
        private void RemovePlayer(int playerId)
        {
            if (!players.TryGetValue(playerId, out ServerPlayer player))
                return;
            
            player.IsConnected = false;
            endPointToPlayerId.Remove(player.EndPoint);
            players.Remove(playerId);
            
            Console.WriteLine($"[Server] Player {playerId} ({player.PlayerName}) disconnected");
            
            // Notify other players
            BroadcastToAll(NetworkProtocol.CreatePlayerLeftMessage(playerId), playerId);
        }

        /// <summary>
        /// Broadcast message to all players except one
        /// </summary>
        private void BroadcastToOthers(int excludePlayerId, byte[] message)
        {
            foreach (var player in players.Values)
            {
                if (player.IsConnected && player.PlayerId != excludePlayerId)
                {
                    try
                    {
                        udpServer.Send(message, message.Length, player.EndPoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[Server] Failed to send to player {player.PlayerId}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Broadcast message to all players
        /// </summary>
        private void BroadcastToAll(byte[] message, int excludePlayerId = -1)
        {
            foreach (var player in players.Values)
            {
                if (player.IsConnected && player.PlayerId != excludePlayerId)
                {
                    try
                    {
                        udpServer.Send(message, message.Length, player.EndPoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[Server] Failed to send to player {player.PlayerId}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Server tick loop - sends periodic updates
        /// </summary>
        private void ServerTickLoop()
        {
            while (isRunning)
            {
                DateTime currentTime = DateTime.UtcNow;
                double deltaTime = (currentTime - lastTickTime).TotalSeconds;
                
                if (deltaTime >= TICK_INTERVAL)
                {
                    lock (lockObject)
                    {
                        // Broadcast game state to all players
                        byte[] gameState = gameStateManager.SerializeState();
                        if (gameState != null && gameState.Length > 0)
                        {
                            byte[] gameStateMsg = NetworkProtocol.CreateGameStateMessage(gameState);
                            BroadcastToAll(gameStateMsg);
                        }
                        
                        // Send all player positions to all players (simplified - in production you'd optimize this)
                        foreach (var player in players.Values)
                        {
                            if (player.IsConnected)
                            {
                                byte[] playerUpdate = NetworkProtocol.CreatePlayerUpdateMessage(
                                    player.PlayerId, player.Position, player.Rotation);
                                BroadcastToOthers(player.PlayerId, playerUpdate);
                            }
                        }
                    }
                    
                    lastTickTime = currentTime;
                }
                
                Thread.Sleep(1); // Small sleep to prevent 100% CPU
            }
        }

        /// <summary>
        /// Cleanup loop - removes timed out players
        /// </summary>
        private void CleanupLoop()
        {
            while (isRunning)
            {
                Thread.Sleep(5000); // Check every 5 seconds
                
                lock (lockObject)
                {
                    List<int> toRemove = new List<int>();
                    DateTime now = DateTime.UtcNow;
                    
                    foreach (var player in players.Values)
                    {
                        if ((now - player.LastHeartbeatTime).TotalSeconds > PLAYER_TIMEOUT)
                        {
                            toRemove.Add(player.PlayerId);
                        }
                    }
                    
                    foreach (int playerId in toRemove)
                    {
                        Console.WriteLine($"[Server] Player {playerId} timed out");
                        RemovePlayer(playerId);
                    }
                }
            }
        }

        /// <summary>
        /// List connected players
        /// </summary>
        public void ListPlayers()
        {
            lock (lockObject)
            {
                if (players.Count == 0)
                {
                    Console.WriteLine("No players connected.");
                    return;
                }
                
                Console.WriteLine($"Connected players ({players.Count}):");
                foreach (var player in players.Values)
                {
                    Console.WriteLine($"  [{player.PlayerId}] {player.PlayerName} - {player.EndPoint}");
                }
            }
        }
    }
}

