using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class ProposalCreateResult
{
	public bool IsSuccess { get; set; }

	public string Message { get; set; } = string.Empty;

	public Guid? ProposalOid { get; set; }

	public Guid? LineOid { get; set; }

	public byte[]? PdfData { get; set; }

	public string? PdfFileName { get; set; }

	public static ProposalCreateResult Success(Guid proposalOid, Guid lineOid, string message)
	{
		return new ProposalCreateResult
		{
			IsSuccess = true,
			ProposalOid = proposalOid,
			LineOid = lineOid,
			Message = message
		};
	}

	public static ProposalCreateResult PdfSuccess(byte[] pdfData, string fileName, string message)
	{
		return new ProposalCreateResult
		{
			IsSuccess = true,
			PdfData = pdfData,
			PdfFileName = fileName,
			Message = message
		};
	}

	public static ProposalCreateResult Failure(string message)
	{
		return new ProposalCreateResult
		{
			IsSuccess = false,
			Message = message
		};
	}
}
