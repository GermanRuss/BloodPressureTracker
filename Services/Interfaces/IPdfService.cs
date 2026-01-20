using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BloodPressureTracker.Models;

namespace BloodPressureTracker.Services.Interfaces
{
    public interface IPdfService
    {
        Task<string> GenerateBloodPressurePdfAsync(List<BloodPressureRecord> records, DateTime startDate, DateTime endDate);
        Task<string> GenerateStatisticsPdfAsync(Statistics statistics, DateTime startDate, DateTime endDate);
        Task<byte[]> GetPdfBytesAsync(string filePath);
    }
}