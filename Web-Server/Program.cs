using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
           // to create new one:
            Server server = new Server();

            var ipAddress = System.Net.IPAddress.Parse("127.0.0.1");
            var port = 80;
            var maxconnections = 10;
            var contentpath = "C:\\Users\\Administrator\\source\\repos\\Web-Server\\Web-Server\\HelloWorld";

            // to start it
            server.start(ipAddress, port, maxconnections, contentpath);
        }
    }
}
