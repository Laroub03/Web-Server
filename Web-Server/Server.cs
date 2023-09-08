using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Web_Server;

namespace WebServer
{
    class WebServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly int maxNumberOfConnections;
        private readonly string contentPath;
        private bool isRunning;
        private readonly Dictionary<string, string> extensions;

        public WebServer(IPAddress ipAddress, int port, int maxNumberOfConnections, string contentPath)
        {
            // Constructor to initialize server settings and configurations
            this.ipAddress = ipAddress;
            this.port = port;
            this.maxNumberOfConnections = maxNumberOfConnections;
            this.contentPath = contentPath;
            this.isRunning = false;

            // Define file extensions and their corresponding content types
            this.extensions = new Dictionary<string, string>()
            {
                { "htm", "text/html" },
                { "html", "text/html" },
                { "xml", "text/xml" },
                { "txt", "text/plain" },
                { "css", "text/css" },
                { "png", "image/png" },
                { "gif", "image/gif" },
                { "jpg", "image/jpg" },
                { "jpeg", "image/jpeg" },
                { "zip", "application/zip" }
            };
        }

        public void Start()
        {
            if (isRunning) return;

            try
            {
                // Create a server socket and start listening for incoming connections
                var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxNumberOfConnections);

                isRunning = true;

                while (isRunning)
                {
                    // Accept incoming client connections and handle them in parallel
                    var clientSocket = serverSocket.Accept();
                    Task.Factory.StartNew(() => HandleRequest(clientSocket));
                }
            }
            catch
            {
                isRunning = false;
            }
        }

        public void Stop()
        {
            // Stop the server by setting the flag to false
            isRunning = false;
        }

        private void HandleRequest(Socket clientSocket)
        {
            using (var clientHandler = new ClientHandler(clientSocket, contentPath, extensions))
            {
                clientHandler.HandleRequest();
            }
        }
    }
}