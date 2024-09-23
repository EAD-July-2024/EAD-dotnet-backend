using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class AuthController:Controller
    {
        private readonly UserRepository _userRepository;
    private readonly JWTService _jwtService;

    public AuthController(UserRepository userRepository, JWTService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userRepository.GetByEmailAsync(model.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        if (!user.IsApproved)
        {
            return Unauthorized("Your account is not yet approved by CSR.");
        }

        var token = _jwtService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
        var user = new ApplicationUser
        {
            Email = model.Email,
            PasswordHash = hashedPassword,
            Role = model.Role
        };

        await _userRepository.CreateAsync(user);
        await _userRepository.NotifyCSR(); // Notify CSR on new account
        return Ok("User created. Pending approval from CSR.");
    }
        
    }
}