using System.Text;

namespace toilet.Web
{
    public class PageBuilder
    {
        private StringBuilder _htmlBuilder = new();
        public string GetDirectoryListing(string path)
        {
            // This entire file is absolutely disgusting
            string displayPath = path.Substring(1, path.Length - 1);
            _htmlBuilder.Clear();
            _htmlBuilder.Append($"<!DOCTYPE html><html style=\"height: 100vh;\"><head><title>Index of {displayPath} - toilet</title><link rel=\"stylesheet\" href=\"https://bootswatch.com/5/flatly/bootstrap.min.css\"></head><body style=\"height: 100%; background-attachment: fixed;\" id=\"drop_zone\" ondrop=\"dropHandler(event);\" ondragover=\"dragOverHandler(event);\">");
            _htmlBuilder.Append($"<nav class=\"navbar navbar-expand-lg navbar-dark bg-primary\"><p class=\"text-white\" style=\"margin: auto; text-align: center;\">Index of {displayPath} - toilet</h1></nav><div class=\"container\" style=\"padding:20px;\">");
            _htmlBuilder.Append($"<div class=\"card border-primary\"><table style=\"margin-bottom: 0;\" class=\"table table-hover\">");
            _htmlBuilder.Append("<thead><tr class=\"card-header\"><th scope=\"col\">Name</th><th scope=\"col\">Last Modified</th><th scope=\"col\">Size</th></tr></thead>");
            _htmlBuilder.Append($"<tbody><tr><th scope=\"row\"><a href=\"../\">../\n</a></th><td class=\"text-muted\">-</td><td class=\"text-muted\">-</td></tr>");

            string[] directories = Directory.GetDirectories(path);
            Array.Sort(directories, StringComparer.CurrentCultureIgnoreCase);

            foreach (string child in directories)
            {
                string dir = child.Substring(1, child.Length - 1);
                _htmlBuilder.Append($"<tbody><tr><th scope=\"row\"><a href=\"{dir}/\">{dir.Split('/').Last()}/\n</a></th><td class=\"text-muted\">{Directory.GetLastWriteTime(child).ToString("yyyy-MM-dd HH:mm")}</td><td class=\"text-muted\">-</td></tr>");
            }
            
            string[] files = Directory.GetFiles(path);
            Array.Sort(files, StringComparer.CurrentCultureIgnoreCase);
            
            foreach (string child in files)
            {
                string file = child.Substring(1, child.Length - 1);
                _htmlBuilder.Append($"<tbody><tr><th scope=\"row\"><a href=\"{file}\">{file.Split('/').Last()}\n</a></th><td class=\"text-muted\">{File.GetLastWriteTime($"{path}/{file}").ToString("yyyy-MM-dd HH:mm")}</td><td class=\"text-muted\">{GetFileSize(new FileInfo(child).Length)}</td></tr>");
            }

            _htmlBuilder.Append($"</tbody></table><div class=\"card-footer\" style=\"display: flex; border-top: 0; padding-top: 20px;\">"+
                                $"<div style=\"flex: 1; padding-right: 8px;\"><form class=\"input-group mb-3\" method=\"POST\" action=\"{displayPath}\" enctype=\"multipart/form-data\"><input class=\"form-control\" type=\"file\" name=\"fileUpload\"><input class=\"btn btn-primary\" type=\"submit\" value=\"Upload File\"></form></div>" +
                                $"<div style=\"flex: 1; text-align: right; padding-left: 8px;\"><form class=\"input-group mb-3\" method=\"GET\" action=\"{displayPath}\"><input class=\"form-control\" type=\"text\" name=\"newFolderDir\" placeholder=\"Folder name\"><input class=\"btn btn-primary\" type=\"submit\" Value=\"New Folder\"></form></div>" +
                                $"</div></div><hr><p class=\"text-muted\" style=\"margin: auto; text-align: center;\">toilet server at {HttpServer.listener.Prefixes.First()}</p><hr></div></body></html>");

            return _htmlBuilder.ToString();
        }

        private string GetFileSize(long fileLength)
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
}