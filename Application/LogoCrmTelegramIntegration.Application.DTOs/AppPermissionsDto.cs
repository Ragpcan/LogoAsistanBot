namespace LogoCrmTelegramIntegration.Application.DTOs;

public class AppPermissionsDto
{
	public bool EnableCariModule { get; set; } = true;

	public bool EnableSalesInvoice { get; set; } = true;

	public bool EnableSalesDispatch { get; set; } = true;

	public bool EnableSalesOrder { get; set; } = true;

	public bool EnablePurchaseInvoice { get; set; } = true;

	public bool EnablePurchaseDispatch { get; set; } = true;

	public bool EnablePurchaseOrder { get; set; } = true;

	public bool EnableStockModule { get; set; } = true;

	public bool EnableEBelgeSending { get; set; } = true;

	public bool EnableCollectionModule { get; set; } = true;

	public bool EnableSalesmanTargetReport { get; set; } = true;

	public bool EnableSmsNotification { get; set; } = true;

	public bool EnableEmailNotification { get; set; } = true;
}
