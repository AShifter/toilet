using System;
using System.Net;
using toilet.Web;

namespace toilet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"toilet started");

            Directory.SetCurrentDirectory(Directory.CreateDirectory("files").FullName);

            //Console.WriteLine(Directory.GetFiles("."));

            HttpServer _httpServer = new HttpServer();
            _httpServer.StartServer("127.0.0.1", 8000);
        }
    }
}