using System;
using System.IO;
using System.Linq;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class OrderPdfGenerator : IOrderPdfGenerator
{
	static OrderPdfGenerator()
	{
		Settings.License = LicenseType.Community;
	}

	public byte[] GenerateOrderPdf(LogoErpOrderPrintDto order)
	{
		Document document = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4);
				page.Margin(20f);
				page.PageColor(Colors.White);
				page.DefaultTextStyle(x => x.FontSize(9f).FontFamily("Arial"));

				page.Header().Element(header => ComposeHeader(header, order));
				page.Content().Element(content => ComposeContent(content, order));
				page.Footer().Element(ComposeFooter);
			});
		});

		using MemoryStream memoryStream = new MemoryStream();
		document.GeneratePdf(memoryStream);
		return memoryStream.ToArray();
	}

	private void ComposeHeader(IContainer container, LogoErpOrderPrintDto order)
	{
		container.Column(col =>
		{
			// Top Banner
			col.Item().Row(row =>
			{
				row.RelativeItem().Column(c =>
				{
					c.Item().Text("MÜŞTERİ SİPARİŞ FORMU").FontSize(16f).Bold().FontColor(Colors.Blue.Darken3);
					c.Item().Text("LOGO CRM & ERP SİPARİŞ BELGESİ").FontSize(8f).SemiBold().FontColor(Colors.Grey.Darken1);
				});

				row.ConstantItem(180f).Column(c =>
				{
					c.Item().Background(Colors.Blue.Lighten5).Border(1f).BorderColor(Colors.Blue.Lighten3).Padding(5f).Column(ic =>
					{
						ic.Item().Row(r =>
						{
							r.RelativeItem().Text("Fiş No:").FontSize(8.5f).SemiBold().FontColor(Colors.Blue.Darken3);
							r.RelativeItem().AlignRight().Text(order.FicheNo).FontSize(10f).Bold().FontColor(Colors.Blue.Darken3);
						});
						ic.Item().Row(r =>
						{
							r.RelativeItem().Text("Tarih:").FontSize(8f).FontColor(Colors.Grey.Darken2);
							r.RelativeItem().AlignRight().Text($"{order.Date:dd.MM.yyyy HH:mm}").FontSize(8.5f).Bold().FontColor(Colors.Grey.Darken3);
						});
						if (!string.IsNullOrWhiteSpace(order.SalesmanName))
						{
							ic.Item().Row(r =>
							{
								r.RelativeItem().Text("Temsilci:").FontSize(8f).FontColor(Colors.Grey.Darken2);
								r.RelativeItem().AlignRight().Text(order.SalesmanName).FontSize(8f).FontColor(Colors.Grey.Darken3);
							});
						}
					});
				});
			});

			col.Item().Height(8f);

			// Customer Info Box
			col.Item().Background(Colors.Grey.Lighten5).Border(1f).BorderColor(Colors.Grey.Lighten3).Padding(8f).Row(row =>
			{
				row.RelativeItem().Column(c =>
				{
					c.Item().Row(r =>
					{
						r.ConstantItem(55f).Text("Cari Kodu:").FontSize(8f).FontColor(Colors.Grey.Darken1);
						r.RelativeItem().Text(order.FirmCode).FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken2);
					});
					c.Item().Row(r =>
					{
						r.ConstantItem(55f).Text("Cari Ünvanı:").FontSize(8f).FontColor(Colors.Grey.Darken1);
						r.RelativeItem().Text(order.FirmName).FontSize(9f).Bold().FontColor(Colors.Grey.Darken4);
					});
				});

				if (!string.IsNullOrWhiteSpace(order.SalesmanName))
				{
					row.ConstantItem(15f);
					row.RelativeItem().Column(c =>
					{
						c.Item().Row(r =>
						{
							r.ConstantItem(55f).Text("Plasiyer:").FontSize(8f).FontColor(Colors.Grey.Darken1);
							r.RelativeItem().Text(order.SalesmanName).FontSize(8.5f).FontColor(Colors.Grey.Darken3);
						});
					});
				}
			});
		});
	}

	private void ComposeContent(IContainer container, LogoErpOrderPrintDto order)
	{
		container.PaddingVertical(8f).Column(column =>
		{
			column.Item().Element(c => ComposeTable(c, order));

			column.Item().Height(8f);

			column.Item().Row(row =>
			{
				row.RelativeItem();
				row.ConstantItem(240f).Element(c => ComposeTotals(c, order));
			});
		});
	}

	private void ComposeTable(IContainer container, LogoErpOrderPrintDto order)
	{
		container.Table(table =>
		{
			table.ColumnsDefinition(columns =>
			{
				columns.ConstantColumn(16f); // #
				columns.ConstantColumn(60f); // Stok Kodu
				columns.RelativeColumn();    // Malzeme Adı (~140pt)
				columns.ConstantColumn(32f); // Miktar
				columns.ConstantColumn(22f); // Brm
				columns.ConstantColumn(44f); // L.Fiyat
				columns.ConstantColumn(26f); // İsk%
				columns.ConstantColumn(42f); // İsk.Tut
				columns.ConstantColumn(44f); // Net Fiyat
				columns.ConstantColumn(24f); // KDV%
				columns.ConstantColumn(42f); // KDV Tut
				columns.ConstantColumn(48f); // Tutar
			});

			table.Header(header =>
			{
				header.Cell().Element(HeaderStyle).Text("#");
				header.Cell().Element(HeaderStyle).Text("Stok Kodu");
				header.Cell().Element(HeaderStyle).Text("Malzeme Açıklaması");
				header.Cell().Element(HeaderStyle).AlignRight().Text("Miktar");
				header.Cell().Element(HeaderStyle).AlignCenter().Text("Brm");
				header.Cell().Element(HeaderStyle).AlignRight().Text("L.Fiyat");
				header.Cell().Element(HeaderStyle).AlignCenter().Text("İsk%");
				header.Cell().Element(HeaderStyle).AlignRight().Text("İsk.Tut");
				header.Cell().Element(HeaderStyle).AlignRight().Text("Net Fiyat");
				header.Cell().Element(HeaderStyle).AlignCenter().Text("KDV%");
				header.Cell().Element(HeaderStyle).AlignRight().Text("KDV Tut");
				header.Cell().Element(HeaderStyle).AlignRight().Text("Tutar");
			});

			int num = 1;
			foreach (LogoErpOrderPrintLineDto line in order.Lines)
			{
				Color rowBg = (num % 2 == 1) ? Colors.Grey.Lighten5 : Colors.White;
				string discRateStr = "-";
				if (line.DiscountRate > 0m)
				{
					discRateStr = $"%{line.DiscountRate:0.#}";
				}
				else if (line.EffectiveListPrice > line.EffectiveUnitPrice && line.EffectiveListPrice > 0m)
				{
					discRateStr = $"%{100m * (1m - line.EffectiveUnitPrice / line.EffectiveListPrice):0.#}";
				}
				decimal discAmt = line.TotalDiscountAmount;
				decimal vatAmt = line.LineTotal * (decimal)(line.VatRate / 100.0);

				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text($"{num}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).Text(line.ItemCode).FontSize(7.5f).Bold().FontColor(Colors.Blue.Darken3);
				table.Cell().Element(c => RowStyle(c, rowBg)).Text(line.ItemName).FontSize(7.5f).FontColor(Colors.Grey.Darken4);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.Amount:N2}").FontSize(7.5f).Bold();
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text(string.IsNullOrWhiteSpace(line.UnitName) ? "ADET" : line.UnitName).FontSize(7f).FontColor(Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.EffectiveListPrice:N2}").FontSize(7.5f);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text(discRateStr).FontSize(7.5f).FontColor(discRateStr != "-" ? Colors.Red.Medium : Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text(discAmt > 0m ? $"{discAmt:N2}" : "-").FontSize(7.5f).FontColor(discAmt > 0m ? Colors.Red.Medium : Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.EffectiveUnitPrice:N2}").FontSize(7.5f).Bold().FontColor(Colors.Grey.Darken4);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text($"%{line.VatRate:0.#}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{vatAmt:N2}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.LineTotal:N2}").FontSize(7.5f).Bold().FontColor(Colors.Blue.Darken4);

				num++;
			}
		});

		static IContainer HeaderStyle(IContainer element)
		{
			return element.Background(Colors.Blue.Darken3).PaddingVertical(4f).PaddingHorizontal(2f).DefaultTextStyle(x => x.FontSize(7.5f).Bold().FontColor(Colors.White));
		}

		static IContainer RowStyle(IContainer element, Color bg)
		{
			return element.Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(3.5f).PaddingHorizontal(2f);
		}
	}

	private void ComposeTotals(IContainer container, LogoErpOrderPrintDto order)
	{
		container.Background(Colors.Grey.Lighten5).Border(1f).BorderColor(Colors.Grey.Lighten3).Padding(8f).Column(c =>
		{
			if (order.LineDiscounts > 0m || order.GeneralDiscountTotal > 0m)
			{
				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Brüt Toplam:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
					r.ConstantItem(100f).AlignRight().Text($"{order.RawGrossTotal:N2} TL").FontSize(8.5f);
				});

				if (order.LineDiscounts > 0m)
				{
					c.Item().Row(r =>
					{
						r.RelativeItem().Text("Kalem İskontoları:").FontSize(8.5f).FontColor(Colors.Red.Medium);
						r.ConstantItem(100f).AlignRight().Text($"-{order.LineDiscounts:N2} TL").FontSize(8.5f).FontColor(Colors.Red.Medium);
					});
				}

				if (order.GeneralDiscountTotal > 0m)
				{
					string discLabel = order.GeneralDiscountRate > 0m ? $"Genel İskonto (%{order.GeneralDiscountRate:0.#}):" : "Genel İskonto:";
					c.Item().Row(r =>
					{
						r.RelativeItem().Text(discLabel).FontSize(8.5f).FontColor(Colors.Red.Medium);
						r.ConstantItem(100f).AlignRight().Text($"-{order.GeneralDiscountTotal:N2} TL").FontSize(8.5f).FontColor(Colors.Red.Medium);
					});
				}

				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Net Matrah:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
					r.ConstantItem(100f).AlignRight().Text($"{order.SubTotal:N2} TL").FontSize(8.5f);
				});
			}
			else
			{
				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Ara Toplam:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
					r.ConstantItem(100f).AlignRight().Text($"{order.SubTotal:N2} TL").FontSize(8.5f);
				});
			}

			c.Item().Row(r =>
			{
				r.RelativeItem().Text("Toplam KDV:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
				r.ConstantItem(100f).AlignRight().Text($"{order.TotalVat:N2} TL").FontSize(8.5f);
			});

			c.Item().PaddingVertical(2f).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

			c.Item().Row(r =>
			{
				r.RelativeItem().Text("GENEL TOPLAM:").FontSize(11f).Bold().FontColor(Colors.Blue.Darken3);
				r.ConstantItem(110f).AlignRight().Text($"{order.NetTotal:N2} TL").FontSize(11f).Bold().FontColor(Colors.Blue.Darken3);
			});
		});
	}

	private void ComposeFooter(IContainer container)
	{
		container.Row(row =>
		{
			row.RelativeItem().Text("Bu belge Logo Asistan Bot tarafından üretilmiştir.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
			row.ConstantItem(100f).AlignRight().Text(x =>
			{
				x.Span("Sayfa ").FontSize(7.5f);
				x.CurrentPageNumber().FontSize(7.5f);
				x.Span(" / ").FontSize(7.5f);
				x.TotalPages().FontSize(7.5f);
			});
		});
	}
}

