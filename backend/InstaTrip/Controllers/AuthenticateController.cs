﻿using InstaTrip.API.Dtos;
using InstaTrip.API.Models;
using InstaTrip.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITouristRouteRepository _touristRouteRepository;

        public AuthenticateController(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITouristRouteRepository touristRouteRepository
        )
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _touristRouteRepository = touristRouteRepository;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginDto loginDto)
        {
            // check username and password
            var loginResult = await _signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                false,
                false
            );
            if(!loginResult.Succeeded)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByNameAsync(loginDto.Email);

            // reate jwt
            // header
            var signingAlgorithm = SecurityAlgorithms.HmacSha256;
            // payload
            var claims = new List<Claim>
            {
                // sub
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                //new Claim(ClaimTypes.Role, "Admin")
            };
            var roleNames = await _userManager.GetRolesAsync(user);
            foreach(var roleName in roleNames)
            {
                var roleClaim = new Claim(ClaimTypes.Role, roleName);
                claims.Add(roleClaim);
            }
            // signiture
            var secretByte = Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]);
            var signingKey = new SymmetricSecurityKey(secretByte);
            var signingCredentials = new SigningCredentials(signingKey, signingAlgorithm);

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials
            ) ;

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            // return 200 ok + jwt
            return Ok(tokenStr);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new ApplicationUser()
            {
                UserName = registerDto.Email,
                Email = registerDto.Email
            };

            // hash password, save user
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded)
            {
                return BadRequest();
            }

            // initialize cart
            var shoppingCart = new ShoppingCart()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id
            };
            await _touristRouteRepository.CreateShoppingCart(shoppingCart);
            await _touristRouteRepository.SaveAsync();

            // return
            return Ok();
        }
    }
}
