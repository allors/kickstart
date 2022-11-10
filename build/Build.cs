using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using System;
using Nuke.Common.Tools.Git;

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    private static bool IsProdMachine => EnvironmentInfo.MachineName.ToLower().Contains("aviation");

    private static bool IsStagingMachine => EnvironmentInfo.MachineName.ToLower().Contains("test");

    [Parameter("Environment - 'development', 'staging' or 'production'")]
    private readonly global::Environment Environment = IsProdMachine ? global::Environment.Production : IsStagingMachine ? global::Environment.Staging : global::Environment.Development;

    [Parameter("Configuration to build - 'release' or 'debug'")]
    private Configuration Configuration => Environment.IsProduction || Environment.IsStaging ? Configuration.Release : Configuration.Debug;

    public string Database => "aviation";

    public string User => "allors";

    public string Password => "fnmP378zmdxGtY!";

    public string ExcelInstallUrl => IsProdMachine ? "https://aviation.dipu.com/excel/" : "https://aviation-test.dipu.com/excel/";

    Target Setup => _ => _
       .Executes(() =>
       {
           GitTasks.Git("reset --hard");
           GitTasks.Git("pull --recurse-submodules");
       });

    Target Install => _ => _
        .After(Setup)
        .Executes(() =>
        {
            NpmTasks.NpmInstall(v => v
                .AddProcessEnvironmentVariable("npm_config_loglevel", "silent")
                .SetProcessWorkingDirectory(Paths.TypescriptModules));
        });

    Target Generate => _ => _
        .After(Setup)
        .After(Install)
        .Executes(() =>
        {
            // Merge
            DotNetRun(v => v
            .SetProjectFile(Paths.CoreDatabaseMerge)
            .SetApplicationArguments($"{Paths.CoreDatabaseResourcesCore} {Paths.BaseDatabaseResourcesBase} {Paths.AppsDatabaseResourcesApps} {Paths.AviationDatabaseResourcesAviation} {Paths.DotnetDatabaseResourcesCustom} {Paths.DotnetDatabaseResources}"));

            // Repository
            DotNetRun(v => v
            .SetProjectFile(Paths.SystemRepositoryGenerate)
            .SetApplicationArguments($"{Paths.DotnetRepositoryDomain} {Paths.SystemRepositoryTemplatesMetaCs} {Paths.DotnetDatabaseMetaGenerated}"));

            // Meta
            DotNetRun(v => v
            .SetProcessWorkingDirectory(Paths.Dotnet)
            .SetProjectFile(Paths.DotnetDatabaseGenerate));
        });

    Target Publish => _ => _
        .After(Setup)
        .After(Install)
        .After(Generate)
        .Executes(() =>
        {
            EnsureCleanDirectory(Paths.Artifacts);

            // Commands
            DotNetPublish(v => v
            .SetVerbosity(DotNetVerbosity.Quiet)
            .SetConfiguration(Configuration)
            .SetProcessWorkingDirectory(Paths.DotnetDatabaseCommands)
            .SetOutput(Paths.ArtifactsCustomCommands));

            // Server
            DotNetPublish(v => v
            .SetVerbosity(DotNetVerbosity.Quiet)
            .SetConfiguration(Configuration)
            .SetProcessWorkingDirectory(Paths.DotnetDatabaseServer)
            .SetOutput(Paths.ArtifactsCustomServer));

            // Excel
            //CopyFile(Paths.CustomWorkspaceExcelConfig / $"Aviation-AddIn64.{Environment}.xll.config", Paths.CustomWorkspaceExcel / $"Aviation-AddIn64.xll.config", FileExistsPolicy.Overwrite);

            //MSBuild(v => v
            //    .SetConfiguration(Configuration)
            //    .SetRestore(true)
            //    .SetProjectFile(Paths.CustomWorkspaceExcelProject)
            //    .SetTargets("Build"));

            //DeleteDirectory(Paths.ArtifactsCustomExcelAddIn);
            //CopyFileToDirectory(Paths.CustomWorkspaceExcel / "bin" / Configuration / "net48" / "Aviation-AddIn64-packed.xll", Paths.ArtifactsCustomExcelAddIn);

            // Intranet
            NpmRun(s => s
                 .AddProcessEnvironmentVariable("NODE_ENV", Environment)
                 .AddProcessEnvironmentVariable("npm_config_loglevel", "silent")
                 .SetProcessWorkingDirectory(Paths.TypescriptModules)
                 .SetCommand($"intranet:build:{Environment}"));

            // Extranet
            NpmRun(s => s
                .AddProcessEnvironmentVariable("NODE_ENV", Environment)
                .AddProcessEnvironmentVariable("npm_config_loglevel", "silent")
                .SetProcessWorkingDirectory(Paths.TypescriptModules)
                .SetCommand($"extranet:build:{Environment}"));
        });

    private Target DomainTest => _ => _
        .DependsOn(Generate)
        .Executes(() =>
        {
            DotNetTest(v => v
                .SetProjectFile(Paths.DotnetDatabaseDomainTests)
                .AddLoggers("trx;LogFileName=CustomDatabaseDomain.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target IntranetScaffold => _ => _
        .Executes(() =>
        {
            DeleteDirectory(Paths.TypescriptE2EIntranetE2EGenerated);

            DotNetRun(s => s
                .SetProjectFile(Paths.TypescriptE2EIntranetScaffoldProject)
                .SetApplicationArguments($"--output {Paths.TypescriptE2EIntranetE2EGenerated} {Paths.TypescriptModulesLibApps} {Paths.TypescriptModulesLibCustom}"));
        });

    private Target IntranetTest => _ => _
        .DependsOn(Publish)
        .DependsOn(IntranetScaffold)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCustomCommands);

            using var server = new Server(Paths.ArtifactsCustomServer);
            using var angular = new Angular(Paths.TypescriptModules, "intranet:serve");
            await server.Ready();
            await angular.Init();

            DotNetBuild(s => s
                .SetProjectFile(Paths.TypescriptE2EIntranetTestsProject));

            ProcessTasks.StartProcess(Paths.TypescriptE2EIntranetTestsPlaywrightCommand, @$"install").WaitForExit();

            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EIntranetTestsProject)
                .AddLoggers("trx;LogFileName=Intranet.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    Target Upgrade => _ => _
        .DependsOn(Setup)
        .DependsOn(Install)
        .DependsOn(Generate)
        .DependsOn(Publish)
        .Executes(
            () =>
            {
                using (var iis = new IIS(new string[] { "aviation-server", "aviation-intranet", "aviation-extranet" }))
                {
                    if (Environment.IsProduction)
                    {
                        ProcessTasks.StartProcess(Paths.DeployCommandsExe, "save -f " + Paths.Population).ThrowOnFailure();
                    }

                    try
                    {
                        var population = Environment.IsProduction ? Paths.Population : Paths.StagingPopulation;
                        ProcessTasks.StartProcess(Paths.ArtifactsCustomCommandsExe, "upgrade -f " + population).ThrowOnFailure();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ProcessTasks.StartProcess(Paths.DeployCommandsExe, "load -f " + Paths.Population).ThrowOnFailure();
                        }
                        catch (Exception)
                        {
                            // TODO: Recovery failed, stop database and notify administrator
                            throw;
                        }

                        throw;
                    }

                    foreach (var path in Paths.Deploy)
                    {
                        try
                        {
                            DeleteDirectory(path);
                        }
                        catch { }
                    }

                    CopyDirectoryRecursively(Paths.ArtifactsCustomCommands, Paths.DeployCommands);
                    CopyDirectoryRecursively(Paths.ArtifactsCustomServer, Paths.DeployServer);
                    CopyDirectoryRecursively(Paths.ArtifactsCustomIntranet, Paths.DeployIntranet);
                    CopyDirectoryRecursively(Paths.ArtifactsCustomExtranet, Paths.DeployExtranet);
                    //CopyDirectoryRecursively(Paths.ArtifactsCustomExcelAddIn, Paths.DeployServerWwwRoot, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
                }
            });

    Target SaveForStaging => _ => _
       .Executes(
           () =>
           {
               ProcessTasks.StartProcess(Paths.DeployCommandsExe, "save -f " + Paths.StagingPopulation).ThrowOnFailure();
           });

    Target CiPrepare => _ => _
        .Executes(() =>
        {
            using var sql = new SqlLocalDB();
            sql.Init(this.Database, this.User, this.Password);

            DeleteDirectory(Paths.Config);
            CopyDirectoryRecursively(Paths.ConfigCi, Paths.Config);
        });

    Target CiDomain => _ => _
        .DependsOn(Install)
        .DependsOn(Generate)
        .DependsOn(CiPrepare)
        .DependsOn(DomainTest);

    Target CiIntranet => _ => _
        .DependsOn(Install)
        .DependsOn(Generate)
        .DependsOn(CiPrepare)
        .DependsOn(IntranetTest);

    //Target Default => _ => _
    //    .DependsOn(CiIntranet);

    Target Default => _ => _
      .DependsOn(Install)
      .DependsOn(Generate);
}
