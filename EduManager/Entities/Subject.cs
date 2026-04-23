using Microsoft.EntityFrameworkCore;

namespace EduManager.Entities;

[Index(nameof(Code), IsUnique = true)]
public class Subject
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
    public bool IsActive { get; set; } = true;
}