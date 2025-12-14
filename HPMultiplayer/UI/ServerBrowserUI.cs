using UnityEngine;
using UnityEngine.InputSystem;
using MelonLoader;
using HPMultiplayer.Networking;
using System.Collections.Generic;
using System.Linq;

namespace HPMultiplayer.UI
{
    /// <summary>
    /// Server browser UI for browsing and joining available games
    /// </summary>
    public class ServerBrowserUI
    {
        private MasterServerClient masterServerClient;
        private NetworkManager networkManager;
        private bool isVisible = false;
        private Vector2 scrollPosition = Vector2.zero;
        private Rect windowRect = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 250, 600, 500);
        private float lastRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 3f; // Refresh server list every 3 seconds
        private string selectedServerId = null;
        
        // Direct connect input fields
        private string directIP = "127.0.0.1";
        private string directPort = "7777";
        private int editingDirectField = 0; // 0 = none, 1 = IP, 2 = Port

        public void Initialize(MasterServerClient masterClient, NetworkManager netManager)
        {
            masterServerClient = masterClient;
            networkManager = netManager;
            MelonLogger.Msg("ServerBrowserUI initialized");
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
            if (visible)
            {
                // Request server list when opened
                masterServerClient?.RequestServerList();
                lastRefreshTime = Time.time;
            }
        }

        public void Update()
        {
            if (!isVisible) return;

            // Auto-refresh server list
            if (Time.time - lastRefreshTime >= REFRESH_INTERVAL)
            {
                masterServerClient?.RequestServerList();
                lastRefreshTime = Time.time;
            }

            // Update master server client
            masterServerClient?.Update();
        }

        public void OnGUI()
        {
            if (!isVisible) return;

            // Draw window background
            GUI.Box(windowRect, "Server Browser - Available Games");

            float x = windowRect.x + 10;
            float y = windowRect.y + 30;
            float width = windowRect.width - 20;
            float buttonHeight = 30f;
            float spacing = 10f;

            // Title
            GUI.Label(new Rect(x, y, width, 25), "Click Refresh to find games, or double-click a server to join");
            y += 30;

            // Refresh button
            if (GUI.Button(new Rect(x, y, width * 0.3f, buttonHeight), "Refresh"))
            {
                masterServerClient?.RequestServerList();
                lastRefreshTime = Time.time;
            }

            // Close button
            if (GUI.Button(new Rect(x + width * 0.7f, y, width * 0.3f, buttonHeight), "Close"))
            {
                isVisible = false;
            }
            y += buttonHeight + spacing;

            // Server list area
            Rect listRect = new Rect(x, y, width, windowRect.height - y - 50);
            GUI.Box(listRect, "");

            // Draw server list
            var servers = masterServerClient?.AvailableServers?.Values?.ToList() ?? new List<ServerInfo>();
            
            if (servers.Count == 0)
            {
                GUI.Label(new Rect(x + 10, y + 10, width - 20, 30), "No servers found. Make sure someone is hosting a game!");
            }
            else
            {
                float itemHeight = 40f;
                float listY = y + 5;
                int index = 0;

                foreach (var server in servers)
                {
                    Rect itemRect = new Rect(x + 5, listY, width - 10, itemHeight);
                    
                    // Highlight selected server
                    if (selectedServerId == $"{server.IP}:{server.Port}")
                    {
                        GUI.color = new Color(0.3f, 0.5f, 1f, 0.5f);
                        GUI.Box(itemRect, "");
                        GUI.color = Color.white;
                    }

                    // Server name
                    GUI.Label(new Rect(itemRect.x + 5, itemRect.y + 5, itemRect.width - 100, 20), server.Name);
                    
                    // Server IP:Port
                    GUI.Label(new Rect(itemRect.x + 5, itemRect.y + 20, itemRect.width - 100, 15), $"{server.IP}:{server.Port}");

                    // Join button
                    if (GUI.Button(new Rect(itemRect.x + itemRect.width - 80, itemRect.y + 5, 75, itemHeight - 10), "Join"))
                    {
                        JoinServer(server);
                    }

                    // Handle double-click to join
                    Event evt = Event.current;
                    if (evt.type == EventType.MouseDown && evt.clickCount == 2 && itemRect.Contains(evt.mousePosition))
                    {
                        JoinServer(server);
                        evt.Use();
                    }
                    else if (evt.type == EventType.MouseDown && itemRect.Contains(evt.mousePosition))
                    {
                        selectedServerId = $"{server.IP}:{server.Port}";
                        evt.Use();
                    }

                    listY += itemHeight + 2;
                    index++;
                }
            }

            // Direct connect section at bottom
            y = windowRect.y + windowRect.height - 40;
            GUI.Label(new Rect(x, y, width * 0.2f, 25), "Direct Connect:");
            y += 25;

            // IP input (manual text field)
            GUI.Label(new Rect(x, y, width * 0.15f, 25), "IP:");
            Rect ipRect = new Rect(x + width * 0.15f, y, width * 0.4f, 25);
            GUI.Box(new Rect(ipRect.x - 2, ipRect.y - 2, ipRect.width + 4, ipRect.height + 4), "");
            if (editingDirectField == 1) GUI.color = Color.yellow;
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && ipRect.Contains(currentEvent.mousePosition))
            {
                editingDirectField = 1;
                currentEvent.Use();
            }
            GUI.Label(ipRect, directIP);
            GUI.color = Color.white;

            // Port input (manual text field)
            GUI.Label(new Rect(x + width * 0.55f, y, width * 0.1f, 25), "Port:");
            Rect portRect = new Rect(x + width * 0.65f, y, width * 0.15f, 25);
            GUI.Box(new Rect(portRect.x - 2, portRect.y - 2, portRect.width + 4, portRect.height + 4), "");
            if (editingDirectField == 2) GUI.color = Color.yellow;
            if (currentEvent.type == EventType.MouseDown && portRect.Contains(currentEvent.mousePosition))
            {
                editingDirectField = 2;
                currentEvent.Use();
            }
            GUI.Label(portRect, directPort);
            GUI.color = Color.white;

            // Handle keyboard input for direct connect fields
            HandleDirectConnectInput();

            // Connect button
            if (GUI.Button(new Rect(x + width * 0.8f, y, width * 0.2f, 25), "Connect"))
            {
                if (int.TryParse(directPort, out int port))
                {
                    networkManager?.ConnectToHost(directIP, port);
                    isVisible = false;
                }
            }
        }

        private void HandleDirectConnectInput()
        {
            if (editingDirectField == 0) return;

            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.KeyDown) return;

            // Handle Ctrl+C (Copy)
            if ((Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed) 
                && Keyboard.current[Key.C].wasPressedThisFrame)
            {
                string textToCopy = editingDirectField == 1 ? directIP : directPort;
                if (!string.IsNullOrEmpty(textToCopy))
                {
                    GUIUtility.systemCopyBuffer = textToCopy;
                    MelonLogger.Msg($"Copied: {textToCopy}");
                }
                currentEvent.Use();
                return;
            }
            
            // Handle Ctrl+V (Paste)
            if ((Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed) 
                && Keyboard.current[Key.V].wasPressedThisFrame)
            {
                string pastedText = GUIUtility.systemCopyBuffer;
                if (!string.IsNullOrEmpty(pastedText))
                {
                    if (editingDirectField == 1)
                    {
                        // IP field - filter to only valid IP characters
                        string filtered = "";
                        foreach (char pasteChar in pastedText)
                        {
                            if (char.IsDigit(pasteChar) || pasteChar == '.' || pasteChar == ':')
                            {
                                filtered += pasteChar;
                            }
                        }
                        directIP = filtered;
                    }
                    else if (editingDirectField == 2)
                    {
                        // Port field - filter to only digits
                        string filtered = "";
                        foreach (char pasteChar in pastedText)
                        {
                            if (char.IsDigit(pasteChar))
                            {
                                filtered += pasteChar;
                            }
                        }
                        directPort = filtered;
                    }
                    MelonLogger.Msg($"Pasted: {pastedText}");
                }
                currentEvent.Use();
                return;
            }

            // Handle backspace
            if (currentEvent.keyCode == KeyCode.Backspace)
            {
                if (editingDirectField == 1 && directIP.Length > 0)
                {
                    directIP = directIP.Substring(0, directIP.Length - 1);
                    currentEvent.Use();
                }
                else if (editingDirectField == 2 && directPort.Length > 0)
                {
                    directPort = directPort.Substring(0, directPort.Length - 1);
                    currentEvent.Use();
                }
                return;
            }

            // Handle Escape or Enter to deselect
            if (currentEvent.keyCode == KeyCode.Escape || currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter)
            {
                editingDirectField = 0;
                currentEvent.Use();
                return;
            }

            // Handle character input
            char c = currentEvent.character;
            if (char.IsControl(c)) return;

            if (editingDirectField == 1)
            {
                // IP field - allow numbers, dots, and colons
                if (char.IsDigit(c) || c == '.' || c == ':')
                {
                    directIP += c;
                    currentEvent.Use();
                }
            }
            else if (editingDirectField == 2)
            {
                // Port field - only numbers
                if (char.IsDigit(c))
                {
                    directPort += c;
                    currentEvent.Use();
                }
            }
        }

        private void JoinServer(ServerInfo server)
        {
            MelonLogger.Msg($"Joining server: {server.Name} at {server.IP}:{server.Port}");
            networkManager?.ConnectToHost(server.IP, server.Port);
            isVisible = false;
        }
    }
}

