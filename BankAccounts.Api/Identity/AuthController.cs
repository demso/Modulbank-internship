﻿using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Notes.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankAccounts.Api.Identity;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IIdentityServerInteractionService interactionService, IConfiguration configuration, IdentityDbContext<BankUser> iDbContext)
    : CustomControllerBase
{
    [HttpPost]
    public async Task<MbResult<string>> Login(LoginData data) {
        var user = await userManager.FindByNameAsync(data.Username);
        if (user == null)
            return Failure(StatusCodes.Status404NotFound, "User not found");

        var result = await signInManager.PasswordSignInAsync(data.Username,
            data.Password, false, false);
        if (!result.Succeeded)
            return Failure(StatusCodes.Status400BadRequest, "Login failed.");

        var token = GenerateJwtToken(user);

        return Success(StatusCodes.Status200OK, token);
    }

    private string GenerateJwtToken(BankUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost]
    public async Task<MbResult<string>> Register(RegisterData data)
    {
        var user = new BankUser {
            UserName = data.Username
        };

        var result = await userManager.CreateAsync(user, data.Password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("\n | ", result.Errors.Select(error => error.Description));
            return Failure(StatusCodes.Status400BadRequest, errorMessage);
        }
        
        await signInManager.SignInAsync(user, false);

        return Success(StatusCodes.Status200OK, "Register succeed.");
    }
}