using System;

using McMaster.Extensions.CommandLineUtils;

namespace SimpleCSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication<Program>();

            app.ShowHint();
            app.Command<BuildCommand>("build", config =>
            {
                config.Description = "Builds a file.";
                config.Conventions.UseDefaultConventions();
            });
            app.Command<BuildCommand>("run", config =>
            {
                config.Description = "Builds and runs a file.";
                config.Conventions.UseDefaultConventions();
            });
            app.Command(string.Empty, config =>
            {
                app.ShowHelp();
                app.ShowHint();
            });

            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            return app.Execute(args);
        }
    }
}
