﻿namespace Gears.Application.Features.Auth;

public sealed record ForgotPasswordRequest(
    string Email
);

public sealed class ForgotPasswordRequestValidator : Validator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress();
    }
}

public sealed class ForgotPassword(
    UserManager<User> userManager,
    IMailService mailService,
    IHttpContextService httpContextService
)
    : Endpoint<ForgotPasswordRequest, Ok>
{
    public override void Configure()
    {
        Post("api/auth/forgot-password");
        AllowAnonymous();
    }

    public override async Task<Ok> ExecuteAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not { IsActive: true })
        {
            // Return a consistent response for both existent and non-existent accounts
            return Ok();
        }

        var link = await GenerateResetPasswordLink(user);

        var mailRequest = new MailRequest(user.Email, "Reset Password", link);

        _ = mailService.Send(mailRequest);

        return Ok();
    }

    private async Task<string> GenerateResetPasswordLink(User user)
    {
        var origin = httpContextService.GetOrigin();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        UriBuilder builder = new(origin) { Path = "reset-password" };
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["Id"] = user.Id;
        query["Token"] = token;
        builder.Query = query.ToString()!;

        return builder.ToString();
    }
}

public sealed class ForgotPasswordSummary : Summary<ForgotPassword>
{
    public ForgotPasswordSummary()
    {
        Response();
        Response(400);
    }
}
