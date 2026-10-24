using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class ProposalRepository : IProposalRepository
{
	private readonly IConfiguration _configuration;

	private readonly ILogger<ProposalRepository> _logger;

	public ProposalRepository(IConfiguration configuration, ILogger<ProposalRepository> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	private string GetConnectionString()
	{
		string? connectionString = _configuration.GetConnectionString("LogoCrmConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException("Logo CRM SQL veritabanı bağlantı dizesi (Connection String) henüz yapılandırılmamış. Lütfen Web Yönetim Paneli üzerinden ayarları girip kaydediniz.");
		}
		return connectionString;
	}

	private IDbConnection CreateConnection()
	{
		return new SqlConnection(GetConnectionString());
	}

	public async Task<Guid?> GetFirmOidByCodeAsync(string firmCode, CancellationToken ct = default(CancellationToken))
	{
		using IDbConnection connection = CreateConnection();
		CommandDefinition command = new CommandDefinition("SELECT Oid FROM MT_Firm WHERE FirmCode = @FirmCode", new
		{
			FirmCode = firmCode
		}, null, null, null, CommandFlags.Buffered, ct);
		return await connection.QueryFirstOrDefaultAsync<Guid?>(command);
	}

	public async Task<Guid?> GetProductOidByCodeAsync(string productCode, CancellationToken ct = default(CancellationToken))
	{
		using IDbConnection connection = CreateConnection();
		CommandDefinition command = new CommandDefinition("SELECT Oid FROM MT_Product WHERE ProductCode = @ProductCode", new
		{
			ProductCode = productCode
		}, null, null, null, CommandFlags.Buffered, ct);
		return await connection.QueryFirstOrDefaultAsync<Guid?>(command);
	}

	public async Task<(Guid proposalOid, Guid lineOid)> CreateProposalWithLineTransactionAsync(Guid firmOid, Guid productOid, decimal amount, string description = "Telegram'dan Hızlı Sipariş", CancellationToken ct = default(CancellationToken))
	{
		using SqlConnection connection = (SqlConnection)CreateConnection();
		await connection.OpenAsync(ct);
		using SqlTransaction transaction = connection.BeginTransaction();
		try
		{
			Guid newProposalOid = Guid.NewGuid();
			Guid newLineOid = Guid.NewGuid();
			var parameters = new
			{
				Oid = newProposalOid,
				ProposalFirm = firmOid,
				ProposalDescription = description
			};
			CancellationToken cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition("\n                INSERT INTO MT_Proposals (\n                    Oid, \n                    ProposalFirm, \n                    ProposalState, \n                    ProposalStartDate, \n                    ProposalDescription\n                ) VALUES (\n                    @Oid, \n                    @ProposalFirm, \n                    0, \n                    GETDATE(), \n                    @ProposalDescription\n                );", parameters, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			var parameters2 = new
			{
				Oid = newLineOid,
				RelatedProposal = newProposalOid,
				ProductOid = productOid,
				Amount = amount
			};
			cancellationToken = ct;
			await connection.ExecuteAsync(new CommandDefinition("\n                INSERT INTO MT_Proposals_Products (\n                    Oid, \n                    RelatedProposal, \n                    ProductOid, \n                    Amount, \n                    LineType\n                ) VALUES (\n                    @Oid, \n                    @RelatedProposal, \n                    @ProductOid, \n                    @Amount, \n                    0\n                );", parameters2, transaction, null, null, CommandFlags.Buffered, cancellationToken));
			await transaction.CommitAsync(ct);
			_logger.LogInformation("SQL Transaction başarıyla commit edildi. ProposalOid: {ProposalOid}, LineOid: {LineOid}", newProposalOid, newLineOid);
			return (proposalOid: newProposalOid, lineOid: newLineOid);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SQL Transaction sırasında hata oluştu. Rollback yapılıyor.");
			await transaction.RollbackAsync(ct);
			throw;
		}
	}
}
