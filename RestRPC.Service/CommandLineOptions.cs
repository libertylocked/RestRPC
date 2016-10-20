using System;
using CommandLine;
using CommandLine.Text;

namespace RestRPC.Service
{
    class CommandLineOptions
    {
        [Option('s', "server", Required = true, HelpText = "URI of specified RestRPC server")]
        public string ServerUriString { get; set; }

        [Option('n', "name", Required = false, HelpText = "Name of this component")]
        public string Name { get; set; }

        [Option('u', "username", Required = true, HelpText = "Username for HTTP auth")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password for HTTP auth")]
        public string Password { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) =>
                HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
