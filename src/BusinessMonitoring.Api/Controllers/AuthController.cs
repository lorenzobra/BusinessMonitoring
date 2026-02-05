using BusinessMonitoring.Api.Models;
using BusinessMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusinessMonitoring.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <remarks>
    /// For demo purposes, valid credentials are:
    /// - Username: admin, Password: demo123
    /// - Username: operator, Password: demo123
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestModel request)
    {
        // Demo authentication - in production, validate against database
        if (!IsValidUser(request.Username, request.Password))
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            return Unauthorized(new ErrorResponseModel
            {
                Error = "Invalid credentials",
                Details = "Username or password is incorrect"
            });
        }

        var token = _jwtTokenService.GenerateToken(request.Username);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Ok(new LoginResponseModel
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = request.Username
        });
    }

    private bool IsValidUser(string username, string password)
    {
        // Demo credentials - in production, check against database with hashed passwords
        var validUsers = new Dictionary<string, string>
        {
            { "admin", "admin" }
        };

        return validUsers.TryGetValue(username, out var validPassword) && validPassword == password;
    }
}

