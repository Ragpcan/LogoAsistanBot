using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IWebUserStore
{
	LoginResponse ValidateUser(string username, string password);

	List<WebUserDto> GetAllUsers();

	WebUserDto? GetUser(string username);

	bool SaveOrUpdateUser(SaveWebUserRequest req, out string errorMessage);

	bool DeleteUser(string username, out string errorMessage);
}
