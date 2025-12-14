using UnityEngine;
using UnityEngine.InputSystem;
using MelonLoader;
using HPMultiplayer.Networking;
using System;

namespace HPMultiplayer.UI
{
    /// <summary>
    /// UI for multiplayer controls (host/join)
    /// </summary>
    public class MultiplayerUI
    {
        private NetworkManager networkManager;
        private MasterServerClient masterServerClient;
        private bool isVisible = false;
        private string hostIP = "127.0.0.1";
        private string portString = "7777";
        private string statusText = "Disconnected";
        
        private Rect windowRect = new Rect(100, 100, 450, 400);
        
        // Text input state
        private int editingField = 0; // 0 = none, 1 = IP, 2 = Port
        private float cursorBlinkTime = 0f;
        private const float CURSOR_BLINK_RATE = 0.5f;
        
        // Store delegates for proper unsubscribe in Il2Cpp
        private Action<int> onPlayerConnectedHandler;
        private Action<int> onPlayerDisconnectedHandler;

        public void Initialize(NetworkManager manager, MasterServerClient masterClient = null)
        {
            networkManager = manager;
            masterServerClient = masterClient;
            
            // Create and store delegates for Il2Cpp compatibility
            onPlayerConnectedHandler = new Action<int>(OnPlayerConnected);
            onPlayerDisconnectedHandler = new Action<int>(OnPlayerDisconnected);
            
            // Subscribe to network events
            networkManager.OnPlayerConnected += onPlayerConnectedHandler;
            networkManager.OnPlayerDisconnected += onPlayerDisconnectedHandler;
            
            MelonLogger.Msg("MultiplayerUI initialized");
        }

        private void OnPlayerConnected(int playerId)
        {
            UpdateStatusText();
            MelonLogger.Msg($"UI: Player {playerId} connected");
        }

        private void OnPlayerDisconnected(int playerId)
        {
            UpdateStatusText();
            MelonLogger.Msg($"UI: Player {playerId} disconnected");
        }
        
        private void UpdateStatusText()
        {
            if (!networkManager.IsConnected)
            {
                statusText = "Disconnected";
            }
            else if (networkManager.IsHost)
            {
                int playerCount = networkManager.ConnectedPlayerCount;
                if (playerCount <= 1)
                {
                    statusText = $"Hosting (Waiting for players...)";
                }
                else
                {
                    statusText = $"Hosting ({playerCount} players)";
                }
            }
            else
            {
                statusText = $"Connected to {networkManager.RemoteIP}";
            }
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
        }

        public void OnGUI()
        {
            if (!isVisible) return;

            // Draw window background box
            GUI.Box(windowRect, "Multiplayer");
            
            // Use completely manual layout to avoid Il2Cpp method stripping
            // Calculate positions manually without GUILayoutUtility
            float x = windowRect.x + 10;
            float y = windowRect.y + 30;
            float width = windowRect.width - 20;
            float lineHeight = 25f;
            float buttonHeight = 30f;
            float spacing = 10f;
            
            // Status - update it based on current connection state
            UpdateStatusText();
            GUI.Label(new Rect(x, y, width, lineHeight), $"Status: {statusText}");
            y += lineHeight + spacing;
            
            // Show host IP address when hosting
            if (networkManager.IsHost && networkManager.IsConnected)
            {
                string hostIPDisplay = $"Your IP: {networkManager.LocalIP}";
                string portDisplay = $"Port: {networkManager.LocalPort}";
                GUI.Label(new Rect(x, y, width, lineHeight), hostIPDisplay);
                y += lineHeight;
                GUI.Label(new Rect(x, y, width, lineHeight), portDisplay);
                y += lineHeight + spacing;
            }
            
            // Host button (only show if not already connected)
            if (!networkManager.IsConnected)
            {
                if (GUI.Button(new Rect(x, y, width, buttonHeight), "Host Game"))
                {
                    int port = int.TryParse(portString, out int p) ? p : 7777;
                    if (networkManager.StartHost(port))
                    {
                        // Register with master server
                        masterServerClient?.RegisterServer($"Player's Game", port);
                        UpdateStatusText();
                    }
                    else
                    {
                        statusText = "Failed to host - Port may be in use";
                    }
                }
                y += buttonHeight + spacing;
                
                // Browse Servers button
                if (GUI.Button(new Rect(x, y, width, buttonHeight), "Browse Servers (Press B)"))
                {
                    // This will be handled by the main mod
                }
                y += buttonHeight + spacing;
            }
            // Join section (only show if not connected)
            if (!networkManager.IsConnected)
            {
                GUI.Label(new Rect(x, y, width, lineHeight), "Connect to Host:");
                y += lineHeight + 2;
            
            // IP Text Field - clickable to edit
            Rect ipLabelRect = new Rect(x, y, width * 0.25f, lineHeight);
            Rect ipFieldRect = new Rect(x + width * 0.25f + 5, y, width * 0.75f - 5, lineHeight);
            
            GUI.Label(ipLabelRect, "Host IP:");
            
            // Draw background box to make field visible
            Color originalColor = GUI.color;
            if (editingField == 1)
            {
                GUI.color = new Color(1f, 1f, 0.7f, 1f); // Highlight when editing
            }
            GUI.Box(new Rect(ipFieldRect.x - 3, ipFieldRect.y - 2, ipFieldRect.width + 6, ipFieldRect.height + 4), "");
            GUI.color = originalColor;
            
            // Handle click to select field
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && ipFieldRect.Contains(currentEvent.mousePosition))
            {
                editingField = 1;
                currentEvent.Use();
            }
            
            // Display IP with cursor if editing
            string ipDisplay = string.IsNullOrEmpty(hostIP) ? "127.0.0.1" : hostIP;
            if (editingField == 1)
            {
                cursorBlinkTime += Time.deltaTime;
                if (cursorBlinkTime > CURSOR_BLINK_RATE * 2f) cursorBlinkTime = 0f;
                bool showCursor = (cursorBlinkTime % (CURSOR_BLINK_RATE * 2f)) < CURSOR_BLINK_RATE;
                if (showCursor)
                {
                    ipDisplay += "|";
                }
            }
            GUI.Label(new Rect(ipFieldRect.x + 2, ipFieldRect.y, ipFieldRect.width - 4, ipFieldRect.height), ipDisplay);
            y += lineHeight + 5;
            
            // Port Text Field - clickable to edit
            Rect portLabelRect = new Rect(x, y, width * 0.25f, lineHeight);
            Rect portFieldRect = new Rect(x + width * 0.25f + 5, y, width * 0.75f - 5, lineHeight);
            
            GUI.Label(portLabelRect, "Port:");
            
            // Draw background box to make field visible
            if (editingField == 2)
            {
                GUI.color = new Color(1f, 1f, 0.7f, 1f); // Highlight when editing
            }
            GUI.Box(new Rect(portFieldRect.x - 3, portFieldRect.y - 2, portFieldRect.width + 6, portFieldRect.height + 4), "");
            GUI.color = originalColor;
            
            // Handle click to select field
            if (currentEvent.type == EventType.MouseDown && portFieldRect.Contains(currentEvent.mousePosition))
            {
                editingField = 2;
                currentEvent.Use();
            }
            
            // Display Port with cursor if editing
            string portDisplay = string.IsNullOrEmpty(portString) ? "7777" : portString;
            if (editingField == 2)
            {
                cursorBlinkTime += Time.deltaTime;
                if (cursorBlinkTime > CURSOR_BLINK_RATE * 2f) cursorBlinkTime = 0f;
                bool showCursor = (cursorBlinkTime % (CURSOR_BLINK_RATE * 2f)) < CURSOR_BLINK_RATE;
                if (showCursor)
                {
                    portDisplay += "|";
                }
            }
            GUI.Label(new Rect(portFieldRect.x + 2, portFieldRect.y, portFieldRect.width - 4, portFieldRect.height), portDisplay);
                y += lineHeight + spacing;
                
                // Handle keyboard input for editing
                HandleTextInput();
                
                // Join button
                if (GUI.Button(new Rect(x, y, width, buttonHeight), "Join Game"))
                {
                    int port = int.TryParse(portString, out int p) ? p : 7777;
                    string ip = string.IsNullOrEmpty(hostIP) ? "127.0.0.1" : hostIP;
                    if (networkManager.ConnectToHost(ip, port))
                    {
                        UpdateStatusText();
                    }
                    else
                    {
                        statusText = "Failed to connect - Check IP/Port and firewall";
                    }
                }
                y += buttonHeight + spacing;
            }
            
            // Disconnect button
            if (networkManager.IsConnected)
            {
                if (GUI.Button(new Rect(x, y, width, buttonHeight), "Disconnect"))
                {
                    // Unregister from master server if hosting
                    if (networkManager.IsHost)
                    {
                        masterServerClient?.UnregisterServer();
                    }
                    networkManager.Shutdown();
                    statusText = "Disconnected";
                }
                y += buttonHeight + spacing;
            }
            
            // Close button
            if (GUI.Button(new Rect(x, y, width, buttonHeight), "Close (Press M to reopen)"))
            {
                isVisible = false;
            }
        }
        
        private void HandleTextInput()
        {
            if (editingField == 0) return;
            
            Event currentEvent = Event.current;
            
            // Only process input during KeyDown events to avoid multiple calls per frame
            if (currentEvent.type != EventType.KeyDown) return;
            
            // Handle Ctrl+C (Copy)
            if ((Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed) 
                && Keyboard.current[Key.C].wasPressedThisFrame)
            {
                string textToCopy = editingField == 1 ? hostIP : portString;
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
                    if (editingField == 1)
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
                        hostIP = filtered;
                    }
                    else if (editingField == 2)
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
                        portString = filtered;
                    }
                    MelonLogger.Msg($"Pasted: {pastedText}");
                }
                currentEvent.Use();
                return;
            }
            
            // Handle Ctrl+A (Select All) - clear field to allow replacing
            if ((Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed) 
                && Keyboard.current[Key.A].wasPressedThisFrame)
            {
                if (editingField == 1)
                {
                    hostIP = "";
                }
                else if (editingField == 2)
                {
                    portString = "";
                }
                currentEvent.Use();
                return;
            }
            
            // Handle backspace
            if (currentEvent.keyCode == KeyCode.Backspace)
            {
                if (editingField == 1 && hostIP.Length > 0)
                {
                    hostIP = hostIP.Substring(0, hostIP.Length - 1);
                    currentEvent.Use();
                }
                else if (editingField == 2 && portString.Length > 0)
                {
                    portString = portString.Substring(0, portString.Length - 1);
                    currentEvent.Use();
                }
                return;
            }
            
            // Handle Escape or Enter to deselect
            if (currentEvent.keyCode == KeyCode.Escape || currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter)
            {
                editingField = 0;
                currentEvent.Use();
                return;
            }
            
            // Handle character input (only process printable characters)
            char c = currentEvent.character;
            if (char.IsControl(c)) return;
            
            if (editingField == 1)
            {
                // IP field - allow numbers, dots, and colons (for IPv6 support)
                if (char.IsDigit(c) || c == '.' || c == ':')
                {
                    hostIP += c;
                    currentEvent.Use();
                }
            }
            else if (editingField == 2)
            {
                // Port field - only numbers
                if (char.IsDigit(c))
                {
                    portString += c;
                    currentEvent.Use();
                }
            }
        }
        
        public void Cleanup()
        {
            if (networkManager != null)
            {
                if (onPlayerConnectedHandler != null)
                    networkManager.OnPlayerConnected -= onPlayerConnectedHandler;
                if (onPlayerDisconnectedHandler != null)
                    networkManager.OnPlayerDisconnected -= onPlayerDisconnectedHandler;
            }
        }
    }
}

