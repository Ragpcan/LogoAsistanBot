using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IAppPermissionStore
{
	AppPermissionsDto GetPermissions();

	void SavePermissions(AppPermissionsDto permissions);
}
