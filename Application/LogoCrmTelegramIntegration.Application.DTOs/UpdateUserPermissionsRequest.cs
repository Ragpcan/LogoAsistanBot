namespace LogoCrmTelegramIntegration.Application.DTOs;

public class UpdateUserPermissionsRequest
{
	public long ChatId { get; set; }

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
