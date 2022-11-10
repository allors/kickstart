using Nuke.Common.IO;

public partial class Paths
{
    // CSharp
    public AbsolutePath Dotnet => Root / "dotnet";
    public AbsolutePath DotnetRepositoryDomain => Dotnet / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetDatabase => Dotnet / "Database";
    public AbsolutePath DotnetDatabaseMetaGenerated => DotnetDatabase / "Meta/generated";
    public AbsolutePath DotnetDatabaseGenerate => DotnetDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetDatabaseCommands => DotnetDatabase / "Commands";
    public AbsolutePath DotnetDatabaseServer => DotnetDatabase / "Server";
    public AbsolutePath DotnetDatabaseDomainTests => DotnetDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetDatabaseResources => DotnetDatabase / "Resources";
    public AbsolutePath DotnetDatabaseResourcesCustom => DotnetDatabaseResources / "Custom";

    public AbsolutePath DotnetWorkspace => Dotnet / "Workspace";
    public AbsolutePath DotnetWorkspaceExcel => DotnetWorkspace / "Kickstart";
    public AbsolutePath CSharpWorkspaceExcelConfig => DotnetWorkspaceExcel / "config";
    public AbsolutePath CSharpWorkspaceExcelProject => DotnetWorkspaceExcel / "Kickstart.csproj";

    // Typescript
    public AbsolutePath Typescript => Root / "typescript";
    
    public AbsolutePath TypescriptModules => Typescript / "modules";
    public AbsolutePath TypescriptModulesLibs => TypescriptModules / "libs";
    public AbsolutePath TypescriptModulesLibApps => TypescriptModulesLibs / "apps-intranet/workspace/angular-material";
    public AbsolutePath TypescriptModulesLibCustom => TypescriptModulesLibs / "intranet/workspace/angular-material";

    public AbsolutePath TypescriptE2E => Typescript / "e2e";
    public AbsolutePath TypescriptE2EIntranet => TypescriptE2E / "intranet";
    public AbsolutePath TypescriptE2EIntranetE2E => TypescriptE2EIntranet / "E2E";
    public AbsolutePath TypescriptE2EIntranetE2EGenerated => TypescriptE2EIntranetE2E / "Generated";
    public AbsolutePath TypescriptE2EIntranetScaffold => TypescriptE2EIntranet / "Scaffold.Command";
    public AbsolutePath TypescriptE2EIntranetScaffoldProject => TypescriptE2EIntranetScaffold / "Scaffold.Command.csproj";
    public AbsolutePath TypescriptE2EIntranetTests => TypescriptE2EIntranet / "Tests";
    public AbsolutePath TypescriptE2EIntranetTestsProject => TypescriptE2EIntranetTests / "Tests.csproj";
    public AbsolutePath TypescriptE2EIntranetTestsPlaywrightCommand => TypescriptE2EIntranetTests / "bin/debug/net6.0/.playwright/node/win32_x64/playwright.cmd";
}
