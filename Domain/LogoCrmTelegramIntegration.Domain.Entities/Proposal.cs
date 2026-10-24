using System;

namespace LogoCrmTelegramIntegration.Domain.Entities;

public class Proposal
{
	public Guid Oid { get; set; }

	public Guid ProposalFirm { get; set; }

	public int ProposalState { get; set; }

	public DateTime ProposalStartDate { get; set; } = DateTime.Now;

	public string ProposalDescription { get; set; } = "Telegram'dan Hızlı Sipariş";
}
