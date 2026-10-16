using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;

    public TimeSpan? WorkedHours =>
        CheckIn.HasValue && CheckOut.HasValue && CheckOut > CheckIn
            ? CheckOut.Value - CheckIn.Value
            : null;
}
