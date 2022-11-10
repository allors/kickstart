using Nuke.Common.IO;

public partial class Paths
{
    private const string POPULATION = "population.xml";

    // Artifacts
    public AbsolutePath Artifacts => Root / "artifacts";
    public AbsolutePath ArtifactsTests => Artifacts / "Tests";
    public AbsolutePath ArtifactsTestsCustomWorkspaceTypescriptDomain => ArtifactsTests / "CustomWorkspaceTypescriptDomain.trx";

    public AbsolutePath ArtifactsCustom => Artifacts / "Custom";
    public AbsolutePath ArtifactsCustomCommands => ArtifactsCustom / "Commands";
    public AbsolutePath ArtifactsCustomCommandsExe => ArtifactsCustomCommands / "Commands.exe";
    public AbsolutePath ArtifactsCustomServer => ArtifactsCustom / "Server";
    public AbsolutePath ArtifactsCustomIntranet => ArtifactsCustom / "Intranet";
    public AbsolutePath ArtifactsCustomExtranet => ArtifactsCustom / "Extranet";
    public AbsolutePath ArtifactsCustomExcelAddIn => ArtifactsCustom / "ExcelAddIn";

    // Config
    public AbsolutePath Config => (AbsolutePath)"/config/aviation";
    public AbsolutePath Population => Config / POPULATION;

    // Config templates
    public AbsolutePath ConfigCi => Root / "config" / "ci";

    // Staging
    public AbsolutePath Staging => (AbsolutePath)"/staging/aviation";
    public AbsolutePath StagingPopulation => Staging / POPULATION;

    // Deploy
    public AbsolutePath DeployCommands => (AbsolutePath)"/bin/aviation/Commands";
    public AbsolutePath DeployCommandsExe => DeployCommands / "Commands.exe";

    public AbsolutePath Sites => (AbsolutePath)"/sites/aviation";
    public AbsolutePath DeployIntranet => Sites / "intranet";
    public AbsolutePath DeployExtranet => Sites / "extranet";
    public AbsolutePath DeployServer => Sites / "server";
    public AbsolutePath DeployServerWwwRoot => DeployServer / "wwwroot";

    public AbsolutePath[] Deploy => new[]
   {
        DeployCommands,
        DeployServer,
        DeployIntranet,
        DeployExtranet,
    };
}
