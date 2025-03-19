using BuberDinner.Application.Common.Interfaces.Authentication;
using BuberDinner.Application.Common.Interfaces.Persistence;
using BuberDinner.Application.Authentication.Common;
using BuberDinner.Domain.Common.Errors;
using BuberDinner.Domain.Entities;
using ErrorOr;
using MediatR;


namespace BuberDinner.Application.Authentication.Queries.Login;

public class LoginCommandHandler :
    IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;

    public LoginCommandHandler(
        IJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
    }

    public Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        if (_userRepository.GetUserByEmail(query.Email) is not User user)
        {
            return Task.FromResult<ErrorOr<AuthenticationResult>>(Errors.Authentication.InvalidCredentials);
        }

        if (user.Password != query.Password)
        {
            return Task.FromResult<ErrorOr<AuthenticationResult>>(new[] { Errors.Authentication.InvalidCredentials });
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Task.FromResult<ErrorOr<AuthenticationResult>>(new AuthenticationResult(user, token));
    }
}
