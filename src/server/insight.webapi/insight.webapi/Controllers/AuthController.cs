using Insight.Services.Interfaces.Core;
using Insight.WebApi.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Insight.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account.
    /// Validates email, password strength, checks uniqueness.
    /// Sends verification email. User cannot log in until email is verified.
    /// </summary>
    /// <param name="request">Registration request with email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration response with success status and user ID</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new RegistrationResponse
            {
                Success = false,
                Message = "Request body is required",
                ErrorCode = "INVALID_REQUEST"
            });

        // Map from API model to service model
        var serviceRequest = new UserRegistrationRequest
        {
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword
        };

        var result = await _userService.RegisterAsync(serviceRequest, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("User registration successful: {UserId}", result.UserId);
            return CreatedAtAction(nameof(Register), new RegistrationResponse
            {
                Success = result.Success,
                UserId = result.UserId,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ValidationErrors = result.ValidationErrors,
                VerificationToken = result.VerificationToken
            });
        }

        // Return appropriate HTTP status based on error code
        return result.ErrorCode switch
        {
            "DUPLICATE_EMAIL" => Conflict(new RegistrationResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ValidationErrors = result.ValidationErrors
            }),
            "WEAK_PASSWORD" or "VALIDATION_ERROR" => BadRequest(new RegistrationResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ValidationErrors = result.ValidationErrors
            }),
            _ => BadRequest(new RegistrationResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ValidationErrors = result.ValidationErrors
            })
        };
    }

    /// <summary>
    /// Verify user email using verification token.
    /// User must call this endpoint with token from verification email.
    /// After verification, user can log in.
    /// </summary>
    /// <param name="request">Verification request with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification response with success status</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyEmailResponse>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrEmpty(request.Token))
            return BadRequest(new VerifyEmailResponse
            {
                Success = false,
                Message = "Verification token is required",
                ErrorCode = "INVALID_REQUEST"
            });

        var result = await _userService.VerifyEmailAsync(request.Token, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Email verification successful");
            return Ok(new VerifyEmailResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode
            });
        }

        return BadRequest(new VerifyEmailResponse
        {
            Success = result.Success,
            Message = result.Message,
            ErrorCode = result.ErrorCode
        });
    }

    /// <summary>
    /// Sign in user with email and password.
    /// Returns JWT access and refresh tokens on successful authentication.
    /// Enforces rate limiting and account lockout after failed attempts.
    /// </summary>
    /// <param name="request">Sign-in request with email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sign-in response with tokens on success, or error message on failure</returns>
    [HttpPost("sign-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SignInResponse>> SignIn(
        [FromBody] SignInRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new SignInResponse
            {
                Success = false,
                Message = "Request body is required",
                ErrorCode = "INVALID_REQUEST"
            });

        // Map from API model to service model
        var serviceRequest = new UserSignInRequest
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        var result = await _userService.SignInAsync(serviceRequest, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("User signed in successfully: {UserId}", result.UserId);
            return Ok(new SignInResponse
            {
                Success = result.Success,
                UserId = result.UserId,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                Message = result.Message
            });
        }

        // Return appropriate HTTP status based on error code
        return result.ErrorCode switch
        {
            "ACCOUNT_LOCKED" => StatusCode(StatusCodes.Status429TooManyRequests, new SignInResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode
            }),
            "INVALID_CREDENTIALS" => Unauthorized(new SignInResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode
            }),
            _ => BadRequest(new SignInResponse
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                ValidationErrors = result.ValidationErrors
            })
        };
    }

    /// <summary>
    /// Health check endpoint for API.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy" });
    }
}
