using SQLite;

namespace v4posme_maui.Models;

[Table("tb_parameter_system")]
public class TbParameterSystem
{
    [PrimaryKey, AutoIncrement] public int ParameterId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
}