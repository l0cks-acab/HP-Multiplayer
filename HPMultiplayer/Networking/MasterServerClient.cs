using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MelonLoader;
using UnityEngine;

namespace HPMultiplayer.Networking
{
    /// <summary>
    /// Client for communicating with a master server to register and browse games
    /// </summary>
    public class MasterServerClient
    {
        private UdpClient udpClient;
        private IPEndPoint masterServerEndPoint;
        private bool isRegistered = false;
        private string serverName = "House Party Game";
        private int gamePort = 7777;
        private float lastHeartbeat = 0f;
        private const float HEARTBEAT_INTERVAL = 5f; // Send heartbeat every 5 seconds
        private const float SERVER_TIMEOUT = 15f; // Remove servers that haven't sent heartbeat in 15 seconds

        // Public master server (can be changed to your own)
        private const string DEFAULT_MASTER_SERVER = "hpmultiplayer-master.herokuapp.com"; // Placeholder - you'd need to host this
        private const int DEFAULT_MASTER_PORT = 27015;

        public Dictionary<string, ServerInfo> AvailableServers { get; private set; } = new Dictionary<string, ServerInfo>();

        public MasterServerClient()
        {
            try
            {
                // Try to resolve master server address
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(DEFAULT_MASTER_SERVER);
                    if (addresses.Length > 0)
                    {
                        masterServerEndPoint = new IPEndPoint(addresses[0], DEFAULT_MASTER_PORT);
                    }
                    else
                    {
                        // Fallback to localhost for testing
                        masterServerEndPoint = new IPEndPoint(IPAddress.Loopback, DEFAULT_MASTER_PORT);
                    }
                }
                catch
                {
                    // If DNS resolution fails, use localhost
                    masterServerEndPoint = new IPEndPoint(IPAddress.Loopback, DEFAULT_MASTER_PORT);
                }

                // Create and bind UDP client to any available port (0 = auto-assign)
                udpClient = new UdpClient(0);
                udpClient.BeginReceive(OnReceive, null);
                
                MelonLogger.Msg("MasterServerClient initialized");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to initialize MasterServerClient: {e.Message}");
                // Don't throw - allow mod to continue without master server
                udpClient = null;
                masterServerEndPoint = null;
            }
        }

        /// <summary>
        /// Register this game as a host on the master server
        /// </summary>
        public void RegisterServer(string serverName, int port)
        {
            this.serverName = serverName;
            this.gamePort = port;
            
            // Try master server registration
            if (masterServerEndPoint != null && udpClient != null)
            {
                try
                {
                    string localIP = GetLocalIPAddress();
                    string message = $"REGISTER|{serverName}|{localIP}|{port}|{GetSteamId()}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    
                    udpClient.Send(data, data.Length, masterServerEndPoint);
                    isRegistered = true;
                    lastHeartbeat = Time.time;
                    
                    MelonLogger.Msg($"Registered server: {serverName} on {localIP}:{port}");
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Master server registration failed: {e.Message} - Using local network discovery instead");
                }
            }
            else
            {
                MelonLogger.Msg("Master server not available - Using local network discovery");
            }
            
            // Always enable local network broadcast discovery
            StartLocalBroadcast(serverName, port);
        }
        
        private UdpClient broadcastClient;
        private UdpClient broadcastListener;
        private IPEndPoint broadcastEndPoint;
        private float lastBroadcastTime = 0f;
        private const float BROADCAST_INTERVAL = 2f; // Broadcast every 2 seconds
        private const int BROADCAST_PORT = 27016;
        private const int LISTENER_PORT = 27017;
        
        private void StartLocalBroadcast(string serverName, int port)
        {
            try
            {
                // Create a separate UDP client for broadcasting
                broadcastClient = new UdpClient();
                broadcastClient.EnableBroadcast = true;
                
                // Broadcast to local network (255.255.255.255)
                broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);
                
                // Start listening for broadcasts on a different port
                if (broadcastListener == null)
                {
                    broadcastListener = new UdpClient(LISTENER_PORT);
                    broadcastListener.BeginReceive(OnBroadcastReceive, broadcastListener);
                }
                
                lastBroadcastTime = Time.time;
                MelonLogger.Msg("Started local network discovery broadcast");
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Failed to start local broadcast: {e.Message}");
            }
        }
        
        private void OnBroadcastReceive(IAsyncResult result)
        {
            try
            {
                if (broadcastListener == null) return;
                
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = broadcastListener.EndReceive(result, ref sender);
                string message = Encoding.UTF8.GetString(data);
                
                // Process broadcast message
                if (message.StartsWith("SERVER|"))
                {
                    string[] parts = message.Split('|');
                    if (parts.Length >= 4)
                    {
                        string name = parts[1];
                        string ip = parts[2];
                        int port = int.TryParse(parts[3], out int p) ? p : 7777;
                        string serverId = $"{ip}:{port}";
                        
                        // Don't add our own server
                        if (ip != GetLocalIPAddress() || port != gamePort)
                        {
                            if (!AvailableServers.ContainsKey(serverId))
                            {
                                AvailableServers[serverId] = new ServerInfo
                                {
                                    Name = name,
                                    IP = ip,
                                    Port = port,
                                    LastUpdate = Time.time
                                };
                                MelonLogger.Msg($"Discovered server via broadcast: {name} at {ip}:{port}");
                            }
                            else
                            {
                                AvailableServers[serverId].LastUpdate = Time.time;
                            }
                        }
                    }
                }
                else if (message == "DISCOVER" && isRegistered)
                {
                    // Someone is looking for servers, respond with our server info
                    try
                    {
                        string localIP = GetLocalIPAddress();
                        string response = $"SERVER|{serverName}|{localIP}|{gamePort}";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        broadcastClient.Send(responseData, responseData.Length, sender);
                    }
                    catch { }
                }
                
                // Continue listening
                if (broadcastListener != null)
                {
                    broadcastListener.BeginReceive(OnBroadcastReceive, broadcastListener);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Error receiving broadcast: {e.Message}");
            }
        }

        /// <summary>
        /// Unregister this game from the master server
        /// </summary>
        public void UnregisterServer()
        {
            if (!isRegistered || masterServerEndPoint == null || udpClient == null) return;

            try
            {
                string message = $"UNREGISTER|{GetSteamId()}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                
                udpClient.Send(data, data.Length, masterServerEndPoint);
                isRegistered = false;
                
                MelonLogger.Msg("Unregistered server");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to unregister server: {e.Message}");
            }
        }

        /// <summary>
        /// Request list of available servers from master server
        /// </summary>
        public void RequestServerList()
        {
            // Try master server first
            if (masterServerEndPoint != null && udpClient != null)
            {
                try
                {
                    string message = "LIST";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udpClient.Send(data, data.Length, masterServerEndPoint);
                    MelonLogger.Msg("Requested server list from master server");
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Master server request failed: {e.Message}");
                }
            }
            
            // Also send a broadcast to discover local servers
            try
            {
                if (broadcastClient == null)
                {
                    broadcastClient = new UdpClient();
                    broadcastClient.EnableBroadcast = true;
                    broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);
                }
                
                // Start listener if not already started
                if (broadcastListener == null)
                {
                    broadcastListener = new UdpClient(LISTENER_PORT);
                    broadcastListener.BeginReceive(OnBroadcastReceive, broadcastListener);
                }
                
                // Send discovery request
                string discoverMessage = "DISCOVER";
                byte[] discoverData = Encoding.UTF8.GetBytes(discoverMessage);
                broadcastClient.Send(discoverData, discoverData.Length, broadcastEndPoint);
                
                MelonLogger.Msg("Sent local network discovery broadcast");
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Local discovery broadcast failed: {e.Message}");
            }
        }

        /// <summary>
        /// Update called every frame
        /// </summary>
        public void Update()
        {
            // Send heartbeat if registered with master server
            if (isRegistered && udpClient != null && Time.time - lastHeartbeat >= HEARTBEAT_INTERVAL)
            {
                try
                {
                    string localIP = GetLocalIPAddress();
                    string message = $"REGISTER|{serverName}|{localIP}|{gamePort}|{GetSteamId()}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udpClient.Send(data, data.Length, masterServerEndPoint);
                    lastHeartbeat = Time.time;
                }
                catch { }
            }
            
            // Send local network broadcast if hosting
            if (broadcastClient != null && isRegistered && Time.time - lastBroadcastTime >= BROADCAST_INTERVAL)
            {
                try
                {
                    string localIP = GetLocalIPAddress();
                    string message = $"SERVER|{serverName}|{localIP}|{gamePort}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    broadcastClient.Send(data, data.Length, broadcastEndPoint);
                    lastBroadcastTime = Time.time;
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Broadcast failed: {e.Message}");
                }
            }

            // Remove stale servers
            List<string> toRemove = new List<string>();
            foreach (var kvp in AvailableServers)
            {
                if (Time.time - kvp.Value.LastUpdate > SERVER_TIMEOUT)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (string key in toRemove)
            {
                AvailableServers.Remove(key);
            }
        }

        private void OnReceive(IAsyncResult result)
        {
            try
            {
                if (udpClient == null) return;

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.EndReceive(result, ref sender);
                string message = Encoding.UTF8.GetString(data);

                ProcessMessage(message, sender);

                // Continue receiving
                udpClient.BeginReceive(OnReceive, null);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error receiving master server data: {e.Message}");
            }
        }

        private void ProcessMessage(string message, IPEndPoint sender)
        {
            string[] parts = message.Split('|');
            if (parts.Length < 1) return;

            string command = parts[0];

            switch (command)
            {
                case "SERVERLIST":
                    // Format: SERVERLIST|name1|ip1|port1|name2|ip2|port2|...
                    for (int i = 1; i < parts.Length; i += 3)
                    {
                        if (i + 2 < parts.Length)
                        {
                            string name = parts[i];
                            string ip = parts[i + 1];
                            int port = int.TryParse(parts[i + 2], out int p) ? p : 7777;
                            string serverId = $"{ip}:{port}";

                            if (!AvailableServers.ContainsKey(serverId))
                            {
                                AvailableServers[serverId] = new ServerInfo
                                {
                                    Name = name,
                                    IP = ip,
                                    Port = port,
                                    LastUpdate = Time.time
                                };
                                MelonLogger.Msg($"Found server: {name} at {ip}:{port}");
                            }
                            else
                            {
                                AvailableServers[serverId].LastUpdate = Time.time;
                            }
                        }
                    }
                    break;

                case "ACK":
                    MelonLogger.Msg("Master server acknowledged registration");
                    break;
            }
        }

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
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            return ip.ToString();
                        }
                    }
                }
                catch { }
                return "Unknown";
            }
        }

        private string GetSteamId()
        {
            // TODO: Get actual Steam ID if available
            // For now, use a combination of machine name and IP
            return $"{Environment.MachineName}_{GetLocalIPAddress()}";
        }

        public void Shutdown()
        {
            UnregisterServer();
            udpClient?.Close();
            udpClient = null;
            broadcastClient?.Close();
            broadcastClient = null;
            broadcastListener?.Close();
            broadcastListener = null;
            AvailableServers.Clear();
        }
    }

    public class ServerInfo
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public float LastUpdate { get; set; }
    }
}

