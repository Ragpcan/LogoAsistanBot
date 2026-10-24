using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public interface IDynamicSqlExecutor
{
    Task<string> ExecuteReadOnlyQueryAsync(string sqlQuery, CancellationToken ct = default);
}

public class DynamicSqlExecutor : IDynamicSqlExecutor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DynamicSqlExecutor> _logger;

    // ─── Kesinlikle yasaklı kelimeler (word boundary ile kontrol edilir) ───
    private static readonly string[] _forbiddenKeywords = new[]
    {
        "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE",
        "ALTER",  "CREATE", "EXEC",   "EXECUTE", "MERGE",
        "GRANT",  "REVOKE", "DENY",
        "xp_cmdshell", "xp_regread", "xp_regwrite",
        "sp_executesql", "sp_execute", "sp_oacreate", "sp_oamethod",
        "OPENROWSET", "OPENDATASOURCE", "OPENQUERY",
        "BULK INSERT", "bcp", "WRITETEXT", "UPDATETEXT"
    };

    // ─── İzin verilen başlangıç kalıpları (SELECT veya CTE) ───
    private static readonly Regex _allowedStartPattern =
        new Regex(@"^\s*(SELECT|WITH)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DynamicSqlExecutor(IConfiguration configuration, ILogger<DynamicSqlExecutor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        string? connectionString = _configuration.GetConnectionString("LogoCrmConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("SQL bağlantı dizesi yapılandırılmamış.");
        return connectionString;
    }

    /// <summary>
    /// SQL yorumlarını siler (-- ve /* */ formatında).
    /// Bypass girişimlerini (DEL/**/ETE gibi) engeller.
    /// </summary>
    private static string StripSqlComments(string sql)
    {
        // /* ... */ yorumlarını sil
        sql = Regex.Replace(sql, @"/\*.*?\*/", " ", RegexOptions.Singleline);
        // -- satır yorumlarını sil
        sql = Regex.Replace(sql, @"--[^\r\n]*", " ");
        return sql;
    }

    public async Task<string> ExecuteReadOnlyQueryAsync(string sqlQuery, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
            return "[GÜVENLİK] Boş sorgu reddedildi.";

        // ─── KATMAN 1: Yorum saldırılarını engelle ───
        string cleanSql = StripSqlComments(sqlQuery);

        // ─── KATMAN 2: Beyaz Liste — sadece SELECT veya WITH (CTE) ile başlayabilir ───
        if (!_allowedStartPattern.IsMatch(cleanSql))
        {
            _logger.LogWarning("Security Block: Sorgu SELECT/WITH ile başlamıyor. Sorgu: {Sql}", sqlQuery.Length > 100 ? sqlQuery[..100] : sqlQuery);
            return "[GÜVENLİK İHLALİ] Reddedildi: Yalnızca SELECT sorgusu çalıştırılabilir.";
        }

        // ─── KATMAN 3: Kara Liste — yasaklı anahtar kelimeler ───
        string upperClean = cleanSql.ToUpperInvariant();
        foreach (var keyword in _forbiddenKeywords)
        {
            string pattern = keyword.Contains("_") || keyword.Contains(" ")
                ? Regex.Escape(keyword.ToUpperInvariant())       // xp_cmdshell, BULK INSERT gibi
                : $@"\b{Regex.Escape(keyword.ToUpperInvariant())}\b"; // kelime sınırı

            if (Regex.IsMatch(upperClean, pattern))
            {
                _logger.LogWarning("Security Block: Yasaklı komut tespit edildi: {Keyword} | Sorgu: {Sql}",
                    keyword, sqlQuery.Length > 100 ? sqlQuery[..100] : sqlQuery);
                return $"[GÜVENLİK İHLALİ] Reddedildi: '{keyword}' komutuna bu arayüzden izin verilmiyor.";
            }
        }

        // ─── KATMAN 4: Veritabanı seviyesinde READ-ONLY transaction ───
        try
        {
            using IDbConnection connection = new SqlConnection(GetConnectionString());
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var cmd = new CommandDefinition(cleanSql, null, transaction, 20, null, CommandFlags.Buffered, ct);
                var results = await connection.QueryAsync<dynamic>(cmd);
                // Transaction'ı commit ETMİYORUZ — sadece okuma yapıldığından fark etmez,
                // ama herhangi bir yan etki (tetikleyici vb.) rollback ile iptal edilir.
                transaction.Rollback();
                return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                try { transaction.Rollback(); } catch { /* ignored */ }
                throw;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Dinamik SQL çalıştırılırken hata: {Sql}", sqlQuery.Length > 200 ? sqlQuery[..200] : sqlQuery);
            return $"[SQL HATASI] {ex.Message}";
        }
    }
}
