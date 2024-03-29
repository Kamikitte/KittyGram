﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Configs;
using Api.Models.Token;
using Common;
using Common.Constants;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public sealed class AuthService
{
    private readonly AuthConfig config;
    private readonly DataContext context;

    public AuthService(IOptions<AuthConfig> config, DataContext context)
    {
        this.context = context;
        this.config = config.Value;
    }

    public async Task<TokenModel> GetToken(string login, string password)
    {
        var user = await GetUserByCredential(login, password);
        var session = await context.UserSessions.AddAsync(new UserSession
        {
            User = user,
            RefreshToken = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        });
        
        await context.SaveChangesAsync();
        return GenerateTokens(session.Entity);
    }

    public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
    {
        var validParams = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = config.SymmetricSecurityKey(),
        };
        
        var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("invalid token");
        }

        if (principal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is not string refreshIdString
            || !Guid.TryParse(refreshIdString, out var refreshId))
        {
            throw new SecurityTokenException("invalid token");
        }

        var session = await GetSessionByRefreshToken(refreshId);
        if (!session.IsActive)
        {
            throw new Exception("session is not active");
        }

        session.RefreshToken = Guid.NewGuid();
        await context.SaveChangesAsync();

        return GenerateTokens(session);
    }

    public async Task<UserSession> GetSessionById(Guid id)
    {
        var session = await context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (session == null)
        {
            throw new Exception("session is not found");
        }

        return session;
    }

    private async Task<User> GetUserByCredential(string login, string pass)
    {
        var user = await context.Users.FirstOrDefaultAsync(x =>
            x.Email.Equals(login, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            throw new Exception("user not found");
        }

        if (!HashHelper.Verify(pass, user.PasswordHash))
        {
            throw new Exception("password is incorrect");
        }

        return user;
    }

    private TokenModel GenerateTokens(UserSession session)
    {
        var dtNow = DateTime.Now;
        if (session.User == null)
        {
            throw new Exception("magic");
        }

        var jwt = new JwtSecurityToken(
            config.Issuer,
            config.Audience,
            notBefore: dtNow,
            claims: new Claim[]
            {
                new(ClaimsIdentity.DefaultNameClaimType, session.User.Name),
                new(ClaimNames.SessionId, session.Id.ToString()),
                new(ClaimNames.Id, session.User.Id.ToString()),
            },
            expires: DateTime.Now.AddMinutes(config.LifeTime),
            signingCredentials: new SigningCredentials(config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );
        
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refresh = new JwtSecurityToken(
            notBefore: dtNow,
            claims: new Claim[]
            {
                new(ClaimNames.RefreshToken, session.RefreshToken.ToString()),
            },
            expires: DateTime.Now.AddHours(config.LifeTime),
            signingCredentials: new SigningCredentials(config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );
        
        var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

        return new TokenModel(encodedJwt, encodedRefresh);
    }

    private async Task<UserSession> GetSessionByRefreshToken(Guid refreshTokenId)
    {
        var session = await context.UserSessions.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshTokenId);
        if (session == null)
        {
            throw new Exception("session is not found");
        }

        return session;
    }
}