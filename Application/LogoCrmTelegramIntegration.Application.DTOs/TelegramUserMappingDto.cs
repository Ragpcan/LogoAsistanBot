using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class TelegramUserMappingDto
{
	public long ChatId { get; set; }

	public string Username { get; set; } = string.Empty;

	public string FullName { get; set; } = string.Empty;

	public DateTime RegisteredAt { get; set; } = DateTime.Now;

	public DateTime LastSeen { get; set; } = DateTime.Now;

	public string Status { get; set; } = "Pending";

	public int? MappedSalesmanRef { get; set; }

	public string? MappedSalesmanCode { get; set; }

	public string? MappedSalesmanName { get; set; }

	public bool IsMapped
	{
		get
		{
			if (MappedSalesmanRef.HasValue)
			{
				return MappedSalesmanRef.Value > 0;
			}
			return false;
		}
	}

	public bool IsApproved => string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase);

	public bool IsBlocked => string.Equals(Status, "Blocked", StringComparison.OrdinalIgnoreCase);

	public bool CanViewAllFirms { get; set; } = true;

	public string AllowedFirmCyphCode { get; set; } = string.Empty;

	public bool CanViewAllItems { get; set; } = true;

	public string AllowedItemCyphCode { get; set; } = string.Empty;

	public bool CanViewAllOrders { get; set; } = true;

	public string AllowedOrderCyphCode { get; set; } = string.Empty;

	public bool CanCreateSalesOrder { get; set; } = true;

	public bool CanViewSalesOrders { get; set; } = true;

	public bool CanCreateDispatch { get; set; } = true;

	public bool CanViewSalesDispatches { get; set; } = true;

	public bool CanCreateSalesInvoice { get; set; }

	public bool CanViewSalesInvoices { get; set; } = true;

	public bool CanViewPurchaseOrders { get; set; }

	public bool CanViewPurchaseDispatches { get; set; }

	public bool CanViewPurchaseInvoices { get; set; }

	public bool CanSendEBelge { get; set; }

	public bool CanCreateCollection { get; set; } = true;

	public bool CanViewSalesmanTarget { get; set; } = true;

	public bool CanSendSms { get; set; } = true;

	public bool CanSendEmail { get; set; } = true;
}
