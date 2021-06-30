using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Logging;

namespace SimpleCSharp
{
    class BuildCommand : CSharpCommand
    {
        protected override Task<int> DoExecute(CommandLineApplication app, IConsole console, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
