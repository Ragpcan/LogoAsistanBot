using System;
using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpDispatchDto
{
	public int LogicalRef { get; set; }

	public string FicheNo { get; set; } = string.Empty;

	public DateTime Date { get; set; } = DateTime.Now;

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public string SalesmanName { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public string LogoBase64 { get; set; } = string.Empty;

	public decimal GrossTotal { get; set; }

	public decimal TotalDiscount { get; set; }

	public decimal TotalVat { get; set; }

	public decimal NetTotal { get; set; }

	public string CarrierInfo { get; set; } = string.Empty;

	public string CarrierRaw { get; set; } = string.Empty;

	public string DriverName { get; set; } = string.Empty;

	public string DriverSurname { get; set; } = string.Empty;

	public string DriverTckNo { get; set; } = string.Empty;

	public string LicensePlate { get; set; } = string.Empty;

	public string TaxOffice { get; set; } = string.Empty;

	public string TaxNo { get; set; } = string.Empty;

	public string Address1 { get; set; } = string.Empty;

	public string Address2 { get; set; } = string.Empty;

	public string City { get; set; } = string.Empty;

	public string Town { get; set; } = string.Empty;

	public string Phone { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FullAddress
	{
		get
		{
			List<string> list = new List<string>();
			if (!string.IsNullOrWhiteSpace(Address1)) list.Add(Address1.Trim());
			if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Address2.Trim());
			if (!string.IsNullOrWhiteSpace(Town)) list.Add(Town.Trim());
			if (!string.IsNullOrWhiteSpace(City)) list.Add(City.Trim());
			return (list.Count > 0) ? string.Join(" ", list) : "Müşteri Adresi Belirtilmedi";
		}
	}

	public List<LogoErpDispatchLineDto> Lines { get; set; } = new List<LogoErpDispatchLineDto>();
}
