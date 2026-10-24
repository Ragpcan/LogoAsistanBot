using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface ITelegramUserStore
{
	void RegisterOrUpdateUser(long chatId, string username, string fullName);

	List<TelegramUserMappingDto> GetAllUsers();

	TelegramUserMappingDto? GetUser(long chatId);

	void MapUserToSalesman(long chatId, int? salesmanRef, string? salesmanCode, string? salesmanName);

	void UpdateUserStatus(long chatId, string status);

	bool UpdateUserPermissions(UpdateUserPermissionsRequest request);
}
