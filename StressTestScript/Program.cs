using System;
using System.IO;
using System.Net;
using System.Text;

namespace StressTestScript {
    enum HttpMethod {GET, POST, PATCH};
    class Program {
        static string AgentName = "TesttingScript";
        static void Main(string[] args) {
            
        }

        private HttpWebResponse HttpRequest(HttpMethod method, string url, string json) {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method.ToString();
            request.UserAgent = AgentName;
            request.ContentType = "application/json";

            byte[] data = Encoding.UTF8.GetBytes(json);
            using var streamWriter = new StreamWriter(request.GetRequestStream());
            streamWriter.Write(data);
            return request.GetResponse() as HttpWebResponse;
        }
    }
}
