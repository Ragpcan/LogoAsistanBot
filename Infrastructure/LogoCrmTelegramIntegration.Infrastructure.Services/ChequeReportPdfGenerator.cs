using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class ChequeReportPdfGenerator : IChequeReportPdfGenerator
{
	static ChequeReportPdfGenerator()
	{
		Settings.License = LicenseType.Community;
	}

	public byte[] GenerateChequeReportPdf(string firmCode, List<LogoErpChequeItemDto> cheques)
	{
		Document document = Document.Create(delegate(IDocumentContainer container)
		{
			container.Page(delegate(PageDescriptor page)
			{
				page.Size(PageSizes.A4);
				page.Margin(20f);
				page.PageColor(Colors.White);
				page.DefaultTextStyle((TextStyle x) => x.FontSize(10f).FontFamily("Arial"));
				page.Header().Element(delegate(IContainer header)
				{
					header.Row(delegate(RowDescriptor row)
					{
						row.RelativeItem().Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text("LOGO CRM & ERP").FontSize(18f)
								.Bold()
								.FontColor(Colors.Blue.Darken2);
							col.Item().Text("MÜŞTERİ ÇEK RAPORU").FontSize(12f)
								.SemiBold()
								.FontColor(Colors.Grey.Darken2);
						});
						row.ConstantItem(150f).Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9f)
								.AlignRight();
							col.Item().Text("Cari Kod: " + firmCode).FontSize(9f)
								.Bold()
								.AlignRight();
						});
					});
				}, "header =>\n                {\n                    header.Row(row =>\n                    {\n                        row.RelativeItem().Column(col =>\n                        {\n                            col.Item().Text(\"LOGO CRM & ERP\").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);\n                            col.Item().Text(\"MÜŞTERİ ÇEK RAPORU\").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);\n                        });\n\n                        row.ConstantItem(150).Column(col =>\n                        {\n                            col.Item().Text($\"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}\").FontSize(9).AlignRight();\n                            col.Item().Text($\"Cari Kod: {firmCode}\").FontSize(9).Bold().AlignRight();\n                        });\n                    });\n                }", "GenerateChequeReportPdf", "C:\\Users\\RAGIP\\Desktop\\AntyGravity\\LogoCrmTelegramIntegration\\src\\Infrastructure\\Services\\ChequeReportPdfGenerator.cs", 31);
				page.Content().PaddingVertical(10f).Column(delegate(ColumnDescriptor col)
				{
					col.Item().Background(Colors.Grey.Lighten4).Padding(10f)
						.Column(delegate(ColumnDescriptor c)
						{
							c.Item().Text("CARI KODU: " + firmCode).FontSize(12f)
								.Bold();
							c.Item().Text($"TOPLAM ÇEK SAYISI: {cheques.Count}").FontSize(10f)
								.FontColor(Colors.Grey.Darken3);
							c.Item().Text($"TOPLAM TUTAR: {cheques.Sum((LogoErpChequeItemDto x) => x.Amount):N2} TL").FontSize(10f)
								.Bold()
								.FontColor(Colors.Red.Medium);
						});
					col.Item().Height(15f);
					col.Item().Text("Açık Çekler Listesi").FontSize(12f)
						.Bold()
						.FontColor(Colors.Grey.Darken3);
					col.Item().Height(5f);
					if (cheques.Count == 0)
					{
						col.Item().Text("Bu cariye ait listelenecek açık çek bulunamadı.").FontSize(10f)
							.Italic();
					}
					else
					{
						col.Item().Table(delegate(TableDescriptor table)
						{
							table.ColumnsDefinition(delegate(TableColumnsDefinitionDescriptor columns)
							{
								columns.ConstantColumn(70f);
								columns.RelativeColumn();
								columns.ConstantColumn(80f);
								columns.ConstantColumn(80f);
								columns.ConstantColumn(90f);
							});
							table.Header(delegate(TableCellDescriptor header)
							{
								header.Cell().Background(Colors.Blue.Darken2).Padding(5f)
									.Text("Portföy No")
									.FontSize(9f)
									.Bold()
									.FontColor(Colors.White);
								header.Cell().Background(Colors.Blue.Darken2).Padding(5f)
									.Text("Asıl Borçlu")
									.FontSize(9f)
									.Bold()
									.FontColor(Colors.White);
								header.Cell().Background(Colors.Blue.Darken2).Padding(5f)
									.Text("Vade")
									.FontSize(9f)
									.Bold()
									.FontColor(Colors.White);
								header.Cell().Background(Colors.Blue.Darken2).Padding(5f)
									.Text("Durum")
									.FontSize(9f)
									.Bold()
									.FontColor(Colors.White);
								header.Cell().Background(Colors.Blue.Darken2).Padding(5f)
									.AlignRight()
									.Text("Tutar")
									.FontSize(9f)
									.Bold()
									.FontColor(Colors.White);
							});
							int num = 0;
							foreach (LogoErpChequeItemDto item in cheques.OrderBy((LogoErpChequeItemDto c) => c.DueDate))
							{
								Color color = ((num % 2 == 0) ? Colors.White : Colors.Grey.Lighten5);
								table.Cell().Background(color).Padding(5f)
									.Text(item.PortfoyNo)
									.FontSize(9f);
								table.Cell().Background(color).Padding(5f)
									.Text(item.Owing)
									.FontSize(9f);
								table.Cell().Background(color).Padding(5f)
									.Text($"{item.DueDate:dd.MM.yyyy}")
									.FontSize(9f);
								table.Cell().Background(color).Padding(5f)
									.Text(item.StatusText)
									.FontSize(9f);
								table.Cell().Background(color).Padding(5f)
									.AlignRight()
									.Text($"{item.Amount:N2} TL")
									.FontSize(9f)
									.FontColor(Colors.Red.Medium);
								num++;
							}
						});
					}
				});
				page.Footer().AlignCenter().Text(delegate(TextDescriptor x)
				{
					x.Span("Sayfa ");
					x.CurrentPageNumber();
					x.Span(" / ");
					x.TotalPages();
				});
			});
		});
		using MemoryStream memoryStream = new MemoryStream();
		document.GeneratePdf(memoryStream);
		return memoryStream.ToArray();
	}
}
