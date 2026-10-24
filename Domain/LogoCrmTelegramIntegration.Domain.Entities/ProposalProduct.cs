using System;

namespace LogoCrmTelegramIntegration.Domain.Entities;

public class ProposalProduct
{
	public Guid Oid { get; set; }

	public Guid RelatedProposal { get; set; }

	public Guid ProductOid { get; set; }

	public decimal Amount { get; set; }

	public int LineType { get; set; }
}
