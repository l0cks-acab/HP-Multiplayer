using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using HPMultiplayer.Networking;
using HPMultiplayer.UI;
using HPMultiplayer.Synchronization;

namespace HPMultiplayer
{
    public class HPMultiplayerMod : MelonMod
    {
        private NetworkManager networkManager;
        private MasterServerClient masterServerClient;
        private MultiplayerUI multiplayerUI;
        private ServerBrowserUI serverBrowserUI;
        private GameStateSync gameStateSync;
        private bool uiVisible = false;
        private bool browserVisible = false;
        private string lastSceneName = "";

        [System.Obsolete]
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("HP Multiplayer mod loaded!");
            MelonLogger.Msg("Press M to toggle multiplayer UI");
            MelonLogger.Msg("Press B to open server browser");
            
            // Initialize network manager
            networkManager = new NetworkManager();
            
            // Initialize master server client
            masterServerClient = new MasterServerClient();
            
            // Initialize UI
            multiplayerUI = new MultiplayerUI();
            multiplayerUI.Initialize(networkManager, masterServerClient);
            
            // Initialize server browser
            serverBrowserUI = new ServerBrowserUI();
            serverBrowserUI.Initialize(masterServerClient, networkManager);
            
            // Initialize game state synchronization
            gameStateSync = new GameStateSync();
            // Subscribe to state changes to send over network
            gameStateSync.OnStateChanged += OnGameStateChanged;
            // Subscribe to received state changes
            networkManager.OnGameStateReceived += OnGameStateReceived;
            
            // Track scene changes
            lastSceneName = SceneManager.GetActiveScene().name;
        }
        
        private void OnGameStateChanged(byte[] stateData)
        {
            // Only send game state if we're the host (host is authoritative for NPCs/interactive objects)
            if (networkManager != null && networkManager.IsHost && networkManager.IsConnected)
            {
                networkManager.SendGameState(stateData);
            }
        }
        
        private void OnGameStateReceived(byte[] stateData)
        {
            // Apply received game state changes
            gameStateSync?.ApplyState(stateData);
        }


        public override void OnUpdate()
        {
            if (Keyboard.current != null)
            {
                // Toggle UI with M key
                if (Keyboard.current[Key.M].wasPressedThisFrame)
                {
                    ToggleUI();
                }
                
                // Toggle server browser with B key
                if (Keyboard.current[Key.B].wasPressedThisFrame)
                {
                    ToggleBrowser();
                }
            }

            // Check for scene changes
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName != lastSceneName)
            {
                OnSceneChanged(currentSceneName);
                lastSceneName = currentSceneName;
            }

            // Update network manager (handles networking and remote player updates)
            networkManager?.Update();
            
            // Update server browser
            serverBrowserUI?.Update();
            
            // Update game state synchronization
            if (networkManager != null && networkManager.IsConnected)
            {
                gameStateSync?.Update();
                
                // Periodically scan for and register new NPCs/interactive objects
                // Only host actively syncs, but clients register to receive updates
                GameObjectFinder.RegisterSyncableObjects(gameStateSync, networkManager.IsHost);
            }
        }

        public override void OnApplicationQuit()
        {
            MelonLogger.Msg("Shutting down multiplayer...");
            multiplayerUI?.Cleanup();
            masterServerClient?.Shutdown();
            networkManager?.Shutdown();
        }

        private void OnSceneChanged(string newSceneName)
        {
            MelonLogger.Msg($"Scene changed from '{lastSceneName}' to '{newSceneName}'");
            
            // Handle scene change for network players
            if (networkManager != null && networkManager.IsConnected)
            {
                networkManager.OnSceneChanged();
            }
        }
        
        public override void OnGUI()
        {
            multiplayerUI?.OnGUI();
            serverBrowserUI?.OnGUI();
            
            // Draw nametags for remote players
            if (networkManager != null && networkManager.IsConnected)
            {
                DrawPlayerNameTags();
            }
        }
        
        private void DrawPlayerNameTags()
        {
            if (networkManager == null) return;
            
            // Get all remote players and draw their nametags
            var remotePlayers = networkManager.GetRemotePlayers();
            foreach (var player in remotePlayers)
            {
                if (player.IsVisibleOnScreen())
                {
                    Vector3 screenPos = player.GetScreenPosition();
                    
                    // Only draw if in front of camera (z > 0)
                    if (screenPos.z > 0)
                    {
                        string playerName = $"Player {player.PlayerId}";
                        Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(playerName));
                        
                        // Convert to GUI coordinates (y is inverted)
                        float guiX = screenPos.x;
                        float guiY = Screen.height - screenPos.y;
                        
                        // Center the label
                        Rect labelRect = new Rect(
                            guiX - labelSize.x / 2,
                            guiY - labelSize.y - 10, // Offset above player
                            labelSize.x,
                            labelSize.y
                        );
                        
                        // Draw shadow for better visibility
                        GUI.color = Color.black;
                        GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), playerName);
                        
                        // Draw text
                        GUI.color = Color.white;
                        GUI.Label(labelRect, playerName);
                    }
                }
            }
            
            GUI.color = Color.white; // Reset color
        }

        private void ToggleUI()
        {
            uiVisible = !uiVisible;
            if (multiplayerUI != null)
            {
                multiplayerUI.SetVisible(uiVisible);
            }
            if (uiVisible)
            {
                browserVisible = false;
                serverBrowserUI?.SetVisible(false);
            }
            MelonLogger.Msg($"Multiplayer UI: {(uiVisible ? "shown" : "hidden")}");
        }

        private void ToggleBrowser()
        {
            browserVisible = !browserVisible;
            if (serverBrowserUI != null)
            {
                serverBrowserUI.SetVisible(browserVisible);
            }
            if (browserVisible)
            {
                uiVisible = false;
                multiplayerUI?.SetVisible(false);
            }
            MelonLogger.Msg($"Server Browser: {(browserVisible ? "shown" : "hidden")}");
        }
    }
}

