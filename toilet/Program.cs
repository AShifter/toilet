using System;
using CommandLine;
using toilet.Web;

namespace toilet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string rootDir = "files";
            string ip = "127.0.0.1";
            int port = 8000;
            
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                rootDir = o.Root;

                if (o.Listen == "")
                {
                    ip = "0.0.0.0";
                }
                else
                {
                    ip = o.Listen;
                }

                port = o.Port;
            });

            Console.Write("toilet started ");

            if (args.Length > 0)
            {
                Console.Write("with args ");
                foreach (string arg in args)
                {
                    Console.Write($"{arg} ");
                }
            }

            Console.WriteLine();

            DirectoryInfo rootDirInfo = Directory.CreateDirectory(rootDir);
            Directory.SetCurrentDirectory(rootDirInfo.FullName);
            Console.WriteLine($"Set current working directory to {rootDirInfo.FullName}");
            
            HttpServer _httpServer = new HttpServer();
            _httpServer.StartServer(ip, port);
        }
        public class Options
        {
            [Option('r', "root", Required = false, HelpText = "The directory to serve. This will act as the root directory in the browser page. Default is files")]
            public string Root { get; set; } = "files";
            
            [Option('l', "listen", Required = false, HelpText = "Where to listen for incoming requests over the network. Default is 127.0.0.1, set to * to listen on all interfaces")]
            public string Listen { get; set; } = "127.0.0.1";

            [Option('p', "port", Required = false, HelpText = "The port to serve the Web UI on. Default is 8000")]
            public int Port { get; set; } = 8000;
        }
    }
}