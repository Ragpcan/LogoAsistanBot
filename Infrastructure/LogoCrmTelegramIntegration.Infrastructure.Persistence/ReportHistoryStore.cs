using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class ReportHistoryStore : IReportHistoryStore
{
    private readonly string _filePath;
    private readonly ILogger<ReportHistoryStore> _logger;
    private readonly object _lock = new();
    private List<ReportHistoryItemDto> _history = new();
    private const int MaxHistoryItems = 500;

    public ReportHistoryStore(ILogger<ReportHistoryStore> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "report_history.json");
        LoadFromFile();
    }

    public List<ReportHistoryItemDto> GetHistory(int take = 100)
    {
        lock (_lock)
        {
            return _history
                .OrderByDescending(h => h.ExecutedAt)
                .Take(take)
                .ToList();
        }
    }

    public void AddHistoryItem(ReportHistoryItemDto item)
    {
        if (item == null) return;

        lock (_lock)
        {
            _history.Insert(0, item);
            if (_history.Count > MaxHistoryItems)
            {
                _history = _history.Take(MaxHistoryItems).ToList();
            }
            SaveToFile();
        }
    }

    public void ClearHistory()
    {
        lock (_lock)
        {
            _history.Clear();
            SaveToFile();
        }
    }

    private void LoadFromFile()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _history = JsonSerializer.Deserialize<List<ReportHistoryItemDto>>(json) ?? new List<ReportHistoryItemDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "report_history.json okunurken hata oluştu.");
                _history = new List<ReportHistoryItemDto>();
            }
        }
    }

    private void SaveToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "report_history.json kaydedilirken hata oluştu.");
        }
    }
}
