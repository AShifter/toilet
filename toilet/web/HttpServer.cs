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
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.Url.AbsolutePath);
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                byte[] data;
                
                if (req.HttpMethod == "POST")
                {
                    try
                    {
                        foreach (string acceptable in req.AcceptTypes)
                        {
                            Console.WriteLine((acceptable));
                        }

                        Task fileUploadTask = ParseFiles(req.InputStream, "multipart/form-data");
                        fileUploadTask.GetAwaiter().GetResult();
                    }
                    catch(Exception e)
                    {
                        data = Encoding.UTF8.GetBytes($"<!DOCTYPE html>400 Bad Request. Internal Exception: {e}");
                    
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
                    string fsPath = HttpUtility.UrlDecode($".{req.Url.AbsolutePath}");

                    if (File.Exists(fsPath))
                    {
                        // if the URI points to a file on disk, read that file.
                        data = File.ReadAllBytes(fsPath);
                    }
                    else
                    {
                        // if not, generate a directory listing
                        data = Encoding.UTF8.GetBytes(String.Format(pageBuilder.GetDirectoryListing(fsPath)));
                    }
                
                    resp.StatusCode = 200;
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
                catch(Exception e)
                {
                    data = Encoding.UTF8.GetBytes($"<!DOCTYPE html>404 Not Found. Internal Exception: {e}");
                    
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
        
        public static async Task ParseFiles(Stream data, string contentType)
        {
            // Read the request stream into memory
            MemoryStream memoryStream = new MemoryStream();
            data.CopyTo(memoryStream);
            byte[] buffer = memoryStream.ToArray();

            // Get filename
            string fileName = Encoding.UTF8.GetString(buffer).Split("\r\n")[1].Split("filename=\"")[1].Replace("\"", "");

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
            File.WriteAllBytes(fileName, buffer);
        } 
        
    }
}