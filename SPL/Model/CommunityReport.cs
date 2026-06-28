using SQLite;

namespace SPL.Model;

public class CommunityReport
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ReporterName { get; set; } = "";

    public string Lokasi { get; set; } = "";

    public string IsiLaporan { get; set; } = "";

    public string Status { get; set; } = "Baru";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}