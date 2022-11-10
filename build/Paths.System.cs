using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath System => Root / "allors/dotnet/system";
    public AbsolutePath SystemRepositoryTemplates => System / "Repository/Templates";
    public AbsolutePath SystemRepositoryTemplatesMetaCs => SystemRepositoryTemplates / "meta.cs.stg";
    public AbsolutePath SystemRepositoryGenerate => System / "Repository/Generate/Generate.csproj";
}
