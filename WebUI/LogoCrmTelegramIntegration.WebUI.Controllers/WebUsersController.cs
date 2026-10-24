using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/webusers")]
public class WebUsersController : ControllerBase
{
	private readonly IWebUserStore _webUserStore;

	public WebUsersController(IWebUserStore webUserStore)
	{
		_webUserStore = webUserStore;
	}

	[HttpGet]
	public IActionResult GetUsers()
	{
		return Ok(_webUserStore.GetAllUsers());
	}

	[HttpPost]
	public IActionResult SaveUser([FromBody] SaveWebUserRequest request)
	{
		if (request == null)
		{
			return BadRequest(new { isSuccess = false, message = "Geçersiz istek gövdesi." });
		}
		if (!_webUserStore.SaveOrUpdateUser(request, out string errorMessage))
		{
			return BadRequest(new { isSuccess = false, message = errorMessage });
		}
		return Ok(new { isSuccess = true, message = $"Kullanıcı '{request.Username}' başarıyla kaydedildi!" });
	}

	[HttpDelete("{username}")]
	public IActionResult DeleteUser(string username)
	{
		if (!_webUserStore.DeleteUser(username, out string errorMessage))
		{
			return BadRequest(new { isSuccess = false, message = errorMessage });
		}
		return Ok(new { isSuccess = true, message = $"Kullanıcı '{username}' silindi." });
	}
}
