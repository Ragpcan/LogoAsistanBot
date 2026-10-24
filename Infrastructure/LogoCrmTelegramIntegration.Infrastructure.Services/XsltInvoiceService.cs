using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SelectPdf;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class XsltInvoiceService : IXsltInvoiceService
{
	private readonly IConfiguration _configuration;

	private readonly ILogger<XsltInvoiceService> _logger;

	private readonly ILogoErpRepository _logoErpRepository;

	public XsltInvoiceService(IConfiguration configuration, ILogger<XsltInvoiceService> logger, ILogoErpRepository logoErpRepository)
	{
		_configuration = configuration;
		_logger = logger;
		_logoErpRepository = logoErpRepository;
	}

	public async Task<string> DetermineInvoiceTypeAsync(string firmCode, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		return await _logoErpRepository.GetCustomerInvoiceTypeAsync(firmCode, firmNo, ct);
	}

	public async Task<string> RenderInvoiceXsltHtmlAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		if (invoice == null)
		{
			throw new ArgumentNullException("invoice");
		}
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string invoiceType = await DetermineInvoiceTypeAsync(invoice.FirmCode, ct);
		string text = ((invoiceType == "EFATURA") ? "eFaturaTemplate.xslt" : "eArsivTemplate.xslt");
		string text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", text);
		string xsltPath = ((!string.IsNullOrWhiteSpace(customXsltPath) && File.Exists(customXsltPath)) ? customXsltPath : text2);
		if (!File.Exists(xsltPath))
		{
			string text3 = Path.Combine(Directory.GetCurrentDirectory(), "src", "Infrastructure", "Templates", text);
			if (File.Exists(text3))
			{
				xsltPath = text3;
			}
			else
			{
				string text4 = Path.Combine(Directory.GetCurrentDirectory(), "Logo_Viewer_App", "uploads", "designs", ((invoiceType == "EFATURA") ? "efatura.xslt" : "earsiv.xslt"));
				if (!File.Exists(text4))
				{
					throw new FileNotFoundException("XSLT Fatura şablon dosyası bulunamadı: " + xsltPath);
				}
				xsltPath = text4;
			}
		}
		_logger.LogInformation("Fatura XSLT belgesi üretiliyor. Tür: {InvoiceType}, Şablon: {XsltPath}", invoiceType, xsltPath);
		string s = BuildUblTrInvoiceXml(invoice, await _logoErpRepository.GetCompanyInfoAsync(firmNo, ct), invoiceType);
		XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
		XsltSettings settings = new XsltSettings(enableDocumentFunction: true, enableScript: true);
		XmlUrlResolver stylesheetResolver = new XmlUrlResolver();
		using (XmlReader stylesheet = XmlReader.Create(xsltPath, new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Parse
		}))
		{
			xslCompiledTransform.Load(stylesheet, settings, stylesheetResolver);
		}
		using XmlReader input = XmlReader.Create(new StringReader(s));
		using StringWriter stringWriter = new StringWriter();
		using XmlWriter results = XmlWriter.Create(stringWriter, xslCompiledTransform.OutputSettings);
		xslCompiledTransform.Transform(input, results);
		return stringWriter.ToString();
	}

	public async Task<byte[]> RenderInvoiceXsltBytesAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		string s = await RenderInvoiceXsltHtmlAsync(invoice, customXsltPath, ct);
		return Encoding.UTF8.GetBytes(s);
	}

	public async Task<byte[]> RenderInvoicePdfAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		if (invoice == null)
		{
			throw new ArgumentNullException("invoice");
		}
		string htmlString = await RenderInvoiceXsltHtmlAsync(invoice, customXsltPath, ct);
		HtmlToPdf htmlToPdf = new HtmlToPdf();
		htmlToPdf.Options.PdfPageSize = PdfPageSize.A4;
		htmlToPdf.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
		htmlToPdf.Options.MarginLeft = 20;
		htmlToPdf.Options.MarginRight = 20;
		htmlToPdf.Options.MarginTop = 20;
		htmlToPdf.Options.MarginBottom = 20;
		PdfDocument pdfDocument = htmlToPdf.ConvertHtmlString(htmlString);
		using MemoryStream memoryStream = new MemoryStream();
		pdfDocument.Save(memoryStream);
		pdfDocument.Close();
		return memoryStream.ToArray();
	}

	private static string F(decimal val)
	{
		return val.ToString("0.00", CultureInfo.InvariantCulture);
	}

	private string BuildUblTrInvoiceXml(LogoErpDispatchDto invoice, LogoCompanyInfoDto? compInfo, string invoiceType)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = ((invoiceType == "EFATURA") ? "TICARIFATURA" : "EARSIVFATURA");
		string value2 = "SATIS";
		stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		stringBuilder.AppendLine("<n1:Invoice xmlns:n1=\"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2\"");
		stringBuilder.AppendLine(" xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\"");
		stringBuilder.AppendLine(" xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\"");
		stringBuilder.AppendLine(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
		stringBuilder.AppendLine("  <cbc:UBLVersionID>2.1</cbc:UBLVersionID>");
		stringBuilder.AppendLine("  <cbc:CustomizationID>TR1.2</cbc:CustomizationID>");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(33, 1, stringBuilder2);
		handler.AppendLiteral("  <cbc:ProfileID>");
		handler.AppendFormatted(value);
		handler.AppendLiteral("</cbc:ProfileID>");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
		handler.AppendLiteral("  <cbc:ID>");
		handler.AppendFormatted(invoice.FicheNo);
		handler.AppendLiteral("</cbc:ID>");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder.AppendLine("  <cbc:CopyIndicator>false</cbc:CopyIndicator>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("  <cbc:UUID>");
		handler.AppendFormatted(Guid.NewGuid());
		handler.AppendLiteral("</cbc:UUID>");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(33, 1, stringBuilder2);
		handler.AppendLiteral("  <cbc:IssueDate>");
		handler.AppendFormatted(invoice.Date, "yyyy-MM-dd");
		handler.AppendLiteral("</cbc:IssueDate>");
		stringBuilder6.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(45, 1, stringBuilder2);
		handler.AppendLiteral("  <cbc:InvoiceTypeCode>");
		handler.AppendFormatted(value2);
		handler.AppendLiteral("</cbc:InvoiceTypeCode>");
		stringBuilder7.AppendLine(ref handler);
		stringBuilder.AppendLine("  <cbc:DocumentCurrencyCode>TRY</cbc:DocumentCurrencyCode>");
		stringBuilder.AppendLine("  <cac:AccountingSupplierParty>");
		stringBuilder.AppendLine("    <cac:Party>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder8 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(39, 1, stringBuilder2);
		handler.AppendLiteral("      <cbc:WebsiteURI>");
		handler.AppendFormatted(compInfo?.WebUrl ?? "www.firm.com");
		handler.AppendLiteral("</cbc:WebsiteURI>");
		stringBuilder8.AppendLine(ref handler);
		stringBuilder.AppendLine("      <cac:PartyIdentification>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder9 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(40, 1, stringBuilder2);
		handler.AppendLiteral("        <cbc:ID schemeID=\"VKN\">");
		handler.AppendFormatted(compInfo?.TaxNr ?? "2710718782");
		handler.AppendLiteral("</cbc:ID>");
		stringBuilder9.AppendLine(ref handler);
		stringBuilder.AppendLine("      </cac:PartyIdentification>");
		stringBuilder.AppendLine("      <cac:PartyName>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder10 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(41, 1, stringBuilder2);
		handler.AppendLiteral("        <cbc:Name><![CDATA[");
		handler.AppendFormatted(compInfo?.Name ?? "DENEME A.Ş.");
		handler.AppendLiteral("]]></cbc:Name>");
		stringBuilder10.AppendLine(ref handler);
		stringBuilder.AppendLine("      </cac:PartyName>");
		stringBuilder.AppendLine("      <cac:PostalAddress>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder11 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(53, 1, stringBuilder2);
		handler.AppendLiteral("        <cbc:StreetName><![CDATA[");
		handler.AppendFormatted(compInfo?.Address ?? "Merkez Mah.");
		handler.AppendLiteral("]]></cbc:StreetName>");
		stringBuilder11.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder12 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(71, 1, stringBuilder2);
		handler.AppendLiteral("        <cbc:CitySubdivisionName><![CDATA[");
		handler.AppendFormatted(compInfo?.City ?? "Adana");
		handler.AppendLiteral("]]></cbc:CitySubdivisionName>");
		stringBuilder12.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder13 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(49, 1, stringBuilder2);
		handler.AppendLiteral("        <cbc:CityName><![CDATA[");
		handler.AppendFormatted(compInfo?.City ?? "Adana");
		handler.AppendLiteral("]]></cbc:CityName>");
		stringBuilder13.AppendLine(ref handler);
		stringBuilder.AppendLine("        <cbc:Country><cbc:Name>Türkiye</cbc:Name></cbc:Country>");
		stringBuilder.AppendLine("      </cac:PostalAddress>");
		stringBuilder.AppendLine("      <cac:PartyTaxScheme>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder14 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(72, 1, stringBuilder2);
		handler.AppendLiteral("        <cac:TaxScheme><cbc:Name><![CDATA[");
		handler.AppendFormatted(compInfo?.TaxOffice ?? "ZİYAPAŞA V.D.");
		handler.AppendLiteral("]]></cbc:Name></cac:TaxScheme>");
		stringBuilder14.AppendLine(ref handler);
		stringBuilder.AppendLine("      </cac:PartyTaxScheme>");
		stringBuilder.AppendLine("    </cac:Party>");
		stringBuilder.AppendLine("  </cac:AccountingSupplierParty>");
		string vknScheme = (invoice.TaxNo.Length == 11) ? "TCKN" : "VKN";
		string vknVal = !string.IsNullOrWhiteSpace(invoice.TaxNo) ? invoice.TaxNo : invoice.FirmCode;
		string addr = !string.IsNullOrWhiteSpace(invoice.FullAddress) ? invoice.FullAddress : "Müşteri Adresi Belirtilmedi";
		string city = !string.IsNullOrWhiteSpace(invoice.City) ? invoice.City : "İSTANBUL";

		stringBuilder.AppendLine("  <cac:AccountingCustomerParty>");
		stringBuilder.AppendLine("    <cac:Party>");
		stringBuilder.AppendLine("      <cac:PartyIdentification>");
		stringBuilder.AppendLine($"        <cbc:ID schemeID=\"{vknScheme}\">{vknVal}</cbc:ID>");
		stringBuilder.AppendLine("      </cac:PartyIdentification>");
		stringBuilder.AppendLine("      <cac:PartyName>");
		stringBuilder.AppendLine($"        <cbc:Name><![CDATA[{invoice.FirmName}]]></cbc:Name>");
		stringBuilder.AppendLine("      </cac:PartyName>");
		stringBuilder.AppendLine("      <cac:PostalAddress>");
		stringBuilder.AppendLine($"        <cbc:StreetName><![CDATA[{addr}]]></cbc:StreetName>");
		stringBuilder.AppendLine($"        <cbc:CityName><![CDATA[{city}]]></cbc:CityName>");
		stringBuilder.AppendLine("        <cac:Country><cbc:Name>Türkiye</cbc:Name></cac:Country>");
		stringBuilder.AppendLine("      </cac:PostalAddress>");
		if (!string.IsNullOrWhiteSpace(invoice.TaxOffice))
		{
			stringBuilder.AppendLine("      <cac:PartyTaxScheme>");
			stringBuilder.AppendLine($"        <cac:TaxScheme><cbc:Name><![CDATA[{invoice.TaxOffice}]]></cbc:Name></cac:TaxScheme>");
			stringBuilder.AppendLine("      </cac:PartyTaxScheme>");
		}
		if (!string.IsNullOrWhiteSpace(invoice.Phone) || !string.IsNullOrWhiteSpace(invoice.Email))
		{
			stringBuilder.AppendLine("      <cac:Contact>");
			if (!string.IsNullOrWhiteSpace(invoice.Phone))
			{
				stringBuilder.AppendLine($"        <cbc:Telephone>{invoice.Phone}</cbc:Telephone>");
			}
			if (!string.IsNullOrWhiteSpace(invoice.Email))
			{
				stringBuilder.AppendLine($"        <cbc:ElectronicMail>{invoice.Email}</cbc:ElectronicMail>");
			}
			stringBuilder.AppendLine("      </cac:Contact>");
		}
		stringBuilder.AppendLine("    </cac:Party>");
		stringBuilder.AppendLine("  </cac:AccountingCustomerParty>");
		decimal num = ((invoice.GrossTotal > 0m) ? invoice.GrossTotal : invoice.Lines.Sum((LogoErpDispatchLineDto l) => l.Amount * l.UnitPrice));
		if (num == 0m)
		{
			num = ((invoice.NetTotal > 0m) ? invoice.NetTotal : 15000m);
		}
		decimal totalDiscount = invoice.TotalDiscount;
		decimal num2 = ((invoice.TotalVat > 0m) ? invoice.TotalVat : invoice.Lines.Sum((LogoErpDispatchLineDto l) => (!(l.VatAmount > 0m)) ? (((l.LineTotal > 0m) ? l.LineTotal : (l.Amount * l.UnitPrice)) * 0.20m) : l.VatAmount));
		if (num2 == 0m)
		{
			num2 = (num - totalDiscount) * 0.20m;
		}
		decimal val = ((invoice.NetTotal > 0m) ? invoice.NetTotal : (num - totalDiscount + num2));
		if (totalDiscount > 0m)
		{
			stringBuilder.AppendLine("  <cac:AllowanceCharge>");
			stringBuilder.AppendLine("    <cbc:ChargeIndicator>false</cbc:ChargeIndicator>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder16 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(46, 1, stringBuilder2);
			handler.AppendLiteral("    <cbc:Amount currencyID=\"TRY\">");
			handler.AppendFormatted(F(totalDiscount));
			handler.AppendLiteral("</cbc:Amount>");
			stringBuilder16.AppendLine(ref handler);
			stringBuilder.AppendLine("  </cac:AllowanceCharge>");
		}
		stringBuilder.AppendLine("  <cac:TaxTotal>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder17 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(52, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:TaxAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(num2));
		handler.AppendLiteral("</cbc:TaxAmount>");
		stringBuilder17.AppendLine(ref handler);
		stringBuilder.AppendLine("    <cac:TaxSubtotal>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder18 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(62, 1, stringBuilder2);
		handler.AppendLiteral("      <cbc:TaxableAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(num - totalDiscount));
		handler.AppendLiteral("</cbc:TaxableAmount>");
		stringBuilder18.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder19 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(54, 1, stringBuilder2);
		handler.AppendLiteral("      <cbc:TaxAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(num2));
		handler.AppendLiteral("</cbc:TaxAmount>");
		stringBuilder19.AppendLine(ref handler);
		stringBuilder.AppendLine("      <cac:TaxCategory>");
		stringBuilder.AppendLine("        <cbc:Percent>20</cbc:Percent>");
		stringBuilder.AppendLine("        <cac:TaxScheme><cbc:Name>KDV</cbc:Name><cbc:TaxTypeCode>0015</cbc:TaxTypeCode></cac:TaxScheme>");
		stringBuilder.AppendLine("      </cac:TaxCategory>");
		stringBuilder.AppendLine("    </cac:TaxSubtotal>");
		stringBuilder.AppendLine("  </cac:TaxTotal>");
		stringBuilder.AppendLine("  <cac:LegalMonetaryTotal>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder20 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(72, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:LineExtensionAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(num - totalDiscount));
		handler.AppendLiteral("</cbc:LineExtensionAmount>");
		stringBuilder20.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder21 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(70, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:TaxExclusiveAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(num - totalDiscount));
		handler.AppendLiteral("</cbc:TaxExclusiveAmount>");
		stringBuilder21.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder22 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(70, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:TaxInclusiveAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(val));
		handler.AppendLiteral("</cbc:TaxInclusiveAmount>");
		stringBuilder22.AppendLine(ref handler);
		if (totalDiscount > 0m)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder23 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(74, 1, stringBuilder2);
			handler.AppendLiteral("    <cbc:AllowanceTotalAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(totalDiscount));
			handler.AppendLiteral("</cbc:AllowanceTotalAmount>");
			stringBuilder23.AppendLine(ref handler);
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder24 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(60, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:PayableAmount currencyID=\"TRY\">");
		handler.AppendFormatted(F(val));
		handler.AppendLiteral("</cbc:PayableAmount>");
		stringBuilder24.AppendLine(ref handler);
		stringBuilder.AppendLine("  </cac:LegalMonetaryTotal>");
		int num3 = 1;
		foreach (LogoErpDispatchLineDto line in invoice.Lines)
		{
			decimal num4 = line.Amount * line.UnitPrice;
			decimal num5 = ((line.DiscountRate > 0m) ? (num4 * (line.DiscountRate / 100m)) : 0m);
			if (num5 == 0m && line.LineTotal > 0m && line.LineTotal < num4)
			{
				num5 = num4 - line.LineTotal;
			}
			decimal num6 = num4 - num5;
			if (num6 <= 0m && line.LineTotal > 0m)
			{
				num6 = line.LineTotal;
			}
			decimal val2 = ((line.VatAmount > 0m) ? line.VatAmount : (num6 * (decimal)(line.VatRate / 100.0)));
			stringBuilder.AppendLine("  <cac:InvoiceLine>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder25 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
			handler.AppendLiteral("    <cbc:ID>");
			handler.AppendFormatted(num3);
			handler.AppendLiteral("</cbc:ID>");
			stringBuilder25.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder26 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(64, 1, stringBuilder2);
			handler.AppendLiteral("    <cbc:InvoicedQuantity unitCode=\"C62\">");
			handler.AppendFormatted(F(line.Amount));
			handler.AppendLiteral("</cbc:InvoicedQuantity>");
			stringBuilder26.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder27 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(72, 1, stringBuilder2);
			handler.AppendLiteral("    <cbc:LineExtensionAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(num6));
			handler.AppendLiteral("</cbc:LineExtensionAmount>");
			stringBuilder27.AppendLine(ref handler);
			if (num5 > 0m)
			{
				stringBuilder.AppendLine("    <cac:AllowanceCharge>");
				stringBuilder.AppendLine("      <cbc:ChargeIndicator>false</cbc:ChargeIndicator>");
				if (line.DiscountRate > 0m)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder28 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(65, 1, stringBuilder2);
					handler.AppendLiteral("      <cbc:MultiplierFactorNumeric>");
					handler.AppendFormatted(line.DiscountRate.ToString("0.00", CultureInfo.InvariantCulture));
					handler.AppendLiteral("</cbc:MultiplierFactorNumeric>");
					stringBuilder28.AppendLine(ref handler);
				}
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder29 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(48, 1, stringBuilder2);
				handler.AppendLiteral("      <cbc:Amount currencyID=\"TRY\">");
				handler.AppendFormatted(F(num5));
				handler.AppendLiteral("</cbc:Amount>");
				stringBuilder29.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder30 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(56, 1, stringBuilder2);
				handler.AppendLiteral("      <cbc:BaseAmount currencyID=\"TRY\">");
				handler.AppendFormatted(F(num4));
				handler.AppendLiteral("</cbc:BaseAmount>");
				stringBuilder30.AppendLine(ref handler);
				stringBuilder.AppendLine("    </cac:AllowanceCharge>");
			}
			stringBuilder.AppendLine("    <cac:TaxTotal>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder31 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(54, 1, stringBuilder2);
			handler.AppendLiteral("      <cbc:TaxAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(val2));
			handler.AppendLiteral("</cbc:TaxAmount>");
			stringBuilder31.AppendLine(ref handler);
			stringBuilder.AppendLine("      <cac:TaxSubtotal>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder32 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(64, 1, stringBuilder2);
			handler.AppendLiteral("        <cbc:TaxableAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(num6));
			handler.AppendLiteral("</cbc:TaxableAmount>");
			stringBuilder32.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder33 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(56, 1, stringBuilder2);
			handler.AppendLiteral("        <cbc:TaxAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(val2));
			handler.AppendLiteral("</cbc:TaxAmount>");
			stringBuilder33.AppendLine(ref handler);
			stringBuilder.AppendLine("        <cac:TaxCategory>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder34 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(37, 1, stringBuilder2);
			handler.AppendLiteral("          <cbc:Percent>");
			handler.AppendFormatted(line.VatRate, "F0");
			handler.AppendLiteral("</cbc:Percent>");
			stringBuilder34.AppendLine(ref handler);
			stringBuilder.AppendLine("          <cac:TaxScheme><cbc:Name>KDV</cbc:Name><cbc:TaxTypeCode>0015</cbc:TaxTypeCode></cac:TaxScheme>");
			stringBuilder.AppendLine("        </cac:TaxCategory>");
			stringBuilder.AppendLine("      </cac:TaxSubtotal>");
			stringBuilder.AppendLine("    </cac:TaxTotal>");
			string codeVal = !string.IsNullOrWhiteSpace(line.ItemCode) ? line.ItemCode : line.ItemName;
			string nameVal = !string.IsNullOrWhiteSpace(line.ItemName) ? line.ItemName : line.ItemCode;

			stringBuilder.AppendLine("    <cac:Item>");
			stringBuilder.AppendLine($"      <cbc:Name><![CDATA[{codeVal}]]></cbc:Name>");
			stringBuilder.AppendLine($"      <cbc:Description><![CDATA[{nameVal}]]></cbc:Description>");
			stringBuilder.AppendLine("      <cac:SellersItemIdentification>");
			stringBuilder.AppendLine($"        <cbc:ID>{codeVal}</cbc:ID>");
			stringBuilder.AppendLine("      </cac:SellersItemIdentification>");
			stringBuilder.AppendLine("    </cac:Item>");
			stringBuilder.AppendLine("    <cac:Price>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder38 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(58, 1, stringBuilder2);
			handler.AppendLiteral("      <cbc:PriceAmount currencyID=\"TRY\">");
			handler.AppendFormatted(F(line.UnitPrice));
			handler.AppendLiteral("</cbc:PriceAmount>");
			stringBuilder38.AppendLine(ref handler);
			stringBuilder.AppendLine("    </cac:Price>");
			stringBuilder.AppendLine("  </cac:InvoiceLine>");
			num3++;
		}
		stringBuilder.AppendLine("</n1:Invoice>");
		return stringBuilder.ToString();
	}
}
