using Microsoft.AspNetCore.Mvc;
using ErrorOr;
using BuberDinner.Application.Services.Authentication.Commands;
using BuberDinner.Application.Services.Authentication.Queries;
using BuberDinner.Contracts.Authentication;
using BuberDinner.Domain.Common.Errors;
using BuberDinner.Application.Services.Authentication;
using MediatR;
using BuberDinner.Application.Authentication.Commands.Register;
using BuberDinner.Application.Authentication.Common;
using BuberDinner.Application.Authentication.Queries.Login;

namespace BuberDinner.Api.Controllers;

[Route("auth")]
public class AuthenticationController : ApiController
{
    private readonly ISender _mediator;

    public AuthenticationController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(BuberDinner.Contracts.Authentication.RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        ErrorOr.ErrorOr<BuberDinner.Application.Authentication.Common.AuthenticationResult> authResult = await _mediator.Send(command);

        return authResult.Match(
            authResult => Ok(MapAuthResult(authResult)),
            errors => Problem(errors)
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(BuberDinner.Contracts.Authentication.LoginRequest request)
    {
        var query = new LoginQuery(
            request.Email,
            request.Password);

        var authResult = await _mediator.Send(query);

        if (authResult.IsError && authResult.FirstError == Errors.Authentication.InvalidCredentials)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: authResult.FirstError.Description);
        }

        return authResult.Match(
            authResult => Ok(MapAuthResult(authResult)),
            errors => Problem(errors)
        );
    }

    private static AuthenticationResponse MapAuthResult(ErrorOr<BuberDinner.Application.Authentication.Common.AuthenticationResult> authResult)
    {
        var response = new AuthenticationResponse(
            authResult.Value.User.Id,
            authResult.Value.User.FirstName,
            authResult.Value.User.LastName,
            authResult.Value.User.Email,
            authResult.Value.Token);

        return response;
    }
}
