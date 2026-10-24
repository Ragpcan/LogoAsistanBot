namespace LogoCrmTelegramIntegration.Application.Interfaces;

public record LogoErpItemDetails(int ItemLogicalRef, int UnitRef, int UnitSetRef, double Vat, string ItemName = "");
