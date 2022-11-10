using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Core => Root / "allors/dotnet/core";
    public AbsolutePath CoreDatabase => Core / "Database";
    public AbsolutePath CoreDatabaseMerge => CoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath CoreDatabaseResources => CoreDatabase / "Resources";
    public AbsolutePath CoreDatabaseResourcesCore => CoreDatabaseResources / "Core";
    public AbsolutePath CoreDatabaseResourcesCustom => CoreDatabaseResources / "Custom";
}