using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Application.Services;

public class ProposalAppService : IProposalAppService
{
	private readonly IProposalRepository _proposalRepository;

	private readonly ILogoErpRepository _logoErpRepository;

	private readonly IProposalLogStore _logStore;

	private readonly ITelegramUserStore _telegramUserStore;

	private readonly ICariStatementPdfGenerator _pdfGenerator;

	private readonly IChequeReportPdfGenerator _chequePdfGenerator;

	private readonly IOrderPdfGenerator _orderPdfGenerator;

	private readonly IConfiguration _configuration;

	private readonly ILogger<ProposalAppService> _logger;

	private readonly IAppPermissionStore? _appPermissionStore;

	public ProposalAppService(IProposalRepository proposalRepository, ILogoErpRepository logoErpRepository, IProposalLogStore logStore, ITelegramUserStore telegramUserStore, IConfiguration configuration, ILogger<ProposalAppService> logger, ICariStatementPdfGenerator? pdfGenerator = null, IChequeReportPdfGenerator? chequePdfGenerator = null, IOrderPdfGenerator? orderPdfGenerator = null, IAppPermissionStore? appPermissionStore = null)
	{
		_proposalRepository = proposalRepository;
		_logoErpRepository = logoErpRepository;
		_logStore = logStore;
		_telegramUserStore = telegramUserStore;
		_configuration = configuration;
		_logger = logger;
		_pdfGenerator = pdfGenerator;
		_chequePdfGenerator = chequePdfGenerator;
		_orderPdfGenerator = orderPdfGenerator;
		_appPermissionStore = appPermissionStore;
	}

	private AppPermissionsDto GetAppPermissions()
	{
		return _appPermissionStore?.GetPermissions() ?? new AppPermissionsDto();
	}

	private static bool TryParseAmount(string str, out decimal amount)
	{
		if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) || decimal.TryParse(str, out amount))
		{
			return amount > 0m;
		}
		return false;
	}

	private static string PreprocessVerbalCommand(string messageText)
	{
		if (string.IsNullOrWhiteSpace(messageText))
		{
			return messageText;
		}
		string text = messageText.Trim();
		Match match = Regex.Match(text, "^(.*?)\\s+(?:carisinin|hesabının|firmasının)?\\s*(ekstresi|ekstresini|ekstre|extre|extresi)\\s*(pdf)?$", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			string text2 = match.Groups[1].Value.Trim();
			if (string.IsNullOrWhiteSpace(match.Groups[3].Value))
			{
				return text2 + " EKSTRE";
			}
			return text2 + " EKSTRE PDF";
		}
		Match match2 = Regex.Match(text, "^(.*?)\\s+(?:carisinin|hesabının)?\\s*(çekleri|çek|cek|çek raporu|çek listesi)\\s*(?:ver|getir|göster|bak)?$", RegexOptions.IgnoreCase);
		if (match2.Success)
		{
			return match2.Groups[1].Value.Trim() + " CEK";
		}
		Match match3 = Regex.Match(text, "^(.*?)\\s+(?:carisinin|hesabının)?\\s*(bakiye|bakiyesi|bakiye durumu|riski|risk|risk durumu|limit|limiti)\\s*(?:kaç|nedir|ver|getir|göster)?$", RegexOptions.IgnoreCase);
		if (match3.Success)
		{
			return match3.Groups[1].Value.Trim() + " RISK";
		}
		Match match4 = Regex.Match(text, "^(.*?)\\s+(?:stok|stoğu|stok miktarı|kalan stok|stok durumu|stok sorgu)\\s*(?:kaç|nedir|ver|getir|göster)?$", RegexOptions.IgnoreCase);
		if (match4.Success)
		{
			return match4.Groups[1].Value.Trim() + " STOK";
		}
		Match match5 = Regex.Match(text, "^(?<firm>.*?)\\s+(?:carisine|hesabına|firmasına)?\\s*(?:sipariş|sipariş oluştur|siparişi gir|sipariş ver)\\s+(?<items>.*)$", RegexOptions.IgnoreCase);
		if (match5.Success)
		{
			string value = match5.Groups["firm"].Value.Trim();
			MatchCollection matchCollection = Regex.Matches(Regex.Replace(match5.Groups["items"].Value.Trim(), "\\s+(?:oluştur|gir|ver|yap)$", "", RegexOptions.IgnoreCase), "(?<item>.+?)\\s+(?<qty>\\d+(?:[\\.,]\\d+)?)\\s*(?:tane|adet)?\\s+(?<price>\\d+(?:[\\.,]\\d+)?)\\s*(?:liradan|lira|tl|TL)?(?:\\s+|$)", RegexOptions.IgnoreCase);
			if (matchCollection.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(value);
				foreach (Match item in matchCollection)
				{
					string value2 = item.Groups["item"].Value.Trim();
					string value3 = item.Groups["qty"].Value.Trim();
					string value4 = item.Groups["price"].Value.Trim();
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
					handler.AppendFormatted(value2);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(value3);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(value4);
					stringBuilder2.AppendLine(ref handler);
				}
				return stringBuilder.ToString().Trim();
			}
		}
		return text;
	}

	public async Task<ProposalCreateResult> ProcessTelegramMessageAsync(string messageText, long? chatId = null, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(messageText))
		{
			return ProposalCreateResult.Failure("❌ Boş mesaj gönderildi.");
		}
		messageText = PreprocessVerbalCommand(messageText);
		string telegramUser = "WebUI / Sistem";
		if (chatId.HasValue && chatId.Value != 0L)
		{
			TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId.Value);
			telegramUser = ((user == null) ? $"ID:{chatId.Value}" : ((!string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.Username)) ? (user.FullName + " (@" + user.Username + ")") : ((!string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : (string.IsNullOrWhiteSpace(user.Username) ? $"ID:{chatId.Value}" : ("@" + user.Username)))));
		}
		ProposalLogItem logItem = new ProposalLogItem
		{
			Id = Guid.NewGuid(),
			Timestamp = DateTime.Now,
			TelegramUser = telegramUser,
			RawMessage = messageText
		};
		string text = _configuration["LogoSystem:TargetSystem"] ?? "CRM";
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
		List<string> list = (from l in messageText.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			select l.Trim() into l
			where !string.IsNullOrWhiteSpace(l)
			select l).ToList();
		if (list.Count > 1 && text.Equals("ERP", StringComparison.OrdinalIgnoreCase))
		{
			return await ProcessMultiLineErpMessageAsync(list, firmNo, periodNo, warehouseNo, logItem, chatId, ct);
		}
		string[] array = messageText.Trim().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			string p0Upper = array[0].ToUpperInvariant().Trim();
			if (p0Upper == "/LISTELE" || p0Upper == "/HELP" || p0Upper == "/KOMUTLAR" || p0Upper == "/START" || p0Upper == "YARDIM" || p0Upper == "?")
			{
				return GetHelpMessage();
			}
			if (p0Upper == "/RAPOR" || p0Upper == "RAPOR" || p0Upper == "/OZET" || p0Upper == "OZET" || p0Upper == "/OZETRAPOR" || p0Upper == "OZETRAPOR")
			{
				return ProposalCreateResult.Failure("📊 **GÜNLÜK YÖNETİCİ ÖZET RAPORU**\n\nLütfen `/rapor` komutunu kullanarak anlık ciro, tahsilat ve sipariş özetinizi alınız.");
			}
			if (p0Upper == "/KRITIKSTOK" || p0Upper == "KRITIKSTOK" || p0Upper == "/KRITIK" || p0Upper == "KRITIK")
			{
				return ProposalCreateResult.Failure("🚨 **KRİTİK STOK RAPORU**\n\nLütfen `/kritikstok` komutunu kullanarak stok seviyesi azalan ürünleri sorgulayınız.");
			}
			if (p0Upper == "EKSTRE" || p0Upper == "EXTRE" || p0Upper == "EKSTRESİ")
			{
				return ProposalCreateResult.Failure("ℹ\ufe0f **Hangi Müşterinin Ekstresini Almak İstiyorsunuz?**\n\nLütfen Müşteri Kodunu ekleyerek yazın veya aşağıdaki örneğe tıklayın:\n\ud83d\udc49 Örnek: `RAGIP EXTRE` veya `RAGIP EKSTRE PDF`");
			}
			if (p0Upper == "RISK" || p0Upper == "RİSK" || p0Upper == "BAKİYE" || p0Upper == "BAKIYE")
			{
				return ProposalCreateResult.Failure("ℹ\ufe0f **Hangi Müşterinin Risk & Bakiye Durumunu Sorgulamak İstiyorsunuz?**\n\nLütfen Müşteri Kodunu ekleyerek yazın veya aşağıdaki örneğe tıklayın:\n\ud83d\udc49 Örnek: `RAGIP RISK` veya `MUST01 RISK`");
			}
			if (p0Upper == "CEK" || p0Upper == "ÇEK" || p0Upper == "SENET")
			{
				return ProposalCreateResult.Failure("ℹ\ufe0f **Hangi Müşterinin Çek / Senet Raporunu Almak İstiyorsunuz?**\n\nLütfen Müşteri Kodunu ekleyerek yazın veya aşağıdaki örneğe tıklayın:\n\ud83d\udc49 Örnek: `RAGIP CEK` veya `MUST01 CEK`");
			}
			try
			{
				List<LogoErpFirmDto> list2 = await _logoErpRepository.SearchFirmsAsync(p0Upper, firmNo, ct);
				if (list2 != null && list2.Count > 0)
				{
					LogoErpFirmDto? obj = list2.FirstOrDefault((LogoErpFirmDto f) => string.Equals(f.FirmCode, p0Upper, StringComparison.OrdinalIgnoreCase)) ?? list2[0];
					string firmCode = obj.FirmCode;
					string firmName = obj.FirmName;
					StringBuilder stringBuilder = new StringBuilder();
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder3 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(43, 1, stringBuilder2);
					handler.AppendLiteral("\ud83c\udfe2 **MÜŞTERİ BİLGİSİ BULUNDU** (`Arama: ");
					handler.AppendFormatted(p0Upper);
					handler.AppendLiteral("`)\n");
					stringBuilder3.AppendLine(ref handler);
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
					handler.AppendLiteral("• **Müşteri Kodu:** `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral("`");
					stringBuilder4.AppendLine(ref handler);
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
					handler.AppendLiteral("• **Müşteri Unvanı:** `");
					handler.AppendFormatted(firmName);
					handler.AppendLiteral("`\n");
					stringBuilder5.AppendLine(ref handler);
					if (list2.Count > 1)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder6 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(40, 1, stringBuilder2);
						handler.AppendLiteral("\ud83d\udd0d **Diğer Eşleşen Müşteriler (");
						handler.AppendFormatted(list2.Count - 1);
						handler.AppendLiteral(" Adet):**");
						stringBuilder6.AppendLine(ref handler);
						foreach (LogoErpFirmDto item in list2.Skip(1).Take(3))
						{
							stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder7 = stringBuilder2;
							handler = new StringBuilder.AppendInterpolatedStringHandler(9, 2, stringBuilder2);
							handler.AppendLiteral("  - `");
							handler.AppendFormatted(item.FirmCode);
							handler.AppendLiteral("` | ");
							handler.AppendFormatted(item.FirmName);
							stringBuilder7.AppendLine(ref handler);
						}
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("\ud83d\udc47 **Bu Müşteri İçin Yapabileceğiniz İşlemler (Butona Tıklayın):**\n");
					stringBuilder.AppendLine("\ud83d\udcc4 **1. Hesap Ekstresi Al:**");
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder8 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(40, 2, stringBuilder2);
					handler.AppendLiteral("• Standart: `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" EKSTRE` veya `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" EKSTRE PDF`");
					stringBuilder8.AppendLine(ref handler);
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder9 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(47, 1, stringBuilder2);
					handler.AppendLiteral("• Tarihli: `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" EKSTRE 01.01.2023 31.12.2023 PDF`\n");
					stringBuilder9.AppendLine(ref handler);
					stringBuilder.AppendLine("⚠\ufe0f **2. Risk & Bakiye Sorgula:**");
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder10 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder2);
					handler.AppendLiteral("• Tıklayın: `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" RISK`\n");
					stringBuilder10.AppendLine(ref handler);
					stringBuilder.AppendLine("\ud83d\udcb3 **3. Çek / Senet Portföyü:**");
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder11 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
					handler.AppendLiteral("• Standart: `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" CEK` veya `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" CEK PDF`");
					stringBuilder11.AppendLine(ref handler);
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder12 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(44, 1, stringBuilder2);
					handler.AppendLiteral("• Tarihli: `");
					handler.AppendFormatted(firmCode);
					handler.AppendLiteral(" CEK 01.01.2023 31.12.2023 PDF`\n");
					stringBuilder12.AppendLine(ref handler);
					if (GetAppPermissions().EnableSalesOrder)
					{
						stringBuilder.AppendLine("\ud83d\uded2 **4. Hızlı Sipariş Verme:**");
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder13 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(24, 1, stringBuilder2);
						handler.AppendLiteral("• Örnek: `");
						handler.AppendFormatted(firmCode);
						handler.AppendLiteral(" A.001.01 10`\n");
						stringBuilder13.AppendLine(ref handler);
						stringBuilder.AppendLine("\ud83d\udcdd **5. Çok Satırlı Sipariş Verme:**");
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder14 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(30, 1, stringBuilder2);
						handler.AppendLiteral("`");
						handler.AppendFormatted(firmCode);
						handler.AppendLiteral("`\n`A.001.01 10`\n`B.002.05 20`");
						stringBuilder14.AppendLine(ref handler);
					}
					string message = stringBuilder.ToString();
					logItem.FirmCode = firmCode;
					logItem.IsSuccess = true;
					logItem.Message = message;
					_logStore.AddLog(logItem);
					return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, message);
				}
			}
			catch (Exception exception)
			{
				_logger.LogWarning(exception, "Tek kelimelik Cari araması yapılırken hata oluştu.");
			}
			try
			{
				ProposalCreateResult proposalCreateResult = await ProcessStockCommandAsync(p0Upper, firmNo, periodNo, warehouseNo, logItem, ct);
				if (proposalCreateResult != null && proposalCreateResult.IsSuccess)
				{
					return proposalCreateResult;
				}
			}
			catch
			{
			}
			return GetHelpMessage();
		}
		if (array.Length >= 2)
		{
			string p0Upper2 = array[0].ToUpperInvariant();
			string text2 = array[1].ToUpperInvariant();
			switch (text2)
			{
			case "EXTRE":
			case "EKSTRE":
			case "EKSTRESİ":
			{
				bool flag2 = array.Any((string p) => p.ToUpperInvariant() == "PDF");
				DateTime? startDate = null;
				DateTime? endDate2 = null;
				List<string> list4 = (from p in array.Skip(2)
					where p.ToUpperInvariant() != "PDF"
					select p).ToList();
				if (list4.Count > 0 && DateTime.TryParseExact(list4[0], new string[4] { "dd.MM.yyyy", "dd/MM/yyyy", "d.M.yyyy", "yyyy-MM-dd" }, null, DateTimeStyles.None, out var result3))
				{
					startDate = result3;
				}
				if (list4.Count > 1 && DateTime.TryParseExact(list4[1], new string[4] { "dd.MM.yyyy", "dd/MM/yyyy", "d.M.yyyy", "yyyy-MM-dd" }, null, DateTimeStyles.None, out var result4))
				{
					endDate2 = result4;
				}
				if (flag2)
				{
					byte[] array3 = await GetStatementPdfAsync(p0Upper2, startDate, endDate2, ct);
					if (array3 == null)
					{
						return ProposalCreateResult.Failure("❌ '" + p0Upper2 + "' kodlu Cari hesabına ait ekstre bilgisi bulunamadı.");
					}
					string text4 = (startDate.HasValue ? $"_{startDate:dd.MM.yy}" : "");
					return ProposalCreateResult.PdfSuccess(array3, "Ekstre_" + p0Upper2 + text4 + ".pdf", "\ud83d\udcc4 `" + p0Upper2 + "` kodlu Cari Hesabın Ekstresi (PDF) ektedir.");
				}
				return await ProcessStatementCommandAsync(p0Upper2, firmNo, periodNo, startDate, endDate2, ct);
			}
			case "CEK":
			case "ÇEK":
			{
				bool flag = array.Any((string p) => p.ToUpperInvariant() == "PDF");
				DateTime? startDate = null;
				DateTime? endDate = null;
				List<string> list3 = (from p in array.Skip(2)
					where p.ToUpperInvariant() != "PDF"
					select p).ToList();
				if (list3.Count > 0 && DateTime.TryParseExact(list3[0], new string[4] { "dd.MM.yyyy", "dd/MM/yyyy", "d.M.yyyy", "yyyy-MM-dd" }, null, DateTimeStyles.None, out var result))
				{
					startDate = result;
				}
				if (list3.Count > 1 && DateTime.TryParseExact(list3[1], new string[4] { "dd.MM.yyyy", "dd/MM/yyyy", "d.M.yyyy", "yyyy-MM-dd" }, null, DateTimeStyles.None, out var result2))
				{
					endDate = result2;
				}
				if (flag)
				{
					byte[] array2 = await GetChequeReportPdfAsync(p0Upper2, startDate, endDate, ct);
					if (array2 == null)
					{
						return ProposalCreateResult.Failure("❌ '" + p0Upper2 + "' kodlu Cari hesabına ait çek portföy bilgisi bulunamadı veya çek yok.");
					}
					string text3 = (startDate.HasValue ? $"_{startDate:dd.MM.yy}" : "");
					return ProposalCreateResult.PdfSuccess(array2, "CekRaporu_" + p0Upper2 + text3 + ".pdf", "\ud83d\udcc4 `" + p0Upper2 + "` kodlu Cari Hesabın Çek Raporu (PDF) ektedir.");
				}
				return await ProcessChequeCommandAsync(p0Upper2, firmNo, periodNo, startDate, endDate, ct);
			}
			}
			if (array.Length == 2)
			{
				switch (text2)
				{
				case "RISK":
				case "RİSK":
					return await ProcessRiskCommandAsync(p0Upper2, firmNo, ct);
				case "GSTOK":
				case "GSTOCK":
					return await ProcessGroupStockCommandAsync(p0Upper2, firmNo, periodNo, warehouseNo, logItem, ct);
				}
				if (p0Upper2 == "GSTOK" || p0Upper2 == "GSTOCK")
				{
					return await ProcessGroupStockCommandAsync(text2, firmNo, periodNo, warehouseNo, logItem, ct);
				}
				if (text2 == "STOK" || text2 == "STOCK")
				{
					return await ProcessStockCommandAsync(p0Upper2, firmNo, periodNo, warehouseNo, logItem, ct);
				}
			}
		}
		if (array.Length != 3 && array.Length != 4)
		{
			return GetHelpMessage();
		}
		if (!GetAppPermissions().EnableSalesOrder)
		{
			string message2 = "⚠\ufe0f Satış Siparişi oluşturma yetkisi/modülü sistem yöneticisi tarafından kısıtlanmıştır.";
			logItem.IsSuccess = false;
			logItem.Message = message2;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message2);
		}
		string firmCode2 = array[0].Trim();
		string productCode = array[1].Trim();
		string text5 = array[2].Trim();
		decimal? unitPrice = null;
		if (array.Length == 4 && TryParseAmount(array[3].Trim(), out var amount))
		{
			unitPrice = amount;
		}
		logItem.FirmCode = firmCode2;
		logItem.ProductCode = productCode;
		if ((!decimal.TryParse(text5, NumberStyles.Any, CultureInfo.InvariantCulture, out var result5) && !decimal.TryParse(text5, out result5)) || result5 <= 0m)
		{
			string message3 = "❌ Geçersiz miktar değeri: '" + text5 + "'. Miktar pozitif bir sayı olmalıdır.";
			logItem.IsSuccess = false;
			logItem.Message = message3;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message3);
		}
		logItem.Amount = result5;
		if (text.Equals("ERP", StringComparison.OrdinalIgnoreCase))
		{
			return await ProcessErpMessageAsync(firmCode2, productCode, result5, logItem, chatId, unitPrice, ct);
		}
		return await ProcessCrmMessageAsync(firmCode2, productCode, result5, logItem, ct);
	}

	private async Task<ProposalCreateResult> ProcessMultiLineErpMessageAsync(List<string> lines, string firmNo, string periodNo, int warehouseNo, ProposalLogItem logItem, long? chatId, CancellationToken ct)
	{
		string firmCode = "";
		List<(string productCode, decimal amount, decimal? unitPrice, decimal discountRate)> parsedItems = new List<(string, decimal, decimal?, decimal)>();
		foreach (string line in lines)
		{
			string[] array = line.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 1 && string.IsNullOrEmpty(firmCode))
			{
				firmCode = array[0];
			}
			else if (array.Length == 2)
			{
				if (TryParseAmount(array[1], out var amount))
				{
					parsedItems.Add((array[0], amount, null, 0m));
				}
			}
			else if (array.Length == 3)
			{
				decimal amount3;
				if (string.IsNullOrEmpty(firmCode))
				{
					firmCode = array[0];
					if (TryParseAmount(array[2], out var amount2))
					{
						parsedItems.Add((array[1], amount2, null, 0m));
					}
				}
				else if (TryParseAmount(array[1], out amount3))
				{
					decimal? item = (TryParseAmount(array[2], out var amount4) ? new decimal?(amount4) : ((decimal?)null));
					parsedItems.Add((array[0], amount3, item, 0m));
				}
			}
			else
			{
				if (array.Length < 4)
				{
					continue;
				}
				int num = 0;
				if (string.IsNullOrEmpty(firmCode) && !decimal.TryParse(array[0], out var _))
				{
					firmCode = array[0];
					num = 1;
				}
				decimal amount8;
				if (num < array.Length && array.Length - num >= 3)
				{
					string item2 = array[num];
					if (TryParseAmount(array[num + 1], out var amount5))
					{
						decimal? item3 = (TryParseAmount(array[num + 2], out var amount6) ? new decimal?(amount6) : ((decimal?)null));
						decimal amount7 = 0m;
						if (array.Length - num >= 4)
						{
							TryParseAmount(array[num + 3].Replace("%", "").Trim(), out amount7);
						}
						parsedItems.Add((item2, amount5, item3, amount7));
					}
				}
				else if (num < array.Length && array.Length - num == 2 && TryParseAmount(array[num + 1], out amount8))
				{
					parsedItems.Add((array[num], amount8, null, 0m));
				}
			}
		}
		if (string.IsNullOrWhiteSpace(firmCode) || parsedItems.Count == 0)
		{
			string message = "❌ Çok satırlı sipariş okunamadı. Örnek Format:\nX.01.016\nA.0001.01 10 150.50\nA.0001.02 20";
			logItem.IsSuccess = false;
			logItem.Message = message;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message);
		}
		logItem.FirmCode = firmCode;
		logItem.ProductCode = string.Join(", ", parsedItems.Select<(string, decimal, decimal?, decimal), string>(((string productCode, decimal amount, decimal? unitPrice, decimal discountRate) p) => p.productCode));
		logItem.Amount = parsedItems.Sum<(string, decimal, decimal?, decimal)>(((string productCode, decimal amount, decimal? unitPrice, decimal discountRate) p) => p.amount);
		int? clientRef = await _logoErpRepository.GetFirmLogicalRefAsync(firmCode, firmNo, ct);
		if (!clientRef.HasValue)
		{
			string message2 = "❌ Logo ERP veritabanında '" + firmCode + "' kodlu Cari hesap bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message2;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message2);
		}
		List<LogoErpOrderLineInput> lineInputs = new List<LogoErpOrderLineInput>();
		foreach (var item5 in parsedItems)
		{
			string pCode = item5.productCode;
			decimal amt = item5.amount;
			decimal? prc = item5.unitPrice;
			decimal dRate = item5.discountRate;
			LogoErpItemDetails logoErpItemDetails = await _logoErpRepository.GetItemDetailsAsync(pCode, firmNo, ct);
			if (logoErpItemDetails == null)
			{
				string message3 = "❌ Logo ERP veritabanında '" + pCode + "' kodlu Malzeme bulunamadı.";
				logItem.IsSuccess = false;
				logItem.Message = message3;
				_logStore.AddLog(logItem);
				return ProposalCreateResult.Failure(message3);
			}
			lineInputs.Add(new LogoErpOrderLineInput
			{
				ProductCode = pCode,
				ItemDetails = logoErpItemDetails,
				Amount = amt,
				UnitPrice = prc,
				DiscountRate = dRate
			});
		}
		int? salesmanRef = null;
		string mappedSalesmanName = null;
		if (chatId.HasValue)
		{
			TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId.Value);
			if (user != null)
			{
				if (user.IsMapped)
				{
					salesmanRef = user.MappedSalesmanRef;
					mappedSalesmanName = ((!string.IsNullOrWhiteSpace(user.MappedSalesmanName)) ? user.MappedSalesmanName : ((!string.IsNullOrWhiteSpace(user.MappedSalesmanCode)) ? user.MappedSalesmanCode : user.FullName));
				}
				else
				{
					mappedSalesmanName = ((!string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : ("@" + user.Username));
				}
			}
		}
		try
		{
			string item4 = (await _logoErpRepository.CreateMultiLineErpOrderTransactionAsync(clientRef.Value, lineInputs, firmNo, periodNo, warehouseNo, "Telegram'dan Hızlı Çok Satırlı Sipariş", salesmanRef, ct)).Item3;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("✅ **SİPARİŞ BAŞARIYLA OLUŞTURULDU!**");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("• **Fiş No:** `");
			handler.AppendFormatted(item4);
			handler.AppendLiteral("`");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
			handler.AppendLiteral("• **Cari Kod:** `");
			handler.AppendFormatted(firmCode);
			handler.AppendLiteral("`");
			stringBuilder4.AppendLine(ref handler);
			if (!string.IsNullOrEmpty(mappedSalesmanName))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
				handler.AppendLiteral("• **Satış Temsilcisi:** `");
				handler.AppendFormatted(mappedSalesmanName);
				handler.AppendLiteral("`");
				stringBuilder5.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(36, 1, stringBuilder2);
			handler.AppendLiteral("• **Toplam Kalem Sayısı:** `");
			handler.AppendFormatted(lineInputs.Count);
			handler.AppendLiteral(" Satır`\n");
			stringBuilder6.AppendLine(ref handler);
			stringBuilder.AppendLine("\ud83d\udccb **Sipariş Kalemleri:**");
			for (int num2 = 0; num2 < lineInputs.Count; num2++)
			{
				LogoErpOrderLineInput logoErpOrderLineInput = lineInputs[num2];
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(16, 3, stringBuilder2);
				handler.AppendFormatted(num2 + 1);
				handler.AppendLiteral(". `");
				handler.AppendFormatted(logoErpOrderLineInput.ProductCode);
				handler.AppendLiteral("` - **");
				handler.AppendFormatted(logoErpOrderLineInput.Amount, "N2");
				handler.AppendLiteral(" ADET**");
				stringBuilder7.AppendLine(ref handler);
			}
			string message4 = stringBuilder.ToString();
			logItem.IsSuccess = true;
			logItem.ProposalOid = Guid.NewGuid();
			logItem.Message = message4;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(logItem.ProposalOid.Value, Guid.NewGuid(), message4);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Logo ERP Çok Satırlı Sipariş oluşturulurken hata oluştu.");
			string message5 = "❌ Logo ERP Veritabanı işlemi sırasında hata oluştu: " + ex.Message;
			logItem.IsSuccess = false;
			logItem.Message = message5;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message5);
		}
	}

	private ProposalCreateResult GetHelpMessage()
	{
		AppPermissionsDto appPermissions = GetAppPermissions();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("ℹ\ufe0f **LOGO TELEGRAM & WEB ERP — YAPI LABİLECEKLER KILAVUZU**\n");
		stringBuilder.AppendLine("Sistemimizde kullanabileceğiniz tüm özellikler ve komutlar aşağıda özetlenmiştir:\n");
		stringBuilder.AppendLine("\ud83c\udfe2 **1. Cari Hesap İşlemleri:**");
		stringBuilder.AppendLine("• **Müşteri Arama:** `RAGIP` *(Müşteri kartı ve işlem butonları)*");
		stringBuilder.AppendLine("• **Hesap Ekstresi:** `RAGIP EXTRE` *(Metin dökümü)* veya `RAGIP EKSTRE PDF` *(İmzalı PDF)*");
		stringBuilder.AppendLine("• **Risk & Bakiye Sorgu:** `RAGIP RISK` *(Limit aşımı ve bakiye analizi)*");
		stringBuilder.AppendLine("• **Çek / Senet Raporu:** `RAGIP CEK` *(Portföydeki açık çekler)*\n");
		if (appPermissions.EnableStockModule)
		{
			stringBuilder.AppendLine("\ud83d\udce6 **2. Stok & Malzeme Kartı İşlemleri:**");
			stringBuilder.AppendLine("• **Ürün Arama:** `TOYOTA` veya `A.001.01 STOK` *(Detaylı ambar stokları)*");
			stringBuilder.AppendLine("• **Malzeme Grubu Dökümü:** `HAMMADDE GSTOK` *(Gruba ait stok bakiyeleri)*\n");
		}
		if (appPermissions.EnableSalesOrder)
		{
			stringBuilder.AppendLine("\ud83d\uded2 **3. Sipariş Verme Yöntemleri:**");
			stringBuilder.AppendLine("• **Sözel Tekli Sipariş:** `MUST01 A.001.01 10` *(Cari Kodu, Malzeme Kodu, Miktar)*");
			stringBuilder.AppendLine("• **Çok Satırlı Sipariş:** `MUST01` ilk satır, kalan satırlara `A.001.01 10` yazarak tek fişte toplama");
			stringBuilder.AppendLine("• **Excel Kataloğuyla Sipariş:** Bota `.xlsx` / `.csv` sipariş şablonu yükleme");
			stringBuilder.AppendLine("• **Sesli Mesajla Sipariş:** Bota ses kaydı atarak otomatik sipariş alma\n");
		}
		if (appPermissions.EnableSalesDispatch)
		{
			stringBuilder.AppendLine("\ud83d\ude9a **4. İrsaliye İşlemleri:**");
			stringBuilder.AppendLine("• **Satış İrsaliyeleri:** `/irsaliyeler` veya Cari menüsünden `\ud83d\ude9a İrsaliye Oluştur`\n");
		}
		if (appPermissions.EnableSalesInvoice)
		{
			stringBuilder.AppendLine("\ud83e\uddfe **5. Fatura İşlemleri:**");
			stringBuilder.AppendLine("• **Satış Faturaları:** `/faturalar` veya Cari menüsünden `\ud83e\uddfe Satış Faturası Oluştur`\n");
		}
		stringBuilder.AppendLine("\ud83d\udca1 *Tüm işlemlerde komut yerine doğrudan butonlara tıklayarak da ilerleyebilirsiniz!*");
		return ProposalCreateResult.Failure(stringBuilder.ToString());
	}

	private async Task<ProposalCreateResult> CheckMultipleFirmsAsync(string firmCode, string firmNo, CancellationToken ct)
	{
		List<LogoErpFirmDto> list = await _logoErpRepository.SearchFirmsAsync(firmCode, firmNo, ct);
		if (list != null && list.Count > 1)
		{
			if (list.FirstOrDefault((LogoErpFirmDto f) => string.Equals(f.FirmCode, firmCode, StringComparison.OrdinalIgnoreCase)) != null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(51, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udd0d **BİRDEN FAZLA CARİ HESAP BULUNDU** (`Arama: ");
			handler.AppendFormatted(firmCode);
			handler.AppendLiteral("`)\n");
			stringBuilder3.AppendLine(ref handler);
			foreach (LogoErpFirmDto item in list.Take(10))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(7, 2, stringBuilder2);
				handler.AppendLiteral("• `");
				handler.AppendFormatted(item.FirmCode);
				handler.AppendLiteral("` - ");
				handler.AppendFormatted(item.FirmName);
				stringBuilder4.AppendLine(ref handler);
			}
			stringBuilder.AppendLine("\n\ud83d\udca1 Lütfen işlem yapmak istediğiniz tam cari kodunu belirtiniz.");
			return ProposalCreateResult.Failure(stringBuilder.ToString());
		}
		return null;
	}

	private async Task<ProposalCreateResult> ProcessStatementCommandAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate, DateTime? endDate, CancellationToken ct)
	{
		ProposalCreateResult proposalCreateResult = await CheckMultipleFirmsAsync(firmCode, firmNo, ct);
		if (proposalCreateResult != null)
		{
			return proposalCreateResult;
		}
		LogoErpFirmStatementDto logoErpFirmStatementDto = await _logoErpRepository.GetFirmStatementAsync(firmCode, firmNo, periodNo, startDate, endDate, ct);
		if (logoErpFirmStatementDto == null)
		{
			return ProposalCreateResult.Failure("❌ '" + firmCode + "' kodlu Cari hesabına ait ekstre bilgisi bulunamadı.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("\ud83d\udcc4 **CARİ HESAP EKSTRESİ**");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
		handler.AppendLiteral("• **Cari Kod:** `");
		handler.AppendFormatted(logoErpFirmStatementDto.FirmCode);
		handler.AppendLiteral("`");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
		handler.AppendLiteral("• **Unvan:** `");
		handler.AppendFormatted(logoErpFirmStatementDto.FirmName);
		handler.AppendLiteral("`");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(24, 1, stringBuilder2);
		handler.AppendLiteral("• **Toplam Borç:** `");
		handler.AppendFormatted(logoErpFirmStatementDto.TotalDebit, "N2");
		handler.AppendLiteral(" TL`");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
		handler.AppendLiteral("• **Toplam Alacak:** `");
		handler.AppendFormatted(logoErpFirmStatementDto.TotalCredit, "N2");
		handler.AppendLiteral(" TL`");
		stringBuilder6.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(27, 1, stringBuilder2);
		handler.AppendLiteral("• \ud83d\udcb0 **Net Bakiye:** `");
		handler.AppendFormatted(logoErpFirmStatementDto.Balance, "N2");
		handler.AppendLiteral(" TL`\n");
		stringBuilder7.AppendLine(ref handler);
		if (logoErpFirmStatementDto.CurrencySummaries != null && logoErpFirmStatementDto.CurrencySummaries.Count > 0)
		{
			stringBuilder.AppendLine("\ud83d\udcb1 **Döviz Bazlı Bakiye Özeti:**");
			foreach (CurrencySummaryDto currencySummary in logoErpFirmStatementDto.CurrencySummaries)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(42, 7, stringBuilder2);
				handler.AppendLiteral("• **");
				handler.AppendFormatted(currencySummary.CurrencySymbol);
				handler.AppendLiteral(":** Borç: `");
				handler.AppendFormatted(currencySummary.TotalDebit, "N2");
				handler.AppendLiteral(" ");
				handler.AppendFormatted(currencySummary.CurrencySymbol);
				handler.AppendLiteral("` | Alacak: `");
				handler.AppendFormatted(currencySummary.TotalCredit, "N2");
				handler.AppendLiteral(" ");
				handler.AppendFormatted(currencySummary.CurrencySymbol);
				handler.AppendLiteral("` | Net: `");
				handler.AppendFormatted(currencySummary.Balance, "N2");
				handler.AppendLiteral(" ");
				handler.AppendFormatted(currencySummary.CurrencySymbol);
				handler.AppendLiteral("`");
				stringBuilder8.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("\ud83d\udcdc **Son Cari Hareketler:**");
		if (logoErpFirmStatementDto.RecentTransactions.Count == 0)
		{
			stringBuilder.AppendLine("_Henüz cari hareket kaydı bulunmuyor._");
		}
		else
		{
			foreach (LogoErpStatementLineDto t in logoErpFirmStatementDto.RecentTransactions)
			{
				string borcAlacak = (t.Debit > 0m) ? "Borç" : "Alacak";
				decimal tutar = (t.Debit > 0m) ? t.Debit : t.Credit;
				string birim = string.IsNullOrWhiteSpace(t.CurrencySymbol) ? "TL" : t.CurrencySymbol;
				string fisTuru = string.IsNullOrWhiteSpace(t.TranType) ? "Cari Hareket" : t.TranType;
				string bakiyeLabel = t.RunningBalance >= 0m ? "(B)" : "(A)";
				string bakiyeMetin = $"{Math.Abs(t.RunningBalance):N2} {birim} {bakiyeLabel}";

				stringBuilder.AppendLine($"• **{t.Date:dd.MM.yyyy}** | `{t.TranNo}` | **{fisTuru}** | {t.LineExp} | **{tutar:N2} {birim}** ({borcAlacak}) | *Bakiye: {bakiyeMetin}*");
			}
		}
		return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, stringBuilder.ToString());
	}

	private async Task<ProposalCreateResult> ProcessChequeCommandAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate, DateTime? endDate, CancellationToken ct)
	{
		ProposalCreateResult proposalCreateResult = await CheckMultipleFirmsAsync(firmCode, firmNo, ct);
		if (proposalCreateResult != null)
		{
			return proposalCreateResult;
		}
		List<LogoErpChequeItemDto> list = await _logoErpRepository.GetFirmChequeReportAsync(firmCode, firmNo, periodNo, startDate, endDate, ct);
		if (list == null || list.Count == 0)
		{
			return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, "\ud83d\udcb3 **CARİ ÇEK RAPORU** (`Cari: " + firmCode + "`)\n\n_Portföyde veya takipte aktif çek kaydı bulunmuyor._");
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(33, 1, stringBuilder2);
		handler.AppendLiteral("\ud83d\udcb3 **CARİ ÇEK RAPORU** (`Cari: ");
		handler.AppendFormatted(firmCode);
		handler.AppendLiteral("`)");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
		handler.AppendLiteral("• **Toplam Çek Adedi:** `");
		handler.AppendFormatted(list.Count);
		handler.AppendLiteral("`");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, stringBuilder2);
		handler.AppendLiteral("• \ud83d\udcb5 **Toplam Tutar:** `");
		handler.AppendFormatted(list.Sum((LogoErpChequeItemDto c) => c.Amount), "N2");
		handler.AppendLiteral(" TL`\n");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder.AppendLine("\ud83d\udcdc **Aktif Çek Listesi:**");
		foreach (LogoErpChequeItemDto item in list)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(45, 4, stringBuilder2);
			handler.AppendLiteral("• Vade: ");
			handler.AppendFormatted(item.DueDate, "dd.MM.yyyy");
			handler.AppendLiteral(" | Portföy No: `");
			handler.AppendFormatted(item.PortfoyNo);
			handler.AppendLiteral("` | Tutar: **");
			handler.AppendFormatted(item.Amount, "N2");
			handler.AppendLiteral(" TL** (");
			handler.AppendFormatted(item.StatusText);
			handler.AppendLiteral(")");
			stringBuilder6.AppendLine(ref handler);
		}
		return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, stringBuilder.ToString());
	}

	private async Task<ProposalCreateResult> ProcessRiskCommandAsync(string firmCode, string firmNo, CancellationToken ct)
	{
		ProposalCreateResult proposalCreateResult = await CheckMultipleFirmsAsync(firmCode, firmNo, ct);
		if (proposalCreateResult != null)
		{
			return proposalCreateResult;
		}
		LogoErpFirmRiskDto logoErpFirmRiskDto = await _logoErpRepository.GetFirmRiskReportAsync(firmCode, firmNo, ct);
		if (logoErpFirmRiskDto == null)
		{
			return ProposalCreateResult.Failure("❌ '" + firmCode + "' kodlu Cari hesabına ait risk bilgisi bulunamadı.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("⚠\ufe0f **CARİ RİSK RAPORU**");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
		handler.AppendLiteral("• **Cari Kod:** `");
		handler.AppendFormatted(logoErpFirmRiskDto.FirmCode);
		handler.AppendLiteral("`");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
		handler.AppendLiteral("• **Unvan:** `");
		handler.AppendFormatted(logoErpFirmRiskDto.FirmName);
		handler.AppendLiteral("`");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder2);
		handler.AppendLiteral("• **Tanımlı Risk Limiti:** `");
		handler.AppendFormatted(logoErpFirmRiskDto.RiskLimit, "N2");
		handler.AppendLiteral(" TL`");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder2);
		handler.AppendLiteral("• **Güncel Bakiye Riski:** `");
		handler.AppendFormatted(logoErpFirmRiskDto.NetBalance, "N2");
		handler.AppendLiteral(" TL`");
		stringBuilder6.AppendLine(ref handler);
		if (logoErpFirmRiskDto.UsableLimit < 0m)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(64, 1, stringBuilder2);
			handler.AppendLiteral("• \ud83d\udd34 **Kalan Kullanılabilir Limit:** `");
			handler.AppendFormatted(logoErpFirmRiskDto.UsableLimit, "N2");
			handler.AppendLiteral(" TL` (⚠\ufe0f LİMİT AŞIMI VAR!)");
			stringBuilder7.AppendLine(ref handler);
		}
		else
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(42, 1, stringBuilder2);
			handler.AppendLiteral("• \ud83d\udfe2 **Kalan Kullanılabilir Limit:** `");
			handler.AppendFormatted(logoErpFirmRiskDto.UsableLimit, "N2");
			handler.AppendLiteral(" TL`");
			stringBuilder8.AppendLine(ref handler);
		}
		return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, stringBuilder.ToString());
	}

	private async Task<ProposalCreateResult> ProcessStockCommandAsync(string code, string firmNo, string periodNo, int warehouseNo, ProposalLogItem logItem, CancellationToken ct)
	{
		List<LogoErpItemStockDto> searchItems = await _logoErpRepository.SearchItemStockAsync(code, firmNo, periodNo, warehouseNo, ct);
		if (searchItems != null && searchItems.Count > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(61, 2, stringBuilder2);
			handler.AppendLiteral("\ud83d\udce6 **MALZEME STOK ARAMA RAPORU** (`Arama: ");
			handler.AppendFormatted(code);
			handler.AppendLiteral("` - ");
			handler.AppendFormatted(searchItems.Count);
			handler.AppendLiteral(" Ürün Bulundu)\n");
			stringBuilder3.AppendLine(ref handler);
			foreach (LogoErpItemStockDto item in searchItems)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 4, stringBuilder2);
				handler.AppendLiteral("• `");
				handler.AppendFormatted(item.ItemCode);
				handler.AppendLiteral("` | ");
				handler.AppendFormatted(item.ItemName);
				handler.AppendLiteral(": **");
				handler.AppendFormatted(item.TotalStock, "N2");
				handler.AppendLiteral(" ");
				handler.AppendFormatted(item.UnitName);
				handler.AppendLiteral("**");
				stringBuilder4.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(90, 1, stringBuilder2);
			handler.AppendLiteral("\n\ud83d\udca1 Detaylı ambar dağılımı görmek istediğiniz ürünün kodunu yazabilirsiniz (Örn: `");
			handler.AppendFormatted(searchItems[0].ItemCode);
			handler.AppendLiteral(" STOK`).");
			stringBuilder5.AppendLine(ref handler);
			string message = stringBuilder.ToString();
			logItem.IsSuccess = true;
			logItem.Message = message;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, message);
		}
		LogoErpItemStockDto logoErpItemStockDto = await _logoErpRepository.GetItemStockAsync(code, firmNo, periodNo, warehouseNo, ct);
		if (logoErpItemStockDto != null)
		{
			logItem.ProductCode = logoErpItemStockDto.ItemCode;
			StringBuilder stringBuilder6 = new StringBuilder();
			stringBuilder6.AppendLine("\ud83d\udce6 **MALZEME STOK RAPORU**");
			StringBuilder stringBuilder2 = stringBuilder6;
			StringBuilder stringBuilder7 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
			handler.AppendLiteral("• **Malzeme Kodu:** `");
			handler.AppendFormatted(logoErpItemStockDto.ItemCode);
			handler.AppendLiteral("`");
			stringBuilder7.AppendLine(ref handler);
			stringBuilder2 = stringBuilder6;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
			handler.AppendLiteral("• **Malzeme Adı:** `");
			handler.AppendFormatted(logoErpItemStockDto.ItemName);
			handler.AppendLiteral("`");
			stringBuilder8.AppendLine(ref handler);
			stringBuilder2 = stringBuilder6;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
			handler.AppendLiteral("• **Grup Kodu:** `");
			handler.AppendFormatted(logoErpItemStockDto.GroupCode);
			handler.AppendLiteral("`");
			stringBuilder9.AppendLine(ref handler);
			stringBuilder2 = stringBuilder6;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
			handler.AppendLiteral("• **KDV Oranı:** `% ");
			handler.AppendFormatted(logoErpItemStockDto.Vat);
			handler.AppendLiteral("`");
			stringBuilder10.AppendLine(ref handler);
			stringBuilder2 = stringBuilder6;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, stringBuilder2);
			handler.AppendLiteral("• \ud83d\udcc8 **GENEL TOPLAM STOK:** `");
			handler.AppendFormatted(logoErpItemStockDto.TotalStock, "N2");
			handler.AppendLiteral(" ");
			handler.AppendFormatted(logoErpItemStockDto.UnitName);
			handler.AppendLiteral("`\n");
			stringBuilder11.AppendLine(ref handler);
			stringBuilder6.AppendLine("\ud83c\udfe2 **Ambar Bazlı Stok Dağılımı:**");
			if (logoErpItemStockDto.WarehouseStocks.Count == 0)
			{
				stringBuilder6.AppendLine("_Ambarlarda henüz stok hareketi bulunmuyor._");
			}
			else
			{
				foreach (LogoErpWarehouseStockDto warehouseStock in logoErpItemStockDto.WarehouseStocks)
				{
					string value = ((warehouseStock.WarehouseNo == 0) ? "Merkez Ambar" : $"Ambar {warehouseStock.WarehouseNo}");
					stringBuilder2 = stringBuilder6;
					StringBuilder stringBuilder12 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(18, 4, stringBuilder2);
					handler.AppendLiteral("• **");
					handler.AppendFormatted(value);
					handler.AppendLiteral(" (No: ");
					handler.AppendFormatted(warehouseStock.WarehouseNo);
					handler.AppendLiteral("):** `");
					handler.AppendFormatted(warehouseStock.StockAmount, "N2");
					handler.AppendLiteral(" ");
					handler.AppendFormatted(logoErpItemStockDto.UnitName);
					handler.AppendLiteral("`");
					stringBuilder12.AppendLine(ref handler);
				}
			}
			string message2 = stringBuilder6.ToString();
			logItem.IsSuccess = true;
			logItem.Message = message2;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, message2);
		}
		if (searchItems != null && searchItems.Count == 1)
		{
			return await ProcessStockCommandAsync(searchItems[0].ItemCode, firmNo, periodNo, warehouseNo, logItem, ct);
		}
		string message3 = "❌ Malzeme bulunamadı: '" + code + "' aramasıyla eşleşen stok kaydı veritabanında bulunamadı.";
		logItem.IsSuccess = false;
		logItem.Message = message3;
		_logStore.AddLog(logItem);
		return ProposalCreateResult.Failure(message3);
	}

	private async Task<ProposalCreateResult> ProcessGroupStockCommandAsync(string groupCode, string firmNo, string periodNo, int warehouseNo, ProposalLogItem logItem, CancellationToken ct)
	{
		List<LogoErpGroupStockItemDto> list = await _logoErpRepository.GetGroupStockSummaryAsync(groupCode, firmNo, periodNo, warehouseNo, ct);
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(51, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udcca **MALZEME GRUBU STOK RAPORU** (`Grup / Arama: ");
			handler.AppendFormatted(groupCode);
			handler.AppendLiteral("`)");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(30, 1, stringBuilder2);
			handler.AppendLiteral("• **Eşleşen Ürün Sayısı:** `");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral("`\n");
			stringBuilder4.AppendLine(ref handler);
			foreach (LogoErpGroupStockItemDto item in list)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 4, stringBuilder2);
				handler.AppendLiteral("• `");
				handler.AppendFormatted(item.ItemCode);
				handler.AppendLiteral("` | ");
				handler.AppendFormatted(item.ItemName);
				handler.AppendLiteral(": **");
				handler.AppendFormatted(item.TotalStock, "N2");
				handler.AppendLiteral(" ");
				handler.AppendFormatted(item.UnitName);
				handler.AppendLiteral("**");
				stringBuilder5.AppendLine(ref handler);
			}
			string message = stringBuilder.ToString();
			logItem.IsSuccess = true;
			logItem.Message = message;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(Guid.Empty, Guid.Empty, message);
		}
		string message2 = "❌ Malzeme Grubu veya Ürün bulunamadı: '" + groupCode + "' aramasıyla eşleşen grup kaydı veritabanında bulunamadı.";
		logItem.IsSuccess = false;
		logItem.Message = message2;
		_logStore.AddLog(logItem);
		return ProposalCreateResult.Failure(message2);
	}

	private async Task<ProposalCreateResult> ProcessErpMessageAsync(string firmCode, string productCode, decimal amount, ProposalLogItem logItem, long? chatId, decimal? unitPrice = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
		Task<int?> clientRefTask = _logoErpRepository.GetFirmLogicalRefAsync(firmCode, firmNo, ct);
		Task<LogoErpItemDetails?> itemDetailsTask = _logoErpRepository.GetItemDetailsAsync(productCode, firmNo, ct);
		await Task.WhenAll(clientRefTask, itemDetailsTask);
		int? clientRef = await clientRefTask;
		LogoErpItemDetails logoErpItemDetails = await itemDetailsTask;
		if (!clientRef.HasValue && logoErpItemDetails == null)
		{
			string message = $"❌ Logo ERP veritabanında hem '{firmCode}' Cari hem de '{productCode}' Malzeme kaydı bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message);
		}
		if (!clientRef.HasValue)
		{
			string message2 = "❌ Logo ERP veritabanında '" + firmCode + "' kodlu Cari hesap bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message2;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message2);
		}
		if (logoErpItemDetails == null)
		{
			string message3 = "❌ Logo ERP veritabanında '" + productCode + "' kodlu Malzeme bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message3;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message3);
		}
		int? salesmanRef = null;
		string mappedSalesmanName = null;
		if (chatId.HasValue)
		{
			TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId.Value);
			if (user != null)
			{
				if (user.IsMapped)
				{
					salesmanRef = user.MappedSalesmanRef;
					mappedSalesmanName = ((!string.IsNullOrWhiteSpace(user.MappedSalesmanName)) ? user.MappedSalesmanName : ((!string.IsNullOrWhiteSpace(user.MappedSalesmanCode)) ? user.MappedSalesmanCode : user.FullName));
				}
				else
				{
					mappedSalesmanName = ((!string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : ("@" + user.Username));
				}
			}
		}
		try
		{
			string item = (await _logoErpRepository.CreateErpOrderTransactionAsync(clientRef.Value, logoErpItemDetails, amount, firmNo, periodNo, warehouseNo, "Telegram'dan Hızlı Sipariş", salesmanRef, unitPrice, ct)).Item3;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("✅ **Logo ERP Siparişi Başarıyla Oluşturuldu!**\n");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("• **Fiş No:** `");
			handler.AppendFormatted(item);
			handler.AppendLiteral("`");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
			handler.AppendLiteral("• **Cari Kod:** `");
			handler.AppendFormatted(firmCode);
			handler.AppendLiteral("`");
			stringBuilder4.AppendLine(ref handler);
			if (!string.IsNullOrEmpty(mappedSalesmanName))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
				handler.AppendLiteral("• **Satış Temsilcisi:** `");
				handler.AppendFormatted(mappedSalesmanName);
				handler.AppendLiteral("`");
				stringBuilder5.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
			handler.AppendLiteral("• **Malzeme Kodu:** `");
			handler.AppendFormatted(productCode);
			handler.AppendLiteral("`");
			stringBuilder6.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("• **Miktar:** `");
			handler.AppendFormatted(amount, "N2");
			handler.AppendLiteral("`");
			stringBuilder7.AppendLine(ref handler);
			if (unitPrice.HasValue && unitPrice.Value > 0m)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(24, 1, stringBuilder2);
				handler.AppendLiteral("• **Birim Fiyat:** `");
				handler.AppendFormatted(unitPrice.Value, "N2");
				handler.AppendLiteral(" TL`");
				stringBuilder8.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder9 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
				handler.AppendLiteral("• **Toplam Tutar:** `");
				handler.AppendFormatted(amount * unitPrice.Value, "N2");
				handler.AppendLiteral(" TL`");
				stringBuilder9.AppendLine(ref handler);
			}
			string message4 = stringBuilder.ToString();
			logItem.IsSuccess = true;
			logItem.ProposalOid = Guid.NewGuid();
			logItem.Message = message4;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(logItem.ProposalOid.Value, Guid.NewGuid(), message4);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Logo ERP Sipariş oluşturulurken hata oluştu.");
			string message5 = "❌ Logo ERP Veritabanı işlemi sırasında hata oluştu: " + ex.Message;
			logItem.IsSuccess = false;
			logItem.Message = message5;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message5);
		}
	}

	private async Task<ProposalCreateResult> ProcessCrmMessageAsync(string firmCode, string productCode, decimal amount, ProposalLogItem logItem, CancellationToken ct)
	{
		Task<Guid?> firmRefTask = _proposalRepository.GetFirmOidByCodeAsync(firmCode, ct);
		Task<Guid?> productRefTask = _proposalRepository.GetProductOidByCodeAsync(productCode, ct);
		await Task.WhenAll(firmRefTask, productRefTask);
		Guid? firmOid = await firmRefTask;
		Guid? guid = await productRefTask;
		if (!firmOid.HasValue && !guid.HasValue)
		{
			string message = $"❌ Logo CRM veritabanında hem '{firmCode}' Cari hem de '{productCode}' Malzeme kaydı bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message);
		}
		if (!firmOid.HasValue)
		{
			string message2 = "❌ Logo CRM veritabanında '" + firmCode + "' kodlu Cari hesap bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message2;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message2);
		}
		if (!guid.HasValue)
		{
			string message3 = "❌ Logo CRM veritabanında '" + productCode + "' kodlu Malzeme bulunamadı.";
			logItem.IsSuccess = false;
			logItem.Message = message3;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message3);
		}
		try
		{
			(Guid, Guid) obj = await _proposalRepository.CreateProposalWithLineTransactionAsync(firmOid.Value, guid.Value, amount, "Telegram'dan Hızlı Teklif", ct);
			Guid item = obj.Item1;
			Guid item2 = obj.Item2;
			string message4 = $"✅ **Logo CRM Teklifi Başarıyla Oluşturuldu!**\n\n• **Cari Kod:** `{firmCode}`\n• **Malzeme Kodu:** `{productCode}`\n• **Miktar:** `{amount:N2}`\n• **Teklif ID (Oid):** `{item}`";
			logItem.IsSuccess = true;
			logItem.ProposalOid = item;
			logItem.LineOid = item2;
			logItem.Message = message4;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Success(item, item2, message4);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Logo CRM Teklifi oluşturulurken hata oluştu.");
			string message5 = "❌ Logo CRM Veritabanı işlemi sırasında hata oluştu: " + ex.Message;
			logItem.IsSuccess = false;
			logItem.Message = message5;
			_logStore.AddLog(logItem);
			return ProposalCreateResult.Failure(message5);
		}
	}

	public async Task<byte[]?> GetStatementPdfAsync(string firmCode, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpFirmStatementDto logoErpFirmStatementDto = await _logoErpRepository.GetFirmStatementAsync(firmCode, firmNo, periodNo, startDate, endDate, ct);
		if (logoErpFirmStatementDto == null)
		{
			return null;
		}
		return _pdfGenerator.GenerateStatementPdf(logoErpFirmStatementDto);
	}

	public async Task<byte[]?> GetChequeReportPdfAsync(string firmCode, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpChequeItemDto> list = await _logoErpRepository.GetFirmChequeReportAsync(firmCode, firmNo, periodNo, startDate, endDate, ct);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return _chequePdfGenerator.GenerateChequeReportPdf(firmCode, list);
	}

	public Task<byte[]?> GetOrderPdfAsync(LogoErpOrderPrintDto orderPrintDto, CancellationToken ct = default(CancellationToken))
	{
		if (_orderPdfGenerator == null || orderPrintDto == null)
		{
			return Task.FromResult<byte[]>(null);
		}
		return Task.FromResult(_orderPdfGenerator.GenerateOrderPdf(orderPrintDto));
	}

	public Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, CancellationToken ct)
	{
		return SearchFirmsAsync(searchTerm, null, ct);
	}

	public Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, CancellationToken ct)
	{
		return SearchItemStockAsync(searchTerm, null, ct);
	}

	public Task<byte[]> GetFirmListExcelAsync(string listType, CancellationToken ct)
	{
		return GetFirmListExcelAsync(listType, null, ct);
	}

	public Task<byte[]> GetStockListExcelAsync(CancellationToken ct)
	{
		return GetStockListExcelAsync(null, ct);
	}

	public async Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		return await _logoErpRepository.SearchFirmsAsync(searchTerm, firmNo, firmCyphCode, ct);
	}

	public async Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string? itemCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var result);
		return await _logoErpRepository.SearchItemStockAsync(searchTerm, firmNo, periodNo, result, itemCyphCode, ct);
	}

	public async Task<byte[]> GetFirmListExcelAsync(string listType, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpFirmBalanceDto> obj = await _logoErpRepository.GetFirmListAsync(listType, firmNo, periodNo, firmCyphCode, ct);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Cari Kodu;Cari Unvani;Bakiye (TL);Bakiye (USD);Bakiye (EUR);Bakiye (GBP);Genel Bakiye (Yerel Para)");
		foreach (LogoErpFirmBalanceDto item in obj)
		{
			string value = item.BalanceTL.ToString("0.00", CultureInfo.InvariantCulture);
			string value2 = item.BalanceUSD.ToString("0.00", CultureInfo.InvariantCulture);
			string value3 = item.BalanceEUR.ToString("0.00", CultureInfo.InvariantCulture);
			string value4 = item.BalanceGBP.ToString("0.00", CultureInfo.InvariantCulture);
			string value5 = item.NetBalanceLocal.ToString("0.00", CultureInfo.InvariantCulture);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 7, stringBuilder2);
			handler.AppendFormatted(item.FirmCode);
			handler.AppendLiteral(";");
			handler.AppendFormatted(item.FirmName);
			handler.AppendLiteral(";");
			handler.AppendFormatted(value);
			handler.AppendLiteral(";");
			handler.AppendFormatted(value2);
			handler.AppendLiteral(";");
			handler.AppendFormatted(value3);
			handler.AppendLiteral(";");
			handler.AppendFormatted(value4);
			handler.AppendLiteral(";");
			handler.AppendFormatted(value5);
			stringBuilder2.AppendLine(ref handler);
		}
		return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(stringBuilder.ToString())).ToArray();
	}

	public async Task<byte[]> GetStockListExcelAsync(string? itemCyphCode = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var result);
		List<LogoErpItemStockDto> list = await _logoErpRepository.SearchItemStockAsync("ALL", firmNo, periodNo, result, itemCyphCode, ct);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Malzeme Kodu;Malzeme Adi;Miktar;Birim;Birim Fiyati (TL)");
		if (list != null)
		{
			foreach (LogoErpItemStockDto item in list)
			{
				string value = item.TotalStock.ToString("0.00", CultureInfo.InvariantCulture);
				string value2 = item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 5, stringBuilder2);
				handler.AppendFormatted(item.ItemCode);
				handler.AppendLiteral(";");
				handler.AppendFormatted(item.ItemName);
				handler.AppendLiteral(";");
				handler.AppendFormatted(value);
				handler.AppendLiteral(";");
				handler.AppendFormatted(item.UnitName);
				handler.AppendLiteral(";");
				handler.AppendFormatted(value2);
				stringBuilder2.AppendLine(ref handler);
			}
		}
		return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(stringBuilder.ToString())).ToArray();
	}
}
