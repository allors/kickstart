using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Base => Root / "allors/dotnet/base";
    public AbsolutePath BaseDatabase => Base / "Database";
    public AbsolutePath BaseDatabaseResources => BaseDatabase / "Resources";
    public AbsolutePath BaseDatabaseResourcesBase => BaseDatabaseResources / "Base";
}