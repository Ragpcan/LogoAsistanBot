using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF;
using QuestPDF.Infrastructure;
using SelectPdf;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class XsltDispatchService : IXsltDispatchService
{
	private readonly IConfiguration _configuration;

	private readonly ILogger<XsltDispatchService> _logger;

	private readonly ILogoErpRepository _logoErpRepository;

	public XsltDispatchService(IConfiguration configuration, ILogger<XsltDispatchService> logger, ILogoErpRepository logoErpRepository)
	{
		_configuration = configuration;
		_logger = logger;
		_logoErpRepository = logoErpRepository;
		Settings.License = LicenseType.Community;
	}

	public async Task<string> RenderDispatchXsltHtmlAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		if (dispatch == null)
		{
			throw new ArgumentNullException("dispatch");
		}
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "eIrsaliyeTemplate.xslt");
		string text2 = _configuration["LogoSystem:XsltTemplatePath"];
		string xsltPath = ((!string.IsNullOrWhiteSpace(customXsltPath) && File.Exists(customXsltPath)) ? customXsltPath : ((!string.IsNullOrWhiteSpace(text2) && File.Exists(text2)) ? text2 : text));
		if (!File.Exists(xsltPath))
		{
			string text3 = Path.Combine(Directory.GetCurrentDirectory(), "src", "Infrastructure", "Templates", "eIrsaliyeTemplate.xslt");
			if (!File.Exists(text3))
			{
				throw new FileNotFoundException("XSLT İrsaliye şablon dosyası bulunamadı: " + xsltPath);
			}
			xsltPath = text3;
		}
		_logger.LogInformation("İrsaliye XSLT belgesi üretiliyor. Kullanılan Şablon: {XsltPath}", xsltPath);
		string s;
		if (File.ReadAllText(xsltPath).Contains("DespatchAdvice"))
		{
			string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
			s = BuildUblTrXml(dispatch, await _logoErpRepository.GetCompanyInfoAsync(firmNo, ct));
		}
		else
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(LogoErpDispatchDto));
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add("", "");
			using StringWriter stringWriter = new StringWriter();
			using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true
			});
			xmlSerializer.Serialize(xmlWriter, dispatch, xmlSerializerNamespaces);
			s = stringWriter.ToString();
		}
		XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
		xslCompiledTransform.Load(xsltPath);
		using XmlReader input = XmlReader.Create(new StringReader(s));
		using StringWriter stringWriter2 = new StringWriter();
		xslCompiledTransform.Transform(input, null, stringWriter2);
		return stringWriter2.ToString();
	}

	public async Task<string> BuildUblTrXmlPublicAsync(LogoErpDispatchDto dispatch, LogoCompanyInfoDto? compInfo = null, CancellationToken ct = default(CancellationToken))
	{
		if (compInfo == null)
		{
			string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
			compInfo = await _logoErpRepository.GetCompanyInfoAsync(firmNo, ct);
		}
		return BuildUblTrXml(dispatch, compInfo);
	}

	private string BuildUblTrXml(LogoErpDispatchDto dispatch, LogoCompanyInfoDto? compInfo)
	{
		string str = ((!string.IsNullOrWhiteSpace(compInfo?.Title)) ? compInfo.Title : ((!string.IsNullOrWhiteSpace(compInfo?.Name)) ? compInfo.Name : (_configuration["LogoSystem:CompanyName"] ?? "LOGO CRM & ERP TEKNOLOJİ A.Ş.")));
		string str2 = ((!string.IsNullOrWhiteSpace(compInfo?.TaxNr)) ? compInfo.TaxNr : "1234567890");
		string str3 = ((!string.IsNullOrWhiteSpace(compInfo?.TaxOffice)) ? compInfo.TaxOffice : "Büyük Mükellefler V.D.");
		string str4 = ((!string.IsNullOrWhiteSpace(compInfo?.Address)) ? compInfo.Address.Trim() : (_configuration["LogoSystem:CompanyAddress"] ?? "İstanbul / Türkiye"));
		string str5 = ((!string.IsNullOrWhiteSpace(compInfo?.City)) ? compInfo.City : "İSTANBUL");
		string text = compInfo?.Phone ?? "";
		string text2 = compInfo?.Fax ?? "";
		string text3 = compInfo?.Email ?? "";
		_ = compInfo?.WebUrl;
		string text4 = compInfo?.MersisNr ?? "";
		string text5 = compInfo?.TradeRegNr ?? "";
		string value = dispatch.Date.ToString("yyyy-MM-dd");
		string value2 = dispatch.Date.ToString("HH:mm:ss");
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		stringBuilder.AppendLine("<n1:DespatchAdvice");
		stringBuilder.AppendLine("    xmlns:n1=\"urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2\"");
		stringBuilder.AppendLine("    xmlns:cac=\"urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2\"");
		stringBuilder.AppendLine("    xmlns:cbc=\"urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2\"");
		stringBuilder.AppendLine("    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
		stringBuilder.AppendLine("    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>");
		stringBuilder.AppendLine("    <cbc:CustomizationID>TR1.2.1</cbc:CustomizationID>");
		stringBuilder.AppendLine("    <cbc:ProfileID>TEMELIRSALIYE</cbc:ProfileID>");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:ID>");
		handler.AppendFormatted(SecurityElement.Escape(dispatch.FicheNo));
		handler.AppendLiteral("</cbc:ID>");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder.AppendLine("    <cbc:CopyIndicator>false</cbc:CopyIndicator>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:UUID>");
		handler.AppendFormatted(Guid.NewGuid());
		handler.AppendLiteral("</cbc:UUID>");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:IssueDate>");
		handler.AppendFormatted(value);
		handler.AppendLiteral("</cbc:IssueDate>");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:IssueTime>");
		handler.AppendFormatted(value2);
		handler.AppendLiteral("</cbc:IssueTime>");
		stringBuilder6.AppendLine(ref handler);
		stringBuilder.AppendLine("    <cbc:DespatchAdviceTypeCode>SEVK</cbc:DespatchAdviceTypeCode>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:Note>");
		handler.AppendFormatted(SecurityElement.Escape(dispatch.Description));
		handler.AppendLiteral("</cbc:Note>");
		stringBuilder7.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder8 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(49, 1, stringBuilder2);
		handler.AppendLiteral("    <cbc:LineCountNumeric>");
		handler.AppendFormatted(dispatch.Lines.Count);
		handler.AppendLiteral("</cbc:LineCountNumeric>");
		stringBuilder8.AppendLine(ref handler);
		stringBuilder.AppendLine("    <cac:DespatchSupplierParty>");
		stringBuilder.AppendLine("        <cac:Party>");
		stringBuilder.AppendLine("            <cac:PartyIdentification>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder9 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(48, 1, stringBuilder2);
		handler.AppendLiteral("                <cbc:ID schemeID=\"VKN\">");
		handler.AppendFormatted(SecurityElement.Escape(str2));
		handler.AppendLiteral("</cbc:ID>");
		stringBuilder9.AppendLine(ref handler);
		stringBuilder.AppendLine("            </cac:PartyIdentification>");
		if (!string.IsNullOrWhiteSpace(text4))
		{
			stringBuilder.AppendLine("            <cac:PartyIdentification>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(53, 1, stringBuilder2);
			handler.AppendLiteral("                <cbc:ID schemeID=\"MERSISNO\">");
			handler.AppendFormatted(SecurityElement.Escape(text4));
			handler.AppendLiteral("</cbc:ID>");
			stringBuilder10.AppendLine(ref handler);
			stringBuilder.AppendLine("            </cac:PartyIdentification>");
		}
		if (!string.IsNullOrWhiteSpace(text5))
		{
			stringBuilder.AppendLine("            <cac:PartyIdentification>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(59, 1, stringBuilder2);
			handler.AppendLiteral("                <cbc:ID schemeID=\"TICARETSICILNO\">");
			handler.AppendFormatted(SecurityElement.Escape(text5));
			handler.AppendLiteral("</cbc:ID>");
			stringBuilder11.AppendLine(ref handler);
			stringBuilder.AppendLine("            </cac:PartyIdentification>");
		}
		stringBuilder.AppendLine("            <cac:PartyName>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder12 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(37, 1, stringBuilder2);
		handler.AppendLiteral("                <cbc:Name>");
		handler.AppendFormatted(SecurityElement.Escape(str));
		handler.AppendLiteral("</cbc:Name>");
		stringBuilder12.AppendLine(ref handler);
		stringBuilder.AppendLine("            </cac:PartyName>");
		stringBuilder.AppendLine("            <cac:PostalAddress>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder13 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(49, 1, stringBuilder2);
		handler.AppendLiteral("                <cbc:StreetName>");
		handler.AppendFormatted(SecurityElement.Escape(str4));
		handler.AppendLiteral("</cbc:StreetName>");
		stringBuilder13.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder14 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(45, 1, stringBuilder2);
		handler.AppendLiteral("                <cbc:CityName>");
		handler.AppendFormatted(SecurityElement.Escape(str5));
		handler.AppendLiteral("</cbc:CityName>");
		stringBuilder14.AppendLine(ref handler);
		stringBuilder.AppendLine("                <cac:Country><cbc:Name>Türkiye</cbc:Name></cac:Country>");
		stringBuilder.AppendLine("            </cac:PostalAddress>");
		stringBuilder.AppendLine("            <cac:PartyTaxScheme>");
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder15 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(68, 1, stringBuilder2);
		handler.AppendLiteral("                <cac:TaxScheme><cbc:Name>");
		handler.AppendFormatted(SecurityElement.Escape(str3));
		handler.AppendLiteral("</cbc:Name></cac:TaxScheme>");
		stringBuilder15.AppendLine(ref handler);
		stringBuilder.AppendLine("            </cac:PartyTaxScheme>");
		if (!string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(text3))
		{
			stringBuilder.AppendLine("            <cac:Contact>");
			if (!string.IsNullOrWhiteSpace(text))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder16 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(47, 1, stringBuilder2);
				handler.AppendLiteral("                <cbc:Telephone>");
				handler.AppendFormatted(SecurityElement.Escape(text));
				handler.AppendLiteral("</cbc:Telephone>");
				stringBuilder16.AppendLine(ref handler);
			}
			if (!string.IsNullOrWhiteSpace(text2))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder17 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(43, 1, stringBuilder2);
				handler.AppendLiteral("                <cbc:Telefax>");
				handler.AppendFormatted(SecurityElement.Escape(text2));
				handler.AppendLiteral("</cbc:Telefax>");
				stringBuilder17.AppendLine(ref handler);
			}
			if (!string.IsNullOrWhiteSpace(text3))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder18 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(57, 1, stringBuilder2);
				handler.AppendLiteral("                <cbc:ElectronicMail>");
				handler.AppendFormatted(SecurityElement.Escape(text3));
				handler.AppendLiteral("</cbc:ElectronicMail>");
				stringBuilder18.AppendLine(ref handler);
			}
			stringBuilder.AppendLine("            </cac:Contact>");
		}
		stringBuilder.AppendLine("        </cac:Party>");
		stringBuilder.AppendLine("    </cac:DespatchSupplierParty>");
		string vknTcknVal = (!string.IsNullOrWhiteSpace(dispatch.TaxNo) && dispatch.TaxNo.All(char.IsDigit)) ? dispatch.TaxNo : "1111111111";
		string vknTcknScheme = (vknTcknVal.Length == 11) ? "TCKN" : "VKN";
		string streetAddr = !string.IsNullOrWhiteSpace(dispatch.Address1)
			? (dispatch.Address1 + (!string.IsNullOrWhiteSpace(dispatch.Address2) ? (" " + dispatch.Address2) : ""))
			: (!string.IsNullOrWhiteSpace(dispatch.FullAddress) && dispatch.FullAddress != "Müşteri Adresi Belirtilmedi" ? dispatch.FullAddress : (dispatch.FirmName + " Adresi"));
		string custCity = !string.IsNullOrWhiteSpace(dispatch.City) ? dispatch.City : "İSTANBUL";
		string custTown = !string.IsNullOrWhiteSpace(dispatch.Town) ? dispatch.Town : "";

		void AppendCustomerParty(string partyTagName)
		{
			stringBuilder.AppendLine($"    <{partyTagName}>");
			stringBuilder.AppendLine("        <cac:Party>");
			stringBuilder.AppendLine("            <cac:PartyIdentification>");
			stringBuilder.AppendLine($"                <cbc:ID schemeID=\"{vknTcknScheme}\">{SecurityElement.Escape(vknTcknVal)}</cbc:ID>");
			stringBuilder.AppendLine("            </cac:PartyIdentification>");
			stringBuilder.AppendLine("            <cac:PartyName>");
			stringBuilder.AppendLine($"                <cbc:Name>{SecurityElement.Escape(dispatch.FirmName)}</cbc:Name>");
			stringBuilder.AppendLine("            </cac:PartyName>");
			stringBuilder.AppendLine("            <cac:PostalAddress>");
			stringBuilder.AppendLine($"                <cbc:StreetName>{SecurityElement.Escape(streetAddr)}</cbc:StreetName>");
			if (!string.IsNullOrWhiteSpace(custTown))
			{
				stringBuilder.AppendLine($"                <cbc:CitySubdivisionName>{SecurityElement.Escape(custTown)}</cbc:CitySubdivisionName>");
			}
			stringBuilder.AppendLine($"                <cbc:CityName>{SecurityElement.Escape(custCity)}</cbc:CityName>");
			stringBuilder.AppendLine("                <cac:Country><cbc:Name>Türkiye</cbc:Name></cac:Country>");
			stringBuilder.AppendLine("            </cac:PostalAddress>");
			if (!string.IsNullOrWhiteSpace(dispatch.TaxOffice))
			{
				stringBuilder.AppendLine("            <cac:PartyTaxScheme>");
				stringBuilder.AppendLine($"                <cac:TaxScheme><cbc:Name>{SecurityElement.Escape(dispatch.TaxOffice)}</cbc:Name></cac:TaxScheme>");
				stringBuilder.AppendLine("            </cac:PartyTaxScheme>");
			}
			if (!string.IsNullOrWhiteSpace(dispatch.Phone) || !string.IsNullOrWhiteSpace(dispatch.Email))
			{
				stringBuilder.AppendLine("            <cac:Contact>");
				if (!string.IsNullOrWhiteSpace(dispatch.Phone))
				{
					stringBuilder.AppendLine($"                <cbc:Telephone>{SecurityElement.Escape(dispatch.Phone)}</cbc:Telephone>");
				}
				if (!string.IsNullOrWhiteSpace(dispatch.Email))
				{
					stringBuilder.AppendLine($"                <cbc:ElectronicMail>{SecurityElement.Escape(dispatch.Email)}</cbc:ElectronicMail>");
				}
				stringBuilder.AppendLine("            </cac:Contact>");
			}
			stringBuilder.AppendLine("        </cac:Party>");
			stringBuilder.AppendLine($"    </{partyTagName}>");
		}

		AppendCustomerParty("cac:DeliveryCustomerParty");
		AppendCustomerParty("cac:BuyerCustomerParty");

		stringBuilder.AppendLine("    <cac:Shipment>");
		stringBuilder.AppendLine($"        <cbc:ID>{SecurityElement.Escape(dispatch.FicheNo)}</cbc:ID>");
		stringBuilder.AppendLine("        <cac:GoodsItem>");
		stringBuilder.AppendLine($"            <cbc:ValueAmount currencyID=\"TRY\">{dispatch.NetTotal.ToString("0.00", invariantCulture)}</cbc:ValueAmount>");
		stringBuilder.AppendLine("        </cac:GoodsItem>");
		stringBuilder.AppendLine("        <cac:ShipmentStage>");
		string str6 = (string.IsNullOrWhiteSpace(dispatch.LicensePlate) ? "--" : dispatch.LicensePlate.Trim());
		stringBuilder.AppendLine("            <cac:TransportMeans>");
		stringBuilder.AppendLine("                <cac:RoadTransport>");
		stringBuilder.AppendLine($"                    <cbc:LicensePlateID>{SecurityElement.Escape(str6)}</cbc:LicensePlateID>");
		stringBuilder.AppendLine("                </cac:RoadTransport>");
		stringBuilder.AppendLine("            </cac:TransportMeans>");
		string str7 = (string.IsNullOrWhiteSpace(dispatch.DriverName) ? "Belirtilmedi" : dispatch.DriverName.Trim());
		string text6 = (string.IsNullOrWhiteSpace(dispatch.DriverSurname) ? "" : dispatch.DriverSurname.Trim());
		stringBuilder.AppendLine("            <cac:DriverPerson>");
		stringBuilder.AppendLine($"                <cbc:FirstName>{SecurityElement.Escape(str7)}</cbc:FirstName>");
		if (!string.IsNullOrWhiteSpace(text6))
		{
			stringBuilder.AppendLine($"                <cbc:FamilyName>{SecurityElement.Escape(text6)}</cbc:FamilyName>");
		}
		if (!string.IsNullOrWhiteSpace(dispatch.DriverTckNo))
		{
			stringBuilder.AppendLine($"                <cbc:NationalityID>{SecurityElement.Escape(dispatch.DriverTckNo)}</cbc:NationalityID>");
			stringBuilder.AppendLine("                <cac:IdentityDocumentReference>");
			stringBuilder.AppendLine($"                    <cbc:ID schemeID=\"TCKN\">{SecurityElement.Escape(dispatch.DriverTckNo)}</cbc:ID>");
			stringBuilder.AppendLine("                </cac:IdentityDocumentReference>");
		}
		stringBuilder.AppendLine("            </cac:DriverPerson>");
		stringBuilder.AppendLine("        </cac:ShipmentStage>");
		stringBuilder.AppendLine("        <cac:Delivery>");
		stringBuilder.AppendLine("            <cac:DeliveryAddress>");
		stringBuilder.AppendLine($"                <cbc:StreetName>{SecurityElement.Escape(streetAddr)}</cbc:StreetName>");
		if (!string.IsNullOrWhiteSpace(custTown))
		{
			stringBuilder.AppendLine($"                <cbc:CitySubdivisionName>{SecurityElement.Escape(custTown)}</cbc:CitySubdivisionName>");
		}
		stringBuilder.AppendLine($"                <cbc:CityName>{SecurityElement.Escape(custCity)}</cbc:CityName>");
		stringBuilder.AppendLine("                <cac:Country><cbc:Name>Türkiye</cbc:Name></cac:Country>");
		stringBuilder.AppendLine("            </cac:DeliveryAddress>");
		stringBuilder.AppendLine("            <cac:Despatch>");
		stringBuilder.AppendLine($"                <cbc:ActualDespatchDate>{value}</cbc:ActualDespatchDate>");
		stringBuilder.AppendLine($"                <cbc:ActualDespatchTime>{value2}</cbc:ActualDespatchTime>");
		stringBuilder.AppendLine("            </cac:Despatch>");
		stringBuilder.AppendLine("        </cac:Delivery>");
		stringBuilder.AppendLine("    </cac:Shipment>");
		for (int i = 0; i < dispatch.Lines.Count; i++)
		{
			LogoErpDispatchLineDto logoErpDispatchLineDto = dispatch.Lines[i];
			int value3 = i + 1;
			stringBuilder.AppendLine("    <cac:DespatchLine>");
			stringBuilder.AppendLine($"        <cbc:ID>{value3}</cbc:ID>");
			stringBuilder.AppendLine($"        <cbc:DeliveredQuantity unitCode=\"NIU\">{logoErpDispatchLineDto.Amount.ToString("0.00", invariantCulture)}</cbc:DeliveredQuantity>");
			stringBuilder.AppendLine("        <cac:OrderLineReference>");
			stringBuilder.AppendLine($"            <cbc:LineID>{value3}</cbc:LineID>");
			stringBuilder.AppendLine("        </cac:OrderLineReference>");

			string itemCodeVal = !string.IsNullOrWhiteSpace(logoErpDispatchLineDto.ItemCode) ? logoErpDispatchLineDto.ItemCode : logoErpDispatchLineDto.ItemName;
			string itemNameVal = !string.IsNullOrWhiteSpace(logoErpDispatchLineDto.ItemName) ? logoErpDispatchLineDto.ItemName : logoErpDispatchLineDto.ItemCode;

			stringBuilder.AppendLine("        <cac:Item>");
			stringBuilder.AppendLine($"            <cbc:Name>{SecurityElement.Escape(itemCodeVal)}</cbc:Name>");
			stringBuilder.AppendLine($"            <cbc:Description>{SecurityElement.Escape(itemNameVal)}</cbc:Description>");
			stringBuilder.AppendLine("            <cac:SellersItemIdentification>");
			stringBuilder.AppendLine($"                <cbc:ID>{SecurityElement.Escape(itemCodeVal)}</cbc:ID>");
			stringBuilder.AppendLine("            </cac:SellersItemIdentification>");
			stringBuilder.AppendLine("        </cac:Item>");
			stringBuilder.AppendLine("        <cac:Shipment>");
			stringBuilder.AppendLine("            <cac:GoodsItem>");
			stringBuilder.AppendLine("                <cac:InvoiceLine>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder34 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(88, 1, stringBuilder2);
			handler.AppendLiteral("                    <cbc:LineExtensionAmount currencyID=\"TRY\">");
			handler.AppendFormatted(logoErpDispatchLineDto.LineTotal.ToString("0.00", invariantCulture));
			handler.AppendLiteral("</cbc:LineExtensionAmount>");
			stringBuilder34.AppendLine(ref handler);
			stringBuilder.AppendLine("                    <cac:Price>");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder35 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(76, 1, stringBuilder2);
			handler.AppendLiteral("                        <cbc:PriceAmount currencyID=\"TRY\">");
			handler.AppendFormatted(logoErpDispatchLineDto.UnitPrice.ToString("0.00", invariantCulture));
			handler.AppendLiteral("</cbc:PriceAmount>");
			stringBuilder35.AppendLine(ref handler);
			stringBuilder.AppendLine("                    </cac:Price>");
			stringBuilder.AppendLine("                </cac:InvoiceLine>");
			stringBuilder.AppendLine("            </cac:GoodsItem>");
			stringBuilder.AppendLine("        </cac:Shipment>");
			stringBuilder.AppendLine("    </cac:DespatchLine>");
		}
		stringBuilder.AppendLine("</n1:DespatchAdvice>");
		return stringBuilder.ToString();
	}

	public async Task<byte[]> RenderDispatchXsltBytesAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		string s = await RenderDispatchXsltHtmlAsync(dispatch, customXsltPath, ct);
		return Encoding.UTF8.GetBytes(s);
	}

	public async Task<byte[]> RenderDispatchPdfAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken))
	{
		if (dispatch == null)
		{
			throw new ArgumentNullException("dispatch");
		}
		_logger.LogInformation("GİB Standartlarına Uygun e-İrsaliye PDF belgesi (XSLT Tasarımı) oluşturuluyor. Fiş No: {FicheNo}", dispatch.FicheNo);
		string htmlString = await RenderDispatchXsltHtmlAsync(dispatch, customXsltPath, ct);
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
}
