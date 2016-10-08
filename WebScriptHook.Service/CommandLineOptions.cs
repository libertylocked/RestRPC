using System;
using CommandLine;
using CommandLine.Text;

namespace WebScriptHook.Service
{
    class CommandLineOptions
    {
        [Option('s', "server", Required = true, HelpText = "URI of specified WebScriptHook server")]
        public string ServerUriString { get; set; }

        [Option('n', "name", Required = false, HelpText = "Name of this component")]
        public string Name { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) =>
                HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
