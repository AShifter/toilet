using System.Reflection;
using System.Resources;
using System.Text;

namespace toilet.Web
{
    public class PageBuilder
    {
        private StringBuilder _htmlBuilder = new();
        public string GetDirectoryListing(string path)
        {
            _htmlBuilder.Clear();
            _htmlBuilder.Append(TemplateResources.HtmlTemplate);
            _htmlBuilder.Replace("<!-- {{InsertStyles}} -->", TemplateResources.StyleTemplate);
            _htmlBuilder.Replace("<!-- {{InsertDirectoryList}} -->", QueryFolder(path));
            _htmlBuilder.Replace("<!-- {{InsertCurrentPath}} -->", path.Substring(1, path.Length - 1));
            _htmlBuilder.Replace("<!-- {{InsertHost}} -->", HttpServer.listener.Prefixes.First());

            return _htmlBuilder.ToString();
        }
        
        public string GetErrorPage(string path, string error, Exception e)
        {
            _htmlBuilder.Clear();
            _htmlBuilder.Append(TemplateResources.NotFoundTemplate);
            _htmlBuilder.Replace("<!-- {{InsertStyles}} -->", TemplateResources.StyleTemplate);
            _htmlBuilder.Replace("<!-- {{InsertException}} -->", e.ToString());
            _htmlBuilder.Replace("<!-- {{InsertCurrentPath}} -->", path.Substring(1, path.Length - 1));
            _htmlBuilder.Replace("<!-- {{InsertHost}} -->", HttpServer.listener.Prefixes.First());
            _htmlBuilder.Replace("<!-- {{InsertError}} -->", error);
                
            

            return _htmlBuilder.ToString();
        }

        private string QueryFolder(string path)
        {
            StringBuilder html = new();

            string[] directories = Directory.GetDirectories(path);
            Array.Sort(directories, StringComparer.CurrentCultureIgnoreCase);

            foreach (string child in directories)
            {
                string dir = child.Substring(1, child.Length - 1);
                html.Append($"<tbody title=\"{dir}\">" +
                                $"<tr>" +
                                    $"<th style=\"max-width: 1px;\" scope=\"row\">" +
                                        $"<a href=\"{dir}/\">{dir.Split('/').Last()}/\n</a>" +
                                    $"</th>" +
                                    $"<td style=\"width: 15%;\" class=\"text-muted\">{Directory.GetLastWriteTime(child).ToString("yyyy-MM-dd HH:mm")}</td>" +
                                    $"<td style=\"width: 10%;\" class=\"text-muted\">-</td>" +
                                $"</tr>" +
                            $"</tbody>");
            }
            
            string[] files = Directory.GetFiles(path);
            Array.Sort(files, StringComparer.CurrentCultureIgnoreCase);
            
            foreach (string child in files)
            {
                string file = child.Substring(1, child.Length - 1);
                if(file == "/favicon.ico") { continue; }
                html.Append($"<tbody title=\"{file}\">" +
                                $"<tr>" +
                                    $"<th style=\"max-width: 1px;\" scope=\"row\">" +
                                        $"<a href=\"{file}\"><span style=\"display: block; overflow: hidden; white-space: nowrap; text-overflow: ellipsis\">{file.Split('/').Last()}</span></a>" +
                                    $"</th>" +
                                    $"<td style=\"width: 15%;\" class=\"text-muted\">{File.GetLastWriteTime(child).ToString("yyyy-MM-dd HH:mm")}</td>" +
                                    $"<td style=\"width: 10%;\" class=\"text-muted\">{GetFormattedFileSize(new FileInfo(child).Length)}</td>" +
                                $"</tr>" +
                            $"</tbody>");
            }

            return html.ToString();
        }

        private string GetFormattedFileSize(long fileLength)
        {
            if (fileLength / (double)1000000000000 > 1)
            {
                return $"{Math.Round((double)fileLength / 1000000000000, 1)} TB";
            }
            
            if (fileLength / (double)1000000000 > 1)
            {
                return $"{Math.Round((double)fileLength / 1000000000, 1)} GB";
            }
            
            if (fileLength / (double)1000000 > 1)
            {
                return $"{Math.Round((double)fileLength / 1000000, 1)} MB";  
            }
            
            if (fileLength / (double)1000 > 1)
            {
                return $"{Math.Round((double)fileLength / 1000, 0)} KB";
            }
            
            return $"{fileLength} B";
        }
    }

    public static class TemplateResources
    {
        public static string HtmlTemplate = GetEmbeddedResource("toilet.Web.Page", "index.html");
        public static string NotFoundTemplate = GetEmbeddedResource("toilet.Web.Page", "error.html");
        public static string StyleTemplate = GetEmbeddedResource("toilet.Web.Page", "styles.css");
        public static string GetEmbeddedResource(string namespacename, string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = namespacename + "." + filename;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}