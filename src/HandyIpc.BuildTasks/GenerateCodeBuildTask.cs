using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nustache.Core;

namespace HandyIpc.BuildTasks
{
    public class GenerateCodeBuildTask : Task
    {
        private const string ClientTemplateFileName = @"ClientTemplate.mustache";
        private const string DispatcherTemplateFileName = @"DispatcherTemplate.mustache";
        private const string ServerProxiesTemplateFileName = @"ServerProxyTemplate.mustache";

        private const string ClientsFileName = "HandyIpcClients.g.cs";
        private const string DispatchersFileName = "HandyIpcDispatchers.g.cs";
        private const string ServerProxiesFileName = "HandyIpcServerProxies.g.cs";

        private static readonly string DllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        [Output]
        public string ClientsCodePath { get; set; }

        [Output]
        public string DispatchersCodePath { get; set; }

        [Output]
        public string ServerProxiesCodePath { get; set; }

        public override bool Execute()
        {
#if DEBUG
            //Debugger.Launch();
#endif

            var fileNames = SourceFiles.Select(item => item.ItemSpec).ToList();
            var data = CodeGenerator.GetTemplateData(fileNames);

            ClientsCodePath = GenerateCode(ClientsFileName, ClientTemplateFileName, data);
            DispatchersCodePath = GenerateCode(DispatchersFileName, DispatcherTemplateFileName, data);
            ServerProxiesCodePath = GenerateCode(ServerProxiesFileName, ServerProxiesTemplateFileName, data);

            return true;
        }

        private string GenerateCode(string outputFileName, string templateFileName, object data)
        {
            var outputPath = Path.Combine(IntermediateOutputPath, outputFileName);
            outputPath = Path.GetFullPath(outputPath);
            var templatePath = Path.Combine(DllDirectory, templateFileName);

            Encoders.HtmlEncode = _ => _;
            var code = Render.FileToString(templatePath, data);
            File.WriteAllText(outputPath, code);

            return outputPath;
        }
    }
}
