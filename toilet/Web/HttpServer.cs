using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Web;

namespace toilet.Web
{
    public class HttpServer
    {
        public static HttpListener listener;

        public PageBuilder pageBuilder = new();
        
        public int reqCounter = 0;

        public void StartServer(string ip, int port)
        {
            // Create a Http server and start listening for incoming connections
            listener = new();
            listener.Prefixes.Add($"http://{ip}:{port.ToString()}/");
            listener.Start();
            Console.WriteLine($"listening on http://{ip}:{port.ToString()}/");

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
        
        public async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine();
                Console.WriteLine($"HTTP {req.HttpMethod} Request from {req.RemoteEndPoint} at {req.Url}");
                Console.WriteLine(req.UserAgent);

                byte[] data;
                string fsPath = HttpUtility.UrlDecode($".{req.Url.AbsolutePath}");
                
                // Check to see if new folder parameter was provided in request URL
                if (req.Url.Query != "" && req.Url.PathAndQuery.Split("?")[1].Contains("newFolderDir"))
                {
                    Directory.CreateDirectory($"{fsPath}/{HttpUtility.UrlDecode(req.Url.PathAndQuery.Split("=")[1])}");
                }

                // Check to see if request is file upload POST                
                if (req.HttpMethod == "POST")
                {
                    try
                    {
                        Task fileUploadTask = ParseFiles(req.InputStream, fsPath);
                        fileUploadTask.GetAwaiter().GetResult();
                    }
                    catch(Exception e)
                    {
                        data = Encoding.UTF8.GetBytes(pageBuilder.GetErrorPage(fsPath, "400 Bad Request", e));
                    
                        resp.ContentType = "text/html";
                        resp.StatusCode = 400;
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                    
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                        continue;
                    }
                }

                // Write the response info
                try
                {
                    if (File.Exists(fsPath))
                    {
                        resp.StatusCode = 200;
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = new FileInfo(fsPath).Length;
                        
                        // if the URI points to a file on disk, read that file.
                        using(Stream source = File.OpenRead(fsPath))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await resp.OutputStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                        
                        resp.Close();
                        //data = File.ReadAllBytes(fsPath);
                    }
                    else
                    {
                        // if not, generate a directory listing
                        data = Encoding.UTF8.GetBytes(pageBuilder.GetDirectoryListing(fsPath));
                    }
                }
                catch(Exception e)
                {
                    data = Encoding.UTF8.GetBytes(pageBuilder.GetErrorPage(fsPath, "404 Not Found", e));
                    
                    resp.ContentType = "text/html";
                    resp.StatusCode = 404;
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                    continue;
                }
            }
        }
        
        public async Task ParseFiles(Stream data, string fsPath)
        {
            // Read the request stream into memory
            MemoryStream memoryStream = new MemoryStream();
            data.CopyTo(memoryStream);
            
            byte[] buffer = memoryStream.ToArray();

            // Get filename
            string fileName = Encoding.UTF8.GetString(buffer).Split("\r\n")[1].Split("filename=\"")[1].Replace("\"", "");

            if (fileName == string.Empty)
            {
                Console.WriteLine("File name empty, nothing to do");
                return;
            }

            // Strip extra parts of the request to get binary data
            string stringBuffer = Encoding.UTF8.GetString(buffer);
            List<byte> bytes = new List<byte>(buffer);
            string[] splitString = stringBuffer.Split('\n');
            int lengthOfFourLines = splitString[0].Length + splitString[1].Length + splitString[2].Length + splitString[3].Length + 4;
            bytes.RemoveRange(0, lengthOfFourLines);
            int lengthOfLastLine = splitString[^2].Length+2;
            bytes.RemoveRange(bytes.Count - lengthOfLastLine, lengthOfLastLine);
            bytes.RemoveAt(bytes.Count - 1);
            buffer = bytes.ToArray();
            
            // Write data to file
            Console.WriteLine($"Writing file to {fsPath}/{fileName}");

            File.WriteAllBytes($"{fsPath}/{fileName}", buffer);
            
            /*
            using (FileStream stream = new FileStream($"{fsPath}/{fileName}", FileMode.Append)) 
            {
                stream.Write(buffer, 0, buffer.Length);           
            }
            */
        }
    }
}