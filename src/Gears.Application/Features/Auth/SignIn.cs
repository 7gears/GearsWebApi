﻿namespace Gears.Application.Features.Auth;

using SignInResponseResultType = Results<Ok<SignInResponse>, NotFound, UnauthorizedHttpResult>;

public sealed record SignInRequest(
    string Email,
    string Password
);

public sealed record SignInResponse(
    string Token
);

public sealed class SignInRequestValidator : Validator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}

public sealed class SignIn(
    UserManager<User> userManager,
    IJwtTokenProvider jwtTokenProvider
)
    : Endpoint<SignInRequest, SignInResponseResultType>
{
    public override void Configure()
    {
        Post("api/signin");
        AllowAnonymous();
    }

    public override async Task<SignInResponseResultType> ExecuteAsync(SignInRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return NotFound();
        }

        if (!user.EmailConfirmed)
        {
            return Unauthorized();
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized();
        }

        var token = await jwtTokenProvider.GetToken(user);

        return Ok(new SignInResponse(token));
    }
}

public sealed class SignInSwaggerSummary : Summary<SignIn>
{
    public SignInSwaggerSummary()
    {
        Response<SignInResponse>();
        Response(400);
        Response(401);
        Response(404);
    }
}
