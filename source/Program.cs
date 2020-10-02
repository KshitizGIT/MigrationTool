using log4net;
using log4net.Config;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MigrationTool
{
    class Program
    {

        static void Main(string[] args)
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(assemblyLocation)
                .AddJsonFile("appsettings.json");

            Environment.Configuration = builder.Build();
            // Handle commandline arguments
            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");
            var optionRecursive = app.Option("-r|--recursive ", "Searches directory recursively. Use -r at the beginning.", CommandOptionType.NoValue);
            var optionDirectory = app.Option("-d|--directory <DIRECTORY>", "The directory containing .sql files", CommandOptionType.SingleValue);
            var optionFile = app.Option("-f|--file <FILE>", "The .sql file", CommandOptionType.SingleValue);
            var optionOutputPath = app.Option("-o|--output <OUTPUT>", "The output path to create reports. By default creates the report in the tools executed path.", CommandOptionType.SingleValue);
            var optionInteractiveMode = app.Option("-i|--interactive <INTERACTIVE>", "Runs the app in interactive mode. Default: true", CommandOptionType.SingleValue); 

            app.OnExecute(() =>
            {
                bool isValid = true;
                char mode = 'd';

                string sqlFileDirectory = string.Empty;
                if (!optionDirectory.HasValue() && !optionFile.HasValue())
                {
                    isValid = false;
                    app.Error.Write("Either file or directory needs to be specified");
                    app.ShowHelp();
                }
                // Both file and directory flag present
                else if (optionDirectory.HasValue() && optionFile.HasValue()) {
                    isValid = false;
                    app.Error.Write("Can only process either file or directory mode.");
                    app.ShowHelp();
                }
                // path to read sql files from
                else
                {
                    //Check if directory exists
                    if (optionDirectory.HasValue())
                    {
                        sqlFileDirectory = optionDirectory.Value().TrimEnd('\"');
                        sqlFileDirectory = Path.GetFullPath(sqlFileDirectory);
                        if (!Directory.Exists(Path.GetFullPath(sqlFileDirectory)))
                        {
                            isValid = false;
                            app.Error.Write("The specified directory doesnot exists. Please provide a existing directory path.");
                        }
                        mode = 'd';
                        if (optionRecursive.HasValue())
                            mode = 'r';
                    }
                    if (optionFile.HasValue()) {
                        sqlFileDirectory = optionFile.Value().TrimEnd('\"');
                        sqlFileDirectory = Path.GetFullPath(sqlFileDirectory);
                        if (!File.Exists(Path.GetFullPath(sqlFileDirectory)))
                        {
                            isValid = false;
                            app.Error.Write("The specified file doesnot exist.");
                        }
                        mode = 'f';
                    }
                }
                if (isValid)
                {
                    //path to create report in
                    var outputFilePath = optionOutputPath.HasValue() ? optionOutputPath.Value() : "." ;
                    var isInteractive = optionInteractiveMode.HasValue() ? Boolean.TrueString.ToLower() == optionInteractiveMode.Value() : true;

                    //Read log4net configs
                    var assembly = Assembly.GetEntryAssembly();
                    var repository = LogManager.GetRepository(assembly);
                    XmlConfigurator.Configure(repository, new FileInfo(Path.Combine(Path.GetDirectoryName(assembly.Location), "log4net.config")));

                    //Begin migration
                    var migrator = new Migrator(Path.GetFullPath(sqlFileDirectory), mode);
                    migrator.ReportPath = Path.GetFullPath(outputFilePath);

                    migrator.RunAsync().Wait();

                    if (isInteractive)
                    {
                        Console.WriteLine("Do you want to view the migration report? Y/N");
                        var answer = Console.ReadKey();
                        if (answer.Key == ConsoleKey.Y)
                        {
                            OpenBrowser(migrator.ReportFileName);
                        }
                    }
                }
                return 0;
            });

            app.Execute(args);
        }
        private static void OpenBrowser(string link)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/C {link}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", link);  // Works ok on linux
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Dont have mac. Untested
            }

        }

    }
}
