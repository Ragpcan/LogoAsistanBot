using System;
using System.Collections.Generic;
using System.IO;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class CariStatementPdfGenerator : ICariStatementPdfGenerator
{
	static CariStatementPdfGenerator()
	{
		Settings.License = LicenseType.Community;
	}

	public byte[] GenerateStatementPdf(LogoErpFirmStatementDto statement)
	{
		Document document = Document.Create(delegate(IDocumentContainer container)
		{
			container.Page(delegate(PageDescriptor page)
			{
				page.Size(PageSizes.A4);
				page.Margin(20f);
				page.PageColor(Colors.White);
				page.DefaultTextStyle((TextStyle x) => x.FontSize(9f).FontFamily("Arial"));

				// ── BAŞLIK ──────────────────────────────────────────────────
				page.Header().Column(delegate(ColumnDescriptor hdr)
				{
					hdr.Item().Row(delegate(RowDescriptor row)
					{
						row.RelativeItem().Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text("Cari Hesap Ekstresi")
								.FontSize(14f).Bold().FontColor(Colors.Black);
						});
						row.ConstantItem(180f).Column(delegate(ColumnDescriptor col)
						{
							col.Item().AlignRight().Text("Sayfa : ").FontSize(8f).FontColor(Colors.Grey.Darken2);
						});
					});

					hdr.Item().Height(4f);

					hdr.Item().Row(delegate(RowDescriptor row)
					{
						row.RelativeItem().Column(delegate(ColumnDescriptor col)
						{
							col.Item().Text($"{DateTime.Now:dd.MM.yyyy}")
								.FontSize(9f).FontColor(Colors.Grey.Darken2);
							col.Item().Text(statement.FirmName)
								.FontSize(9f).FontColor(Colors.Grey.Darken2);
						});
						row.ConstantItem(180f).Column(delegate(ColumnDescriptor col)
						{
							if (statement.FilterStartDate.HasValue || statement.FilterEndDate.HasValue)
							{
								string baslangic = statement.FilterStartDate?.ToString("dd.MM.yyyy") ?? "Baslangic";
								string bitis = statement.FilterEndDate?.ToString("dd.MM.yyyy") ?? "Bugun";
								col.Item().AlignRight().Text("Tarih Araligi :")
									.FontSize(8f).FontColor(Colors.Grey.Darken2);
								col.Item().AlignRight().Text($"{baslangic} - {bitis}")
									.FontSize(8f).FontColor(Colors.Grey.Darken2);
							}
						});
					});

					hdr.Item().Height(6f);

					// Cari bilgi seridi
					hdr.Item().Background(Colors.Grey.Lighten4).Padding(6f).Row(delegate(RowDescriptor row)
					{
						row.RelativeItem().Text($"Cari Kodu : {statement.FirmCode}  |  Unvan : {statement.FirmName}")
							.FontSize(9f).Bold();
					});

					hdr.Item().Height(4f);

					// Ozet kutular
					hdr.Item().Row(delegate(RowDescriptor row)
					{
						row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
							.Padding(5f).Column(delegate(ColumnDescriptor c)
							{
								c.Item().Text("Toplam Borc (TL)").FontSize(8f).FontColor(Colors.Grey.Darken1);
								c.Item().Text($"{statement.TotalDebit:N2} TL").FontSize(11f)
									.Bold().FontColor(Colors.Red.Medium);
							});
						row.ConstantItem(6f);
						row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
							.Padding(5f).Column(delegate(ColumnDescriptor c)
							{
								c.Item().Text("Toplam Alacak (TL)").FontSize(8f).FontColor(Colors.Grey.Darken1);
								c.Item().Text($"{statement.TotalCredit:N2} TL").FontSize(11f)
									.Bold().FontColor(Colors.Green.Medium);
							});
						row.ConstantItem(6f);
						Color bakiyeOzetRenk = (statement.Balance >= 0m) ? Colors.Red.Darken1 : Colors.Green.Darken1;
						row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
							.Padding(5f).Column(delegate(ColumnDescriptor c)
							{
								c.Item().Text("Net Bakiye (TL)").FontSize(8f).FontColor(Colors.Grey.Darken1);
								c.Item().Text($"{statement.Balance:N2} TL").FontSize(11f)
									.Bold().FontColor(bakiyeOzetRenk);
							});
					});
					hdr.Item().Height(8f);
				});

				// ── İCERİK: HAREKET TABLOSU ────────────────────────────────
				page.Content().Column(delegate(ColumnDescriptor col)
				{
					col.Item().Text("Cari Hesap Hareketleri")
						.FontSize(10f).Bold().FontColor(Colors.Grey.Darken3);
					col.Item().Height(4f);

					col.Item().Table(delegate(TableDescriptor table)
					{
						// Kolon genislikleri: Tarih | Fis No | Fis Turu | Aciklama | Borc | Alacak | Bakiye
						table.ColumnsDefinition(delegate(TableColumnsDefinitionDescriptor cols)
						{
							cols.ConstantColumn(60f);   // Tarih
							cols.ConstantColumn(75f);   // Fis No
							cols.ConstantColumn(100f);  // Fis Turu
							cols.RelativeColumn();       // Aciklama
							cols.ConstantColumn(70f);   // Borc
							cols.ConstantColumn(70f);   // Alacak
							cols.ConstantColumn(80f);   // Bakiye
						});

						// Baslik satiri
						table.Header(delegate(TableCellDescriptor header)
						{
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.Text("Tarih").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.Text("Fis No.").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.Text("Fis Turu").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.Text("Aciklama").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.AlignRight().Text("Borc").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.AlignRight().Text("Alacak").FontSize(8f).Bold().FontColor(Colors.White);
							header.Cell().Background(Colors.Grey.Darken3).Padding(4f)
								.AlignRight().Text("Bakiye").FontSize(8f).Bold().FontColor(Colors.White);
						});

						// Veri satirlari
						int satirNo = 0;
						foreach (LogoErpStatementLineDto t in statement.RecentTransactions)
						{
							Color satirRenk = (satirNo % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;

							// Bakiye (A)=Alacak (B)=Borc - Logo'nun standart gosterimi
							string bakiyeLabel = t.RunningBalance >= 0m ? "(B)" : "(A)";
							string bakiyeMetin = $"{Math.Abs(t.RunningBalance):N2} {bakiyeLabel}";
							Color bakiyeRengi = t.RunningBalance >= 0m ? Colors.Red.Darken1 : Colors.Green.Darken1;

							table.Cell().Background(satirRenk).Padding(4f)
								.Text($"{t.Date:dd.MM.yyyy}").FontSize(8f);
							table.Cell().Background(satirRenk).Padding(4f)
								.Text(t.TranNo).FontSize(8f);
							table.Cell().Background(satirRenk).Padding(4f)
								.Text(t.TranType).FontSize(8f);
							table.Cell().Background(satirRenk).Padding(4f)
								.Text(t.LineExp).FontSize(8f);
							table.Cell().Background(satirRenk).Padding(4f).AlignRight()
								.Text(t.Debit > 0m ? $"{t.Debit:N2}" : "-").FontSize(8f)
								.FontColor(t.Debit > 0m ? Colors.Red.Medium : Colors.Grey.Darken1);
							table.Cell().Background(satirRenk).Padding(4f).AlignRight()
								.Text(t.Credit > 0m ? $"{t.Credit:N2}" : "-").FontSize(8f)
								.FontColor(t.Credit > 0m ? Colors.Green.Darken1 : Colors.Grey.Darken1);
							table.Cell().Background(satirRenk).Padding(4f).AlignRight()
								.Text(bakiyeMetin).FontSize(8f).FontColor(bakiyeRengi);

							satirNo++;
						}
					});

					// Toplam satiri
					col.Item().Height(4f);
					col.Item().Table(delegate(TableDescriptor table)
					{
						table.ColumnsDefinition(delegate(TableColumnsDefinitionDescriptor columns)
						{
							columns.RelativeColumn(3f);
							columns.RelativeColumn(1f);
							columns.RelativeColumn(1f);
							columns.RelativeColumn(1f);
						});
						table.Cell().Background(Colors.Grey.Lighten4).Padding(5f)
							.Text("Genel Toplam / Bakiye").FontSize(9f).Bold();
						table.Cell().Background(Colors.Grey.Lighten4).Padding(5f)
							.AlignRight().Text($"{statement.TotalDebit:N2} TL").FontSize(8f)
							.Bold().FontColor(Colors.Red.Medium);
						table.Cell().Background(Colors.Grey.Lighten4).Padding(5f)
							.AlignRight().Text($"{statement.TotalCredit:N2} TL").FontSize(8f)
							.Bold().FontColor(Colors.Green.Darken1);
						table.Cell().Background(Colors.Grey.Lighten4).Padding(5f)
							.AlignRight().Text($"{Math.Abs(statement.Balance):N2} {(statement.Balance >= 0m ? "(B)" : "(A)")}").FontSize(8f)
							.Bold().FontColor(statement.Balance >= 0m ? Colors.Red.Darken1 : Colors.Green.Darken1);
					});

					// Doviz ozeti (varsa)
					if (statement.CurrencySummaries.Count > 0)
					{
						col.Item().Height(10f);
						col.Item().Text("Doviz Bazli Bakiye Ozeti")
							.FontSize(9f).Bold().FontColor(Colors.Blue.Darken2);
						col.Item().Height(4f);
						col.Item().Row(delegate(RowDescriptor r)
						{
							foreach (CurrencySummaryDto cs in statement.CurrencySummaries)
							{
								r.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten3)
									.Padding(5f).Column(delegate(ColumnDescriptor c)
									{
										c.Item().Text($"{cs.CurrencySymbol} Bakiyesi").FontSize(8f)
											.FontColor(Colors.Grey.Darken2);
										c.Item().Text($"{cs.Balance:N2} {cs.CurrencySymbol}").FontSize(10f).Bold();
									});
								r.ConstantItem(5f);
							}
						});
					}
				});

				// ── FOOTER ────────────────────────────────────────────────
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
