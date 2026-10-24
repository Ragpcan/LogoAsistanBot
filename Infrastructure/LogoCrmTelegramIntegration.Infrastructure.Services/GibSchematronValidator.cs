using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class GibSchematronValidator : IGibSchematronValidator
{
	private readonly IConfiguration _config;

	private readonly ILogger<GibSchematronValidator> _logger;

	private static readonly Dictionary<string, string> SchematronFileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["DESPATCHADVICE"] = "TR_DispatchAdvice_Schematron.xslt",
		["INVOICE"] = "TR_Invoice_Schematron.xslt",
		["EARSIV"] = "TR_EArchiveInvoice_Schematron.xslt"
	};

	public GibSchematronValidator(IConfiguration config, ILogger<GibSchematronValidator> logger)
	{
		_config = config;
		_logger = logger;
	}

	private string ResolveSchematronDir()
	{
		string text = _config["EBelgeWebService:SchematronDirectory"] ?? "Schematron";
		if (Path.IsPathRooted(text))
		{
			return text;
		}
		string text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, text);
		if (Directory.Exists(text2))
		{
			return text2;
		}
		return Path.Combine(Directory.GetCurrentDirectory(), "src", "Infrastructure", text);
	}

	private string? GetSchematronFilePath(string documentType)
	{
		if (!SchematronFileMap.TryGetValue(documentType, out string value))
		{
			return null;
		}
		string text = Path.Combine(ResolveSchematronDir(), value);
		if (!File.Exists(text))
		{
			return null;
		}
		return text;
	}

	public bool IsSchematronAvailable(string documentType)
	{
		return GetSchematronFilePath(documentType) != null;
	}

	public async Task<SchematronResult> ValidateAsync(string ublXml, string documentType, CancellationToken ct = default(CancellationToken))
	{
		string schematronPath = GetSchematronFilePath(documentType);
		if (schematronPath == null)
		{
			_logger.LogWarning("Şematron dosyası bulunamadı. DocType: {DocType}, Dizin: {Dir}", documentType, ResolveSchematronDir());
			return new SchematronResult
			{
				IsValid = true,
				Errors = new List<SchematronValidationError>
				{
					new SchematronValidationError
					{
						Severity = "warning",
						RuleId = "SCHEMATRON_NOT_FOUND",
						Message = documentType + " için Şematron dosyası bulunamadı: " + ResolveSchematronDir() + ". Doğrulama atlandı."
					}
				}
			};
		}
		_logger.LogInformation("GİB Şematron doğrulaması başlatıldı. DocType: {DocType}, Şematron: {Path}", documentType, schematronPath);
		return await Task.Run(delegate
		{
			try
			{
				XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
				XsltSettings settings = new XsltSettings(enableDocumentFunction: true, enableScript: false);
				xslCompiledTransform.Load(schematronPath, settings, new XmlUrlResolver());
				using XmlReader input = XmlReader.Create(new StringReader(ublXml));
				using StringWriter stringWriter = new StringWriter();
				xslCompiledTransform.Transform(input, null, stringWriter);
				return ParseSvrl(stringWriter.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GİB Şematron doğrulaması sırasında hata oluştu.");
				return new SchematronResult
				{
					IsValid = false,
					Errors = new List<SchematronValidationError>
					{
						new SchematronValidationError
						{
							Severity = "error",
							RuleId = "SCHEMATRON_RUNTIME_ERROR",
							Message = "Şematron çalışma zamanı hatası: " + ex.Message
						}
					}
				};
			}
		}, ct);
	}

	private static SchematronResult ParseSvrl(string svrl)
	{
		SchematronResult schematronResult = new SchematronResult
		{
			IsValid = true,
			RawSvrl = svrl
		};
		if (string.IsNullOrWhiteSpace(svrl))
		{
			return schematronResult;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(svrl);
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("svrl", "http://purl.oclc.org/dsdl/svrl");
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//svrl:failed-assert", xmlNamespaceManager);
			if (xmlNodeList != null)
			{
				foreach (XmlNode item in xmlNodeList)
				{
					schematronResult.IsValid = false;
					schematronResult.Errors.Add(new SchematronValidationError
					{
						Severity = "error",
						RuleId = (item.Attributes?["id"]?.Value ?? item.Attributes?["test"]?.Value ?? "?"),
						Location = (item.Attributes?["location"]?.Value ?? string.Empty),
						Message = (item.SelectSingleNode("svrl:text", xmlNamespaceManager)?.InnerText?.Trim() ?? item.InnerText.Trim())
					});
				}
			}
			XmlNodeList xmlNodeList2 = xmlDocument.SelectNodes("//svrl:successful-report", xmlNamespaceManager);
			if (xmlNodeList2 != null)
			{
				foreach (XmlNode item2 in xmlNodeList2)
				{
					schematronResult.Errors.Add(new SchematronValidationError
					{
						Severity = "warning",
						RuleId = (item2.Attributes?["id"]?.Value ?? item2.Attributes?["test"]?.Value ?? "?"),
						Location = (item2.Attributes?["location"]?.Value ?? string.Empty),
						Message = (item2.SelectSingleNode("svrl:text", xmlNamespaceManager)?.InnerText?.Trim() ?? item2.InnerText.Trim())
					});
				}
			}
		}
		catch
		{
			schematronResult.IsValid = false;
			schematronResult.Errors.Add(new SchematronValidationError
			{
				Severity = "error",
				RuleId = "SVRL_PARSE_ERROR",
				Message = "Şematron çıktısı (SVRL) parse edilemedi. Ham çıktı RawSvrl alanında mevcuttur."
			});
		}
		return schematronResult;
	}
}
