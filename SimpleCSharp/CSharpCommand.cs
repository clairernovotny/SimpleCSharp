using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

using Microsoft.Extensions.Logging;

using static SimpleCSharp.HRESULT;

namespace SimpleCSharp
{
    abstract class CSharpCommand
    {
        [Option("--colors", "Enable color output on the command line.", CommandOptionType.NoValue)]
        public bool Colors { get; set; } = false;
        [Option("-v | --verbose", "Include additional output.", CommandOptionType.NoValue)]
        public bool Verbose { get; set; }

        [Option("-q | --quiet", "Do not print any output to the console.", CommandOptionType.NoValue)]
        public bool Quiet { get; set; }

        [Argument(0, "file", "The path to the file")]
        public string? File { get; set; }

        public LogLevel LogLevel
        {
            get
            {
                if (Quiet)
                {
                    return LogLevel.Critical;
                }
                else if (Verbose)
                {
                    return LogLevel.Trace;
                }
                else
                {
                    return LogLevel.Information;
                }
            }
        }

        public ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
        {
            if(File == null)
            {
                // look at the current directory for a single .cs file
                var di = new DirectoryInfo(appContext.WorkingDirectory);
                var csFiles = di.GetFiles("*.cs");
                if(csFiles.Length > 1)
                {
                    return new ValidationResult("More than one file exists, filename must be provided.");
                }
                else if(csFiles.Length == 0)
                {
                    return new ValidationResult("No .cs files exist in the current directory.");
                }

                File = csFiles[0].FullName;
            }

            if(!System.IO.File.Exists(File))
            {
                return new ValidationResult($"File '{File}' does not exist.");
            }

            return ValidationResult.Success!;
        }

        public int OnValidationError(ValidationResult result, CommandLineApplication<BuildCommand> command, IConsole console)
        {
            console.ForegroundColor = ConsoleColor.Red;
            console.Error.WriteLine(result.ErrorMessage);
            console.ResetColor();
            command.ShowHint();
            return E_INVALIDARG;
        }

        private void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.AddConsole(console => {
                console.DisableColors = !Colors;
            });
            builder.SetMinimumLevel(LogLevel);
        }


        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            using(var loggerFactory = LoggerFactory.Create(ConfigureLogging))
            {
                var logger = loggerFactory.CreateLogger(GetType());

                string? tempDirectory = null;
                try
                { 
                    tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDirectory);
                    var fileName = Path.GetFileNameWithoutExtension(File);
                    System.IO.File.WriteAllText(Path.Combine(tempDirectory, $"{fileName}.csproj"), Resources.Template);
                    System.IO.File.Copy(File, Path.Combine(tempDirectory, $"{fileName}.cs"));                   

                }
                finally
                {
                    if (tempDirectory != null)
                    {
                        Directory.Delete(tempDirectory, true);
                    }
                }

                return await DoExecute(app, console, logger);
            }
        }


        protected abstract Task<int> DoExecute(CommandLineApplication app, IConsole console, ILogger logger);
    }
}
