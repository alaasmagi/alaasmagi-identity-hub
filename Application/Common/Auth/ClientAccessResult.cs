using Application.Auth.Responses;
using Domain;

namespace Application.Common.Auth;

internal sealed record ClientAccessResult(
    AppUser User,
    Client Client,
    AppUserClient? UserClient,
    LoginResponse? Response,
    string? Error);
