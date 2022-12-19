using System.Text;

namespace toilet.Web
{
    public class PageBuilder
    {
        private StringBuilder _htmlBuilder = new();
        public string GetDirectoryListing(string path)
        {
            string displayPath = path.Substring(1, path.Length - 1);
            _htmlBuilder.Clear();
            _htmlBuilder.Append($"<!DOCTYPE html><html style=\"height: 90%; padding: 8px;\"<head><title>Index of {displayPath}</title></head><body style=\"height: 100%; margin: 0px;\" id=\"drop_zone\" ondrop=\"dropHandler(event);\" ondragover=\"dragOverHandler(event);\">");
            _htmlBuilder.Append($"<h1>Index of {displayPath}</h1><hr>");
            _htmlBuilder.Append("<pre>");
            _htmlBuilder.Append($"<a href=\"../\">../\n</a>");
        
            foreach (string child in Directory.GetDirectories(path))
            {
                string dir = child.Substring(1, child.Length - 1);
                _htmlBuilder.Append($"<a href=\"{dir}\">{dir.Split('/').Last()}/\n</a>");
            }
            foreach (string child in Directory.GetFiles(path))
            {
                string file = child.Substring(1, child.Length - 1);
                _htmlBuilder.Append($"<a href=\"{file}\">{file.Split('/').Last()}\n</a>");
            }

            _htmlBuilder.Append($"</pre><hr><i>toilet/0.0.1 server at {HttpServer.listener.Prefixes.First()}</i><hr>"+
                                $"<form method=\"POST\" action=\"{displayPath}\" enctype=\"multipart/form-data\"><input type=\"file\" name=\"fileUpload\"><input type=\"submit\" value=\"Upload File\"></form>" +
                                $"<form method=\"GET\" action=\"{displayPath}\"><input type=\"text\" name=\"newFolderDir\"><input type=\"submit\" Value=\"New Folder\"></form>" +
                                "</body></html>");

            return _htmlBuilder.ToString();
        }
    }
}