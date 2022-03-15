using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ui/{p1?}/{p2?}/{p3?}")] HttpRequest req, ExecutionContext context)
        {
            var TrimString = "octo-onboarding";
            string functionRoot = context.FunctionAppDirectory;
            var root = functionRoot.Substring(0, functionRoot.LastIndexOf(TrimString) + 1 + TrimString.Length);
            return new FileStreamResult(File.OpenRead($"{root}/build/index.html"), "text/html; charset=UTF-8");
        }

        private static readonly string[][] FileMap = new string[][]
        {
            new [] { "static/css/", "text/css; charset=utf-8"},
            new [] { "static/media/", "image/svg+xml; charset=UTF-8"},
            new [] { "static/js/", "application/javascript; charset=UTF-8"},
            new [] { "manifest.json", "application/json; charset=UTF-8"},
            new [] { "service-worker.js", "application/javascript; charset=UTF-8"},
            new [] { "favicon.ico", "image/x-icon"}
        };


        [FunctionName("ServeStaticFile")]
        public static IActionResult ServeStaticFile(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ExecutionContext context)
        {
            var TrimString = "octo-onboarding";
            string functionRoot = context.FunctionAppDirectory;
            var root = functionRoot.Substring(0, functionRoot.LastIndexOf(TrimString) + 1 + TrimString.Length);
            var file = req.Query["file"].ToString();

            var contentType = FileMap.FirstOrDefault((kv => file.StartsWith(kv[0])));

            var filePath = $"{root}/build/{file}";

            return File.Exists(filePath) ?
                (IActionResult)new FileStreamResult(File.OpenRead(filePath), contentType[1]) :
                new NotFoundResult();
        }
    }
}
