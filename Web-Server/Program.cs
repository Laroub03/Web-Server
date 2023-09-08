using System;
using System.Net;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string contentPath = "C:\\Users\\Administrator\\source\\repos\\Web-Server\\Web-Server\\HelloWorld";
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 80;
            int maxConnections = 10;

            try
            {
                // Create a new server instance.
                var server = new WebServer(ipAddress, port, maxConnections, contentPath);

                // Start the server.
                server.Start();

                Console.WriteLine("Server started successfully.");

                // Keep the console application running.
                Console.WriteLine("Press Enter to stop the server...");
                Console.ReadLine();

                // Stop the server when done.
                server.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
