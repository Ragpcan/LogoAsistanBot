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

public class StandardPdfGenerator : IStandardPdfGenerator
{
	static StandardPdfGenerator()
	{
		Settings.License = LicenseType.Community;
	}

	public byte[] GenerateStandardDispatchPdf(LogoErpDispatchDto dispatch, bool isInvoice = false)
	{
		Document document = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4);
				page.Margin(20f);
				page.PageColor(Colors.White);
				page.DefaultTextStyle(x => x.FontSize(9f).FontFamily("Arial"));

				page.Header().Element(header => ComposeHeader(header, dispatch, isInvoice));
				page.Content().Element(content => ComposeContent(content, dispatch, isInvoice));
				page.Footer().Element(ComposeFooter);
			});
		});

		using MemoryStream memoryStream = new MemoryStream();
		document.GeneratePdf(memoryStream);
		return memoryStream.ToArray();
	}

	private void ComposeHeader(IContainer container, LogoErpDispatchDto dispatch, bool isInvoice)
	{
		string title = isInvoice ? "SATIŞ FATURASI" : "SATIŞ İRSALİYESİ";
		string subTitle = isInvoice ? "LOGO ERP RESMİ FATURA DÖKÜM FORMU" : "LOGO ERP RESMİ İRSALİYE SEVK FORMU";

		container.Column(col =>
		{
			// Top Banner
			col.Item().Row(row =>
			{
				row.RelativeItem().Column(c =>
				{
					c.Item().Text(title).FontSize(16f).Bold().FontColor(Colors.Blue.Darken3);
					c.Item().Text(subTitle).FontSize(8f).SemiBold().FontColor(Colors.Grey.Darken1);
				});

				row.ConstantItem(180f).Column(c =>
				{
					c.Item().Background(Colors.Blue.Lighten5).Border(1f).BorderColor(Colors.Blue.Lighten3).Padding(5f).Column(ic =>
					{
						ic.Item().Row(r =>
						{
							r.RelativeItem().Text("Belge No:").FontSize(8.5f).SemiBold().FontColor(Colors.Blue.Darken3);
							r.RelativeItem().AlignRight().Text(dispatch.FicheNo).FontSize(10f).Bold().FontColor(Colors.Blue.Darken3);
						});
						ic.Item().Row(r =>
						{
							r.RelativeItem().Text("Tarih:").FontSize(8f).FontColor(Colors.Grey.Darken2);
							r.RelativeItem().AlignRight().Text($"{dispatch.Date:dd.MM.yyyy}").FontSize(8.5f).Bold().FontColor(Colors.Grey.Darken3);
						});
						if (!string.IsNullOrWhiteSpace(dispatch.SalesmanName))
						{
							ic.Item().Row(r =>
							{
								r.RelativeItem().Text("Plasiyer:").FontSize(8f).FontColor(Colors.Grey.Darken2);
								r.RelativeItem().AlignRight().Text(dispatch.SalesmanName).FontSize(8f).FontColor(Colors.Grey.Darken3);
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
						r.RelativeItem().Text(dispatch.FirmCode).FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken2);
					});
					c.Item().Row(r =>
					{
						r.ConstantItem(55f).Text("Cari Ünvanı:").FontSize(8f).FontColor(Colors.Grey.Darken1);
						r.RelativeItem().Text(dispatch.FirmName).FontSize(9f).Bold().FontColor(Colors.Grey.Darken4);
					});
					if (!string.IsNullOrWhiteSpace(dispatch.TaxOffice) || !string.IsNullOrWhiteSpace(dispatch.TaxNo))
					{
						c.Item().Row(r =>
						{
							r.ConstantItem(55f).Text("Vergi D/No:").FontSize(8f).FontColor(Colors.Grey.Darken1);
							r.RelativeItem().Text($"{dispatch.TaxOffice} / {dispatch.TaxNo}").FontSize(8f).FontColor(Colors.Grey.Darken3);
						});
					}
				});

				row.ConstantItem(15f);

				row.RelativeItem().Column(c =>
				{
					if (!string.IsNullOrWhiteSpace(dispatch.FullAddress))
					{
						c.Item().Row(r =>
						{
							r.ConstantItem(45f).Text("Adres:").FontSize(8f).FontColor(Colors.Grey.Darken1);
							r.RelativeItem().Text(dispatch.FullAddress).FontSize(8f).FontColor(Colors.Grey.Darken3);
						});
					}
					if (!string.IsNullOrWhiteSpace(dispatch.Phone) || !string.IsNullOrWhiteSpace(dispatch.Email))
					{
						c.Item().Row(r =>
						{
							r.ConstantItem(45f).Text("İletişim:").FontSize(8f).FontColor(Colors.Grey.Darken1);
							r.RelativeItem().Text($"{dispatch.Phone} {dispatch.Email}".Trim()).FontSize(8f).FontColor(Colors.Grey.Darken3);
						});
					}
					if (!string.IsNullOrWhiteSpace(dispatch.CarrierInfo) || !string.IsNullOrWhiteSpace(dispatch.DriverName))
					{
						c.Item().Row(r =>
						{
							r.ConstantItem(45f).Text("Taşıyıcı:").FontSize(8f).FontColor(Colors.Grey.Darken1);
							r.RelativeItem().Text($"{dispatch.CarrierInfo} {dispatch.DriverName} {dispatch.DriverSurname}".Trim()).FontSize(8f).Italic().FontColor(Colors.Grey.Darken3);
						});
					}
				});
			});
		});
	}

	private void ComposeContent(IContainer container, LogoErpDispatchDto dispatch, bool isInvoice)
	{
		container.PaddingVertical(8f).Column(column =>
		{
			column.Item().Element(c => ComposeTable(c, dispatch, isInvoice));

			column.Item().Height(8f);

			column.Item().Row(row =>
			{
				// Left Notes
				row.RelativeItem().Column(c =>
				{
					if (!string.IsNullOrWhiteSpace(dispatch.Description))
					{
						c.Item().Background(Colors.Grey.Lighten5).Border(1f).BorderColor(Colors.Grey.Lighten3).Padding(6f).Column(nc =>
						{
							nc.Item().Text("Açıklama / Notlar:").FontSize(8f).Bold().FontColor(Colors.Grey.Darken3);
							nc.Item().Text(dispatch.Description).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
						});
					}
				});

				row.ConstantItem(20f);

				// Right Totals
				row.ConstantItem(240f).Element(c => ComposeTotals(c, dispatch, isInvoice));
			});
		});
	}

	private void ComposeTable(IContainer container, LogoErpDispatchDto dispatch, bool isInvoice)
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

			for (int i = 0; i < dispatch.Lines.Count; i++)
			{
				LogoErpDispatchLineDto line = dispatch.Lines[i];
				decimal grossTotal = line.Amount * line.UnitPrice;
				decimal discAmt = line.DiscountAmount;
				decimal netLine = line.LineTotal > 0m ? line.LineTotal : (grossTotal - discAmt);
				decimal netUnitPrice = line.Amount > 0m ? (netLine / line.Amount) : line.UnitPrice;
				decimal vatAmt = line.VatAmount;
				Color rowBg = (i % 2 == 1) ? Colors.Grey.Lighten5 : Colors.White;

				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text($"{i + 1}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).Text(line.ItemCode).FontSize(7.5f).Bold().FontColor(Colors.Blue.Darken3);
				table.Cell().Element(c => RowStyle(c, rowBg)).Text(line.ItemName).FontSize(7.5f).FontColor(Colors.Grey.Darken4);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.Amount:N2}").FontSize(7.5f).Bold();
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text(string.IsNullOrWhiteSpace(line.UnitName) ? "ADET" : line.UnitName).FontSize(7f).FontColor(Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{line.UnitPrice:N2}").FontSize(7.5f);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text(line.DiscountRate > 0m ? $"%{line.DiscountRate:0.#}" : "-").FontSize(7.5f).FontColor(line.DiscountRate > 0m ? Colors.Red.Medium : Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text(discAmt > 0m ? $"{discAmt:N2}" : "-").FontSize(7.5f).FontColor(discAmt > 0m ? Colors.Red.Medium : Colors.Grey.Darken1);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{netUnitPrice:N2}").FontSize(7.5f).Bold().FontColor(Colors.Grey.Darken4);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignCenter().Text($"%{line.VatRate:0.#}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{vatAmt:N2}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
				table.Cell().Element(c => RowStyle(c, rowBg)).AlignRight().Text($"{netLine:N2}").FontSize(7.5f).Bold().FontColor(Colors.Blue.Darken4);
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

	private void ComposeTotals(IContainer container, LogoErpDispatchDto dispatch, bool isInvoice)
	{
		decimal grossTotal = ((dispatch.GrossTotal > 0m) ? dispatch.GrossTotal : dispatch.Lines.Sum(l => l.Amount * l.UnitPrice));
		decimal totalDiscount = dispatch.TotalDiscount;
		decimal vatTotal = ((dispatch.TotalVat > 0m) ? dispatch.TotalVat : dispatch.Lines.Sum(l => l.VatAmount > 0m ? l.VatAmount : (l.LineTotal * 0.20m)));
		decimal netTotal = ((dispatch.NetTotal > 0m) ? dispatch.NetTotal : (grossTotal - totalDiscount + vatTotal));
		decimal netMatrah = grossTotal - totalDiscount;

		container.Background(Colors.Grey.Lighten5).Border(1f).BorderColor(Colors.Grey.Lighten3).Padding(8f).Column(c =>
		{
			c.Item().Row(r =>
			{
				r.RelativeItem().Text("Brüt Toplam:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
				r.ConstantItem(100f).AlignRight().Text($"{grossTotal:N2} TL").FontSize(8.5f);
			});

			if (totalDiscount > 0m)
			{
				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Toplam İndirim:").FontSize(8.5f).FontColor(Colors.Red.Medium);
					r.ConstantItem(100f).AlignRight().Text($"-{totalDiscount:N2} TL").FontSize(8.5f).FontColor(Colors.Red.Medium);
				});
				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Net Matrah:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
					r.ConstantItem(100f).AlignRight().Text($"{netMatrah:N2} TL").FontSize(8.5f);
				});
			}

			if (isInvoice || vatTotal > 0m)
			{
				c.Item().Row(r =>
				{
					r.RelativeItem().Text("Hesaplanan KDV:").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
					r.ConstantItem(100f).AlignRight().Text($"{vatTotal:N2} TL").FontSize(8.5f);
				});
			}

			c.Item().PaddingVertical(2f).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

			c.Item().Row(r =>
			{
				r.RelativeItem().Text("GENEL TOPLAM:").FontSize(11f).Bold().FontColor(Colors.Blue.Darken3);
				r.ConstantItem(110f).AlignRight().Text($"{netTotal:N2} TL").FontSize(11f).Bold().FontColor(Colors.Blue.Darken3);
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
