using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class LogoErpRepository : ILogoErpRepository
{
	private readonly IConfiguration _configuration;

	private readonly ILogger<LogoErpRepository> _logger;

	private static readonly ConcurrentDictionary<string, (List<LogoErpSalesmanDto> Salesmen, DateTime CachedAt)> _salesmenCache = new ConcurrentDictionary<string, (List<LogoErpSalesmanDto>, DateTime)>();

	public LogoErpRepository(IConfiguration configuration, ILogger<LogoErpRepository> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	private string GetConnectionString()
	{
		string? connectionString = _configuration.GetConnectionString("LogoCrmConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException("SQL veritabanÃƒâ€Ã‚Â± baÃƒâ€Ã…\u00b8lantÃƒâ€Ã‚Â± dizesi (Connection String) henÃƒÆ’Ã‚Â¼z yapÃƒâ€Ã‚Â±landÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â±lmamÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8.");
		}
		return connectionString;
	}

	private IDbConnection CreateConnection()
	{
		return new SqlConnection(GetConnectionString());
	}

	private static string SanitizeNo(string input, string defaultValue)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return defaultValue;
		}
		string text = new string(input.Where(char.IsDigit).ToArray());
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text.PadLeft(defaultValue.Length, '0');
		}
		return defaultValue;
	}

	private static string BuildSqlPattern(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return "%";
		}
		string text = input.Trim().Replace("*", "%");
		if (!text.Contains("%"))
		{
			text = "%" + text + "%";
		}
		return text;
	}

	private async Task<(string csTable, string ctTable)> ResolveChequeTablesAsync(IDbConnection connection, string firmNo, string periodNo, CancellationToken ct)
	{
		string[] array = new string[2]
		{
			$"LG_{firmNo}_{periodNo}_CSCARD",
			"LG_" + firmNo + "_CSCARD"
		};
		string[] ctCandidates = new string[2]
		{
			$"LG_{firmNo}_{periodNo}_CSTRANS",
			"LG_" + firmNo + "_CSTRANS"
		};
		string chosenCs = $"LG_{firmNo}_{periodNo}_CSCARD";
		string chosenCt = $"LG_{firmNo}_{periodNo}_CSTRANS";
		string[] array2 = array;
		foreach (string cs in array2)
		{
			try
			{
				var parameters = new
				{
					CandidateName = cs
				};
				CancellationToken cancellationToken = ct;
				int? num = await connection.ExecuteScalarAsync<int?>(new CommandDefinition("SELECT 1 FROM sys.tables WHERE name = @CandidateName", parameters, null, null, null, CommandFlags.Buffered, cancellationToken));
				if (num.HasValue && num.Value == 1)
				{
					chosenCs = cs;
					break;
				}
			}
			catch
			{
			}
		}
		array2 = ctCandidates;
		foreach (string cs in array2)
		{
			try
			{
				var parameters2 = new
				{
					CandidateName = cs
				};
				CancellationToken cancellationToken = ct;
				int? num2 = await connection.ExecuteScalarAsync<int?>(new CommandDefinition("SELECT 1 FROM sys.tables WHERE name = @CandidateName", parameters2, null, null, null, CommandFlags.Buffered, cancellationToken));
				if (num2.HasValue && num2.Value == 1)
				{
					chosenCt = cs;
					break;
				}
			}
			catch
			{
			}
		}
		_logger.LogInformation("ÃƒÆ’Ã¢â‚¬Â¡ek raporu iÃƒÆ’Ã‚Â§in belirlenen tablolar: CSCARD={CsTable}, CSTRANS={CtTable}", chosenCs, chosenCt);
		return (csTable: chosenCs, ctTable: chosenCt);
	}

	private async Task<HashSet<string>> GetTableColumnsDirectAsync(SqlConnection connection, SqlTransaction? transaction, string tableName, CancellationToken ct)
	{
		HashSet<string> columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			if (connection.State != ConnectionState.Open)
			{
				await connection.OpenAsync(ct);
			}
			using SqlCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			string sanitizedTable = tableName.Replace("[", "").Replace("]", "");
			cmd.CommandText = "SELECT TOP 0 * FROM [" + sanitizedTable + "]";
			using SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync(ct);
			for (int i = 0; i < sqlDataReader.FieldCount; i++)
			{
				columnNames.Add(sqlDataReader.GetName(i));
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{TableName} tablosunun sütun yapısı okunamadı.", tableName);
		}
		return columnNames;
	}

	private async Task<string> ResolveStockSourceTableOrViewAsync(IDbConnection connection, string firmNo, string periodNo, CancellationToken ct)
	{
		string[] array = new string[5]
		{
			$"LV_{firmNo}_{periodNo}_STINVTOT",
			"LV_" + firmNo + "_STINVTOT",
			$"LV_{firmNo}_{periodNo}_GNTOTST",
			"LV_" + firmNo + "_GNTOTST",
			$"LG_{firmNo}_{periodNo}_STINVTOT"
		};
		string[] array2 = array;
		foreach (string candidate in array2)
		{
			try
			{
				object obj = await connection.ExecuteScalarAsync<object>(new CommandDefinition("SELECT OBJECT_ID(@CandidateName)", new
				{
					CandidateName = candidate
				}, null, null, null, CommandFlags.Buffered, ct));
				if (obj != null && obj != DBNull.Value)
				{
					_logger.LogInformation("Stok verisi iÃƒÆ’Ã‚Â§in hedef View/Tablo kaynaÃƒâ€Ã…\u00b8Ãƒâ€Ã‚Â± seÃƒÆ’Ã‚Â§ildi: {Candidate}", candidate);
					return candidate;
				}
			}
			catch (Exception exception)
			{
				_logger.LogDebug(exception, "{Candidate} kaynaÃƒâ€Ã…\u00b8Ãƒâ€Ã‚Â± kontrol edilirken atlandÃƒâ€Ã‚Â±.", candidate);
			}
		}
		return $"LG_{firmNo}_{periodNo}_STINVTOT";
	}

	public async Task<int?> GetFirmLogicalRefAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string tableName = "LG_" + firmNo + "_CLCARD";
		string pattern = BuildSqlPattern(firmCode);
		string commandText = "\r\n            SELECT TOP 1 LOGICALREF \r\n            FROM " + tableName + " \r\n            WHERE CODE = @FirmCode OR CODE LIKE @Pattern OR DEFINITION_ LIKE @Pattern\r\n            ORDER BY CASE WHEN CODE = @FirmCode THEN 1 ELSE 2 END";
		using IDbConnection connection = CreateConnection();
		try
		{
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				FirmCode = firmCode,
				Pattern = pattern
			}, null, null, null, CommandFlags.Buffered, ct);
			return await connection.QueryFirstOrDefaultAsync<int?>(command);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{TableName} tablosundan Cari sorgulanÃƒâ€žÃ‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.", tableName);
			return null;
		}
	}

	public async Task<LogoErpFirmDetailDto?> GetFirmDetailCardAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string clTable = "LG_" + firmNo + "_CLCARD";
		string pattern = BuildSqlPattern(firmCode);
		string sql = $@"
            SELECT TOP 1
                CODE AS FirmCode,
                DEFINITION_ AS FirmName,
                ISNULL(TAXOFFICE, '') AS TaxOffice,
                ISNULL(TAXNR, ISNULL(TCKNO, '')) AS TaxNumber,
                ISNULL(TELNRS1, ISNULL(TELNRS2, '')) AS Phone,
                ISNULL(EMAILADDR, '') AS Email,
                ISNULL(ADDR1, '') + ' ' + ISNULL(ADDR2, '') AS Address,
                ISNULL(CITY, '') AS City
            FROM {clTable} WITH (NOLOCK)
            WHERE CODE = @FirmCode OR CODE LIKE @Pattern OR DEFINITION_ LIKE @Pattern
            ORDER BY CASE WHEN CODE = @FirmCode THEN 1 ELSE 2 END";
		using IDbConnection connection = CreateConnection();
		try
		{
			var command = new CommandDefinition(sql, new { FirmCode = firmCode, Pattern = pattern }, null, null, null, CommandFlags.Buffered, ct);
			return await connection.QueryFirstOrDefaultAsync<LogoErpFirmDetailDto>(command);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{clTable} tablosundan Cari Detay Kartı sorgulanırken hata oluştu.", clTable);
			return null;
		}
	}

	public async Task<LogoErpItemDetails?> GetItemDetailsAsync(string productCode, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string itemsTable = "LG_" + firmNo + "_ITEMS";
		string unitTable = "LG_" + firmNo + "_UNITSETL";
		string pattern = BuildSqlPattern(productCode);
		string commandText = $"\r\n            SELECT TOP 1\r\n                I.LOGICALREF AS ItemLogicalRef,\r\n                ISNULL(\r\n                    (SELECT TOP 1 LOGICALREF FROM {unitTable} U WHERE U.UNITSETREF = I.UNITSETREF AND (U.MAINUNIT = 1 OR U.LINENR = 1)),\r\n                    (SELECT TOP 1 LOGICALREF FROM {unitTable} U WHERE U.UNITSETREF = I.UNITSETREF)\r\n                ) AS UnitRef,\r\n                ISNULL(I.UNITSETREF, 0) AS UnitSetRef,\r\n                ISNULL(I.VAT, 20) AS Vat,\r\n                ISNULL(I.NAME, '') AS ItemName\r\n            FROM {itemsTable} I\r\n            WHERE I.CODE = @ProductCode OR I.CODE LIKE @Pattern OR I.NAME LIKE @Pattern\r\n            ORDER BY CASE WHEN I.CODE = @ProductCode THEN 1 ELSE 2 END";
		using IDbConnection connection = CreateConnection();
		try
		{
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				ProductCode = productCode,
				Pattern = pattern
			}, null, null, null, CommandFlags.Buffered, ct);
			return await connection.QueryFirstOrDefaultAsync<LogoErpItemDetails>(command);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{ItemsTable} ve {UnitTable} tablolarÃƒâ€Ã‚Â±ndan Malzeme sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.", itemsTable, unitTable);
			return null;
		}
	}

	public async Task<(int ficheRef, int lineRef, string ficheNo)> CreateErpOrderTransactionAsync(int clientRef, LogoErpItemDetails itemDetails, decimal amount, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - HÃƒâ€Ã‚Â±zlÃƒâ€Ã‚Â± SipariÃƒâ€¦Ã…\u00b8", int? salesmanRef = null, decimal? unitPrice = null, CancellationToken ct = default(CancellationToken))
	{
		LogoErpOrderLineInput item = new LogoErpOrderLineInput
		{
			ProductCode = "",
			ItemDetails = itemDetails,
			Amount = amount,
			UnitPrice = unitPrice
		};
		var (item2, source, item3) = await CreateMultiLineErpOrderTransactionAsync(clientRef, new List<LogoErpOrderLineInput> { item }, firmNo, periodNo, warehouseNo, description, salesmanRef, ct);
		return (ficheRef: item2, lineRef: source.FirstOrDefault(), ficheNo: item3);
	}

	public async Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateMultiLineErpOrderTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - HÃƒâ€Ã‚Â±zlÃƒâ€Ã‚Â± SipariÃƒâ€¦Ã…\u00b8", int? salesmanRef = null, CancellationToken ct = default(CancellationToken))
	{
		if (lineInputs == null || lineInputs.Count == 0)
		{
			throw new ArgumentException("SipariÃƒâ€¦Ã…\u00b8 iÃƒÆ’Ã‚Â§in en az 1 satÃƒâ€Ã‚Â±r girmelisiniz.", "lineInputs");
		}
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string ficheTable = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string lineTable = $"LG_{firmNo}_{periodNo}_ORFLINE";
		string stinvTable = $"LG_{firmNo}_{periodNo}_STINVTOT";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			string prefix = _configuration["LogoErp:OrderFichePrefix"] ?? "SIP";
			string ficheNo = await GetNextBotFicheNoAsync(connection, transaction, ficheTable, prefix, ct);
			HashSet<string> ficheColumns = await GetTableColumnsDirectAsync(connection, transaction, ficheTable, ct);
			string logoTimeCalcSql = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))";
			List<string> fCols = new List<string> { "TRCODE", "FICHENO", "DATE_", "CLIENTREF", "SOURCEINDEX", "SOURCECOSTGRP", "GENEXP1", "STATUS" };
			List<string> fVals = new List<string> { "1", "@FicheNo", "CAST(GETDATE() AS DATE)", "@ClientRef", "@SourceIndex", "0", "@GenExp", "1" };
			AddFicheColIf("GENEXCTYP", "2");
			AddFicheColIf("LINEEXCTYP", "0");
			AddFicheColIf("RECSTATUS", "1");
			AddFicheColIf("APPROVE", "0");
			AddFicheColIf("WFSTATUS", "0");
			AddFicheColIf("PRINTCNT", "0");
			AddFicheColIf("SENDCNT", "0");
			AddFicheColIf("REPORTRATE", "1");
			AddFicheColIf("REPORTNET", "0");
			AddFicheColIf("CLOSED", "0");
			AddFicheColIf("CANCELLED", "0");
			AddFicheColIf("LINECNT", "@LineCount");
			AddFicheColIf("NETTOTAL", "0");
			AddFicheColIf("GROSSTOTAL", "0");
			AddFicheColIf("TOTALDISCOUNTS", "0");
			AddFicheColIf("TOTALDISCOUNTED", "0");
			AddFicheColIf("TOTALEXPENSES", "0");
			AddFicheColIf("TOTALPROMOTIONS", "0");
			AddFicheColIf("TOTALVAT", "0");
			AddFicheColIf("ACCOUNTREF", "0");
			AddFicheColIf("CENTERREF", "0");
			if (salesmanRef.HasValue && salesmanRef.Value > 0)
			{
				AddFicheColIf("SALESMANREF", salesmanRef.Value.ToString());
			}
			else
			{
				AddFicheColIf("SALESMANREF", "0");
			}
			AddFicheColIf("EINVOICE", "1");
			AddFicheColIf("INSTEADOFDESP", "1");
			AddFicheColIf("NOTIFYCRDREF", "2");
			AddFicheColIf("GUID", "NEWID()");
			if (ficheColumns.Contains("TIME_"))
			{
				fCols.Add("TIME_");
				fVals.Add(logoTimeCalcSql);
			}
			else if (ficheColumns.Contains("FTIME"))
			{
				fCols.Add("FTIME");
				fVals.Add(logoTimeCalcSql);
			}
			AddFicheColIf("CAPIBLOCK_CREATEDBY", "1");
			AddFicheColIf("CAPIBLOCK_CREADEDDATE", "GETDATE()");
			AddFicheColIf("CAPIBLOCK_CREDATE", "GETDATE()");
			AddFicheColIf("CAPIBLOCK_CREATEDHOUR", "DATEPART(hour, GETDATE())");
			AddFicheColIf("CAPIBLOCK_CREATEDMIN", "DATEPART(minute, GETDATE())");
			AddFicheColIf("CAPIBLOCK_CREATEDSEC", "DATEPART(second, GETDATE())");
			AddFicheColIf("CAPIBLOCK_MODIFIEDBY", "0");
			AddFicheColIf("CAPIBLOCK_MODIFIEDHOUR", "0");
			AddFicheColIf("CAPIBLOCK_MODIFIEDMIN", "0");
			AddFicheColIf("CAPIBLOCK_MODIFIEDSEC", "0");
			string commandText = $"INSERT INTO {ficheTable} ({string.Join(", ", fCols)}) VALUES ({string.Join(", ", fVals)}); SELECT CAST(SCOPE_IDENTITY() as int);";
			var parameters = new
			{
				FicheNo = ficheNo,
				ClientRef = clientRef,
				SourceIndex = warehouseNo,
				GenExp = description,
				LineCount = lineInputs.Count
			};
			CancellationToken cancellationToken = ct;
			int ficheRef = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText, parameters, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			HashSet<string> lineColumns = await GetTableColumnsDirectAsync(connection, transaction, lineTable, ct);
			string orfHeaderRefCol = (lineColumns.Contains("ORDFICHEREF") ? "ORDFICHEREF" : (lineColumns.Contains("ORFICHEREF") ? "ORFICHEREF" : (lineColumns.Contains("ORFLINEREF") ? "ORFLINEREF" : "ORFICHEREF")));
			string orfUnitRefCol = (lineColumns.Contains("UOMREF") ? "UOMREF" : (lineColumns.Contains("UNITREF") ? "UNITREF" : "UOMREF"));
			_logger.LogInformation("CanlÃƒâ€Ã‚Â± SÃƒÆ’Ã‚Â¼tun ÃƒÆ’Ã¢â‚¬Â¡ÃƒÆ’Ã‚Â¶zÃƒÆ’Ã‚Â¼mlemesi - ORFLINE ({Count} col): HeaderRef={orfHeaderRefCol}, UnitRef={orfUnitRefCol}", lineColumns.Count, orfHeaderRefCol, orfUnitRefCol);
			HashSet<string> stinvColumns = await GetTableColumnsDirectAsync(connection, transaction, stinvTable, ct);
			List<int> createdLineRefs = new List<int>();
			decimal totalRawGrossSum = 0m;
			decimal totalLineDiscounts = 0m;
			decimal totalLineNetSum = 0m;
			decimal totalVatSum = 0m;
			for (int i = 0; i < lineInputs.Count; i++)
			{
				LogoErpOrderLineInput input = lineInputs[i];
				int lineNr = i + 1;
				decimal valueOrDefault = input.UnitPrice.GetValueOrDefault();
				decimal num = input.Amount * valueOrDefault;
				decimal num2 = num;
				decimal num3 = 0m;
				if (input.DiscountRate > 0m)
				{
					num3 = num * (input.DiscountRate / 100m);
					num2 = num - num3;
				}
				decimal num4 = num2 * (decimal)input.ItemDetails.Vat / 100m;
				totalRawGrossSum += num;
				totalLineDiscounts += num3;
				totalLineNetSum += num2;
				totalVatSum += num4;
				List<string> lCols = new List<string>
				{
					orfHeaderRefCol, "CLIENTREF", "STOCKREF", "LINETYPE", "TRCODE", "DATE_", "AMOUNT", "PRICE", "VAT", orfUnitRefCol,
					"USREF", "UINFO1", "UINFO2", "SOURCEINDEX", "STATUS"
				};
				List<string> lVals = new List<string>
				{
					"@OrficheRef", "@ClientRef", "@StockRef", "0", "1", "CAST(GETDATE() AS DATE)", "@Amount", "@Price", "@Vat", "@UnitRef",
					"@UsRef", "1", "1", "@SourceIndex", "1"
				};
				AddLineColIf("RECSTATUS", "1");
				AddLineColIf("APPROVE", "0");
				AddLineColIf("WFSTATUS", "0");
				if (salesmanRef.HasValue && salesmanRef.Value > 0)
				{
					AddLineColIf("SALESMANREF", salesmanRef.Value.ToString());
				}
				else
				{
					AddLineColIf("SALESMANREF", "0");
				}
				AddLineColIf("READONLY", "0");
				AddLineColIf("DETLINE", "0");
				AddLineColIf("DORESERVE", "0");
				AddLineColIf("INUSE", "0");
				AddLineColIf("BILLEDITEM", "0");
				AddLineColIf("CPSTFLAG", "0");
				AddLineColIf("TRGFLAG", "0");
				AddLineColIf("CLOSED", "0");
				AddLineColIf("CANCELLED", "0");
				AddLineColIf("LINEEXP", "@LineExp");
				AddLineColIf("REPORTRATE", "1");
				AddLineColIf("LINENET", "@LineTotal");
				AddLineColIf("TOTAL", "@GrossTotal");
				AddLineColIf("DISTOT", "@DiscAmnt");
				AddLineColIf("DISTDISC", "@DiscAmnt");
				AddLineColIf("SHIPPEDAMOUNT", "0");
				AddLineColIf("VATAMNT", "@VatAmnt");
				AddLineColIf("VATMATRAH", "@LineTotal");
				AddLineColIf("TRCURR", "1");
				AddLineColIf("ADDTAXEFFECTKDV", "1");
				AddLineColIf("DUEDATE", "CAST(GETDATE() AS DATE)");
				AddLineColIf("ORGDUEDATE", "CAST(GETDATE() AS DATE)");
				AddLineColIf("ORGAMOUNT", "@Amount");
				AddLineColIf("DISCPER", "@DiscPer");
				AddLineColIf("DISPER", "@DiscPer");
				if (lineColumns.Contains("TIME_"))
				{
					lCols.Add("TIME_");
					lVals.Add(logoTimeCalcSql);
				}
				else if (lineColumns.Contains("FTIME"))
				{
					lCols.Add("FTIME");
					lVals.Add(logoTimeCalcSql);
				}
				if (lineColumns.Contains("LINENR"))
				{
					lCols.Add("LINENR");
					lVals.Add("@LineNr");
				}
				AddLineColIf("CAPIBLOCK_CREATEDBY", "1");
				AddLineColIf("CAPIBLOCK_CREADEDDATE", "GETDATE()");
				AddLineColIf("CAPIBLOCK_CREDATE", "GETDATE()");
				AddLineColIf("CAPIBLOCK_CREATEDHOUR", "DATEPART(hour, GETDATE())");
				AddLineColIf("CAPIBLOCK_CREATEDMIN", "DATEPART(minute, GETDATE())");
				AddLineColIf("CAPIBLOCK_CREATEDSEC", "DATEPART(second, GETDATE())");
				string commandText2 = $"INSERT INTO {lineTable} ({string.Join(", ", lCols)}) VALUES ({string.Join(", ", lVals)}); SELECT CAST(SCOPE_IDENTITY() as int);";
				var parameters2 = new
				{
					OrficheRef = ficheRef,
					ClientRef = clientRef,
					StockRef = input.ItemDetails.ItemLogicalRef,
					Amount = input.Amount,
					Price = valueOrDefault,
					GrossTotal = num,
					LineTotal = num2,
					DiscAmnt = num3,
					DiscPer = input.DiscountRate,
					VatAmnt = num4,
					Vat = input.ItemDetails.Vat,
					UnitRef = input.ItemDetails.UnitRef,
					UsRef = input.ItemDetails.UnitSetRef,
					SourceIndex = warehouseNo,
					LineNr = lineNr,
					LineExp = description
				};
				cancellationToken = ct;
				createdLineRefs.Add(await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText2, parameters2, transaction, null, null, CommandFlags.Buffered, cancellationToken)));
				List<string> list = new List<string>();
				if (stinvColumns.Contains("RESERVED"))
				{
					list.Add("RESERVED = ISNULL(RESERVED, 0) + @Amount");
				}
				if (stinvColumns.Contains("ACTPORD"))
				{
					list.Add("ACTPORD = ISNULL(ACTPORD, 0) + @Amount");
				}
				else if (stinvColumns.Contains("ACTPEOP"))
				{
					list.Add("ACTPEOP = ISNULL(ACTPEOP, 0) + @Amount");
				}
				if (list.Count <= 0)
				{
					continue;
				}
				string setSql = string.Join(", ", list);
				List<string> insertCols = new List<string> { "STOCKREF", "INVENNO" };
				List<string> insertVals = new List<string> { "@StockRef", "@InvenNo" };
				if (stinvColumns.Contains("RESERVED"))
				{
					insertCols.Add("RESERVED");
					insertVals.Add("@Amount");
				}
				if (stinvColumns.Contains("ACTPORD"))
				{
					insertCols.Add("ACTPORD");
					insertVals.Add("@Amount");
				}
				else if (stinvColumns.Contains("ACTPEOP"))
				{
					insertCols.Add("ACTPEOP");
					insertVals.Add("@Amount");
				}
				int[] array = new int[2] { warehouseNo, -1 };
				int[] array2 = array;
				foreach (int invenNo in array2)
				{
					string commandText3 = $"\r\n                            IF EXISTS (SELECT 1 FROM {stinvTable} WHERE STOCKREF = @StockRef AND INVENNO = @InvenNo)\r\n                            BEGIN\r\n                                UPDATE {stinvTable} SET {setSql} WHERE STOCKREF = @StockRef AND INVENNO = @InvenNo\r\n                            END\r\n                            ELSE\r\n                            BEGIN\r\n                                INSERT INTO {stinvTable} ({string.Join(", ", insertCols)}) VALUES ({string.Join(", ", insertVals)})\r\n                            END";
					var parameters3 = new
					{
						StockRef = input.ItemDetails.ItemLogicalRef,
						InvenNo = invenNo,
						Amount = input.Amount
					};
					try
					{
						cancellationToken = ct;
						await connection.ExecuteAsync(new CommandDefinition(commandText3, parameters3, transaction, null, null, CommandFlags.Buffered, cancellationToken));
					}
					catch (Exception exception)
					{
						_logger.LogWarning(exception, "STINVTOT Rezerve stok gÃƒÆ’Ã‚Â¼ncellenirken ikincil uyarÃƒâ€Ã‚Â± alÃƒâ€Ã‚Â±ndÃƒâ€Ã‚Â±.");
					}
				}
				void AddLineColIf(string colName, string val)
				{
					if (lineColumns.Contains(colName))
					{
						lCols.Add(colName);
						lVals.Add(val);
					}
				}
			}
			if (totalRawGrossSum > 0m)
			{
				try
				{
					decimal netTotal = totalLineNetSum + totalVatSum;
					string commandText4 = "UPDATE " + ficheTable + " SET NETTOTAL = @NetTotal, GROSSTOTAL = @GrossTotal, TOTALDISCOUNTS = @TotalDiscounts, TOTALDISCOUNTED = @TotalDiscounted, TOTALVAT = @TotalVat WHERE LOGICALREF = @FicheRef";
					var parameters4 = new
					{
						NetTotal = netTotal,
						GrossTotal = totalRawGrossSum,
						TotalDiscounts = totalLineDiscounts,
						TotalDiscounted = totalLineNetSum,
						TotalVat = totalVatSum,
						FicheRef = ficheRef
					};
					cancellationToken = ct;
					await connection.ExecuteAsync(new CommandDefinition(commandText4, parameters4, transaction, null, null, CommandFlags.Buffered, cancellationToken));
				}
				catch (Exception exception2)
				{
					_logger.LogWarning(exception2, "ORFICHE baÃƒâ€¦Ã…\u00b8lÃƒâ€Ã‚Â±k toplamlarÃƒâ€Ã‚Â± gÃƒÆ’Ã‚Â¼ncellenirken ikincil uyarÃƒâ€Ã‚Â± alÃƒâ€Ã‚Â±ndÃƒâ€Ã‚Â±.");
				}
			}
			string historyTable = $"LG_{firmNo}_{periodNo}_HISTORY";
			try
			{
				if ((await GetTableColumnsDirectAsync(connection, transaction, historyTable, ct)).Count > 0)
				{
					List<string> values = new List<string> { "TABLEID", "DATAREF", "MODIFIEDBY", "MODIFIEDDATE", "MODIFIEDHOUR", "MODIFIEDMIN", "MODIFIEDSEC", "USERNAME", "MODIFTXT" };
					List<string> values2 = new List<string> { "12", "@FicheRef", "1", "GETDATE()", "DATEPART(hour, GETDATE())", "DATEPART(minute, GETDATE())", "DATEPART(second, GETDATE())", "'BOT'", "'Telegram Bot (@LogoCrmBot)'" };
					string commandText5 = $"INSERT INTO {historyTable} ({string.Join(", ", values)}) VALUES ({string.Join(", ", values2)})";
					var parameters5 = new
					{
						FicheRef = ficheRef
					};
					cancellationToken = ct;
					await connection.ExecuteAsync(new CommandDefinition(commandText5, parameters5, transaction, null, null, CommandFlags.Buffered, cancellationToken));
				}
			}
			catch (Exception exception3)
			{
				_logger.LogWarning(exception3, "HISTORY kaydÃƒâ€Ã‚Â± oluÃƒâ€¦Ã…\u00b8turulurken atlandÃƒâ€Ã‚Â±.");
			}
			await transaction.CommitAsync(ct);
			_logger.LogInformation("Logo ERP ÃƒÆ’Ã¢â‚¬â€œneri SipariÃƒâ€¦Ã…\u00b8i (STATUS = 1, RECSTATUS = 1, CAPIBLOCK_CREADEDDATE & HISTORY) SQL Transaction baÃƒâ€¦Ã…\u00b8arÃƒâ€Ã‚Â±yla commit edildi. FicheRef: {FicheRef}, FicheNo: {FicheNo}, LineCount: {LineCount}", ficheRef, ficheNo, lineInputs.Count);
			return (ficheRef: ficheRef, lineRefs: createdLineRefs, ficheNo: ficheNo);
			void AddFicheColIf(string colName, string val)
			{
				if (ficheColumns.Contains(colName))
				{
					fCols.Add(colName);
					fVals.Add(val);
				}
			}
		}
		catch (Exception exception4)
		{
			_logger.LogError(exception4, "Logo ERP SipariÃƒâ€¦Ã…\u00b8 Transaction sÃƒâ€Ã‚Â±rasÃƒâ€Ã‚Â±nda hata oluÃƒâ€¦Ã…\u00b8tu. Rollback yapÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±yor.");
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	private async Task<string> GetNextBotFicheNoAsync(SqlConnection connection, SqlTransaction? transaction, string tableName, string prefix = "FAT", CancellationToken ct = default(CancellationToken), params string[] extraTables)
	{
		if (string.IsNullOrWhiteSpace(prefix))
		{
			prefix = "FAT";
		}
		if (prefix.StartsWith("BOT", StringComparison.OrdinalIgnoreCase))
		{
			prefix = ((prefix.Length > 3) ? prefix.Substring(3) : ((tableName.IndexOf("STFICHE", StringComparison.OrdinalIgnoreCase) >= 0) ? "IRS" : ((tableName.IndexOf("ORFICHE", StringComparison.OrdinalIgnoreCase) < 0) ? "FAT" : "SIP")));
		}
		if (string.IsNullOrWhiteSpace(prefix))
		{
			prefix = "FAT";
		}
		string text = _configuration["LogoErp:DocumentYear"] ?? DateTime.Now.Year.ToString();
		if (string.IsNullOrWhiteSpace(text))
		{
			text = DateTime.Now.Year.ToString();
		}
		string fullPrefix = prefix;
		if (!prefix.EndsWith(text) && prefix.Length <= 4)
		{
			fullPrefix = prefix + text;
		}
		long maxNum = 0L;
		List<string> list = new List<string> { tableName };
		if (extraTables != null)
		{
			list.AddRange(extraTables);
		}
		foreach (string item in list)
		{
			try
			{
				string commandText = "SELECT FICHENO FROM " + item + " WITH (NOLOCK) WHERE FICHENO LIKE @PrefixPattern";
				foreach (string item2 in await connection.QueryAsync<string>(new CommandDefinition(commandText, new
				{
					PrefixPattern = fullPrefix + "%"
				}, transaction, null, null, CommandFlags.Buffered, ct)))
				{
					if (!string.IsNullOrEmpty(item2) && item2.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase) && item2.Length > fullPrefix.Length && long.TryParse(item2.Substring(fullPrefix.Length), out var result) && result > maxNum)
					{
						maxNum = result;
					}
				}
			}
			catch
			{
			}
		}
		long num = maxNum + 1;
		int num2 = Math.Max(1, 16 - fullPrefix.Length);
		string text2 = num.ToString().PadLeft(num2, '0');
		if (text2.Length > num2)
		{
			text2 = text2.Substring(text2.Length - num2);
		}
		string text3 = fullPrefix + text2;
		return (text3.Length > 16) ? text3.Substring(0, 16) : text3;
	}

	private static string Trunc(string? value, int maxLen)
	{
		string text;
		if ((value ?? "").Length <= maxLen)
		{
			text = value;
			if (text == null)
			{
				return "";
			}
		}
		else
		{
			text = (value ?? "").Substring(0, maxLen);
		}
		return text;
	}

	private static string BuildTempTableUpdateSql(string tempTableName, HashSet<string> availableColumns, Dictionary<string, string> desiredUpdates)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> desiredUpdate in desiredUpdates)
		{
			string key = desiredUpdate.Key;
			string value = desiredUpdate.Value;
			if (key.Equals("TIME_OR_FTIME", StringComparison.OrdinalIgnoreCase))
			{
				if (availableColumns.Contains("TIME_"))
				{
					list.Add("TIME_ = " + value);
				}
				else if (availableColumns.Contains("FTIME"))
				{
					list.Add("FTIME = " + value);
				}
			}
			else if (availableColumns != null && availableColumns.Count > 0 ? availableColumns.Contains(key) : !key.Equals("SHIPBYCODE", StringComparison.OrdinalIgnoreCase))
			{
				list.Add(key + " = " + value);
			}
		}
		if (list.Count == 0)
		{
			return "";
		}
		return $"UPDATE {tempTableName} SET {string.Join(", ", list)};";
	}

	public async Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateErpDispatchTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - SatÄ±ÅŸ Ä°rsaliyesi", int? salesmanRef = null, string? carrierOrDriverInfo = null, CancellationToken ct = default(CancellationToken))
	{
		if (lineInputs == null || lineInputs.Count == 0)
		{
			throw new ArgumentException("Ä°rsaliye iÃ§in en az 1 satÄ±r girmelisiniz.", "lineInputs");
		}
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string ficheTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string lineTable = $"LG_{firmNo}_{periodNo}_STLINE";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			string text = $"LG_{firmNo}_{periodNo}_INVOICE";
			string dispatchPrefix = _configuration["LogoErp:DispatchFichePrefix"] ?? "IRS";
			string ficheNo = await GetNextBotFicheNoAsync(connection, transaction, ficheTable, dispatchPrefix, ct, text);
			HashSet<string> ficheColumns = await GetTableColumnsDirectAsync(connection, transaction, ficheTable, ct);
			HashSet<string> lineColumns = await GetTableColumnsDirectAsync(connection, transaction, lineTable, ct);
			string commandText = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE FICHENO LIKE @DispatchPrefix AND TRCODE = 8 AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
			var parameters = new
			{
				DispatchPrefix = dispatchPrefix + "%"
			};
			CancellationToken cancellationToken = ct;
			int refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText, parameters, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			if (refStFicheId == 0)
			{
				string commandText2 = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE TRCODE = 8 AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText2, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStFicheId == 0)
			{
				string commandText3 = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE TRCODE = 8 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText3, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStFicheId == 0)
			{
				throw new InvalidOperationException("Logo ERP'de klonlamak iÃ§in Ã¶rnek bir satÄ±ÅŸ irsaliyesi kaydÄ± bulunamadÄ±. LÃ¼tfen Ã¶nce Logo ERP'de manuel olarak en az bir satÄ±ÅŸ irsaliyesi oluÅŸturun.");
			}
			int refStLineId = 0;
			if (refStFicheId > 0)
			{
				string commandText4 = "SELECT TOP 1 LOGICALREF FROM " + lineTable + " WITH (NOLOCK) WHERE STFICHEREF = @RefStFicheId AND LINETYPE = 0 ORDER BY LOGICALREF DESC";
				var parameters2 = new
				{
					RefStFicheId = refStFicheId
				};
				cancellationToken = ct;
				refStLineId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText4, parameters2, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStLineId == 0)
			{
				string commandText5 = "SELECT TOP 1 LOGICALREF FROM " + lineTable + " WITH (NOLOCK) WHERE TRCODE = 8 AND LINETYPE = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStLineId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText5, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			List<int> createdLineRefs = new List<int>();
			decimal totalRawGrossSum = 0m;
			decimal totalLineDiscounts = 0m;
			decimal totalLineNetSum = 0m;
			decimal totalVatSum = 0m;
			string value = BuildTempTableUpdateSql("#TempFiche", ficheColumns, new Dictionary<string, string>
			{
				["FICHENO"] = "@FicheNo",
				["GUID"] = "NEWID()",
				["DATE_"] = "CAST(GETDATE() AS DATE)",
				["TIME_OR_FTIME"] = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))",
				["CLIENTREF"] = "@ClientRef",
				["RECVREF"] = "@ClientRef",
				["GENEXP1"] = "@GenExp",
				["GENEXP2"] = "ISNULL(@CarrierInfo, GENEXP2)",
				["SOURCEINDEX"] = "@SourceIndex",
				["SALESMANREF"] = "CASE WHEN @SalesmanRef IS NOT NULL AND @SalesmanRef > 0 THEN @SalesmanRef ELSE SALESMANREF END",
				["STATUS"] = "1",
				["CANCELLED"] = "0",
				["BILLED"] = "0",
				["INVOICEREF"] = "0",
				["SHIPBYCODE"] = "@CarrierInfo",
				["TRCODE"] = "8",
				["IOCODE"] = "4",
				["EDESPATCH"] = "1",
				["EDESPSTATUS"] = "1",
				["EINVOICETYP"] = "1",
				["PROFILEID"] = "1",
				["RECSTATUS"] = "1",
				["CAPIBLOCK_CREATEDBY"] = "1",
				["CAPIBLOCK_CREADEDDATE"] = "GETDATE()"
			});
			string commandText6 = $"\r\n                SET NOCOUNT ON;\r\n                DECLARE @NewId INT;\r\n                IF OBJECT_ID('tempdb..#TempFiche') IS NOT NULL DROP TABLE #TempFiche;\r\n                SELECT * INTO #TempFiche FROM {ficheTable} WHERE LOGICALREF = @RefStFicheId;\r\n                ALTER TABLE #TempFiche DROP COLUMN LOGICALREF;\r\n                {value}\r\n                INSERT INTO {ficheTable} SELECT * FROM #TempFiche;\r\n                SET @NewId = CAST(SCOPE_IDENTITY() AS INT);\r\n                DROP TABLE #TempFiche;\r\n                SELECT @NewId;";
			var parameters3 = new
			{
				RefStFicheId = refStFicheId,
				FicheNo = ficheNo,
				ClientRef = clientRef,
				GenExp = Trunc(description, 100),
				CarrierInfo = Trunc(carrierOrDriverInfo ?? "", 100),
				SourceIndex = warehouseNo,
				SalesmanRef = salesmanRef
			};
			cancellationToken = ct;
			int ficheRef = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText6, parameters3, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			string updateLineSqlTemplate = BuildTempTableUpdateSql("#TempLine", lineColumns, new Dictionary<string, string>
			{
				["STFICHEREF"] = "@StFicheRef",
				["INVOICEREF"] = "0",
				["STOCKREF"] = "@StockRef",
				["CLIENTREF"] = "@ClientRef",
				["DATE_"] = "CAST(GETDATE() AS DATE)",
				["TIME_OR_FTIME"] = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))",
				["SALESMANREF"] = "CASE WHEN @SalesmanRef IS NOT NULL AND @SalesmanRef > 0 THEN @SalesmanRef ELSE SALESMANREF END",
				["AMOUNT"] = "@Amount",
				["PRICE"] = "@Price",
				["VAT"] = "@Vat",
				["UOMREF"] = "@UnitRef",
				["USREF"] = "@UsRef",
				["LINENET"] = "@LineTotal",
				["TOTAL"] = "@GrossTotal",
				["DISCPER"] = "@DiscPer",
				["DISTDISC"] = "@DiscAmnt",
				["DISTOT"] = "@DiscAmnt",
				["VATAMNT"] = "@VatAmnt",
				["VATMATRAH"] = "@LineTotal",
				["SOURCEINDEX"] = "@SourceIndex",
				["DESTINDEX"] = "@SourceIndex",
				["LINENR"] = "@LineNr",
				["STFICHELNNO"] = "@LineNr",
				["CANCELLED"] = "0",
				["STATUS"] = "1",
				["LINETYPE"] = "0",
				["TRCODE"] = "8",
				["IOCODE"] = "4",
				["BILLED"] = "0",
				["BILLEDITEM"] = "0",
				["UINFO1"] = "1",
				["UINFO2"] = "1",
				["UNITCONV1"] = "1",
				["UNITCONV2"] = "1",
				["RECSTATUS"] = "1"
			});
			for (int i = 0; i < lineInputs.Count; i++)
			{
				LogoErpOrderLineInput logoErpOrderLineInput = lineInputs[i];
				int lineNr = i + 1;
				decimal valueOrDefault = logoErpOrderLineInput.UnitPrice.GetValueOrDefault();
				decimal num = logoErpOrderLineInput.Amount * valueOrDefault;
				decimal num2 = num;
				decimal num3 = 0m;
				if (logoErpOrderLineInput.DiscountRate > 0m)
				{
					num3 = num * (logoErpOrderLineInput.DiscountRate / 100m);
					num2 = num - num3;
				}
				decimal num4 = num2 * (decimal)logoErpOrderLineInput.ItemDetails.Vat / 100m;
				totalRawGrossSum += num;
				totalLineDiscounts += num3;
				totalLineNetSum += num2;
				totalVatSum += num4;
				if (refStLineId > 0)
				{
					string commandText7 = $"\r\n                        SET NOCOUNT ON;\r\n                        DECLARE @NewId INT;\r\n                        IF OBJECT_ID('tempdb..#TempLine') IS NOT NULL DROP TABLE #TempLine;\r\n                        SELECT * INTO #TempLine FROM {lineTable} WHERE LOGICALREF = @RefStLineId;\r\n                        ALTER TABLE #TempLine DROP COLUMN LOGICALREF;\r\n                        {updateLineSqlTemplate}\r\n                        INSERT INTO {lineTable} SELECT * FROM #TempLine;\r\n                        SET @NewId = CAST(SCOPE_IDENTITY() AS INT);\r\n                        DROP TABLE #TempLine;\r\n                        SELECT @NewId;";
					var parameters4 = new
					{
						RefStLineId = refStLineId,
						StFicheRef = ficheRef,
						StockRef = logoErpOrderLineInput.ItemDetails.ItemLogicalRef,
						ClientRef = clientRef,
						SalesmanRef = salesmanRef,
						Amount = logoErpOrderLineInput.Amount,
						Price = valueOrDefault,
						DiscPer = logoErpOrderLineInput.DiscountRate,
						DiscAmnt = num3,
						Vat = logoErpOrderLineInput.ItemDetails.Vat,
						UnitRef = logoErpOrderLineInput.ItemDetails.UnitRef,
						UsRef = logoErpOrderLineInput.ItemDetails.UnitSetRef,
						LineTotal = num2,
						GrossTotal = num,
						VatAmnt = num4,
						SourceIndex = warehouseNo,
						LineNr = lineNr
					};
					cancellationToken = ct;
					createdLineRefs.Add(await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText7, parameters4, transaction, null, null, CommandFlags.Buffered, cancellationToken)));
					continue;
				}
				List<string> values = new List<string>
				{
					"STFICHEREF", "INVOICEREF", "CLIENTREF", "STOCKREF", "LINETYPE", "TRCODE", "DATE_", "AMOUNT", "PRICE", "VAT",
					"UOMREF", "USREF", "UINFO1", "UINFO2", "SOURCEINDEX", "STATUS", "IOCODE", "LINENR", "LINENET", "TOTAL",
					"VATAMNT", "VATMATRAH", "CANCELLED", "BILLED", "DISCPER", "DISTDISC"
				};
				List<string> values2 = new List<string>
				{
					"@StFicheRef", "0", "@ClientRef", "@StockRef", "0", "8", "CAST(GETDATE() AS DATE)", "@Amount", "@Price", "@Vat",
					"@UnitRef", "@UsRef", "1", "1", "@SourceIndex", "1", "4", "@LineNr", "@LineTotal", "@GrossTotal",
					"@VatAmnt", "@LineTotal", "0", "0", "@DiscPer", "@DiscAmnt"
				};
				string commandText8 = $"INSERT INTO {lineTable} ({string.Join(", ", values)}) VALUES ({string.Join(", ", values2)}); SELECT CAST(SCOPE_IDENTITY() as int);";
				var parameters5 = new
				{
					StFicheRef = ficheRef,
					ClientRef = clientRef,
					StockRef = logoErpOrderLineInput.ItemDetails.ItemLogicalRef,
					Amount = logoErpOrderLineInput.Amount,
					Price = valueOrDefault,
					DiscPer = logoErpOrderLineInput.DiscountRate,
					DiscAmnt = num3,
					Vat = logoErpOrderLineInput.ItemDetails.Vat,
					UnitRef = logoErpOrderLineInput.ItemDetails.UnitRef,
					UsRef = logoErpOrderLineInput.ItemDetails.UnitSetRef,
					SourceIndex = warehouseNo,
					LineNr = lineNr,
					LineTotal = num2,
					GrossTotal = num,
					VatAmnt = num4
				};
				cancellationToken = ct;
				createdLineRefs.Add(await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText8, parameters5, transaction, null, null, CommandFlags.Buffered, cancellationToken)));
			}
			decimal netTotal = totalLineNetSum + totalVatSum;
			decimal totalDiscounted = totalRawGrossSum - totalLineDiscounts;
			decimal vatInclGross = totalRawGrossSum + totalVatSum;
			List<string> list = new List<string> { "NETTOTAL = @NetTotal", "GROSSTOTAL = @GrossTotal", "TOTALDISCOUNTS = @TotalDiscounts", "TOTALVAT = @TotalVat" };
			if (ficheColumns.Contains("LINECNT"))
			{
				list.Add("LINECNT = @LineCount");
			}
			if (ficheColumns.Contains("REPORTNET"))
			{
				list.Add("REPORTNET = @NetTotal");
			}
			if (ficheColumns.Contains("TRNET"))
			{
				list.Add("TRNET = @NetTotal");
			}
			if (ficheColumns.Contains("TOTALDISCOUNTED"))
			{
				list.Add("TOTALDISCOUNTED = @TotalDiscounted");
			}
			if (ficheColumns.Contains("VATINCLGROSS"))
			{
				list.Add("VATINCLGROSS = @VatInclGross");
			}
			string commandText9 = $"UPDATE {ficheTable} SET {string.Join(", ", list)} WHERE LOGICALREF = @FicheRef OR RTRIM(FICHENO) LIKE RTRIM(@FicheNo) + '%'";
			var parameters6 = new
			{
				NetTotal = netTotal,
				GrossTotal = totalRawGrossSum,
				TotalDiscounts = totalLineDiscounts,
				TotalDiscounted = totalDiscounted,
				TotalVat = totalVatSum,
				VatInclGross = vatInclGross,
				LineCount = lineInputs.Count,
				FicheRef = ficheRef,
				FicheNo = ficheNo
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText9, parameters6, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			await transaction.CommitAsync(ct);
			_logger.LogInformation("Logo ERP SatÄ±ÅŸ Ä°rsaliyesi (Referans Klonlama) baÅŸarÄ±yla oluÅŸturuldu: FicheRef: {FicheRef}, FicheNo: {FicheNo}", ficheRef, ficheNo);
			return (ficheRef: ficheRef, lineRefs: createdLineRefs, ficheNo: ficheNo);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Logo ERP SatÄ±ÅŸ Ä°rsaliyesi kaydedilirken hata oluÅŸtu.");
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	public async Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateErpInvoiceTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - SatÄ±ÅŸ FaturasÄ±", int? salesmanRef = null, CancellationToken ct = default(CancellationToken))
	{
		if (lineInputs == null || lineInputs.Count == 0)
		{
			throw new ArgumentException("Fatura iÃ§in en az 1 satÄ±r girmelisiniz.", "lineInputs");
		}
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string invTable = $"LG_{firmNo}_{periodNo}_INVOICE";
		string ficheTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string lineTable = $"LG_{firmNo}_{periodNo}_STLINE";
		string clTable = "LG_" + firmNo + "_CLCARD";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			string prefix = _configuration["LogoErp:InvoiceFichePrefix"] ?? "FAT";
			string dispatchPrefix = _configuration["LogoErp:DispatchFichePrefix"] ?? "IRS";
			string invoiceFicheNo = await GetNextBotFicheNoAsync(connection, transaction, invTable, prefix, ct, ficheTable);
			string stFicheNo = await GetNextBotFicheNoAsync(connection, transaction, ficheTable, dispatchPrefix, ct);
			HashSet<string> invColumns = await GetTableColumnsDirectAsync(connection, transaction, invTable, ct);
			HashSet<string> ficheColumns = await GetTableColumnsDirectAsync(connection, transaction, ficheTable, ct);
			HashSet<string> lineColumns = await GetTableColumnsDirectAsync(connection, transaction, lineTable, ct);
			string safeGenExp = Trunc(description, 100);
			string commandText = "SELECT ISNULL(ACCEPTEINV, 0) FROM " + clTable + " WITH (NOLOCK) WHERE LOGICALREF = @ClientRef";
			var parameters = new
			{
				ClientRef = clientRef
			};
			CancellationToken cancellationToken = ct;
			int eInvoiceType = ((await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText, parameters, transaction, null, null, CommandFlags.Buffered, cancellationToken)) == 1) ? 1 : 2);
			string commandText2 = "SELECT TOP 1 LOGICALREF FROM " + invTable + " WITH (NOLOCK) WHERE FICHENO LIKE '000033%' AND TRCODE IN (7, 8) AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
			cancellationToken = ct;
			int refInvoiceId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText2, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			if (refInvoiceId == 0)
			{
				string commandText3 = "SELECT TOP 1 LOGICALREF FROM " + invTable + " WITH (NOLOCK) WHERE TRCODE IN (7, 8) AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refInvoiceId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText3, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refInvoiceId == 0)
			{
				string commandText4 = "SELECT TOP 1 LOGICALREF FROM " + invTable + " WITH (NOLOCK) WHERE TRCODE IN (7, 8) ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refInvoiceId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText4, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refInvoiceId == 0)
			{
				throw new InvalidOperationException("Logo ERP'de klonlamak iÃ§in Ã¶rnek bir satÄ±ÅŸ faturasÄ± kaydÄ± bulunamadÄ±. LÃ¼tfen Ã¶nce Logo ERP'de manuel olarak en az bir satÄ±ÅŸ faturasÄ± oluÅŸturun (Ã–rnek fiÅŸ no: 000033).");
			}
			string commandText5 = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE INVOICEREF = @RefInvoiceId AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
			var parameters2 = new
			{
				RefInvoiceId = refInvoiceId
			};
			cancellationToken = ct;
			int refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText5, parameters2, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			if (refStFicheId == 0)
			{
				string commandText6 = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE TRCODE = 8 AND ISNULL(BILLED, 0) = 1 AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText6, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStFicheId == 0)
			{
				string commandText7 = "SELECT TOP 1 LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE TRCODE = 8 AND ISNULL(CANCELLED, 0) = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStFicheId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText7, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStFicheId == 0)
			{
				throw new InvalidOperationException("Logo ERP'de klonlamak iÃ§in Ã¶rnek bir irsaliye (STFICHE) kaydÄ± bulunamadÄ±. LÃ¼tfen Ã¶nce Logo ERP'de manuel olarak en az bir irsaliye veya fatura oluÅŸturun.");
			}
			int refStLineId = 0;
			if (refStFicheId > 0)
			{
				string commandText8 = "SELECT TOP 1 LOGICALREF FROM " + lineTable + " WITH (NOLOCK) WHERE STFICHEREF = @RefStFicheId AND LINETYPE = 0 ORDER BY LOGICALREF DESC";
				var parameters3 = new
				{
					RefStFicheId = refStFicheId
				};
				cancellationToken = ct;
				refStLineId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText8, parameters3, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStLineId == 0 && refInvoiceId > 0)
			{
				string commandText9 = "SELECT TOP 1 LOGICALREF FROM " + lineTable + " WITH (NOLOCK) WHERE INVOICEREF = @RefInvoiceId AND LINETYPE = 0 ORDER BY LOGICALREF DESC";
				var parameters4 = new
				{
					RefInvoiceId = refInvoiceId
				};
				cancellationToken = ct;
				refStLineId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText9, parameters4, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			if (refStLineId == 0)
			{
				string commandText10 = "SELECT TOP 1 LOGICALREF FROM " + lineTable + " WITH (NOLOCK) WHERE TRCODE = 8 AND LINETYPE = 0 ORDER BY LOGICALREF DESC";
				cancellationToken = ct;
				refStLineId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText10, null, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			List<int> createdLineRefs = new List<int>();
			decimal totalRawGrossSum = 0m;
			decimal totalLineDiscounts = 0m;
			decimal totalLineNetSum = 0m;
			decimal totalVatSum = 0m;
			string value = BuildTempTableUpdateSql("#TempInvoice", invColumns, new Dictionary<string, string>
			{
				["FICHENO"] = "@FicheNo",
				["GUID"] = "NEWID()",
				["DATE_"] = "CAST(GETDATE() AS DATE)",
				["TIME_OR_FTIME"] = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))",
				["CLIENTREF"] = "@ClientRef",
				["RECVREF"] = "@ClientRef",
				["GENEXP1"] = "@GenExp",
				["SOURCEINDEX"] = "@SourceIndex",
				["SALESMANREF"] = "CASE WHEN @SalesmanRef IS NOT NULL AND @SalesmanRef > 0 THEN @SalesmanRef ELSE SALESMANREF END",
				["EINVOICE"] = "@EInvoiceType",
				["PROFILEID"] = "@EInvoiceType",
				["EDESPATCH"] = "0",
				["ESTATUS"] = "0",
				["STATUS"] = "1",
				["CANCELLED"] = "0",
				["BILLED"] = "1",
				["TRCODE"] = "8",
				["CAPIBLOCK_CREATEDBY"] = "1",
				["CAPIBLOCK_CREADEDDATE"] = "GETDATE()"
			});
			string commandText11 = $"\r\n                SET NOCOUNT ON;\r\n                IF OBJECT_ID('tempdb..#TempInvoice') IS NOT NULL DROP TABLE #TempInvoice;\r\n                SELECT * INTO #TempInvoice FROM {invTable} WHERE LOGICALREF = @RefInvoiceId;\r\n                ALTER TABLE #TempInvoice DROP COLUMN LOGICALREF;\r\n                {value}\r\n                INSERT INTO {invTable} SELECT * FROM #TempInvoice;\r\n                DROP TABLE #TempInvoice;";
			var parameters5 = new
			{
				RefInvoiceId = refInvoiceId,
				FicheNo = invoiceFicheNo,
				ClientRef = clientRef,
				GenExp = safeGenExp,
				SourceIndex = warehouseNo,
				SalesmanRef = salesmanRef,
				EInvoiceType = eInvoiceType
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText11, parameters5, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			string commandText12 = $"SELECT ISNULL((SELECT TOP 1 LOGICALREF FROM {invTable} WITH (NOLOCK) WHERE RTRIM(FICHENO) = RTRIM(@FicheNo) OR FICHENO LIKE @FicheNo + '%' ORDER BY LOGICALREF DESC), (SELECT TOP 1 LOGICALREF FROM {invTable} WITH (NOLOCK) ORDER BY LOGICALREF DESC))";
			var parameters6 = new
			{
				FicheNo = invoiceFicheNo
			};
			cancellationToken = ct;
			int invoiceRef = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText12, parameters6, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			string value2 = BuildTempTableUpdateSql("#TempFiche", ficheColumns, new Dictionary<string, string>
			{
				["FICHENO"] = "@FicheNo",
				["GUID"] = "NEWID()",
				["INVOICEREF"] = "@InvoiceRef",
				["DATE_"] = "CAST(GETDATE() AS DATE)",
				["TIME_OR_FTIME"] = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))",
				["CLIENTREF"] = "@ClientRef",
				["RECVREF"] = "@ClientRef",
				["GENEXP1"] = "@GenExp",
				["SOURCEINDEX"] = "@SourceIndex",
				["SALESMANREF"] = "CASE WHEN @SalesmanRef IS NOT NULL AND @SalesmanRef > 0 THEN @SalesmanRef ELSE SALESMANREF END",
				["EDESPATCH"] = "0",
				["EDESPATCHSTATUS"] = "0",
				["ESTATUS"] = "0",
				["STATUS"] = "1",
				["CANCELLED"] = "0",
				["BILLED"] = "1",
				["TRCODE"] = "8",
				["CAPIBLOCK_CREATEDBY"] = "1",
				["CAPIBLOCK_CREADEDDATE"] = "GETDATE()"
			});
			string commandText13 = $"\r\n                SET NOCOUNT ON;\r\n                IF OBJECT_ID('tempdb..#TempFiche') IS NOT NULL DROP TABLE #TempFiche;\r\n                SELECT * INTO #TempFiche FROM {ficheTable} WHERE LOGICALREF = @RefStFicheId;\r\n                ALTER TABLE #TempFiche DROP COLUMN LOGICALREF;\r\n                {value2}\r\n                INSERT INTO {ficheTable} SELECT * FROM #TempFiche;\r\n                DROP TABLE #TempFiche;";
			var parameters7 = new
			{
				RefStFicheId = refStFicheId,
				InvoiceRef = invoiceRef,
				FicheNo = stFicheNo,
				ClientRef = clientRef,
				GenExp = safeGenExp,
				SourceIndex = warehouseNo,
				SalesmanRef = salesmanRef
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText13, parameters7, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			string commandText14 = $"SELECT ISNULL((SELECT TOP 1 LOGICALREF FROM {ficheTable} WITH (NOLOCK) WHERE RTRIM(FICHENO) = RTRIM(@FicheNo) OR FICHENO LIKE @FicheNo + '%' ORDER BY LOGICALREF DESC), (SELECT TOP 1 LOGICALREF FROM {ficheTable} WITH (NOLOCK) ORDER BY LOGICALREF DESC))";
			var parameters8 = new
			{
				FicheNo = stFicheNo
			};
			cancellationToken = ct;
			int ficheRef = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText14, parameters8, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			string updateLineSqlTemplate = BuildTempTableUpdateSql("#TempLine", lineColumns, new Dictionary<string, string>
			{
				["INVOICEREF"] = "@InvoiceRef",
				["STFICHEREF"] = "@StFicheRef",
				["STOCKREF"] = "@StockRef",
				["LINEEXP"] = "@ItemName",
				["CLIENTREF"] = "@ClientRef",
				["DATE_"] = "CAST(GETDATE() AS DATE)",
				["TIME_OR_FTIME"] = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))",
				["SALESMANREF"] = "CASE WHEN @SalesmanRef IS NOT NULL AND @SalesmanRef > 0 THEN @SalesmanRef ELSE SALESMANREF END",
				["AMOUNT"] = "@Amount",
				["PRICE"] = "@Price",
				["VAT"] = "@Vat",
				["UOMREF"] = "@UnitRef",
				["USREF"] = "@UsRef",
				["LINENET"] = "@LineTotal",
				["TOTAL"] = "@GrossTotal",
				["DISCPER"] = "@DiscPer",
				["DISTDISC"] = "@DiscAmnt",
				["DISTOT"] = "@DiscAmnt",
				["VATAMNT"] = "@VatAmnt",
				["VATMATRAH"] = "@LineTotal",
				["SOURCEINDEX"] = "@SourceIndex",
				["DESTINDEX"] = "@SourceIndex",
				["LINENR"] = "@LineNr",
				["INVOICELN"] = "@LineNr",
				["STFICHELNNO"] = "@LineNr",
				["INVOICELNNO"] = "@LineNr",
				["CANCELLED"] = "0",
				["STATUS"] = "1",
				["LINETYPE"] = "0",
				["TRCODE"] = "8",
				["IOCODE"] = "4",
				["BILLED"] = "1",
				["BILLEDITEM"] = "0",
				["UINFO1"] = "1",
				["UINFO2"] = "1",
				["UNITCONV1"] = "1",
				["UNITCONV2"] = "1",
				["ORDTRANSREF"] = "0",
				["ORDFICHEREF"] = "0",
				["OFFTRANSREF"] = "0",
				["OFFICHEREF"] = "0",
				["MONTH_"] = "MONTH(GETDATE())",
				["YEAR_"] = "YEAR(GETDATE())",
				["GLOBTRANS"] = "0",
				["CALCDEF"] = "0",
				["RECSTATUS"] = "1"
			});
			for (int i = 0; i < lineInputs.Count; i++)
			{
				LogoErpOrderLineInput input = lineInputs[i];
				int lineNr = i + 1;
				decimal listPrice = input.UnitPrice.GetValueOrDefault();
				decimal grossTotal = input.Amount * listPrice;
				decimal lineTotal = grossTotal;
				decimal num = 0m;
				if (input.DiscountRate > 0m)
				{
					num = grossTotal * (input.DiscountRate / 100m);
					lineTotal = grossTotal - num;
				}
				decimal vatAmnt = lineTotal * (decimal)input.ItemDetails.Vat / 100m;
				totalRawGrossSum += grossTotal;
				totalLineDiscounts += num;
				totalLineNetSum += lineTotal;
				totalVatSum += vatAmnt;
				int num2;
				if (refStLineId > 0)
				{
					string commandText15 = $"\r\n                        SET NOCOUNT ON;\r\n                        DECLARE @NewId INT;\r\n                        IF OBJECT_ID('tempdb..#TempLine') IS NOT NULL DROP TABLE #TempLine;\r\n                        SELECT * INTO #TempLine FROM {lineTable} WHERE LOGICALREF = @RefStLineId;\r\n                        ALTER TABLE #TempLine DROP COLUMN LOGICALREF;\r\n                        {updateLineSqlTemplate}\r\n                        INSERT INTO {lineTable} SELECT * FROM #TempLine;\r\n                        SET @NewId = CAST(SCOPE_IDENTITY() AS INT);\r\n                        DROP TABLE #TempLine;\r\n                        SELECT @NewId;";
					var parameters9 = new
					{
						RefStLineId = refStLineId,
						InvoiceRef = invoiceRef,
						StFicheRef = ficheRef,
						StockRef = input.ItemDetails.ItemLogicalRef,
						ItemName = (input.ProductCode ?? ""),
						ClientRef = clientRef,
						SalesmanRef = salesmanRef,
						Amount = input.Amount,
						Price = listPrice,
						DiscPer = input.DiscountRate,
						DiscAmnt = num,
						Vat = input.ItemDetails.Vat,
						UnitRef = input.ItemDetails.UnitRef,
						UsRef = input.ItemDetails.UnitSetRef,
						LineTotal = lineTotal,
						GrossTotal = grossTotal,
						VatAmnt = vatAmnt,
						SourceIndex = warehouseNo,
						LineNr = lineNr
					};
					cancellationToken = ct;
					num2 = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText15, parameters9, transaction, null, null, CommandFlags.Buffered, cancellationToken));
					createdLineRefs.Add(num2);
				}
				else
				{
					List<string> list = new List<string>
					{
						"STFICHEREF", "INVOICEREF", "CLIENTREF", "STOCKREF", "LINETYPE", "TRCODE", "DATE_", "AMOUNT", "PRICE", "VAT",
						"UOMREF", "USREF", "UINFO1", "UINFO2", "SOURCEINDEX", "STATUS", "IOCODE", "MONTH_", "YEAR_", "LINENET",
						"TOTAL", "VATAMNT", "VATMATRAH", "CANCELLED", "BILLED", "DISCPER", "DISTDISC"
					};
					List<string> list2 = new List<string>
					{
						"@StFicheRef", "@InvoiceRef", "@ClientRef", "@StockRef", "0", "8", "CAST(GETDATE() AS DATE)", "@Amount", "@Price", "@Vat",
						"@UnitRef", "@UsRef", "1", "1", "@SourceIndex", "1", "4", "MONTH(GETDATE())", "YEAR(GETDATE())", "@LineTotal",
						"@GrossTotal", "@VatAmnt", "@LineTotal", "0", "1", "@DiscPer", "@DiscAmnt"
					};
					if (lineColumns.Contains("INVOICELNNO"))
					{
						list.Add("INVOICELNNO");
						list2.Add("@LineNr");
					}
					if (lineColumns.Contains("STFICHELNNO"))
					{
						list.Add("STFICHELNNO");
						list2.Add("@LineNr");
					}
					if (lineColumns.Contains("INVOICELN"))
					{
						list.Add("INVOICELN");
						list2.Add("@LineNr");
					}
					if (lineColumns.Contains("LINENR"))
					{
						list.Add("LINENR");
						list2.Add("@LineNr");
					}
					string commandText16 = $"INSERT INTO {lineTable} ({string.Join(", ", list)}) VALUES ({string.Join(", ", list2)}); SELECT CAST(SCOPE_IDENTITY() as int);";
					var parameters10 = new
					{
						StFicheRef = ficheRef,
						InvoiceRef = invoiceRef,
						ClientRef = clientRef,
						StockRef = input.ItemDetails.ItemLogicalRef,
						Amount = input.Amount,
						Price = listPrice,
						DiscPer = input.DiscountRate,
						DiscAmnt = num,
						Vat = input.ItemDetails.Vat,
						UnitRef = input.ItemDetails.UnitRef,
						UsRef = input.ItemDetails.UnitSetRef,
						SourceIndex = warehouseNo,
						LineNr = lineNr,
						LineTotal = lineTotal,
						GrossTotal = grossTotal,
						VatAmnt = vatAmnt
					};
					cancellationToken = ct;
					num2 = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText16, parameters10, transaction, null, null, CommandFlags.Buffered, cancellationToken));
					createdLineRefs.Add(num2);
				}
				List<string> list3 = new List<string>
				{
					"INVOICEREF = @InvoiceRef", "STFICHEREF = @StFicheRef", "CLIENTREF = @ClientRef", "STOCKREF = @StockRef", "AMOUNT = @Amount", "PRICE = @Price", "VAT = @Vat", "UOMREF = @UnitRef", "USREF = @UsRef", "LINENET = @LineTotal",
					"TOTAL = @GrossTotal", "VATAMNT = @VatAmnt", "VATMATRAH = @LineTotal", "BILLED = 1", "BILLEDITEM = 0", "CANCELLED = 0", "STATUS = 1", "LINETYPE = 0", "TRCODE = 8", "IOCODE = 4",
					"MONTH_ = MONTH(GETDATE())", "YEAR_ = YEAR(GETDATE())"
				};
				if (lineColumns.Contains("INVOICELNNO"))
				{
					list3.Add("INVOICELNNO = ISNULL(NULLIF(@LineNr, 0), 1)");
				}
				if (lineColumns.Contains("STFICHELNNO"))
				{
					list3.Add("STFICHELNNO = ISNULL(NULLIF(@LineNr, 0), 1)");
				}
				if (lineColumns.Contains("INVOICELN"))
				{
					list3.Add("INVOICELN = ISNULL(NULLIF(@LineNr, 0), 1)");
				}
				if (lineColumns.Contains("LINENR"))
				{
					list3.Add("LINENR = ISNULL(NULLIF(@LineNr, 0), 1)");
				}
				string commandText17 = $"UPDATE {lineTable} SET {string.Join(", ", list3)} WHERE LOGICALREF = @LineRef";
				var parameters11 = new
				{
					InvoiceRef = invoiceRef,
					StFicheRef = ficheRef,
					ClientRef = clientRef,
					StockRef = input.ItemDetails.ItemLogicalRef,
					Amount = input.Amount,
					Price = listPrice,
					Vat = input.ItemDetails.Vat,
					UnitRef = input.ItemDetails.UnitRef,
					UsRef = input.ItemDetails.UnitSetRef,
					LineTotal = lineTotal,
					GrossTotal = grossTotal,
					VatAmnt = vatAmnt,
					LineNr = lineNr,
					LineRef = num2
				};
				cancellationToken = ct;
				await connection.ExecuteAsync(new CommandDefinition(commandText17, parameters11, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			decimal netTotal = totalLineNetSum + totalVatSum;
			decimal totalDiscounted = totalRawGrossSum - totalLineDiscounts;
			decimal vatInclGross = totalRawGrossSum + totalVatSum;
			List<string> list4 = new List<string> { "NETTOTAL = @NetTotal", "GROSSTOTAL = @GrossTotal", "TOTALDISCOUNTS = @TotalDiscounts", "TOTALVAT = @TotalVat" };
			if (ficheColumns.Contains("LINECNT"))
			{
				list4.Add("LINECNT = @LineCount");
			}
			if (ficheColumns.Contains("REPORTNET"))
			{
				list4.Add("REPORTNET = @NetTotal");
			}
			if (ficheColumns.Contains("TRNET"))
			{
				list4.Add("TRNET = @NetTotal");
			}
			if (ficheColumns.Contains("TOTALDISCOUNTED"))
			{
				list4.Add("TOTALDISCOUNTED = @TotalDiscounted");
			}
			if (ficheColumns.Contains("VATINCLGROSS"))
			{
				list4.Add("VATINCLGROSS = @VatInclGross");
			}
			string commandText18 = $"UPDATE {ficheTable} SET {string.Join(", ", list4)} WHERE LOGICALREF = @FicheRef";
			var parameters12 = new
			{
				NetTotal = netTotal,
				GrossTotal = totalRawGrossSum,
				TotalDiscounts = totalLineDiscounts,
				TotalDiscounted = totalDiscounted,
				TotalVat = totalVatSum,
				VatInclGross = vatInclGross,
				LineCount = lineInputs.Count,
				FicheRef = ficheRef
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText18, parameters12, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			List<string> list5 = new List<string> { "NETTOTAL = @NetTotal", "GROSSTOTAL = @GrossTotal", "TOTALDISCOUNTS = @TotalDiscounts", "TOTALVAT = @TotalVat" };
			if (invColumns.Contains("LINECNT"))
			{
				list5.Add("LINECNT = @LineCount");
			}
			if (invColumns.Contains("REPORTNET"))
			{
				list5.Add("REPORTNET = @NetTotal");
			}
			if (invColumns.Contains("TRNET"))
			{
				list5.Add("TRNET = @NetTotal");
			}
			if (invColumns.Contains("TOTALDISCOUNTED"))
			{
				list5.Add("TOTALDISCOUNTED = @TotalDiscounted");
			}
			if (invColumns.Contains("VATINCLGROSS"))
			{
				list5.Add("VATINCLGROSS = @VatInclGross");
			}
			string commandText19 = $"UPDATE {invTable} SET {string.Join(", ", list5)} WHERE LOGICALREF = @InvoiceRef";
			var parameters13 = new
			{
				NetTotal = netTotal,
				GrossTotal = totalRawGrossSum,
				TotalDiscounts = totalLineDiscounts,
				TotalDiscounted = totalDiscounted,
				TotalVat = totalVatSum,
				VatInclGross = vatInclGross,
				LineCount = lineInputs.Count,
				InvoiceRef = invoiceRef
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText19, parameters13, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			List<string> list6 = new List<string> { "INVOICEREF = @InvoiceRef", "BILLED = 1" };
			if (ficheColumns.Contains("EDESPATCH"))
			{
				list6.Add("EDESPATCH = 0");
			}
			if (ficheColumns.Contains("EDESPATCHSTATUS"))
			{
				list6.Add("EDESPATCHSTATUS = 0");
			}
			if (ficheColumns.Contains("ESTATUS"))
			{
				list6.Add("ESTATUS = 0");
			}
			string commandText20 = $"\r\n                SET NOCOUNT ON;\r\n                UPDATE {ficheTable} SET {string.Join(", ", list6)} \r\n                WHERE LOGICALREF = @FicheRef;\r\n\r\n                UPDATE {lineTable} SET INVOICEREF = @InvoiceRef, STFICHEREF = @FicheRef, BILLED = 1, LINETYPE = 0, TRCODE = 8 \r\n                WHERE STFICHEREF = @FicheRef;";
			var parameters14 = new
			{
				FicheRef = ficheRef,
				InvoiceRef = invoiceRef
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText20, parameters14, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			await transaction.CommitAsync(ct);
			_logger.LogInformation("Logo ERP SatÄ±ÅŸ FaturasÄ± (Referans Klonlama) baÅŸarÄ±yla oluÅŸturuldu: FicheRef: {FicheRef}, InvoiceRef: {InvoiceRef}, FicheNo: {FicheNo}", ficheRef, invoiceRef, invoiceFicheNo);
			return (ficheRef: ficheRef, lineRefs: createdLineRefs, ficheNo: invoiceFicheNo);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Logo ERP SatÄ±ÅŸ FaturasÄ± kaydedilirken hata oluÅŸtu.");
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	public async Task<LogoErpDispatchDto?> GetDispatchDetailAsync(string ficheNo, string firmNo, string periodNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string ficheTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string lineTable = $"LG_{firmNo}_{periodNo}_STLINE";
		string clTable = "LG_" + firmNo + "_CLCARD";
		string itemTable = "LG_" + firmNo + "_ITEMS";
		string unitTable = "LG_" + firmNo + "_UNITSETL";
		string slsTable = "LG_SLSMAN";
		string commandText = $"\r\n            SELECT TOP 1\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(C.TAXOFFICE, '') AS TaxOffice,\r\n                ISNULL(C.TAXNR, '') AS TaxNo,\r\n                ISNULL(C.ADDR1, '') AS Address1,\r\n                ISNULL(C.ADDR2, '') AS Address2,\r\n                ISNULL(C.CITY, '') AS City,\r\n                ISNULL(C.TOWN, '') AS Town,\r\n                ISNULL(C.TELNRS1, '') AS Phone,\r\n                ISNULL(C.EMAILADDR, '') AS Email,\r\n                ISNULL(S.DEFINITION_, ISNULL(S.CODE, 'Belirtilmedi')) AS SalesmanName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.GENEXP2, '') AS CarrierRaw,\r\n                ISNULL(O.GROSSTOTAL, 0) AS GrossTotal,\r\n                ISNULL(O.TOTALDISCOUNTS, 0) AS TotalDiscount,\r\n                ISNULL(O.TOTALVAT, 0) AS TotalVat,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal\r\n            FROM {ficheTable} O WITH (NOLOCK)\r\n            LEFT JOIN {clTable} C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            LEFT JOIN {slsTable} S WITH (NOLOCK) ON O.SALESMANREF = S.LOGICALREF\r\n            WHERE (O.FICHENO = @FicheNo OR LTRIM(RTRIM(O.FICHENO)) = LTRIM(RTRIM(@FicheNo))) AND ISNULL(O.CANCELLED, 0) = 0";
		using IDbConnection connection = CreateConnection();
		_ = 2;
		try
		{
			bool isInvoice = false;
			var parameters = new
			{
				FicheNo = ficheNo
			};
			CancellationToken cancellationToken = ct;
			LogoErpDispatchDto header = await connection.QueryFirstOrDefaultAsync<LogoErpDispatchDto>(new CommandDefinition(commandText, parameters, null, null, null, CommandFlags.Buffered, cancellationToken));
			if (header == null)
			{
				string value = $"LG_{firmNo}_{periodNo}_INVOICE";
				string commandText2 = $"\r\n                    SELECT TOP 1\r\n                        O.LOGICALREF AS LogicalRef,\r\n                        O.FICHENO AS FicheNo,\r\n                        O.DATE_ AS Date,\r\n                        ISNULL(C.CODE, '') AS FirmCode,\r\n                        ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                        ISNULL(C.TAXOFFICE, '') AS TaxOffice,\r\n                        ISNULL(C.TAXNR, '') AS TaxNo,\r\n                        ISNULL(C.ADDR1, '') AS Address1,\r\n                        ISNULL(C.ADDR2, '') AS Address2,\r\n                        ISNULL(C.CITY, '') AS City,\r\n                        ISNULL(C.TOWN, '') AS Town,\r\n                        ISNULL(C.TELNRS1, '') AS Phone,\r\n                        ISNULL(C.EMAILADDR, '') AS Email,\r\n                        ISNULL(S.DEFINITION_, ISNULL(S.CODE, 'Belirtilmedi')) AS SalesmanName,\r\n                        ISNULL(O.GENEXP1, '') AS Description,\r\n                        ISNULL(O.GENEXP2, '') AS CarrierRaw,\r\n                        ISNULL(O.GROSSTOTAL, 0) AS GrossTotal,\r\n                        ISNULL(O.TOTALDISCOUNTS, 0) AS TotalDiscount,\r\n                        ISNULL(O.TOTALVAT, 0) AS TotalVat,\r\n                        ISNULL(O.NETTOTAL, 0) AS NetTotal\r\n                    FROM {value} O WITH (NOLOCK)\r\n                    LEFT JOIN {clTable} C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n                    LEFT JOIN {slsTable} S WITH (NOLOCK) ON O.SALESMANREF = S.LOGICALREF\r\n                    WHERE (O.FICHENO = @FicheNo OR LTRIM(RTRIM(O.FICHENO)) = LTRIM(RTRIM(@FicheNo))) AND ISNULL(O.CANCELLED, 0) = 0";
				var parameters2 = new
				{
					FicheNo = ficheNo
				};
				cancellationToken = ct;
				header = await connection.QueryFirstOrDefaultAsync<LogoErpDispatchDto>(new CommandDefinition(commandText2, parameters2, null, null, null, CommandFlags.Buffered, cancellationToken));
				if (header != null)
				{
					isInvoice = true;
				}
			}
			if (header == null)
			{
				return null;
			}
			string value2 = (isInvoice ? ("(L.INVOICEREF = @RefId OR L.STFICHEREF IN (SELECT LOGICALREF FROM " + ficheTable + " WITH (NOLOCK) WHERE INVOICEREF = @RefId))") : ("(L.STFICHEREF = @RefId)"));
			string commandText3 = $@"
                SELECT
                    ISNULL(I.CODE, '') AS ItemCode,
                    ISNULL(NULLIF(I.NAME, ''), ISNULL(NULLIF(L.LINEEXP, ''), 'Malzeme Sürümü')) AS ItemName,
                    ISNULL(L.AMOUNT, 0) AS Amount,
                    ISNULL(U.CODE, 'ADET') AS UnitName,
                    ISNULL(L.PRICE, 0) AS UnitPrice,
                    CASE 
                        WHEN ISNULL(L.DISCPER, 0) > 0 THEN L.DISCPER 
                        WHEN ISNULL(L.TOTAL, 0) > 0 AND ISNULL(L.DISTDISC, 0) > 0 THEN (L.DISTDISC / L.TOTAL) * 100.0 
                        WHEN ISNULL(L.PRICE, 0) > 0 AND ISNULL(L.LINENET, 0) > 0 AND (L.AMOUNT * L.PRICE) > L.LINENET 
                             THEN (((L.AMOUNT * L.PRICE) - L.LINENET) / (L.AMOUNT * L.PRICE)) * 100.0 
                        ELSE 0 
                    END AS DiscountRate,
                    ISNULL(NULLIF(L.LINENET, 0), L.AMOUNT * L.PRICE) AS LineTotal,
                    ISNULL(L.VAT, ISNULL(I.VAT, 20)) AS VatRate
                FROM {lineTable} L WITH (NOLOCK)
                LEFT JOIN {itemTable} I WITH (NOLOCK) ON L.STOCKREF = I.LOGICALREF
                LEFT JOIN {unitTable} U WITH (NOLOCK) ON L.UOMREF = U.LOGICALREF
                WHERE {value2} AND L.LINETYPE IN (0, 4) AND ISNULL(L.CANCELLED, 0) = 0
                ORDER BY L.LOGICALREF ASC";
			var parameters3 = new
			{
				RefId = header.LogicalRef,
				FicheNo = ficheNo
			};
			cancellationToken = ct;
			header.Lines = (await connection.QueryAsync<LogoErpDispatchLineDto>(new CommandDefinition(commandText3, parameters3, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			ParseCarrierRaw(header);
			return header;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Ãƒâ€Ã‚Â°rsaliye detayÃƒâ€Ã‚Â± ({FicheNo}) sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.", ficheNo);
			return null;
		}
	}

	public static void ParseCarrierRaw(LogoErpDispatchDto dto)
	{
		if (dto == null)
		{
			return;
		}
		string text = (dto.CarrierInfo = (dto.CarrierRaw ?? string.Empty).Trim());
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}

		if (text.Contains('|'))
		{
			string[] array = text.Split('|');
			if (array.Length >= 1 && !string.IsNullOrWhiteSpace(array[0])) dto.CarrierInfo = array[0].Trim();
			if (array.Length >= 3 && !string.IsNullOrWhiteSpace(array[2])) dto.LicensePlate = array[2].Trim();
			if (array.Length >= 5 && !string.IsNullOrWhiteSpace(array[4])) dto.DriverName = array[4].Trim();
			if (array.Length >= 6 && !string.IsNullOrWhiteSpace(array[5])) dto.DriverSurname = array[5].Trim();
			if (array.Length >= 7 && !string.IsNullOrWhiteSpace(array[6])) dto.DriverTckNo = array[6].Trim();
			return;
		}

		var tcknMatch = System.Text.RegularExpressions.Regex.Match(text, @"\b\d{10,11}\b");
		if (tcknMatch.Success)
		{
			dto.DriverTckNo = tcknMatch.Value;
			text = text.Replace(tcknMatch.Value, " ").Trim();
		}

		var plateMatch = System.Text.RegularExpressions.Regex.Match(text, @"\b\d{2}\s*[A-Za-z]{1,3}\s*\d{2,4}\b");
		if (plateMatch.Success)
		{
			dto.LicensePlate = plateMatch.Value.ToUpper().Trim();
			text = text.Replace(plateMatch.Value, " ").Trim();
		}

		text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(fn|plaka|şoför|sofor|tckn|vkn|taşıyıcı|tasiyici|araç|arac)\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
		if (!string.IsNullOrWhiteSpace(text))
		{
			string[] array2 = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 1)
			{
				dto.DriverName = array2[0];
			}
			else if (array2.Length >= 2)
			{
				dto.DriverName = string.Join(" ", array2.Take(array2.Length - 1));
				dto.DriverSurname = array2.Last();
			}
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetDispatchesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string stTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string clTable = "LG_" + firmNo + "_CLCARD";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		HashSet<string> hashSet = await GetTableColumnsDirectAsync(connection, null, stTable, ct);
		string value = (hashSet.Contains("EDESPATCH") ? "ISNULL(O.EDESPATCH, 0)" : "0");
		string value2 = (hashSet.Contains("EDESPATCHSTATUS") ? "ISNULL(O.EDESPATCHSTATUS, 0)" : (hashSet.Contains("ESTATUS") ? "ISNULL(O.ESTATUS, 0)" : "0"));
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(637, 5, stringBuilder2);
		handler.AppendLiteral("\r\n            SELECT TOP (");
		handler.AppendFormatted(limit);
		handler.AppendLiteral(")\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                O.STATUS AS Status,\r\n                ");
		handler.AppendFormatted(value);
		handler.AppendLiteral(" AS EDespatch,\r\n                ");
		handler.AppendFormatted(value2);
		handler.AppendLiteral(" AS EStatus\r\n            FROM ");
		handler.AppendFormatted(stTable);
		handler.AppendLiteral(" O WITH (NOLOCK)\r\n            LEFT JOIN ");
		handler.AppendFormatted(clTable);
		handler.AppendLiteral(" C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE = 8 AND (O.CANCELLED IS NULL OR O.CANCELLED = 0)");
		stringBuilder2.AppendLine(ref handler);
		DynamicParameters dynamicParameters = new DynamicParameters();
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine(" AND C.CODE LIKE @FirmCode");
			dynamicParameters.Add("FirmCode", "%" + firmCodeFilter + "%");
		}
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ >= @StartDate");
			dynamicParameters.Add("StartDate", startDate.Value.Date);
		}
		if (endDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ <= @EndDate");
			dynamicParameters.Add("EndDate", endDate.Value.Date);
		}
		stringBuilder.AppendLine(" ORDER BY O.DATE_ DESC, O.LOGICALREF DESC");
		try
		{
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(new CommandDefinition(stringBuilder.ToString(), dynamicParameters, null, null, null, CommandFlags.Buffered, ct))).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SatÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 Ãƒâ€Ã‚Â°rsaliyeleri listelenirken SQL hatasÃƒâ€Ã‚Â± alÃƒâ€Ã‚Â±ndÃƒâ€Ã‚Â±.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<(int ficheRef, List<int> lineRefs, string dispatchFicheNo)> ConvertOrderToDispatchAsync(string orderFicheNo, string firmNo, string periodNo, int warehouseNo, string description = "SipariÃƒâ€¦Ã…\u00b8ten OluÃƒâ€¦Ã…\u00b8turulan SatÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 Ãƒâ€Ã‚Â°rsaliyesi", int? salesmanRef = null, string? carrierOrDriverInfo = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string orfFicheTable = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string orfLineTable = $"LG_{firmNo}_{periodNo}_ORFLINE";
		string stFicheTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string stLineTable = $"LG_{firmNo}_{periodNo}_STLINE";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			string commandText = "\r\n                SELECT TOP 1 LOGICALREF, CLIENTREF, SALESMANREF, SOURCEINDEX\r\n                FROM " + orfFicheTable + " WITH (UPDLOCK)\r\n                WHERE FICHENO = @OrderFicheNo AND ISNULL(CANCELLED, 0) = 0";
			var parameters = new
			{
				OrderFicheNo = orderFicheNo
			};
			CancellationToken cancellationToken = ct;
			dynamic val = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(commandText, parameters, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			if (val == null)
			{
				throw new InvalidOperationException("'" + orderFicheNo + "' numaralÃƒâ€Ã‚Â± sipariÃƒâ€¦Ã…\u00b8 bulunamadÃƒâ€Ã‚Â± veya iptal edilmiÃƒâ€¦Ã…\u00b8.");
			}
			int orderFicheRef = (int)val.LOGICALREF;
			int clientRef = (int)val.CLIENTREF;
			int orderSalesmanRef = salesmanRef ?? ((val.SALESMANREF != null) ? ((int)val.SALESMANREF) : 0);
			string commandText2 = "\r\n                SELECT LOGICALREF, STOCKREF, AMOUNT, ISNULL(SHIPPEDAMOUNT, 0) AS SHIPPEDAMOUNT, PRICE, VAT, UOMREF, USREF, LINENET, DISCPER\r\n                FROM " + orfLineTable + " WITH (UPDLOCK)\r\n                WHERE ORDFICHEREF = @OrderFicheRef AND LINETYPE = 0 AND ISNULL(CLOSED, 0) = 0 AND AMOUNT > ISNULL(SHIPPEDAMOUNT, 0)";
			var parameters2 = new
			{
				OrderFicheRef = orderFicheRef
			};
			cancellationToken = ct;
			List<object> orderLines = (await connection.QueryAsync(new CommandDefinition(commandText2, parameters2, transaction, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			if (orderLines.Count == 0)
			{
				throw new InvalidOperationException("'" + orderFicheNo + "' sipariÃƒâ€¦Ã…\u00b8inin sevk edilecek (aÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±k) satÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â± kalmamÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8tÃƒâ€Ã‚Â±r. TÃƒÆ’Ã‚Â¼m kalemler sevk edilmiÃƒâ€¦Ã…\u00b8.");
			}
			string prefix = _configuration["LogoErp:DispatchFichePrefix"] ?? "IRS";
			string dispatchFicheNo = await GetNextBotFicheNoAsync(connection, transaction, stFicheTable, prefix, ct);
			HashSet<string> ficheColumns = await GetTableColumnsDirectAsync(connection, transaction, stFicheTable, ct);
			string logoTimeCalcSql = "((DATEPART(hour, GETDATE()) * 16777216) + (DATEPART(minute, GETDATE()) * 65536) + (DATEPART(second, GETDATE()) * 256))";
			List<string> fCols = new List<string> { "TRCODE", "FICHENO", "DATE_", "CLIENTREF", "SOURCEINDEX", "SOURCECOSTGRP", "GENEXP1", "STATUS", "IOCODE" };
			List<string> fVals = new List<string> { "8", "@FicheNo", "CAST(GETDATE() AS DATE)", "@ClientRef", "@SourceIndex", "0", "@GenExp", "1", "4" };
			AddFicheColIf("RECSTATUS", "1");
			AddFicheColIf("APPROVE", "0");
			AddFicheColIf("CANCELLED", "0");
			AddFicheColIf("LINECNT", "@LineCount");
			AddFicheColIf("GUID", "NEWID()");
			AddFicheColIf("EDESPATCH", "1");
			AddFicheColIf("EDESPSTATUS", "1");
			AddFicheColIf("EINVOICETYP", "1");
			AddFicheColIf("PROFILEID", "1");
			if (!string.IsNullOrWhiteSpace(carrierOrDriverInfo))
			{
				AddFicheColIf("GENEXP2", "@CarrierInfo");
			}
			if (orderSalesmanRef > 0)
			{
				AddFicheColIf("SALESMANREF", orderSalesmanRef.ToString());
			}
			if (ficheColumns.Contains("TIME_"))
			{
				fCols.Add("TIME_");
				fVals.Add(logoTimeCalcSql);
			}
			else if (ficheColumns.Contains("FTIME"))
			{
				fCols.Add("FTIME");
				fVals.Add(logoTimeCalcSql);
			}
			AddFicheColIf("CAPIBLOCK_CREATEDBY", "1");
			AddFicheColIf("CAPIBLOCK_CREADEDDATE", "GETDATE()");
			string commandText3 = $"INSERT INTO {stFicheTable} ({string.Join(", ", fCols)}) VALUES ({string.Join(", ", fVals)}); SELECT CAST(SCOPE_IDENTITY() as int);";
			var parameters3 = new
			{
				FicheNo = dispatchFicheNo,
				ClientRef = clientRef,
				SourceIndex = warehouseNo,
				GenExp = description + " (SipariÃƒâ€¦Ã…\u00b8: " + orderFicheNo + ")",
				CarrierInfo = (carrierOrDriverInfo ?? ""),
				LineCount = orderLines.Count
			};
			cancellationToken = ct;
			int stFicheRef = await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText3, parameters3, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			HashSet<string> lineColumns = await GetTableColumnsDirectAsync(connection, transaction, stLineTable, ct);
			List<int> createdLineRefs = new List<int>();
			decimal totalRawGrossSum = 0m;
			decimal totalLineDiscounts = 0m;
			decimal totalLineNetSum = 0m;
			decimal totalVatSum = 0m;
			for (int i = 0; i < orderLines.Count; i++)
			{
				dynamic val2 = orderLines[i];
				int orderLineRef = (int)val2.LOGICALREF;
				int stockRef = (int)val2.STOCKREF;
				decimal amountToShip = (decimal)val2.AMOUNT - (decimal)val2.SHIPPEDAMOUNT;
				decimal num = (decimal)val2.PRICE;
				decimal num2 = (decimal)val2.VAT;
				decimal num3 = (decimal)val2.DISCPER;
				decimal num4 = amountToShip * num;
				decimal num5 = ((num3 > 0m) ? (num4 * (num3 / 100m)) : 0m);
				decimal num6 = num4 - num5;
				decimal num7 = num6 * (num2 / 100m);
				totalRawGrossSum += num4;
				totalLineDiscounts += num5;
				totalLineNetSum += num6;
				totalVatSum += num7;
				List<string> lCols = new List<string>
				{
					"STFICHEREF", "CLIENTREF", "STOCKREF", "LINETYPE", "TRCODE", "DATE_", "IOCODE", "AMOUNT", "PRICE", "VAT",
					"UOMREF", "USREF", "UINFO1", "UINFO2", "SOURCEINDEX", "ORDTRANSREF", "ORDFICHEREF"
				};
				List<string> lVals = new List<string>
				{
					"@StFicheRef", "@ClientRef", "@StockRef", "0", "8", "CAST(GETDATE() AS DATE)", "3", "@Amount", "@Price", "@Vat",
					"@UomRef", "@UsRef", "1", "1", "@SourceIndex", "@OrdTransRef", "@OrdFicheRef"
				};
				AddLineColIf("RECSTATUS", "1");
				AddLineColIf("CANCELLED", "0");
				AddLineColIf("LINENET", "@LineNet");
				AddLineColIf("TOTAL", "@GrossTotal");
				AddLineColIf("DISTOT", "@DiscAmnt");
				AddLineColIf("DISTISC", "@DiscAmnt");
				AddLineColIf("DISCPER", num3.ToString("0.00", CultureInfo.InvariantCulture));
				AddLineColIf("DISPER", num3.ToString("0.00", CultureInfo.InvariantCulture));
				AddLineColIf("VATAMNT", "@VatAmnt");
				AddLineColIf("VATMATRAH", "@LineNet");
				if (lineColumns.Contains("TIME_"))
				{
					lCols.Add("TIME_");
					lVals.Add(logoTimeCalcSql);
				}
				else if (lineColumns.Contains("FTIME"))
				{
					lCols.Add("FTIME");
					lVals.Add(logoTimeCalcSql);
				}
				string commandText4 = $"INSERT INTO {stLineTable} ({string.Join(", ", lCols)}) VALUES ({string.Join(", ", lVals)}); SELECT CAST(SCOPE_IDENTITY() as int);";
				var parameters4 = new
				{
					StFicheRef = stFicheRef,
					ClientRef = clientRef,
					StockRef = stockRef,
					Amount = amountToShip,
					Price = num,
					Vat = num2,
					UomRef = (int)val2.UOMREF,
					UsRef = (int)val2.USREF,
					SourceIndex = warehouseNo,
					OrdTransRef = orderLineRef,
					OrdFicheRef = orderFicheRef,
					LineNet = num6,
					GrossTotal = num4,
					DiscAmnt = num5,
					VatAmnt = num7
				};
				cancellationToken = ct;
				createdLineRefs.Add(await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText4, parameters4, transaction, null, null, CommandFlags.Buffered, cancellationToken)));
				string commandText5 = "\r\n                    UPDATE " + orfLineTable + "\r\n                    SET SHIPPEDAMOUNT = ISNULL(SHIPPEDAMOUNT, 0) + @Amount,\r\n                        CLOSED = CASE WHEN ISNULL(SHIPPEDAMOUNT, 0) + @Amount >= AMOUNT THEN 1 ELSE ISNULL(CLOSED, 0) END\r\n                    WHERE LOGICALREF = @OrderLineRef";
				var parameters5 = new
				{
					Amount = amountToShip,
					OrderLineRef = orderLineRef
				};
				cancellationToken = ct;
				await connection.ExecuteAsync(new CommandDefinition(commandText5, parameters5, transaction, null, null, CommandFlags.Buffered, cancellationToken));
				void AddLineColIf(string colName, string item)
				{
					if (lineColumns.Contains(colName))
					{
						lCols.Add(colName);
						lVals.Add(item);
					}
				}
			}
			string commandText6 = "SELECT COUNT(*) FROM " + orfLineTable + " WHERE ORDFICHEREF = @OrderFicheRef AND LINETYPE = 0 AND ISNULL(CLOSED, 0) = 0";
			var parameters6 = new
			{
				OrderFicheRef = orderFicheRef
			};
			cancellationToken = ct;
			if (await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText6, parameters6, transaction, null, null, CommandFlags.Buffered, cancellationToken)) == 0)
			{
				string commandText7 = "UPDATE " + orfFicheTable + " SET CLOSED = 1 WHERE LOGICALREF = @OrderFicheRef";
				var parameters7 = new
				{
					OrderFicheRef = orderFicheRef
				};
				cancellationToken = ct;
				await connection.ExecuteAsync(new CommandDefinition(commandText7, parameters7, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			}
			decimal netTotal = totalLineNetSum + totalVatSum;
			string commandText8 = "UPDATE " + stFicheTable + " SET NETTOTAL = @NetTotal, GROSSTOTAL = @GrossTotal, TOTALDISCOUNTS = @TotalDiscounts, TOTALVAT = @TotalVat WHERE LOGICALREF = @FicheRef";
			var parameters8 = new
			{
				NetTotal = netTotal,
				GrossTotal = totalRawGrossSum,
				TotalDiscounts = totalLineDiscounts,
				TotalVat = totalVatSum,
				FicheRef = stFicheRef
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition(commandText8, parameters8, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			await transaction.CommitAsync(ct);
			_logger.LogInformation("SipariÃƒâ€¦Ã…\u00b8 ({OrderFicheNo}) baÃƒâ€¦Ã…\u00b8arÃƒâ€Ã‚Â±yla SatÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 Ãƒâ€Ã‚Â°rsaliyesine ({DispatchFicheNo}) dÃƒÆ’Ã‚Â¶nÃƒÆ’Ã‚Â¼Ãƒâ€¦Ã…\u00b8tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼ldÃƒÆ’Ã‚Â¼ ve Logo ERP baÃƒâ€Ã…\u00b8landÃƒâ€Ã‚Â±. StFicheRef: {StFicheRef}", orderFicheNo, dispatchFicheNo, stFicheRef);
			return (ficheRef: stFicheRef, lineRefs: createdLineRefs, dispatchFicheNo: dispatchFicheNo);
			void AddFicheColIf(string colName, string item)
			{
				if (ficheColumns.Contains(colName))
				{
					fCols.Add(colName);
					fVals.Add(item);
				}
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SipariÃƒâ€¦Ã…\u00b8i irsaliyeleÃƒâ€¦Ã…\u00b8tirme transaction sÃƒâ€Ã‚Â±rasÃƒâ€Ã‚Â±nda hata oluÃƒâ€¦Ã…\u00b8tu: {OrderFicheNo}", orderFicheNo);
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	public async Task<LogoErpFirmRiskDto?> GetFirmRiskReportAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string clTable = "LG_" + firmNo + "_CLCARD";
		string pattern = BuildSqlPattern(firmCode);
		string value = _configuration["LogoSystem:PeriodNo"] ?? "01";
		string value2 = $"LG_{firmNo}_{value}_CLFLINE";
		string commandText = $"\r\n            SELECT TOP 1\r\n                C.CODE AS FirmCode,\r\n                C.DEFINITION_ AS FirmName,\r\n                ISNULL(C.RISKLIMIT, 0) AS RiskLimit,\r\n                ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN F.AMOUNT ELSE 0 END), 0) AS Debit,\r\n                ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN F.AMOUNT ELSE 0 END), 0) AS Credit,\r\n                45 AS AverageMaturityDays,\r\n                'Limit Aşımı Yok' AS RiskStatus\r\n            FROM {clTable} C WITH (NOLOCK)\r\n            LEFT JOIN {value2} F WITH (NOLOCK) ON C.LOGICALREF = F.CLIENTREF AND F.CANCELLED = 0\r\n            WHERE C.CODE = @FirmCode OR C.CODE LIKE @Pattern OR C.DEFINITION_ LIKE @Pattern\r\n            GROUP BY C.CODE, C.DEFINITION_, C.RISKLIMIT\r\n            ORDER BY CASE WHEN C.CODE = @FirmCode THEN 1 ELSE 2 END";
		using IDbConnection connection = CreateConnection();
		try
		{
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				FirmCode = firmCode,
				Pattern = pattern
			}, null, null, null, CommandFlags.Buffered, ct);
			return await connection.QueryFirstOrDefaultAsync<LogoErpFirmRiskDto>(command);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{ClTable} tablosundan Risk sorgulanÄ±rken hata oluÅŸtu.", clTable);
			return null;
		}
	}

	public async Task<List<LogoErpChequeItemDto>> GetFirmChequeReportAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string clTable = "LG_" + firmNo + "_CLCARD";
		string pattern = BuildSqlPattern(firmCode);
		using IDbConnection connection = CreateConnection();
		(string, string) tuple = await ResolveChequeTablesAsync(connection, firmNo, periodNo, ct);
		string csTable = tuple.Item1;
		string ctTable = tuple.Item2;
		string commandText = $@"
            SELECT DISTINCT
                CS.LOGICALREF,
                ISNULL(CS.PORTFOYNO, '-') AS PortfoyNo,
                ISNULL(CS.OWING, '-') AS Owing,
                CS.DUEDATE AS DueDate,
                ISNULL(CS.AMOUNT, 0) AS Amount,
                CASE ISNULL(CS.CURRSTAT, 0)
                    WHEN 1 THEN N'Portföyde'
                    WHEN 2 THEN N'Ciro Edildi'
                    WHEN 3 THEN N'Tahsilde'
                    WHEN 4 THEN N'Tahsil Edildi'
                    WHEN 5 THEN N'İade Edildi'
                    WHEN 6 THEN N'Protestolu / Karşılıksız'
                    WHEN 7 THEN N'Müşteriye İade'
                    WHEN 9 THEN N'Aktarıldı'
                    ELSE N'Diğer'
                END AS StatusText
            FROM {csTable} CS
            INNER JOIN {ctTable} CT ON CT.CSREF = CS.LOGICALREF
            INNER JOIN {clTable} CL ON (CT.CARDREF = CL.LOGICALREF AND (CT.CARDMD = 5 OR CT.CARDMD IS NULL))
            WHERE (CL.CODE = @FirmCode OR CL.CODE LIKE @Pattern OR CL.DEFINITION_ LIKE @Pattern)
              AND (@StartDate IS NULL OR CS.DUEDATE >= @StartDate OR CT.DATE_ >= @StartDate)
              AND (@EndDate IS NULL OR CS.DUEDATE <= @EndDate OR CT.DATE_ <= @EndDate)
            ORDER BY CS.DUEDATE ASC";
		try
		{
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				FirmCode = firmCode,
				Pattern = pattern,
				StartDate = startDate?.Date,
				EndDate = endDate?.Date
			}, null, null, null, CommandFlags.Buffered, ct);
			List<LogoErpChequeItemDto> list = (await connection.QueryAsync<LogoErpChequeItemDto>(command)).ToList();
			_logger.LogInformation("ÃƒÆ’Ã¢â‚¬Â¡ek Raporu: {CsTable} -> {FirmCode} / {Pattern} iÃƒÆ’Ã‚Â§in {Count} aktif ÃƒÆ’Ã‚Â§ek.", csTable, firmCode, pattern, list.Count);
			return list;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "ÃƒÆ’Ã¢â‚¬Â¡ek Raporu sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu. CSCARD={CsTable}, CSTRANS={CtTable}", csTable, ctTable);
			return new List<LogoErpChequeItemDto>();
		}
	}

	public async Task<LogoErpFirmStatementDto?> GetFirmStatementAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string clTable = "LG_" + firmNo + "_CLCARD";
		string clfTable = $"LG_{firmNo}_{periodNo}_CLFLINE";
		string pattern = BuildSqlPattern(firmCode);
		string commandText = "\r\n            SELECT TOP 1 CODE AS FirmCode, DEFINITION_ AS FirmName \r\n            FROM " + clTable + " \r\n            WHERE CODE = @FirmCode OR CODE LIKE @Pattern OR DEFINITION_ LIKE @Pattern\r\n            ORDER BY CASE WHEN CODE = @FirmCode THEN 1 ELSE 2 END";
		using IDbConnection connection = CreateConnection();
		_ = 3;
		try
		{
			var parameters = new
			{
				FirmCode = firmCode,
				Pattern = pattern
			};
			CancellationToken cancellationToken = ct;
			LogoErpFirmStatementDto firmInfo = await connection.QueryFirstOrDefaultAsync<LogoErpFirmStatementDto>(new CommandDefinition(commandText, parameters, null, null, null, CommandFlags.Buffered, cancellationToken));
			if (firmInfo == null)
			{
				return null;
			}
			string actualFirmCode = firmInfo.FirmCode;
			string sqlLines = $@"
                SELECT
                    F.DATE_ AS Date,
                    ISNULL(F.TRANNO, '-') AS TranNo,
                    CASE ISNULL(F.TRCODE, 0)
                        WHEN 1  THEN (CASE WHEN F.SIGN = 0 THEN N'Satınalma (Alım) Faturası' ELSE N'Nakit Tahsilat' END)
                        WHEN 2  THEN N'Nakit Ödeme'
                        WHEN 3  THEN N'Borç Dekontu'
                        WHEN 4  THEN N'Alacak Dekontu'
                        WHEN 5  THEN N'Virman Fişi'
                        WHEN 6  THEN N'Kur Farkı Fişi'
                        WHEN 12 THEN N'Özel Fiş'
                        WHEN 14 THEN N'Açılış Fişi'
                        WHEN 31 THEN N'Satınalma (Alım) Faturası'
                        WHEN 32 THEN N'Perakende Satış İade Faturası'
                        WHEN 33 THEN N'Toptan Satış İade Faturası'
                        WHEN 34 THEN N'Alınan Hizmet Faturası'
                        WHEN 36 THEN N'Satınalma İade (Alım İade) Faturası'
                        WHEN 37 THEN N'Perakende Satış Faturası'
                        WHEN 38 THEN N'Toptan Satış Faturası'
                        WHEN 39 THEN N'Verilen Hizmet Faturası'
                        WHEN 41 THEN N'Çek Girişi (Müşteri Çeki)'
                        WHEN 42 THEN N'Senet Girişi (Müşteri Senedi)'
                        WHEN 43 THEN N'Çek Çıkışı (Cari Hesaba)'
                        WHEN 44 THEN N'Senet Çıkışı (Cari Hesaba)'
                        WHEN 45 THEN N'Verilen Serbest Meslek Makbuzu'
                        WHEN 46 THEN N'Alınan Serbest Meslek Makbuzu'
                        WHEN 56 THEN N'Müstahsil Makbuzu'
                        WHEN 61 THEN N'Çek Girişi (Müşteri Çeki)'
                        WHEN 62 THEN N'Çek Çıkışı (Cari Hesaba)'
                        WHEN 63 THEN N'Senet Girişi (Müşteri Senedi)'
                        WHEN 64 THEN N'Senet Çıkışı (Cari Hesaba)'
                        WHEN 65 THEN N'Borç Çekimiz (Kendi Çekimiz)'
                        WHEN 66 THEN N'Borç Senedimiz (Kendi Senedimiz)'
                        WHEN 70 THEN N'Kredi Kartı Fişi'
                        WHEN 71 THEN N'Kredi Kartı İade Fişi'
                        WHEN 72 THEN N'Firma Kredi Kartı Fişi'
                        WHEN 73 THEN N'Firma Kredi Kartı İade Fişi'
                        WHEN 81 THEN N'Satınalma Siparişi'
                        WHEN 82 THEN N'Satış Siparişi'
                        ELSE N'Cari Hareket'
                    END AS TranType,
                    ISNULL(F.LINEEXP, N'Cari Hareket') AS LineExp,
                    ISNULL(F.AMOUNT, 0) * (CASE WHEN F.SIGN = 0 THEN 1 ELSE 0 END) AS Debit,
                    ISNULL(F.AMOUNT, 0) * (CASE WHEN F.SIGN = 1 THEN 1 ELSE 0 END) AS Credit,
                    ISNULL(F.TRCURR, 0) AS TrCurr,
                    CASE ISNULL(F.TRCURR, 0)
                        WHEN 1 THEN '$'
                        WHEN 2 THEN 'EUR'
                        WHEN 16 THEN 'GBP'
                        ELSE 'TL'
                    END AS CurrencySymbol,
                    ISNULL(F.TRNET, F.AMOUNT) AS FxAmount
                FROM {clfTable} F
                INNER JOIN {clTable} C ON C.LOGICALREF = F.CLIENTREF
                WHERE C.CODE = @FirmCode AND F.CANCELLED = 0
                AND (@StartDate IS NULL OR F.DATE_ >= @StartDate)
                AND (@EndDate IS NULL OR F.DATE_ <= @EndDate)
                ORDER BY F.DATE_ ASC, F.LOGICALREF ASC";
			string commandText2 = $"\r\n                SELECT \r\n                    ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN F.AMOUNT ELSE 0 END), 0) AS TotalDebit,\r\n                    ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN F.AMOUNT ELSE 0 END), 0) AS TotalCredit\r\n                FROM {clfTable} F\r\n                INNER JOIN {clTable} C ON C.LOGICALREF = F.CLIENTREF\r\n                WHERE C.CODE = @FirmCode AND F.CANCELLED = 0\r\n                AND (@StartDate IS NULL OR F.DATE_ >= @StartDate)\r\n                AND (@EndDate IS NULL OR F.DATE_ <= @EndDate)";
			string sqlCurrencyTotals = $"\r\n                SELECT \r\n                    CASE ISNULL(F.TRCURR, 0)\r\n                        WHEN 1 THEN '$'\r\n                        WHEN 2 THEN 'EUR'\r\n                        WHEN 16 THEN 'GBP'\r\n                        ELSE 'TL'\r\n                    END AS CurrencySymbol,\r\n                    ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS TotalDebit,\r\n                    ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS TotalCredit\r\n                FROM {clfTable} F\r\n                INNER JOIN {clTable} C ON C.LOGICALREF = F.CLIENTREF\r\n                WHERE C.CODE = @FirmCode AND F.CANCELLED = 0\r\n                AND (@StartDate IS NULL OR F.DATE_ >= @StartDate)\r\n                AND (@EndDate IS NULL OR F.DATE_ <= @EndDate)\r\n                GROUP BY F.TRCURR";
			var parameters2 = new
			{
				FirmCode = actualFirmCode,
				StartDate = startDate?.Date,
				EndDate = endDate?.Date
			};
			cancellationToken = ct;
			dynamic val = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(commandText2, parameters2, null, null, null, CommandFlags.Buffered, cancellationToken));
			if (val != null)
			{
				firmInfo.TotalDebit = Convert.ToDecimal(val.TotalDebit);
				firmInfo.TotalCredit = Convert.ToDecimal(val.TotalCredit);
			}
			var parameters3 = new
			{
				FirmCode = actualFirmCode,
				StartDate = startDate?.Date,
				EndDate = endDate?.Date
			};
			cancellationToken = ct;
			firmInfo.CurrencySummaries = (await connection.QueryAsync<CurrencySummaryDto>(new CommandDefinition(sqlCurrencyTotals, parameters3, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			var parameters4 = new
			{
				FirmCode = actualFirmCode,
				StartDate = startDate?.Date,
				EndDate = endDate?.Date
			};
			cancellationToken = ct;
			var ekstreHareketler = (await connection.QueryAsync<LogoErpStatementLineDto>(new CommandDefinition(sqlLines, parameters4, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			decimal cumBalance = 0m;
			foreach (var hareket in ekstreHareketler)
			{
				cumBalance += hareket.Debit - hareket.Credit;
				hareket.RunningBalance = cumBalance;
			}
			firmInfo.RecentTransactions = ekstreHareketler;
			firmInfo.FilterStartDate = startDate?.Date;
			firmInfo.FilterEndDate = endDate?.Date;
			return firmInfo;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Cari Ekstre sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return null;
		}
	}

	public async Task<LogoErpItemStockDto?> GetItemStockAsync(string productCode, string firmNo, string periodNo, int warehouseNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = "LG_" + firmNo + "_ITEMS";
		string value2 = "LG_" + firmNo + "_UNITSETL";
		string stlineTable = $"LG_{firmNo}_{periodNo}_STLINE";
		string pattern = BuildSqlPattern(productCode);
		using IDbConnection connection = CreateConnection();
		_ = 3;
		try
		{
			string commandText = $"\r\n                SELECT TOP 1\r\n                    I.LOGICALREF AS ItemLogicalRef,\r\n                    I.CODE AS ItemCode,\r\n                    I.NAME AS ItemName,\r\n                    ISNULL(I.STGRPCODE, '-') AS GroupCode,\r\n                    ISNULL(U.NAME, 'ADET') AS UnitName,\r\n                    ISNULL(I.VAT, 20) AS Vat\r\n                FROM {value} I\r\n                LEFT JOIN {value2} U ON U.UNITSETREF = I.UNITSETREF AND U.MAINUNIT = 1\r\n                WHERE I.CODE = @ProductCode OR I.CODE LIKE @Pattern OR I.NAME LIKE @Pattern\r\n                ORDER BY CASE WHEN I.CODE = @ProductCode THEN 1 ELSE 2 END";
			var parameters = new
			{
				ProductCode = productCode,
				Pattern = pattern
			};
			CancellationToken cancellationToken = ct;
			dynamic val = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(commandText, parameters, null, null, null, CommandFlags.Buffered, cancellationToken));
			if (val == null)
			{
				return null;
			}
			int itemLogicalRef = Convert.ToInt32(val.ItemLogicalRef);
			LogoErpItemStockDto dto = new LogoErpItemStockDto
			{
				ItemCode = (Convert.ToString(val.ItemCode) ?? productCode),
				ItemName = (Convert.ToString(val.ItemName) ?? ""),
				GroupCode = (Convert.ToString(val.GroupCode) ?? "-"),
				UnitName = (Convert.ToString(val.UnitName) ?? "ADET"),
				Vat = Convert.ToDouble(val.Vat)
			};
			string commandText2 = "\r\n                SELECT \r\n                    S.INVENNO AS WarehouseNo,\r\n                    ISNULL(SUM(S.ONHAND), 0) AS StockAmount\r\n                FROM " + await ResolveStockSourceTableOrViewAsync(connection, firmNo, periodNo, ct) + " S\r\n                WHERE S.STOCKREF = @StockRef\r\n                GROUP BY S.INVENNO\r\n                ORDER BY S.INVENNO ASC";
			var parameters2 = new
			{
				StockRef = itemLogicalRef
			};
			cancellationToken = ct;
			List<LogoErpWarehouseStockDto> source = (await connection.QueryAsync<LogoErpWarehouseStockDto>(new CommandDefinition(commandText2, parameters2, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			LogoErpWarehouseStockDto logoErpWarehouseStockDto = source.FirstOrDefault((LogoErpWarehouseStockDto w) => w.WarehouseNo == -1);
			List<LogoErpWarehouseStockDto> list = source.Where((LogoErpWarehouseStockDto w) => w.WarehouseNo >= 0).ToList();
			if (logoErpWarehouseStockDto != null && logoErpWarehouseStockDto.StockAmount > 0m)
			{
				dto.TotalStock = logoErpWarehouseStockDto.StockAmount;
			}
			else
			{
				dto.TotalStock = list.Sum((LogoErpWarehouseStockDto w) => w.StockAmount);
			}
			dto.WarehouseStocks = list;
			if (dto.TotalStock == 0m)
			{
				_logger.LogInformation("STINVTOT kaynaÃƒâ€Ã…\u00b8Ãƒâ€Ã‚Â± 0 dÃƒÆ’Ã‚Â¶ndÃƒÆ’Ã‚Â¼. STLINE stok hareketlerinden canlÃƒâ€Ã‚Â± stok hesaplanÃƒâ€Ã‚Â±yor...");
				string commandText3 = "\r\n                    SELECT \r\n                        SOURCEINDEX AS WarehouseNo,\r\n                        ISNULL(SUM(CASE \r\n                            WHEN TRCODE IN (1, 2, 3, 13, 14, 50) THEN AMOUNT \r\n                            WHEN TRCODE IN (6, 7, 8, 11, 12, 51) THEN -AMOUNT \r\n                            ELSE 0 \r\n                        END), 0) AS StockAmount\r\n                    FROM " + stlineTable + "\r\n                    WHERE STOCKREF = @StockRef AND CANCELLED = 0\r\n                    GROUP BY SOURCEINDEX\r\n                    ORDER BY SOURCEINDEX ASC";
				try
				{
					var parameters3 = new
					{
						StockRef = itemLogicalRef
					};
					cancellationToken = ct;
					List<LogoErpWarehouseStockDto> list2 = (await connection.QueryAsync<LogoErpWarehouseStockDto>(new CommandDefinition(commandText3, parameters3, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
					if (list2.Count > 0)
					{
						dto.WarehouseStocks = list2;
						dto.TotalStock = list2.Sum((LogoErpWarehouseStockDto w) => w.StockAmount);
					}
				}
				catch (Exception exception)
				{
					_logger.LogWarning(exception, "STLINE hareket toplamÃƒâ€Ã‚Â± sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
				}
			}
			if (dto.WarehouseStocks.Count == 0)
			{
				dto.WarehouseStocks.Add(new LogoErpWarehouseStockDto
				{
					WarehouseNo = 0,
					WarehouseName = "Merkez Ambar",
					StockAmount = dto.TotalStock
				});
			}
			return dto;
		}
		catch (Exception exception2)
		{
			_logger.LogError(exception2, "Malzeme stok sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return null;
		}
	}

	public async Task<List<LogoErpGroupStockItemDto>> GetGroupStockSummaryAsync(string groupCode, string firmNo, string periodNo, int warehouseNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string itemsTable = "LG_" + firmNo + "_ITEMS";
		string unitTable = "LG_" + firmNo + "_UNITSETL";
		string pattern = BuildSqlPattern(groupCode);
		using IDbConnection connection = CreateConnection();
		_ = 1;
		try
		{
			string value = await ResolveStockSourceTableOrViewAsync(connection, firmNo, periodNo, ct);
			CommandDefinition command = new CommandDefinition($"\r\n                SELECT TOP 25\r\n                    I.CODE AS ItemCode,\r\n                    I.NAME AS ItemName,\r\n                    ISNULL(U.NAME, 'ADET') AS UnitName,\r\n                    ISNULL((\r\n                        SELECT SUM(S.ONHAND) \r\n                        FROM {value} S \r\n                        WHERE S.STOCKREF = I.LOGICALREF AND (S.INVENNO = -1 OR S.INVENNO >= 0)\r\n                    ), 0) AS TotalStock\r\n                FROM {itemsTable} I\r\n                LEFT JOIN {unitTable} U ON U.UNITSETREF = I.UNITSETREF AND U.MAINUNIT = 1\r\n                WHERE I.STGRPCODE = @GroupCode OR I.STGRPCODE LIKE @Pattern OR I.CODE LIKE @Pattern OR I.NAME LIKE @Pattern", new
			{
				GroupCode = groupCode,
				Pattern = pattern
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpGroupStockItemDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Grup stok ÃƒÆ’Ã‚Â¶zeti sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpGroupStockItemDto>();
		}
	}

	public async Task<List<LogoErpSalesmanDto>> GetSalesmenAsync(string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		if (_salesmenCache.TryGetValue(firmNo, out (List<LogoErpSalesmanDto>, DateTime) value) && (DateTime.Now - value.Item2).TotalMinutes < 10.0)
		{
			return value.Item1;
		}
		string[] array = new string[3]
		{
			"LG_" + firmNo + "_SLSMAN",
			"LG_SLSMAN",
			"LG_" + firmNo + "_SALESMAN"
		};
		using IDbConnection connection = CreateConnection();
		string[] array2 = array;
		foreach (string tbl in array2)
		{
			try
			{
				string commandText = "SELECT LOGICALREF AS LogicalRef, ISNULL(CODE, '-') AS Code, ISNULL(DEFINITION_, '-') AS Definition FROM " + tbl + " ORDER BY CODE ASC";
				List<LogoErpSalesmanDto> list = (await connection.QueryAsync<LogoErpSalesmanDto>(new CommandDefinition(commandText, null, null, null, null, CommandFlags.Buffered, ct))).ToList();
				if (list.Count > 0)
				{
					List<LogoErpSalesmanDto> list2 = (from s in list
						group s by new { s.Code, s.Definition } into g
						select g.First() into s
						orderby s.Code
						select s).ToList();
					_salesmenCache[firmNo] = (list2, DateTime.Now);
					_logger.LogDebug("{Table} tablosundan {Count} adet benzersiz SatÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 Temsilcisi (Plasiyer) ÃƒÆ’Ã‚Â§ekildi.", tbl, list2.Count);
					return list2;
				}
			}
			catch (Exception exception)
			{
				_logger.LogDebug(exception, "{Table} tablosu sorgulanÃƒâ€Ã‚Â±rken atlandÃƒâ€Ã‚Â±.", tbl);
			}
		}
		return new List<LogoErpSalesmanDto>();
	}

	public Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string firmNo, CancellationToken ct)
	{
		return SearchFirmsAsync(searchTerm, firmNo, null, ct);
	}

	public Task<List<LogoErpFirmBalanceDto>> GetFirmListAsync(string listType, string firmNo, string periodNo, CancellationToken ct)
	{
		return GetFirmListAsync(listType, firmNo, periodNo, null, ct);
	}

	public Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string firmNo, string periodNo, int warehouseNo, CancellationToken ct)
	{
		return SearchItemStockAsync(searchTerm, firmNo, periodNo, warehouseNo, null, ct);
	}

	public Task<List<LogoErpOpenOrderSummaryDto>> GetOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit, CancellationToken ct)
	{
		return GetOrdersAsync(firmCodeFilter, startDate, endDate, firmNo, periodNo, null, null, limit, ct);
	}

	public Task<List<LogoErpOpenOrderSummaryDto>> GetOpenOrdersAsync(string? firmCodeFilter, string firmNo, string periodNo, int limit, CancellationToken ct)
	{
		return GetOpenOrdersAsync(firmCodeFilter, firmNo, periodNo, null, null, limit, ct);
	}

	public async Task<LogoErpOpenOrderDetailDto?> GetOpenOrderDetailAsync(string ficheNo, string firmNo, string periodNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string lineTable = $"LG_{firmNo}_{periodNo}_ORFLINE";
		string value2 = "LG_" + firmNo + "_CLCARD";
		string itemTable = "LG_" + firmNo + "_ITEMS";
		string unitTable = "LG_" + firmNo + "_UNITSETL";
		string value3 = "LG_SLSMAN";
		string commandText = $"\r\n            SELECT TOP 1\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(S.DEFINITION_, ISNULL(S.CODE, 'Belirtilmedi')) AS SalesmanName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.GROSSTOTAL, 0) AS GrossTotal,\r\n                ISNULL(O.TOTALDISCOUNTS, 0) AS TotalDiscount,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal\r\n            FROM {value} O WITH (NOLOCK)\r\n            LEFT JOIN {value2} C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            LEFT JOIN {value3} S WITH (NOLOCK) ON O.SALESMANREF = S.LOGICALREF\r\n            WHERE O.FICHENO = @FicheNo AND ISNULL(O.CANCELLED, 0) = 0";
		using IDbConnection connection = CreateConnection();
		_ = 1;
		try
		{
			var parameters = new
			{
				FicheNo = ficheNo
			};
			CancellationToken cancellationToken = ct;
			LogoErpOpenOrderDetailDto header = await connection.QueryFirstOrDefaultAsync<LogoErpOpenOrderDetailDto>(new CommandDefinition(commandText, parameters, null, null, null, CommandFlags.Buffered, cancellationToken));
			if (header == null)
			{
				return null;
			}
			string commandText2 = $@"
                SELECT
                    ISNULL(I.CODE, '') AS ItemCode,
                    ISNULL(NULLIF(L.LINEEXP, ''), ISNULL(I.NAME, '')) AS ItemName,
                    ISNULL(L.AMOUNT, 0) AS Amount,
                    ISNULL(L.SHIPPEDAMOUNT, 0) AS ShippedAmount,
                    ISNULL(U.CODE, 'ADET') AS UnitName,
                    ISNULL(L.PRICE, 0) AS ListPrice,
                    CASE 
                        WHEN ISNULL(L.DISCPER, 0) > 0 THEN L.DISCPER 
                        WHEN ISNULL(L.TOTAL, 0) > 0 AND ISNULL(L.DISTDISC, 0) > 0 THEN (L.DISTDISC / L.TOTAL) * 100.0 
                        WHEN ISNULL(L.PRICE, 0) > 0 AND ISNULL(L.LINENET, 0) > 0 AND (L.AMOUNT * L.PRICE) > L.LINENET 
                             THEN (((L.AMOUNT * L.PRICE) - L.LINENET) / (L.AMOUNT * L.PRICE)) * 100.0 
                        ELSE 0 
                    END AS DiscountRate,
                    ISNULL(NULLIF(L.LINENET, 0) / NULLIF(L.AMOUNT, 0), ISNULL(L.PRICE, 0)) AS NetPrice,
                    ISNULL(NULLIF(L.LINENET, 0), L.AMOUNT * L.PRICE) AS LineNetTotal,
                    ISNULL(L.VAT, ISNULL(I.VAT, 20)) AS VatRate
                FROM {lineTable} L WITH (NOLOCK)
                LEFT JOIN {itemTable} I WITH (NOLOCK) ON L.STOCKREF = I.LOGICALREF
                LEFT JOIN {unitTable} U WITH (NOLOCK) ON L.UOMREF = U.LOGICALREF
                WHERE L.ORDFICHEREF = @OrficheRef AND L.LINETYPE = 0 AND ISNULL(L.CANCELLED, 0) = 0
                ORDER BY L.LOGICALREF ASC";
			var parameters2 = new
			{
				OrficheRef = header.LogicalRef
			};
			cancellationToken = ct;
			header.Lines = (await connection.QueryAsync<LogoErpOpenOrderLineDto>(new CommandDefinition(commandText2, parameters2, null, null, null, CommandFlags.Buffered, cancellationToken))).ToList();
			return header;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SipariÅŸ detayÄ± ({FicheNo}) sorgulanÄ±rken hata oluÅŸtu.", ficheNo);
			return null;
		}
	}

	public async Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string firmNo, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string text = "LG_" + firmNo + "_CLCARD";
		string pattern = BuildSqlPattern(searchTerm);
		string commandText = "\r\n            SELECT TOP 20\r\n                LOGICALREF AS LogicalRef,\r\n                CODE AS FirmCode,\r\n                DEFINITION_ AS FirmName\r\n            FROM " + text + "\r\n            WHERE ACTIVE = 0 AND (CODE = @SearchTerm OR CODE LIKE @Pattern OR DEFINITION_ LIKE @Pattern)\r\n              AND (@FirmCyphCode IS NULL OR @FirmCyphCode = '' OR CYPHCODE = @FirmCyphCode OR CYPHCODE LIKE '%' + @FirmCyphCode + '%')\r\n            ORDER BY CASE WHEN CODE = @SearchTerm THEN 1 ELSE 2 END, CODE ASC";
		using IDbConnection connection = CreateConnection();
		try
		{
			string firmCyphCode2 = (string.IsNullOrWhiteSpace(firmCyphCode) ? null : firmCyphCode.Trim());
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				SearchTerm = searchTerm,
				Pattern = pattern,
				FirmCyphCode = firmCyphCode2
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpFirmDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Cari esnek arama yapÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpFirmDto>();
		}
	}

	private static (string p1, string p2, string p3) BuildSqlPatternVariants(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return (p1: "%", p2: "%", p3: "%");
		}
		string text = input.Trim().Replace("*", "%");
		if (!text.Contains("%"))
		{
			text = "%" + text + "%";
		}
		string item = text.ToLowerInvariant();
		string item2 = text.ToUpperInvariant();
		string item3 = text.Replace("i", "Ãƒâ€Ã‚Â°").Replace("Ãƒâ€Ã‚Â±", "I").ToUpper();
		return (p1: item, p2: item2, p3: item3);
	}

	public async Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string firmNo, string periodNo, int warehouseNo, string? itemCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string itemsTable = "LG_" + firmNo + "_ITEMS";
		string unitTable = "LG_" + firmNo + "_UNITSETL";
		(string, string, string) tuple = BuildSqlPatternVariants(searchTerm);
		string p1 = tuple.Item1;
		string p2 = tuple.Item2;
		string p3 = tuple.Item3;
		bool isAll = string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Equals("ALL", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("*") || searchTerm.Equals("TÃƒÆ’Ã…â€œMÃƒÆ’Ã…â€œ", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("HEPSÃƒâ€Ã‚Â°", StringComparison.OrdinalIgnoreCase);
		int topCount = (isAll ? 500 : 50);
		string prcTable = "LG_" + firmNo + "_PRCLIST";
		using IDbConnection connection = CreateConnection();
		_ = 1;
		try
		{
			string value = await ResolveStockSourceTableOrViewAsync(connection, firmNo, periodNo, ct);
			string value2 = (isAll ? "(I.ACTIVE = 0 OR I.ACTIVE IS NULL) AND (@ItemCyphCode IS NULL OR @ItemCyphCode = '' OR I.CYPHCODE = @ItemCyphCode OR I.CYPHCODE LIKE '%' + @ItemCyphCode + '%')" : "(I.ACTIVE = 0 OR I.ACTIVE IS NULL) AND (I.CODE = @SearchTerm OR I.CODE LIKE @P1 OR I.CODE LIKE @P2 OR I.NAME LIKE @P1 OR I.NAME LIKE @P2 OR I.NAME LIKE @P3 OR I.STGRPCODE LIKE @P1 OR I.STGRPCODE LIKE @P2) AND (@ItemCyphCode IS NULL OR @ItemCyphCode = '' OR I.CYPHCODE = @ItemCyphCode OR I.CYPHCODE LIKE '%' + @ItemCyphCode + '%')");
			string commandText = $"\r\n                SELECT TOP {topCount}\r\n                    I.LOGICALREF AS ItemLogicalRef,\r\n                    I.CODE AS ItemCode,\r\n                    I.NAME AS ItemName,\r\n                    ISNULL(I.STGRPCODE, '-') AS GroupCode,\r\n                    ISNULL(U.NAME, 'ADET') AS UnitName,\r\n                    ISNULL(I.VAT, 20) AS Vat,\r\n                    ISNULL((\r\n                        SELECT TOP 1 P.PRICE \r\n                        FROM {prcTable} P WITH (NOLOCK)\r\n                        WHERE P.CARDREF = I.LOGICALREF AND P.PTYPE = 2 AND P.ACTIVE = 0\r\n                        ORDER BY P.LOGICALREF DESC\r\n                    ), 0) AS UnitPrice,\r\n                    ISNULL((\r\n                        SELECT SUM(S.ONHAND)\r\n                        FROM {value} S WITH (NOLOCK)\r\n                        WHERE S.STOCKREF = I.LOGICALREF AND (S.INVENNO = -1 OR S.INVENNO >= 0)\r\n                    ), 0) AS TotalStock\r\n                FROM {itemsTable} I WITH (NOLOCK)\r\n                LEFT JOIN {unitTable} U WITH (NOLOCK) ON U.UNITSETREF = I.UNITSETREF AND U.MAINUNIT = 1\r\n                WHERE {value2}\r\n                ORDER BY I.CODE ASC";
			string itemCyphCode2 = (string.IsNullOrWhiteSpace(itemCyphCode) ? null : itemCyphCode.Trim());
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				SearchTerm = searchTerm,
				P1 = p1,
				P2 = p2,
				P3 = p3,
				ItemCyphCode = itemCyphCode2
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpItemStockDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Esnek malzeme stok aramasÃƒâ€Ã‚Â± yapÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpItemStockDto>();
		}
	}

	public async Task<List<(string Code, LogoErpItemDetails Details)>> SearchItemDetailsAsync(string searchTerm, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string value = "LG_" + firmNo + "_ITEMS";
		string value2 = "LG_" + firmNo + "_UNITSETL";
		string pattern = BuildSqlPattern(searchTerm);
		string commandText = $"\r\n            SELECT TOP 20\r\n                I.CODE AS ProductCode,\r\n                I.LOGICALREF AS ItemLogicalRef,\r\n                ISNULL(\r\n                    (SELECT TOP 1 LOGICALREF FROM {value2} U WHERE U.UNITSETREF = I.UNITSETREF AND (U.MAINUNIT = 1 OR U.LINENR = 1)),\r\n                    (SELECT TOP 1 LOGICALREF FROM {value2} U WHERE U.UNITSETREF = I.UNITSETREF)\r\n                ) AS UnitRef,\r\n                ISNULL(I.UNITSETREF, 0) AS UnitSetRef,\r\n                ISNULL(I.VAT, 20) AS Vat,\r\n                ISNULL(I.NAME, '') AS ItemName\r\n            FROM {value} I\r\n            WHERE I.ACTIVE = 0 AND (I.CODE = @SearchTerm OR I.CODE LIKE @Pattern OR I.NAME LIKE @Pattern)\r\n            ORDER BY CASE WHEN I.CODE = @SearchTerm THEN 1 ELSE 2 END, I.CODE ASC";
		using IDbConnection connection = CreateConnection();
		try
		{
			IEnumerable<object> obj = await connection.QueryAsync(new CommandDefinition(commandText, new
			{
				SearchTerm = searchTerm,
				Pattern = pattern
			}, null, null, null, CommandFlags.Buffered, ct));
			List<(string, LogoErpItemDetails)> list = new List<(string, LogoErpItemDetails)>();
			foreach (dynamic item2 in obj)
			{
				dynamic val = Convert.ToString(item2.ProductCode) ?? "";
				LogoErpItemDetails item = new LogoErpItemDetails(Convert.ToInt32(item2.ItemLogicalRef), Convert.ToInt32(item2.UnitRef), Convert.ToInt32(item2.UnitSetRef), Convert.ToDouble(item2.Vat), Convert.ToString(item2.ItemName) ?? "");
				list.Add(((string)(object)val, item));
			}
			return list;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Esnek malzeme detay aramasÃƒâ€Ã‚Â± yapÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<(string, LogoErpItemDetails)>();
		}
	}

	public async Task<List<LogoErpFirmBalanceDto>> GetFirmListAsync(string listType, string firmNo, string periodNo, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = "LG_" + firmNo + "_CLCARD";
		string value2 = $"LG_{firmNo}_{periodNo}_CLFLINE";
		string value3 = "";
		if (listType == "DEBT")
		{
			value3 = "HAVING \r\n                (ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0)) > 0";
		}
		else if (listType == "CREDIT")
		{
			value3 = "HAVING \r\n                (ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0)) > 0";
		}
		string commandText = $"\r\n            SELECT \r\n                C.CODE AS FirmCode,\r\n                C.DEFINITION_ AS FirmName,\r\n                \r\n                ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) IN (0, 20) AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS BalanceTL,\r\n                \r\n                ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 1 AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 1 AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS BalanceUSD,\r\n                \r\n                ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 2 AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 2 AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS BalanceEUR,\r\n                \r\n                ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 16 AND F.SIGN = 0 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR, 0) = 16 AND F.SIGN = 1 THEN ISNULL(F.TRNET, F.AMOUNT) ELSE 0 END), 0) AS BalanceGBP,\r\n\r\n                ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN F.AMOUNT ELSE 0 END), 0) \r\n                - ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN F.AMOUNT ELSE 0 END), 0) AS NetBalanceLocal\r\n\r\n            FROM {value} C\r\n            LEFT JOIN {value2} F ON C.LOGICALREF = F.CLIENTREF AND F.CANCELLED = 0\r\n            WHERE (@FirmCyphCode IS NULL OR @FirmCyphCode = '' OR C.CYPHCODE = @FirmCyphCode OR C.CYPHCODE LIKE '%' + @FirmCyphCode + '%')\r\n            GROUP BY C.CODE, C.DEFINITION_\r\n            {value3}\r\n            ORDER BY C.DEFINITION_ ASC";
		using IDbConnection connection = CreateConnection();
		try
		{
			string firmCyphCode2 = (string.IsNullOrWhiteSpace(firmCyphCode) ? null : firmCyphCode.Trim());
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				FirmCyphCode = firmCyphCode2
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpFirmBalanceDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Cari liste ({ListType}) sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.", listType);
			return new List<LogoErpFirmBalanceDto>();
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, string? orderCyphCode = null, string? firmCyphCode = null, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string value2 = $"LG_{firmNo}_{periodNo}_ORFLINE";
		string value3 = "LG_" + firmNo + "_CLCARD";
		string commandText = $"\r\n            SELECT TOP (@Limit)\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                (SELECT COUNT(*) FROM {value2} L WITH (NOLOCK) WHERE L.ORDFICHEREF = O.LOGICALREF AND L.LINETYPE = 0 AND ISNULL(L.CANCELLED, 0) = 0) AS LineCount,\r\n                ISNULL(O.GROSSTOTAL, 0) AS GrossTotal,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                CASE \r\n                    WHEN O.STATUS = 1 THEN 'ÃƒÂ¢Ã‚\u008fÃ‚Â³ ÃƒÆ’Ã¢â‚¬â€œneri SipariÃƒâ€¦Ã…\u00b8 (Onay Bekliyor)'\r\n                    WHEN O.STATUS = 2 THEN 'ÃƒÂ¢Ã¢â‚¬ÂºÃ¢â‚¬\u009d Sevk Edilemez'\r\n                    WHEN O.STATUS = 3 THEN 'ÃƒÂ¢Ã…â€œÃ¢â‚¬Â¦ OnaylandÃƒâ€Ã‚Â±'\r\n                    WHEN O.STATUS = 4 THEN 'Ã„Å¸Ã…\u00b8Ã…Â¡Ã…Â¡ Sevk Edilebilir (AÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±k)'\r\n                    WHEN O.STATUS = 8 THEN 'Ã„Å¸Ã…\u00b8Ã¢â‚¬\u009dÃ¢â‚¬â„¢ KapanmÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 SipariÃƒâ€¦Ã…\u00b8'\r\n                    ELSE 'Ã„Å¸Ã…\u00b8Ã¢â‚¬Å“Ã‚Â¦ SipariÃƒâ€¦Ã…\u00b8'\r\n                END AS StatusText\r\n            FROM {value} O WITH (NOLOCK)\r\n            LEFT JOIN {value3} C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE IN (1, 2) AND ISNULL(O.CANCELLED, 0) = 0\r\n              AND (@StartDate IS NULL OR O.DATE_ >= @StartDate)\r\n              AND (@EndDate IS NULL OR O.DATE_ <= @EndDate)\r\n              AND (@FirmCode IS NULL OR C.CODE = @FirmCode OR C.CODE LIKE @FirmCodeLike)\r\n              AND (@OrderCyphCode IS NULL OR @OrderCyphCode = '' OR O.CYPHCODE = @OrderCyphCode OR O.CYPHCODE LIKE '%' + @OrderCyphCode + '%')\r\n              AND (@FirmCyphCode IS NULL OR @FirmCyphCode = '' OR C.CYPHCODE = @FirmCyphCode OR C.CYPHCODE LIKE '%' + @FirmCyphCode + '%')\r\n            ORDER BY O.DATE_ DESC, O.LOGICALREF DESC";
		using IDbConnection connection = CreateConnection();
		try
		{
			string firmCode = (string.IsNullOrWhiteSpace(firmCodeFilter) ? null : firmCodeFilter.Trim());
			string firmCodeLike = (string.IsNullOrWhiteSpace(firmCodeFilter) ? null : ("%" + firmCodeFilter.Trim() + "%"));
			string orderCyphCode2 = (string.IsNullOrWhiteSpace(orderCyphCode) ? null : orderCyphCode.Trim());
			string firmCyphCode2 = (string.IsNullOrWhiteSpace(firmCyphCode) ? null : firmCyphCode.Trim());
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				Limit = limit,
				StartDate = startDate,
				EndDate = endDate,
				FirmCode = firmCode,
				FirmCodeLike = firmCodeLike,
				OrderCyphCode = orderCyphCode2,
				FirmCyphCode = firmCyphCode2
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SipariÃƒâ€¦Ã…\u00b8ler tarih filtresiyle sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetOpenOrdersAsync(string? firmCodeFilter, string firmNo, string periodNo, string? orderCyphCode = null, string? firmCyphCode = null, int limit = 20, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string value2 = $"LG_{firmNo}_{periodNo}_ORFLINE";
		string value3 = "LG_" + firmNo + "_CLCARD";
		string commandText = $"\r\n            SELECT TOP (@Limit)\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                (SELECT COUNT(*) FROM {value2} L WITH (NOLOCK) WHERE L.ORDFICHEREF = O.LOGICALREF AND L.LINETYPE = 0 AND ISNULL(L.CANCELLED, 0) = 0) AS LineCount,\r\n                ISNULL(O.GROSSTOTAL, 0) AS GrossTotal,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                CASE \r\n                    WHEN O.STATUS = 1 THEN 'ÃƒÆ’Ã¢â‚¬â€œneri SipariÃƒâ€¦Ã…\u00b8 (Onay Bekliyor)'\r\n                    WHEN O.STATUS = 2 THEN 'Sevk Edilemez'\r\n                    WHEN O.STATUS = 3 THEN 'OnaylandÃƒâ€Ã‚Â±'\r\n                    WHEN O.STATUS = 4 THEN 'Sevk Edilebilir (AÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±k)'\r\n                    ELSE 'AÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±k SipariÃƒâ€¦Ã…\u00b8'\r\n                END AS StatusText\r\n            FROM {value} O WITH (NOLOCK)\r\n            LEFT JOIN {value3} C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE = 1 AND ISNULL(O.CANCELLED, 0) = 0 AND ISNULL(O.STATUS, 1) IN (1, 2, 3, 4)\r\n              AND (@FirmCode IS NULL OR C.CODE = @FirmCode OR C.CODE LIKE @FirmCodeLike)\r\n              AND (@OrderCyphCode IS NULL OR @OrderCyphCode = '' OR O.CYPHCODE = @OrderCyphCode OR O.CYPHCODE LIKE '%' + @OrderCyphCode + '%')\r\n              AND (@FirmCyphCode IS NULL OR @FirmCyphCode = '' OR C.CYPHCODE = @FirmCyphCode OR C.CYPHCODE LIKE '%' + @FirmCyphCode + '%')\r\n            ORDER BY O.DATE_ DESC, O.LOGICALREF DESC";
		using IDbConnection connection = CreateConnection();
		try
		{
			string firmCode = (string.IsNullOrWhiteSpace(firmCodeFilter) ? null : firmCodeFilter.Trim());
			string firmCodeLike = (string.IsNullOrWhiteSpace(firmCodeFilter) ? null : ("%" + firmCodeFilter.Trim() + "%"));
			string orderCyphCode2 = (string.IsNullOrWhiteSpace(orderCyphCode) ? null : orderCyphCode.Trim());
			string firmCyphCode2 = (string.IsNullOrWhiteSpace(firmCyphCode) ? null : firmCyphCode.Trim());
			CommandDefinition command = new CommandDefinition(commandText, new
			{
				Limit = limit,
				FirmCode = firmCode,
				FirmCodeLike = firmCodeLike,
				OrderCyphCode = orderCyphCode2,
				FirmCyphCode = firmCyphCode2
			}, null, null, null, CommandFlags.Buffered, ct);
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(command)).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±k sipariÃƒâ€¦Ã…\u00b8ler sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<LogoCompanyInfoDto?> GetCompanyInfoAsync(string firmNo, CancellationToken ct = default(CancellationToken))
	{
		int.TryParse(SanitizeNo(firmNo, "001"), out var firmNr);
		string commandText = "\r\n            SELECT TOP 1\r\n                ISNULL(NAME, '') AS Name,\r\n                ISNULL(TITLE, '') AS Title,\r\n                ISNULL(STREET, '') + ' ' + ISNULL(ROAD, '') + ' No:' + ISNULL(DOORNR, '') AS Address,\r\n                ISNULL(CITY, '') AS City,\r\n                ISNULL(COUNTRY, 'TÃƒÆ’Ã‚Â¼rkiye') AS Country,\r\n                ISNULL(PHONE1, '') AS Phone,\r\n                ISNULL(FAX, '') AS Fax,\r\n                ISNULL(TAXOFF, '') AS TaxOffice,\r\n                ISNULL(TAXNR, '') AS TaxNr,\r\n                ISNULL(WEBADDR, '') AS WebUrl,\r\n                ISNULL(EMAILADDR, '') AS Email,\r\n                ISNULL(MERSISNO, '') AS MersisNr,\r\n                ISNULL(TRADEREGISNO, '') AS TradeRegNr\r\n            FROM L_CAPIFIRM WITH (NOLOCK)\r\n            WHERE NR = @FirmNr";
		using IDbConnection connection = CreateConnection();
		try
		{
			return await connection.QueryFirstOrDefaultAsync<LogoCompanyInfoDto>(new CommandDefinition(commandText, new
			{
				FirmNr = firmNr
			}, null, null, null, CommandFlags.Buffered, ct));
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "L_CAPIFIRM firmasÃƒâ€Ã‚Â±ndan firma bilgisi ÃƒÆ’Ã‚Â§ekilirken hata oluÃƒâ€¦Ã…\u00b8tu. FirmNr: {FirmNr}", firmNr);
			return null;
		}
	}

	public async Task<string> GetNextDispatchFicheNoAsync(string firmNo, string periodNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string stFicheTable = $"LG_{firmNo}_{periodNo}_STFICHE";
		string dispatchPrefix = _configuration["LogoErp:DispatchFichePrefix"] ?? "IRS";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		return await GetNextBotFicheNoAsync(connection, null, stFicheTable, dispatchPrefix, ct);
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetInvoicesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string invTable = $"LG_{firmNo}_{periodNo}_INVOICE";
		string clTable = "LG_" + firmNo + "_CLCARD";
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		HashSet<string> hashSet = await GetTableColumnsDirectAsync(connection, null, invTable, ct);
		string value = (hashSet.Contains("EINVOICE") ? "ISNULL(O.EINVOICE, 0)" : "0");
		string value2 = (hashSet.Contains("ESTATUS") ? "ISNULL(O.ESTATUS, 0)" : (hashSet.Contains("EDESPATCHSTATUS") ? "ISNULL(O.EDESPATCHSTATUS, 0)" : "0"));
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(628, 5, stringBuilder2);
		handler.AppendLiteral("\r\n            SELECT TOP (");
		handler.AppendFormatted(limit);
		handler.AppendLiteral(")\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                O.STATUS AS Status,\r\n                ");
		handler.AppendFormatted(value);
		handler.AppendLiteral(" AS EInvoice,\r\n                ");
		handler.AppendFormatted(value2);
		handler.AppendLiteral(" AS EStatus\r\n            FROM ");
		handler.AppendFormatted(invTable);
		handler.AppendLiteral(" O WITH (NOLOCK)\r\n            LEFT JOIN ");
		handler.AppendFormatted(clTable);
		handler.AppendLiteral(" C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE IN (6, 7, 8, 9, 10, 14) AND ISNULL(O.CANCELLED, 0) = 0");
		stringBuilder2.AppendLine(ref handler);
		DynamicParameters dynamicParameters = new DynamicParameters();
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine(" AND C.CODE LIKE @FirmCode");
			dynamicParameters.Add("FirmCode", "%" + firmCodeFilter + "%");
		}
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ >= @StartDate");
			dynamicParameters.Add("StartDate", startDate.Value.Date);
		}
		if (endDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ <= @EndDate");
			dynamicParameters.Add("EndDate", endDate.Value.Date);
		}
		stringBuilder.AppendLine(" ORDER BY O.DATE_ DESC, O.LOGICALREF DESC");
		try
		{
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(new CommandDefinition(stringBuilder.ToString(), dynamicParameters, null, null, null, CommandFlags.Buffered, ct))).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SatÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 faturalarÃƒâ€Ã‚Â± sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_ORFICHE";
		string value2 = "LG_" + firmNo + "_CLCARD";
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(561, 3, stringBuilder2);
		handler.AppendLiteral("\r\n            SELECT TOP (");
		handler.AppendFormatted(limit);
		handler.AppendLiteral(")\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                O.STATUS AS Status\r\n            FROM ");
		handler.AppendFormatted(value);
		handler.AppendLiteral(" O WITH (NOLOCK)\r\n            LEFT JOIN ");
		handler.AppendFormatted(value2);
		handler.AppendLiteral(" C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE = 1 AND ISNULL(O.CANCELLED, 0) = 0");
		stringBuilder2.AppendLine(ref handler);
		DynamicParameters dynamicParameters = new DynamicParameters();
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine(" AND C.CODE LIKE @FirmCode");
			dynamicParameters.Add("FirmCode", "%" + firmCodeFilter + "%");
		}
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ >= @StartDate");
			dynamicParameters.Add("StartDate", startDate.Value.Date);
		}
		if (endDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ <= @EndDate");
			dynamicParameters.Add("EndDate", endDate.Value.Date);
		}
		stringBuilder.AppendLine(" ORDER BY O.DATE_ DESC, O.LOGICALREF DESC");
		using IDbConnection connection = CreateConnection();
		try
		{
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(new CommandDefinition(stringBuilder.ToString(), dynamicParameters, null, null, null, CommandFlags.Buffered, ct))).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AlÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 sipariÃƒâ€¦Ã…\u00b8leri sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseDispatchesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_STFICHE";
		string value2 = "LG_" + firmNo + "_CLCARD";
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(567, 3, stringBuilder2);
		handler.AppendLiteral("\r\n            SELECT TOP (");
		handler.AppendFormatted(limit);
		handler.AppendLiteral(")\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                O.STATUS AS Status\r\n            FROM ");
		handler.AppendFormatted(value);
		handler.AppendLiteral(" O WITH (NOLOCK)\r\n            LEFT JOIN ");
		handler.AppendFormatted(value2);
		handler.AppendLiteral(" C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE IN (1, 2) AND ISNULL(O.CANCELLED, 0) = 0");
		stringBuilder2.AppendLine(ref handler);
		DynamicParameters dynamicParameters = new DynamicParameters();
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine(" AND C.CODE LIKE @FirmCode");
			dynamicParameters.Add("FirmCode", "%" + firmCodeFilter + "%");
		}
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ >= @StartDate");
			dynamicParameters.Add("StartDate", startDate.Value.Date);
		}
		if (endDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ <= @EndDate");
			dynamicParameters.Add("EndDate", endDate.Value.Date);
		}
		stringBuilder.AppendLine(" ORDER BY O.DATE_ DESC, O.LOGICALREF DESC");
		using IDbConnection connection = CreateConnection();
		try
		{
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(new CommandDefinition(stringBuilder.ToString(), dynamicParameters, null, null, null, CommandFlags.Buffered, ct))).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AlÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 irsaliyeleri sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseInvoicesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string value = $"LG_{firmNo}_{periodNo}_INVOICE";
		string value2 = "LG_" + firmNo + "_CLCARD";
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(567, 3, stringBuilder2);
		handler.AppendLiteral("\r\n            SELECT TOP (");
		handler.AppendFormatted(limit);
		handler.AppendLiteral(")\r\n                O.LOGICALREF AS LogicalRef,\r\n                O.FICHENO AS FicheNo,\r\n                O.DATE_ AS Date,\r\n                ISNULL(C.CODE, '') AS FirmCode,\r\n                ISNULL(C.DEFINITION_, '') AS FirmName,\r\n                ISNULL(O.GENEXP1, '') AS Description,\r\n                ISNULL(O.NETTOTAL, 0) AS NetTotal,\r\n                O.STATUS AS Status\r\n            FROM ");
		handler.AppendFormatted(value);
		handler.AppendLiteral(" O WITH (NOLOCK)\r\n            LEFT JOIN ");
		handler.AppendFormatted(value2);
		handler.AppendLiteral(" C WITH (NOLOCK) ON O.CLIENTREF = C.LOGICALREF\r\n            WHERE O.TRCODE IN (1, 2) AND ISNULL(O.CANCELLED, 0) = 0");
		stringBuilder2.AppendLine(ref handler);
		DynamicParameters dynamicParameters = new DynamicParameters();
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine(" AND C.CODE LIKE @FirmCode");
			dynamicParameters.Add("FirmCode", "%" + firmCodeFilter + "%");
		}
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ >= @StartDate");
			dynamicParameters.Add("StartDate", startDate.Value.Date);
		}
		if (endDate.HasValue)
		{
			stringBuilder.AppendLine(" AND O.DATE_ <= @EndDate");
			dynamicParameters.Add("EndDate", endDate.Value.Date);
		}
		stringBuilder.AppendLine(" ORDER BY O.DATE_ DESC, O.LOGICALREF DESC");
		using IDbConnection connection = CreateConnection();
		try
		{
			return (await connection.QueryAsync<LogoErpOpenOrderSummaryDto>(new CommandDefinition(stringBuilder.ToString(), dynamicParameters, null, null, null, CommandFlags.Buffered, ct))).ToList();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AlÃƒâ€Ã‚Â±Ãƒâ€¦Ã…\u00b8 faturalarÃƒâ€Ã‚Â± sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu.");
			return new List<LogoErpOpenOrderSummaryDto>();
		}
	}

	public async Task<string> GetCustomerInvoiceTypeAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		string text = "LG_" + firmNo + "_CLCARD";
		string commandText = "SELECT ISNULL(ACCEPTEINV, 0) FROM " + text + " WITH (NOLOCK) WHERE CODE = @FirmCode";
		using IDbConnection connection = CreateConnection();
		try
		{
			return (await connection.ExecuteScalarAsync<int>(new CommandDefinition(commandText, new
			{
				FirmCode = firmCode
			}, null, null, null, CommandFlags.Buffered, ct)) == 1) ? "EFATURA" : "EARSIV";
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Cari e-Fatura/e-ArÃƒâ€¦Ã…\u00b8iv mÃƒÆ’Ã‚Â¼kellefiyet durumu sorgulanÃƒâ€Ã‚Â±rken hata oluÃƒâ€¦Ã…\u00b8tu: {FirmCode}", firmCode);
			return "EARSIV";
		}
	}

	public async Task<(int lineRef, string ficheNo)> CreateErpCollectionTransactionAsync(int clientRef, decimal amount, int trCode, string firmNo, string periodNo, string description = "Telegram Bot (@LogoCrmBot) - Tahsilat Fişi", int? salesmanRef = null, CancellationToken ct = default(CancellationToken))
	{
		firmNo = SanitizeNo(firmNo, "001");
		periodNo = SanitizeNo(periodNo, "01");
		string clfTable = $"LG_{firmNo}_{periodNo}_CLFLINE";

		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			string prefix = trCode switch
			{
				36 => "POS",
				37 => "CEK",
				_ => "THS"
			};
			string ficheNo = await GetNextBotFicheNoAsync(connection, transaction, clfTable, prefix, ct);

			int refLineId = 0;
			try
			{
				refLineId = await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition($"SELECT TOP 1 LOGICALREF FROM {clfTable} WITH (NOLOCK) ORDER BY LOGICALREF DESC", null, transaction, null, null, CommandFlags.Buffered, ct));
			}
			catch { }

			int insertedLineId = 0;
			if (refLineId > 0)
			{
				string insertSql = $@"
					SET NOCOUNT ON;
					DECLARE @NewId INT;
					IF OBJECT_ID('tempdb..#TempCl') IS NOT NULL DROP TABLE #TempCl;
					SELECT * INTO #TempCl FROM {clfTable} WHERE LOGICALREF = @RefLineId;
					ALTER TABLE #TempCl DROP COLUMN LOGICALREF;
					UPDATE #TempCl SET 
						CLIENTREF = @ClientRef,
						DATE_ = CAST(GETDATE() AS DATE),
						TRCODE = @TrCode,
						AMOUNT = @Amount,
						LINEEXP = @Description,
						SIGN = 1,
						TRANNO = @FicheNo,
						CANCELLED = 0,
						TRCURR = 0,
						SALESMANREF = ISNULL(@SalesmanRef, SALESMANREF);
					INSERT INTO {clfTable} SELECT * FROM #TempCl;
					SET @NewId = CAST(SCOPE_IDENTITY() AS INT);
					DROP TABLE #TempCl;
					SELECT @NewId;";

				insertedLineId = await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(insertSql, new
				{
					RefLineId = refLineId,
					ClientRef = clientRef,
					TrCode = trCode,
					Amount = amount,
					Description = description,
					FicheNo = ficheNo,
					SalesmanRef = salesmanRef
				}, transaction, null, null, CommandFlags.Buffered, ct));
			}
			else
			{
				string directInsert = $@"
					INSERT INTO {clfTable} (CLIENTREF, DATE_, TRCODE, AMOUNT, LINEEXP, SIGN, TRANNO, CANCELLED, TRCURR)
					VALUES (@ClientRef, CAST(GETDATE() AS DATE), @TrCode, @Amount, @Description, 1, @FicheNo, 0, 0);
					SELECT CAST(SCOPE_IDENTITY() AS INT);";
				insertedLineId = await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(directInsert, new
				{
					ClientRef = clientRef,
					TrCode = trCode,
					Amount = amount,
					Description = description,
					FicheNo = ficheNo
				}, transaction, null, null, CommandFlags.Buffered, ct));
			}

			transaction.Commit();
			return (insertedLineId, ficheNo);
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			_logger.LogError(ex, "Tahsilat fişi kesilirken hata oluştu.");
			throw;
		}
	}
}
