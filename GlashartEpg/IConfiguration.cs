namespace GlashartEpg
{
    public interface IConfiguration
    {
        string DataFolder { get; }
        string EpgUrl { get; }
        int Days { get; }
    }
}