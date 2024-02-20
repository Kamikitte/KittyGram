namespace Api.Models.Token;

public sealed class TokenRequestModel
{
    public TokenRequestModel(string login, string password)
    {
        Login = login;
        Password = password;
    }

    public string Login { get; set; }
    
    public string Password { get; set; }
}

public sealed class RefreshTokenRequestModel
{
    public RefreshTokenRequestModel(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    public string RefreshToken { get; set; }
}