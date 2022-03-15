using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HostingFunctionsApp
{
    public static class ServeStaticFiles
    {

        [FunctionName("status")]
        public static string Status(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req, ExecutionContext context)
        {
            string root = context.FunctionAppDirectory;
            var json = File.ReadAllText($"{root}/SomeSettings.json");
            dynamic jObject = JObject.Parse(json);
            var version = jObject.version;
            return $"live-{version}";
        }

        // Serves static files for client UI
        [FunctionName("ui")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ui/{p1?}/{p2?}/{p3?}")] HttpRequest req,
            string p1,
            string p2,
            string p3,
            ExecutionContext context
        )
        {
            var TrimString = "octo-onboarding";
            string functionRoot = context.FunctionAppDirectory;
            var root = functionRoot.Substring(0, functionRoot.LastIndexOf(TrimString) + 1 + TrimString.Length);

            // Sanitizing input, just in case
            string path = $"{Path.GetFileName(p1)}";
            path += p2 != null ? $"/{Path.GetFileName(p2)}" : "";
            path += p3 != null ? $"/{Path.GetFileName(p3)}" : "";

            var contentType = FileMap.FirstOrDefault((kv => path.StartsWith(kv[0])));
            if (contentType != null)
            {
                return File.Exists(root + path) ?
                    (IActionResult)new FileStreamResult(File.OpenRead(root + path), contentType[1]) :
                    new NotFoundResult();
            }
            // Returning index.html by default, to support client routing
            var test = File.ReadAllText($"{root}/build/index.html");
            return new FileStreamResult(File.OpenRead($"{root}/build/index.html"), "text/html; charset=UTF-8");
        }

        private static readonly string[][] FileMap = new string[][]
        {
        new [] { "build/static/css/", "text/css; charset=utf-8"},
        new [] { "build/static/media/", "image/svg+xml; charset=UTF-8"},
        new [] { "build/static/js/", "application/javascript; charset=UTF-8"},
        new [] { "build/manifest.json", "application/json; charset=UTF-8"},
        new [] { "build/service-worker.js", "application/javascript; charset=UTF-8"},
        new [] { "build/favicon.ico", "image/x-icon"}
        };
    }
}
