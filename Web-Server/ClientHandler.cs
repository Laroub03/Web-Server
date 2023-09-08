using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class ClientHandler : IDisposable
    {
        private readonly Socket clientSocket;
        private readonly string contentPath;
        private readonly Dictionary<string, string> extensions;

        public ClientHandler(Socket clientSocket, string contentPath, Dictionary<string, string> extensions)
        {
            // Constructor to initialize client handler with socket and configurations
            this.clientSocket = clientSocket;
            this.contentPath = contentPath;
            this.extensions = extensions;
        }

        public void HandleRequest()
        {
            try
            {
                // Create a buffer to receive the incoming HTTP request
                byte[] buffer = new byte[10240]; // 10 kb, just in case
                int receivedBCount = clientSocket.Receive(buffer); // Receive the request
                string strReceived = Encoding.UTF8.GetString(buffer, 0, receivedBCount);

                // Parse the HTTP method (e.g., GET, POST) from the request
                string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

                // Find the start and length of the requested URL
                int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
                int length = strReceived.LastIndexOf("HTTP") - start - 1;
                string requestedUrl = strReceived.Substring(start, length);

                // Extract the requested file path from the URL
                string requestedFile;
                if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                    requestedFile = requestedUrl.Split('?')[0];
                else
                {
                    // Respond with a "501 Not Implemented" error for unsupported HTTP methods
                    NotSupported();
                    return;
                }

                // Normalize the file path and remove potential directory traversal attempts
                requestedFile = requestedFile.Replace("/", "\\").Replace("\\..", "");

                // Find the file extension in the requested file path
                start = requestedFile.LastIndexOf('.') + 1;

                if (start > 0)
                {
                    length = requestedFile.Length - start;
                    string extension = requestedFile.Substring(start, length);

                    // Check if the extension is recognized and if the file exists
                    if (extensions.ContainsKey(extension) && File.Exists(contentPath + requestedFile))
                    {
                        // Respond with a "200 OK" status and send the requested file content
                        SendOkResponse(File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    }
                    else
                    {
                        // Respond with a "404 Not Found" error if the file is not found or the extension is not recognized
                        NotFound();
                    }
                }
                else
                {
                    // Handle cases where the requested URL ends with a directory (e.g., "/path/") by attempting to serve an index file
                    if (requestedFile.Substring(length - 1, 1) != "\\")
                        requestedFile += "\\";
                    if (File.Exists(contentPath + requestedFile + "index.htm"))
                    {
                        // Respond with a "200 OK" status and send the index.htm file content
                        SendOkResponse(File.ReadAllBytes(contentPath + requestedFile + "\\index.htm"), "text/html");
                    }
                    else if (File.Exists(contentPath + requestedFile + "index.html"))
                    {
                        // Respond with a "200 OK" status and send the index.html file content
                        SendOkResponse(File.ReadAllBytes(contentPath + requestedFile + "\\index.html"), "text/html");
                    }
                    else
                    {
                        // Respond with a "404 Not Found" error if no index file is found
                        NotFound();
                    }
                }
            }
            catch
            {
                // Close the client socket in case of an error
                CloseClientSocket();
            }
        }


        private void NotSupported()
        {
            // Respond with a "501 Not Implemented" error
            SendResponse("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>Atasoy Simple Web Server</h2><div>501 - Method Not Implemented</div></body></html>", "501 Not Implemented", "text/html");
        }

        private void NotFound()
        {
            // Respond with a "404 Not Found" error
            SendResponse("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>Atasoy Simple Web Server</h2><div>404 - Not Found</div></body></html>", "404 Not Found", "text/html");
        }

        private void SendOkResponse(byte[] content, string contentType)
        {
            // Respond with a "200 OK" status and the requested content
            SendResponse(content, "200 OK", contentType);
        }

        private void SendResponse(string strContent, string responseCode, string contentType)
        {
            // Send an HTML response with the specified status code and content
            byte[] contentBytes = Encoding.UTF8.GetBytes(strContent);
            SendResponse(contentBytes, responseCode, contentType);
        }

        private void SendResponse(byte[] contentBytes, string responseCode, string contentType)
        {
            try
            {
                // Construct and send the HTTP response header followed by the content
                byte[] headerBytes = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 " + responseCode + "\r\n" +
                    "Server: Atasoy Simple Web Server\r\n" +
                    "Content-Length: " + contentBytes.Length.ToString() + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-Type: " + contentType + "\r\n\r\n");

                clientSocket.Send(headerBytes);
                clientSocket.Send(contentBytes);
            }
            catch
            {
                CloseClientSocket();
            }
            finally
            {
                CloseClientSocket();
            }
        }

        private void CloseClientSocket()
        {
            try
            {
                // Close the client socket
                clientSocket.Close();
            }
            catch { }
        }

        public void Dispose()
        {
            CloseClientSocket();
        }
    }
}
