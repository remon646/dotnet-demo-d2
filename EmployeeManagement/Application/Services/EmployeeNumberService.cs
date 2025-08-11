using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 社員番号自動生成サービス
/// </summary>
public class EmployeeNumberService : IDisposable
{
    private readonly IEmployeeNumberRepository _employeeNumberRepository;
    private static readonly SemaphoreSlim _generationSemaphore = new(1, 1);
    private readonly ILogger<EmployeeNumberService>? _logger;
    private readonly Timer? _cleanupTimer;
    private bool _disposed = false;

    public EmployeeNumberService(
        IEmployeeNumberRepository employeeNumberRepository,
        ILogger<EmployeeNumberService>? logger = null)
    {
        _employeeNumberRepository = employeeNumberRepository;
        _logger = logger;
        
        // Start cleanup timer - runs every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredReservations, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// 新しい社員番号を生成
    /// </summary>
    public async Task<string> GenerateNewEmployeeNumberAsync(int? year = null)
    {
        var targetYear = year ?? DateTime.Now.Year;
        return await _employeeNumberRepository.GetNextAvailableNumberAsync(targetYear);
    }

    /// <summary>
    /// 社員番号を生成して予約する（レースコンディション防止）
    /// </summary>
    public async Task<string> GenerateAndReserveEmployeeNumberAsync(int? year = null)
    {
        await _generationSemaphore.WaitAsync();
        try
        {
            var targetYear = year ?? DateTime.Now.Year;
            var nextNumber = await _employeeNumberRepository.GetNextAvailableNumberAsync(targetYear);
            
            // Immediately reserve the number to prevent race conditions
            var employeeNumber = new EmployeeNumber
            {
                Number = nextNumber,
                IssueYear = targetYear,
                SequenceNumber = ExtractSequenceNumber(nextNumber),
                IssuedAt = DateTime.Now,
                IsActive = true,
                Remarks = "Reserved for employee creation"
            };

            await _employeeNumberRepository.AddAsync(employeeNumber);
            return nextNumber;
        }
        finally
        {
            _generationSemaphore.Release();
        }
    }

    /// <summary>
    /// 社員番号を発行
    /// </summary>
    public async Task<EmployeeNumber> IssueEmployeeNumberAsync(int? year = null, string? remarks = null)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var nextNumber = await GenerateNewEmployeeNumberAsync(targetYear);
        
        var employeeNumber = new EmployeeNumber
        {
            Number = nextNumber,
            IssueYear = targetYear,
            SequenceNumber = ExtractSequenceNumber(nextNumber),
            IssuedAt = DateTime.Now,
            IsActive = true,
            Remarks = remarks
        };

        return await _employeeNumberRepository.AddAsync(employeeNumber);
    }

    /// <summary>
    /// 全ての社員番号を取得
    /// </summary>
    public async Task<IEnumerable<EmployeeNumber>> GetAllAsync()
    {
        return await _employeeNumberRepository.GetAllAsync();
    }

    /// <summary>
    /// 社員番号の使用状況を取得
    /// </summary>
    public async Task<EmployeeNumberUsageInfo> GetUsageInfoAsync(int? year = null)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var yearNumbers = await _employeeNumberRepository.GetByYearAsync(targetYear);
        var allNumbers = await _employeeNumberRepository.GetAllAsync();

        return new EmployeeNumberUsageInfo
        {
            Year = targetYear,
            TotalIssued = yearNumbers.Count(),
            ActiveCount = yearNumbers.Count(n => n.IsActive),
            InactiveCount = yearNumbers.Count(n => !n.IsActive),
            NextAvailableNumber = await _employeeNumberRepository.GetNextAvailableNumberAsync(targetYear),
            LastIssuedNumber = yearNumbers.OrderByDescending(n => n.SequenceNumber).FirstOrDefault()?.Number,
            AllYearsTotal = allNumbers.Count()
        };
    }

    /// <summary>
    /// 社員番号を廃番にする
    /// </summary>
    public async Task<bool> DeactivateEmployeeNumberAsync(string number, string? reason = null)
    {
        var employeeNumber = await _employeeNumberRepository.GetByNumberAsync(number);
        if (employeeNumber == null) return false;

        employeeNumber.IsActive = false;
        employeeNumber.Remarks = string.IsNullOrEmpty(reason) 
            ? employeeNumber.Remarks 
            : $"{employeeNumber.Remarks} [廃番理由: {reason}]";
        employeeNumber.UpdatedAt = DateTime.Now;

        await _employeeNumberRepository.UpdateAsync(employeeNumber);
        return true;
    }

    /// <summary>
    /// 社員番号の備考を更新
    /// </summary>
    public async Task<bool> UpdateEmployeeNumberRemarksAsync(string number, string remarks)
    {
        var employeeNumber = await _employeeNumberRepository.GetByNumberAsync(number);
        if (employeeNumber == null) return false;

        employeeNumber.Remarks = remarks;
        employeeNumber.UpdatedAt = DateTime.Now;

        await _employeeNumberRepository.UpdateAsync(employeeNumber);
        return true;
    }

    /// <summary>
    /// 社員番号を即座に予約する（重複防止用）
    /// </summary>
    public async Task<string> ReserveEmployeeNumberAsync(int? year = null, string? reservedBy = null)
    {
        await _generationSemaphore.WaitAsync();
        try
        {
            var targetYear = year ?? DateTime.Now.Year;
            var nextNumber = await _employeeNumberRepository.GetNextAvailableNumberAsync(targetYear);
            
            var reservation = new EmployeeNumber
            {
                Number = nextNumber,
                IssueYear = targetYear,
                SequenceNumber = ExtractSequenceNumber(nextNumber),
                IssuedAt = DateTime.Now,
                IsActive = false, // Reserved state
                Status = EmployeeNumberStatus.Reserved, // Set status field properly
                Remarks = $"Reserved for: {reservedBy ?? "Unknown"}"
            };

            await _employeeNumberRepository.AddAsync(reservation);
            return nextNumber;
        }
        finally
        {
            _generationSemaphore.Release();
        }
    }

    /// <summary>
    /// 予約された社員番号を解除する
    /// </summary>
    public async Task<bool> ReleaseReservationAsync(string employeeNumber)
    {
        var reservation = await _employeeNumberRepository.GetByNumberAsync(employeeNumber);
        if (reservation == null || reservation.IsActive) 
            return false;

        await _employeeNumberRepository.DeleteAsync(reservation);
        return true;
    }

    /// <summary>
    /// 予約された社員番号を有効化する
    /// </summary>
    public async Task<bool> ActivateReservationAsync(string employeeNumber, string employeeName)
    {
        var reservation = await _employeeNumberRepository.GetByNumberAsync(employeeNumber);
        if (reservation == null || reservation.IsActive) 
            return false;

        reservation.IsActive = true;
        reservation.Status = EmployeeNumberStatus.Active; // Set status field properly
        reservation.Remarks = $"Assigned to: {employeeName}";
        reservation.UpdatedAt = DateTime.Now;
        
        await _employeeNumberRepository.UpdateAsync(reservation);
        return true;
    }

    /// <summary>
    /// 予約が有効かどうかを確認する
    /// </summary>
    public async Task<bool> IsReservationValidAsync(string employeeNumber)
    {
        var reservation = await _employeeNumberRepository.GetByNumberAsync(employeeNumber);
        return reservation != null && !reservation.IsActive;
    }

    /// <summary>
    /// 社員番号のフォーマット検証
    /// </summary>
    public bool ValidateEmployeeNumberFormat(string number)
    {
        if (string.IsNullOrEmpty(number) || number.Length != 10)
            return false;

        if (!number.StartsWith("EMP"))
            return false;

        var yearPart = number.Substring(3, 4);
        var sequencePart = number.Substring(7, 3);

        return int.TryParse(yearPart, out var year) && 
               year >= 2000 && year <= 9999 &&
               int.TryParse(sequencePart, out var sequence) && 
               sequence >= 1 && sequence <= 999;
    }

    /// <summary>
    /// 社員番号から連番を抽出
    /// </summary>
    private int ExtractSequenceNumber(string employeeNumber)
    {
        if (employeeNumber.Length >= 10)
        {
            var sequencePart = employeeNumber.Substring(7, 3);
            if (int.TryParse(sequencePart, out var sequence))
                return sequence;
        }
        return 0;
    }
    
    /// <summary>
    /// 期限切れの予約をクリーンアップする
    /// </summary>
    private async void CleanupExpiredReservations(object? state)
    {
        if (_disposed) return;
        
        try
        {
            var allNumbers = await _employeeNumberRepository.GetAllAsync();
            var expiredReservations = allNumbers.Where(n => 
                !n.IsActive && // Reserved state
                n.Remarks != null && n.Remarks.StartsWith("Reserved for:") &&
                n.IssuedAt.AddMinutes(30) < DateTime.Now // 30 minute expiration
            ).ToList();
            
            foreach (var expired in expiredReservations)
            {
                await _employeeNumberRepository.DeleteAsync(expired);
                _logger?.LogInformation("Cleaned up expired reservation: {Number}", expired.Number);
            }
            
            if (expiredReservations.Any())
            {
                _logger?.LogInformation("Cleaned up {Count} expired reservations", expiredReservations.Count);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during cleanup of expired reservations");
        }
    }
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _cleanupTimer?.Dispose();
        // Note: Static semaphore should not be disposed
    }
}

/// <summary>
/// 社員番号使用状況情報
/// </summary>
public class EmployeeNumberUsageInfo
{
    public int Year { get; set; }
    public int TotalIssued { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public string? NextAvailableNumber { get; set; }
    public string? LastIssuedNumber { get; set; }
    public int AllYearsTotal { get; set; }
}