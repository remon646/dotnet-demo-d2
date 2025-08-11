namespace EmployeeManagement.Domain.Enums;

public enum Department
{
    Sales = 1,      // 営業部
    Development = 2, // 開発部
    GeneralAffairs = 3, // 総務部
    HumanResources = 4  // 人事部
}

public static class DepartmentExtensions
{
    public static string ToDisplayName(this Department department)
    {
        return department switch
        {
            Department.Sales => "営業部",
            Department.Development => "開発部",
            Department.GeneralAffairs => "総務部",
            Department.HumanResources => "人事部",
            _ => department.ToString()
        };
    }

    public static Department FromDisplayName(string displayName)
    {
        return displayName switch
        {
            "営業部" => Department.Sales,
            "開発部" => Department.Development,
            "総務部" => Department.GeneralAffairs,
            "人事部" => Department.HumanResources,
            _ => throw new ArgumentException($"Unknown department: {displayName}")
        };
    }
}