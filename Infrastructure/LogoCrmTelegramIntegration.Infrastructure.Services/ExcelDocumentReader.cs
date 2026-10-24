using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public static class ExcelDocumentReader
{
	public static List<List<string>> ReadXlsxRows(Stream stream)
	{
		List<List<string>> list = new List<List<string>>();
		try
		{
			using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
			List<string> list2 = new List<string>();
			ZipArchiveEntry entry = zipArchive.GetEntry("xl/sharedStrings.xml");
			if (entry != null)
			{
				using Stream stream2 = entry.Open();
				XDocument xDocument = XDocument.Load(stream2);
				XNamespace xNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
				foreach (XElement item2 in xDocument.Descendants(xNamespace + "si"))
				{
					string item = string.Concat(from t in item2.Descendants(xNamespace + "t")
						select t.Value);
					list2.Add(item);
				}
			}
			ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry("xl/worksheets/sheet1.xml") ?? zipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => e.FullName.StartsWith("xl/worksheets/sheet"));
			if (zipArchiveEntry == null)
			{
				return list;
			}
			using Stream stream3 = zipArchiveEntry.Open();
			XDocument xDocument2 = XDocument.Load(stream3);
			XNamespace xNamespace2 = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
			foreach (XElement item3 in xDocument2.Descendants(xNamespace2 + "row"))
			{
				List<string> list3 = new List<string>();
				int num = 0;
				foreach (XElement item4 in item3.Descendants(xNamespace2 + "c"))
				{
					string cellRef = ((string?)item4.Attribute("r")) ?? "";
					string text = ((string?)item4.Attribute("t")) ?? "";
					string text2 = item4.Element(xNamespace2 + "v")?.Value ?? "";
					int columnIndex;
					for (columnIndex = GetColumnIndex(cellRef); num + 1 < columnIndex; num++)
					{
						list3.Add("");
					}
					string text3 = text2;
					if (text == "s" && int.TryParse(text2, out var result) && result >= 0 && result < list2.Count)
					{
						text3 = list2[result];
					}
					else if (text == "inlineStr")
					{
						text3 = string.Concat(from t in item4.Descendants(xNamespace2 + "t")
							select t.Value);
					}
					list3.Add(text3.Trim());
					num = columnIndex;
				}
				if (list3.Any((string c) => !string.IsNullOrWhiteSpace(c)))
				{
					list.Add(list3);
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static int GetColumnIndex(string cellRef)
	{
		if (string.IsNullOrWhiteSpace(cellRef))
		{
			return 1;
		}
		string text = new string(cellRef.TakeWhile(char.IsLetter).ToArray()).ToUpperInvariant();
		int num = 0;
		string text2 = text;
		foreach (char c in text2)
		{
			num = num * 26 + (c - 65 + 1);
		}
		if (num <= 0)
		{
			return 1;
		}
		return num;
	}
}
