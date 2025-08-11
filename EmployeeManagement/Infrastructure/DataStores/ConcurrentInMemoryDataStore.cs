using System.Collections.Concurrent;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Infrastructure.DataStores;

public class ConcurrentInMemoryDataStore
{
    private readonly ConcurrentDictionary<string, Employee> _employees = new();
    private readonly ConcurrentDictionary<string, DepartmentMaster> _departments = new();
    private readonly ConcurrentDictionary<string, User> _users = new();
    private readonly ConcurrentDictionary<string, object> _collections = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// 指定された型のコレクションを取得または作成
    /// </summary>
    public ConcurrentDictionary<string, T> GetOrCreateCollection<T>(string collectionName)
    {
        return (ConcurrentDictionary<string, T>)_collections.GetOrAdd(collectionName, 
            _ => new ConcurrentDictionary<string, T>());
    }

    public ConcurrentInMemoryDataStore()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        // Seed users
        _users.TryAdd("admin", new User
        {
            UserId = "admin",
            Password = "password",
            DisplayName = "管理者",
            IsAdmin = true,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now.AddDays(-1)
        });

        _users.TryAdd("user", new User
        {
            UserId = "user",
            Password = "password",
            DisplayName = "一般ユーザー",
            IsAdmin = false,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now.AddDays(-2)
        });

        // Seed departments
        _departments.TryAdd("DEPT001", new DepartmentMaster
        {
            DepartmentCode = "DEPT001",
            DepartmentName = "営業部",
            ManagerName = "田中部長",
            ManagerEmployeeNumber = "EMP002",
            DepartmentType = Department.Sales,
            EstablishedDate = new DateTime(2020, 4, 1),
            Extension = "100",
            Description = "営業活動を担当する部署です。",
            IsActive = true,
            CreatedAt = DateTime.Now.AddMonths(-6),
            UpdatedAt = DateTime.Now.AddDays(-1)
        });

        _departments.TryAdd("DEPT002", new DepartmentMaster
        {
            DepartmentCode = "DEPT002",
            DepartmentName = "開発部",
            ManagerName = "佐藤部長",
            ManagerEmployeeNumber = "EMP003",
            DepartmentType = Department.Development,
            EstablishedDate = new DateTime(2019, 4, 1),
            Extension = "200",
            Description = "システム開発を担当する部署です。",
            IsActive = true,
            CreatedAt = DateTime.Now.AddMonths(-8),
            UpdatedAt = DateTime.Now.AddDays(-2)
        });

        _departments.TryAdd("DEPT003", new DepartmentMaster
        {
            DepartmentCode = "DEPT003",
            DepartmentName = "総務部",
            ManagerName = "鈴木美咲",
            ManagerEmployeeNumber = "EMP2024004",
            DepartmentType = Department.GeneralAffairs,
            EstablishedDate = new DateTime(2018, 4, 1),
            Extension = "300",
            Description = "総務業務を担当する部署です。",
            IsActive = true,
            CreatedAt = DateTime.Now.AddMonths(-10),
            UpdatedAt = DateTime.Now.AddDays(-3)
        });

        _departments.TryAdd("DEPT004", new DepartmentMaster
        {
            DepartmentCode = "DEPT004",
            DepartmentName = "人事部",
            ManagerName = "高橋部長",
            ManagerEmployeeNumber = "EMP005",
            DepartmentType = Department.HumanResources,
            EstablishedDate = new DateTime(2018, 4, 1),
            Extension = "400",
            Description = "人事業務を担当する部署です。",
            IsActive = true,
            CreatedAt = DateTime.Now.AddMonths(-10),
            UpdatedAt = DateTime.Now.AddDays(-4)
        });

        // Seed employees with department history
        var employee1 = new Employee
        {
            EmployeeNumber = "EMP2024001",
            Name = "山田太郎",
            JoinDate = new DateTime(2024, 4, 1),
            Email = "yamada@company.com",
            PhoneNumber = "03-1234-5678",
            CreatedAt = DateTime.Now.AddMonths(-3),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };
        
        employee1.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee1.EmployeeNumber,
            Department = Department.Sales,
            Position = Position.Supervisor,
            StartDate = employee1.JoinDate,
            EndDate = null,
            TransferReason = "新卒入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        employee1.DepartmentHistories.Add(employee1.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024001", employee1);

        // Employee 2
        var employee2 = new Employee
        {
            EmployeeNumber = "EMP2024002",
            Name = "佐藤花子",
            JoinDate = new DateTime(2024, 4, 1),
            Email = "sato@company.com",
            PhoneNumber = "03-1234-5679",
            CreatedAt = DateTime.Now.AddMonths(-3),
            UpdatedAt = DateTime.Now.AddDays(-2)
        };
        employee2.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee2.EmployeeNumber,
            Department = Department.Development,
            Position = Position.DepartmentHead,
            StartDate = employee2.JoinDate,
            EndDate = null,
            TransferReason = "新卒入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee2.DepartmentHistories.Add(employee2.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024002", employee2);

        // Employee 3
        var employee3 = new Employee
        {
            EmployeeNumber = "EMP2024003",
            Name = "田中一郎",
            JoinDate = new DateTime(2023, 10, 1),
            Email = "tanaka@company.com",
            PhoneNumber = "03-1234-5680",
            CreatedAt = DateTime.Now.AddMonths(-9),
            UpdatedAt = DateTime.Now.AddDays(-3)
        };
        employee3.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee3.EmployeeNumber,
            Department = Department.Sales,
            Position = Position.Manager,
            StartDate = employee3.JoinDate,
            EndDate = null,
            TransferReason = "中途入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee3.DepartmentHistories.Add(employee3.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024003", employee3);

        // Employee 4
        var employee4 = new Employee
        {
            EmployeeNumber = "EMP2024004",
            Name = "鈴木美咲",
            JoinDate = new DateTime(2024, 1, 15),
            Email = "suzuki@company.com",
            PhoneNumber = "03-1234-5681",
            CreatedAt = DateTime.Now.AddMonths(-6),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };
        employee4.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee4.EmployeeNumber,
            Department = Department.GeneralAffairs,
            Position = Position.DepartmentHead,
            StartDate = employee4.JoinDate,
            EndDate = null,
            TransferReason = "部長昇格",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee4.DepartmentHistories.Add(employee4.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024004", employee4);

        // Employee 5
        var employee5 = new Employee
        {
            EmployeeNumber = "EMP2024005",
            Name = "高橋健太",
            JoinDate = new DateTime(2022, 4, 1),
            Email = "takahashi@company.com",
            PhoneNumber = "03-1234-5682",
            CreatedAt = DateTime.Now.AddMonths(-15),
            UpdatedAt = DateTime.Now.AddDays(-5)
        };
        employee5.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee5.EmployeeNumber,
            Department = Department.GeneralAffairs,
            Position = Position.DepartmentHead,
            StartDate = employee5.JoinDate,
            EndDate = null,
            TransferReason = "新卒入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee5.DepartmentHistories.Add(employee5.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024005", employee5);

        // Employee 6
        var employee6 = new Employee
        {
            EmployeeNumber = "EMP2024006",
            Name = "伊藤良子",
            JoinDate = new DateTime(2023, 7, 1),
            Email = "ito@company.com",
            PhoneNumber = "03-1234-5683",
            CreatedAt = DateTime.Now.AddMonths(-12),
            UpdatedAt = DateTime.Now.AddDays(-2)
        };
        employee6.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee6.EmployeeNumber,
            Department = Department.Development,
            Position = Position.General,
            StartDate = employee6.JoinDate,
            EndDate = null,
            TransferReason = "中途入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee6.DepartmentHistories.Add(employee6.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024006", employee6);

        // Employee 7
        var employee7 = new Employee
        {
            EmployeeNumber = "EMP2024007",
            Name = "渡辺裕太",
            JoinDate = new DateTime(2024, 2, 1),
            Email = "watanabe@company.com",
            PhoneNumber = "03-1234-5684",
            CreatedAt = DateTime.Now.AddMonths(-5),
            UpdatedAt = DateTime.Now.AddDays(-4)
        };
        employee7.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee7.EmployeeNumber,
            Department = Department.Sales,
            Position = Position.General,
            StartDate = employee7.JoinDate,
            EndDate = null,
            TransferReason = "新卒入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee7.DepartmentHistories.Add(employee7.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024007", employee7);

        // Employee 8
        var employee8 = new Employee
        {
            EmployeeNumber = "EMP2024008",
            Name = "加藤さくら",
            JoinDate = new DateTime(2023, 9, 15),
            Email = "kato@company.com",
            PhoneNumber = "03-1234-5685",
            CreatedAt = DateTime.Now.AddMonths(-10),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };
        employee8.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee8.EmployeeNumber,
            Department = Department.HumanResources,
            Position = Position.Manager,
            StartDate = employee8.JoinDate,
            EndDate = null,
            TransferReason = "中途入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee8.DepartmentHistories.Add(employee8.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024008", employee8);

        // Employee 9
        var employee9 = new Employee
        {
            EmployeeNumber = "EMP2024009",
            Name = "中村大輔",
            JoinDate = new DateTime(2024, 3, 1),
            Email = "nakamura@company.com",
            PhoneNumber = "03-1234-5686",
            CreatedAt = DateTime.Now.AddMonths(-4),
            UpdatedAt = DateTime.Now.AddDays(-3)
        };
        employee9.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee9.EmployeeNumber,
            Department = Department.Development,
            Position = Position.Manager,
            StartDate = employee9.JoinDate,
            EndDate = null,
            TransferReason = "新卒入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee9.DepartmentHistories.Add(employee9.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024009", employee9);

        // Employee 10
        var employee10 = new Employee
        {
            EmployeeNumber = "EMP2024010",
            Name = "小林真理",
            JoinDate = new DateTime(2023, 12, 1),
            Email = "kobayashi@company.com",
            PhoneNumber = "03-1234-5687",
            CreatedAt = DateTime.Now.AddMonths(-7),
            UpdatedAt = DateTime.Now.AddDays(-2)
        };
        employee10.CurrentDepartmentHistory = new DepartmentHistory
        {
            HistoryId = Guid.NewGuid().ToString(),
            EmployeeNumber = employee10.EmployeeNumber,
            Department = Department.GeneralAffairs,
            Position = Position.General,
            StartDate = employee10.JoinDate,
            EndDate = null,
            TransferReason = "中途入社",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        employee10.DepartmentHistories.Add(employee10.CurrentDepartmentHistory);
        _employees.TryAdd("EMP2024010", employee10);
    }

    // Employee operations
    public IEnumerable<Employee> GetAllEmployees()
    {
        return _employees.Values.ToList();
    }

    public Employee? GetEmployee(string employeeNumber)
    {
        _employees.TryGetValue(employeeNumber, out var employee);
        return employee;
    }

    public bool AddEmployee(Employee employee)
    {
        employee.CreatedAt = DateTime.Now;
        employee.UpdatedAt = DateTime.Now;
        return _employees.TryAdd(employee.EmployeeNumber, employee);
    }

    public bool UpdateEmployee(Employee employee)
    {
        lock (_lockObject)
        {
            if (_employees.ContainsKey(employee.EmployeeNumber))
            {
                employee.UpdatedAt = DateTime.Now;
                _employees[employee.EmployeeNumber] = employee;
                return true;
            }
            return false;
        }
    }

    public bool DeleteEmployee(string employeeNumber)
    {
        return _employees.TryRemove(employeeNumber, out _);
    }

    // Department operations
    public IEnumerable<DepartmentMaster> GetAllDepartments()
    {
        return _departments.Values.ToList();
    }

    public DepartmentMaster? GetDepartment(string departmentCode)
    {
        _departments.TryGetValue(departmentCode, out var department);
        return department;
    }

    public bool AddDepartment(DepartmentMaster department)
    {
        department.CreatedAt = DateTime.Now;
        department.UpdatedAt = DateTime.Now;
        return _departments.TryAdd(department.DepartmentCode, department);
    }

    public bool UpdateDepartment(DepartmentMaster department)
    {
        lock (_lockObject)
        {
            if (_departments.ContainsKey(department.DepartmentCode))
            {
                department.UpdatedAt = DateTime.Now;
                _departments[department.DepartmentCode] = department;
                return true;
            }
            return false;
        }
    }

    public bool DeleteDepartment(string departmentCode)
    {
        // Safety check: Don't delete if employees are assigned to this department
        var departmentMaster = GetDepartment(departmentCode);
        if (departmentMaster == null) return false;
        
        var employeesInDepartment = _employees.Values
            .Where(e => e.CurrentDepartment == departmentMaster.DepartmentType)
            .Count();

        if (employeesInDepartment > 0)
        {
            return false; // Cannot delete department with employees
        }

        return _departments.TryRemove(departmentCode, out _);
    }

    // User operations
    public User? GetUser(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public bool ValidateUser(string userId, string password)
    {
        var user = GetUser(userId);
        return user != null && user.Password == password;
    }

    public void UpdateLastLogin(string userId)
    {
        lock (_lockObject)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                user.LastLoginAt = DateTime.Now;
                _users[userId] = user;
            }
        }
    }

    // Statistics
    public int GetEmployeeCount()
    {
        return _employees.Count;
    }

    public int GetDepartmentCount()
    {
        return _departments.Count;
    }

    public int GetActiveDepartmentCount()
    {
        return _departments.Values.Count(d => d.IsActive);
    }

    public int GetDepartmentsWithManagerCount()
    {
        return _departments.Values.Count(d => !string.IsNullOrEmpty(d.ManagerName));
    }

    public DateTime GetLastDepartmentUpdateDate()
    {
        return _departments.Values.DefaultIfEmpty().Max(d => d?.UpdatedAt ?? DateTime.MinValue);
    }
}