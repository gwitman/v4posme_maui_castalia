using SQLite;

namespace v4posme_maui.Models;

[Table("tb_logs")]
public class TbLogs
{
    [PrimaryKey, AutoIncrement] public int LogId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Severity { get; set; }
    public string? Logs { get; set; }
}
