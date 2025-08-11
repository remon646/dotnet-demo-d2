namespace EmployeeManagement.Domain.Enums;

public enum Position
{
    General = 1,        // 一般
    Supervisor = 2,     // 主任
    Manager = 3,        // 課長
    DepartmentHead = 4  // 部長
}

public static class PositionExtensions
{
    public static string ToDisplayName(this Position position)
    {
        return position switch
        {
            Position.General => "一般",
            Position.Supervisor => "主任",
            Position.Manager => "課長",
            Position.DepartmentHead => "部長",
            _ => position.ToString()
        };
    }

    public static Position FromDisplayName(string displayName)
    {
        return displayName switch
        {
            "一般" => Position.General,
            "主任" => Position.Supervisor,
            "課長" => Position.Manager,
            "部長" => Position.DepartmentHead,
            _ => throw new ArgumentException($"Unknown position: {displayName}")
        };
    }
}