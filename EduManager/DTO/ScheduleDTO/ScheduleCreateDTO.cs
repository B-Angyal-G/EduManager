namespace EduManager.DTO.ScheduleDTO;

public class ScheduleCreateDTO
{
    /// <example>weekly</example>
    public string Type { get; set; } = "weekly"; // "weekly" vagy "blocked"

    // Heti órákhoz
    /// <example>2024-09-02</example>
    public DateTime? FirstDate { get; set; } // Az első tanítási nap (hétfő)
    /// <example>08:00:00</example>
    public TimeSpan? StartTime { get; set; }
    /// <example>09:30:00</example>
    public TimeSpan? EndTime { get; set; }
    /// <example>1</example>
    public int? DayOfWeek { get; set; } // 1 (Hétfő) - 5 (Péntek)

    // Tömbösített órákhoz
    public List<DateTimeRangeDTO>? Occurrences { get; set; }
}

public class DateTimeRangeDTO
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}