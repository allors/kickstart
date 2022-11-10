using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Apps => Root / "allors/dotnet/apps";
    public AbsolutePath AppsDatabase => Apps / "Database";
    public AbsolutePath AppsDatabaseResources => AppsDatabase / "Resources";
    public AbsolutePath AppsDatabaseResourcesApps => AppsDatabaseResources / "Apps";
}