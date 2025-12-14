using System;
using System.Net;
using HPMultiplayer.Server;

namespace HPMultiplayer.Server
{
    class Program
    {
        private static ServerNetworkManager server;
        private static bool running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("HP Multiplayer Dedicated Server");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Parse command line arguments
            int port = 7777;
            int maxPlayers = 16;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-port" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int p))
                        port = p;
                    i++;
                }
                else if (args[i] == "-maxplayers" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int m))
                        maxPlayers = m;
                    i++;
                }
                else if (args[i] == "-help" || args[i] == "-h")
                {
                    ShowHelp();
                    return;
                }
            }

            Console.WriteLine($"Server Configuration:");
            Console.WriteLine($"  Port: {port}");
            Console.WriteLine($"  Max Players: {maxPlayers}");
            Console.WriteLine();

            // Start server
            server = new ServerNetworkManager(port, maxPlayers);
            
            if (!server.Start())
            {
                Console.WriteLine("Failed to start server! Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Server started successfully on port {port}");
            Console.WriteLine("Type 'help' for commands, 'quit' to stop the server");
            Console.WriteLine();

            // Handle console commands
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                running = false;
            };

            while (running)
            {
                string command = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(command))
                    continue;

                if (command == "quit" || command == "exit" || command == "stop")
                {
                    running = false;
                    break;
                }
                else if (command == "help")
                {
                    ShowCommands();
                }
                else if (command == "status" || command == "info")
                {
                    ShowStatus();
                }
                else if (command == "players" || command == "list")
                {
                    server.ListPlayers();
                }
                else
                {
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                }
            }

            // Shutdown
            Console.WriteLine("\nShutting down server...");
            server.Stop();
            Console.WriteLine("Server stopped. Press any key to exit...");
            Console.ReadKey();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: HPMultiplayer.Server.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -port <number>       Server port (default: 7777)");
            Console.WriteLine("  -maxplayers <number> Maximum players (default: 16)");
            Console.WriteLine("  -help, -h            Show this help message");
            Console.WriteLine();
        }

        private static void ShowCommands()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help, status, players, quit");
            Console.WriteLine();
        }

        private static void ShowStatus()
        {
            if (server != null)
            {
                Console.WriteLine($"Server Status:");
                Console.WriteLine($"  Running: {server.IsRunning}");
                Console.WriteLine($"  Players: {server.ConnectedPlayerCount} / {server.MaxPlayers}");
                Console.WriteLine($"  Port: {server.Port}");
                Console.WriteLine();
            }
        }
    }
}

