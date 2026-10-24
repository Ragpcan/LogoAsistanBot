using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IWebUserStore _webUserStore;

	private readonly ILogger<AuthController> _logger;

	public AuthController(IWebUserStore webUserStore, ILogger<AuthController> logger)
	{
		_webUserStore = webUserStore;
		_logger = logger;
	}

	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		if (request == null)
		{
			return BadRequest(new LoginResponse
			{
				IsSuccess = false,
				Message = "Geçersiz istek."
			});
		}
		LoginResponse loginResponse = _webUserStore.ValidateUser(request.Username, request.Password);
		if (loginResponse.IsSuccess)
		{
			_logger.LogInformation("Web kullanıcısı giriş yaptı: {Username} (Rol: {Role})", request.Username, loginResponse.User?.Role);
			return Ok(loginResponse);
		}
		return Unauthorized(loginResponse);
	}

	[HttpGet("users")]
	public IActionResult GetUsers()
	{
		List<WebUserDto> allUsers = _webUserStore.GetAllUsers();
		return Ok(allUsers);
	}

	[HttpPost("users")]
	public IActionResult SaveUser([FromBody] SaveWebUserRequest request)
	{
		if (request == null)
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = "Geçersiz istek gövdesi."
			});
		}
		if (!_webUserStore.SaveOrUpdateUser(request, out string errorMessage))
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = errorMessage
			});
		}
		return Ok(new
		{
			isSuccess = true,
			message = "Kullanıcı '" + request.Username + "' başarıyla kaydedildi!"
		});
	}

	[HttpDelete("users/{username}")]
	public IActionResult DeleteUser(string username)
	{
		if (!_webUserStore.DeleteUser(username, out string errorMessage))
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = errorMessage
			});
		}
		return Ok(new
		{
			isSuccess = true,
			message = "Kullanıcı '" + username + "' silindi."
		});
	}
}
