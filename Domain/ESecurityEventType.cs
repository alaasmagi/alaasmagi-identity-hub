namespace Domain;

public enum ESecurityEventType
{
    Login,
    Logout,
    TokenRefresh,
    FailedAttempt,
    PasswordChanged,
    TwoFactorEnabled,
    TwoFactorDisabled,
    ConsentGiven,
    ConsentRevoked,
    AccountBanned,
    AccountUnbanned
}