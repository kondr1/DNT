using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dnt.Commands;
using Dnt.Commands.Infrastructure;

using Microsoft.Build.Locator;

using NConsole;

namespace Dnt
{
    class Program
    {
        static int Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            var assembly = Assembly.GetEntryAssembly();
            ConsoleUtilities.Write("DNT (DotNetTools, https://github.com/RSuter/DNT, v" + assembly.GetName().Version + ")\n");
            ConsoleUtilities.Write("Binary: " + assembly.Location + "\n\n");
            try
            {
                // MSBuildLocator takes care of finding the default MSBuild / Visual Studio instance
                // and setting up all necessary environment variables and paths for MSBuild.
                var instanses = MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions
                {
                    DiscoveryTypes = DiscoveryType.DeveloperConsole | DiscoveryType.VisualStudioSetup | DiscoveryType.DotNetSdk
                });
                
                var iii = instanses.ToList();
                
#if NETCOREAPP3_1_OR_GREATER
                var x = BlackFox.VsWhere.VsInstances.GetAll().OrderByDescending(x => new Version(x.InstallationVersion)).First();

                var a = (VisualStudioInstance)Activator
                    .CreateInstance(
                        typeof(VisualStudioInstance),
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new object[]
                        {
                            x.DisplayName,
                            x.InstallationPath,
                            new Version(x.InstallationVersion),
                            DiscoveryType.VisualStudioSetup
                        },
                        culture: null);
                iii.Add(a);
#endif
                MSBuildLocator.RegisterInstance(iii.Find(x => x.DiscoveryType == DiscoveryType.VisualStudioSetup));
                var processor = new CommandLineProcessor(new CoreConsoleHost());
                processor.RegisterCommandsFromAssembly(typeof(CommandBase).Assembly);
                processor.Process(args);
            }
            catch (Exception e)
            {
                ConsoleUtilities.WriteError(e.ToString());
                return 1;
            }
            finally
            {
                ConsoleUtilities.WriteColor("\nElapsed time: " + stopwatch.Elapsed, ConsoleColor.DarkCyan);
            }

            return 0;
        }
    }
}