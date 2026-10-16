namespace HR.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int PresentToday { get; set; }
    public int PendingLeaves { get; set; }
    public int ApprovedLeavesThisMonth { get; set; }
}
