using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext dataContext;
        private readonly ITokenService tokenService;

        public AccountController(DataContext dataContext,ITokenService tokenService)
        {
            this.dataContext = dataContext;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            if(await this.UserExists(registerDto.Username)) return BadRequest("Username is alreay taken");

            using var hmac = new HMACSHA512();
            var user = new AppUser {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            this.dataContext.Add(user);

            await this.dataContext.SaveChangesAsync();

            return new UserDto {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        } 

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await this.dataContext.User
                .SingleOrDefaultAsync(user => user.UserName == loginDto.Username.ToLower());

            if(user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); 

            for(int i = 0; i < computedHash.Length; i++){
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username){
            return await this.dataContext.User.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}