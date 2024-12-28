using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE.Models;
using MiracleLandBE.LogicalServices;
using Microsoft.AspNetCore.Authorization;
using MiracleLandBE.MinimalModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Configuration;
using Azure.Core;

namespace MiracleLandBE.CsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsUserAccountsController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly LoginTokenGenerate _loginTokenGenerate;
        private readonly string _jwtKey;
        private readonly ImageUploader.ImgUploader _imgUploader;

        public CsUserAccountsController(TsmgbeContext context, LoginTokenGenerate loginTokenGenerate, IConfiguration configuration, ImageUploader.ImgUploader imgUploader)
        {
            _context = context;
            _loginTokenGenerate = loginTokenGenerate;
            _jwtKey = configuration["Jwt:Key"];
            _imgUploader = imgUploader;
        }

        [AllowAnonymous]
        [HttpPost("CsLogin")]
        public async Task<IActionResult> Login(UserLoginRequest loginRequest)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            if (userAccount == null || !PasswordProcess.VerifyPassword(loginRequest.Password, userAccount.Password))
            {
                return Unauthorized("Wrong username or password");
            }

            var token = _loginTokenGenerate.GenerateJwtToken(userAccount);

            return Ok(new { Token = token });
        }

        [AllowAnonymous]
        [HttpGet("ValidateToken")]
        public IActionResult ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);

                return Ok("Token is valid.");
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }



        [HttpGet("GetAccountInfo")]
        public async Task<ActionResult<GetAccountInfo>> GetAccountInfo([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidString = principal.Identity?.Name;
                if (string.IsNullOrEmpty(uidString))
                {
                    return Unauthorized("Invalid token.");
                }

                if (!Guid.TryParse(uidString, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Uid == uid);
                if (userAccount == null)
                {
                    return NotFound("User not found.");
                }

                var accountInfo = new GetAccountInfo
                {
                    token = token,
                    Username = userAccount.Username,
                    Email = userAccount.Email,
                    Phone = userAccount.Phone,
                    Address = userAccount.Address,
                    Avt = userAccount.Avt
                };

                return Ok(accountInfo);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }


        ///api/CsUserAccounts/UpdateUserInfo
        [HttpPut("UpdateUserInfo")]
        public async Task<IActionResult> UpdateUserInfo([FromForm] UserAccountUpdate userAccountUpdate)
        {
            if (string.IsNullOrEmpty(userAccountUpdate.token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(userAccountUpdate.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidString = principal.Identity?.Name;

                if (!Guid.TryParse(uidString, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Uid == uid);
                if (userAccount == null)
                {
                    return NotFound("User not found.");
                }

                if (!string.IsNullOrEmpty(userAccountUpdate.Email))
                {
                    userAccount.Email = userAccountUpdate.Email;
                }

                if (!string.IsNullOrEmpty(userAccountUpdate.Phone))
                {
                    userAccount.Phone = userAccountUpdate.Phone;
                }

                if (!string.IsNullOrEmpty(userAccountUpdate.Address))
                {
                    userAccount.Address = userAccountUpdate.Address;
                }

                if (!string.IsNullOrEmpty(userAccountUpdate.AvatarContent))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(userAccountUpdate.AvatarContent);
                        string imagePath = Path.GetTempFileName();
                        await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

                        string avatarUrl = await _imgUploader.UploadImageAsync(imagePath);
                        userAccount.Avt = avatarUrl;

                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"Image upload failed: {ex.Message}");
                    }
                }

                _context.Entry(userAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("User information updated successfully.");
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }


        [HttpPost("Register")]
        public async Task<ActionResult<UserAccount>> PostUserAccount(UserRegisterRequest userRegisterRequest)
        {
            if (await _context.UserAccounts.AnyAsync(u => u.Username == userRegisterRequest.Username))
            {
                return BadRequest("Error: Username already exists.");
            }
            var userAccount = new UserAccount
            {
                Uid = Guid.NewGuid(),
                Username = userRegisterRequest.Username,
                Password = PasswordProcess.EncryptData(userRegisterRequest.Password),
                Type = "Customer",
                Email = userRegisterRequest.Email,
                Phone = userRegisterRequest.Phone,
                Address = userRegisterRequest.Address,
                IsActive = true,
                Avt = "https://i.ibb.co/HzkrGtb/s-l500.jpg"
            };
            _context.UserAccounts.Add(userAccount);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserAccountExists(userAccount.Uid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok("User account created successfully.");
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest request)
        {
            if (string.IsNullOrEmpty(request.token))
            {
                return BadRequest("Login is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(request.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidString = principal.Identity?.Name;
                if (string.IsNullOrEmpty(uidString))
                {
                    return Unauthorized("Invalid token.");
                }

                if (!Guid.TryParse(uidString, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Uid == uid);
                if (userAccount == null)
                {
                    return NotFound("User not found.");
                }

                if (!PasswordProcess.VerifyPassword(request.CurrentPassword, userAccount.Password))
                {
                    return BadRequest("Current password is incorrect.");
                }
                userAccount.Password = PasswordProcess.EncryptData(request.NewPassword);
                _context.Entry(userAccount).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Ok("Password changed successfully.");
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }

        private bool UserAccountExists(Guid id)
        {
            return _context.UserAccounts.Any(e => e.Uid == id);
        }
    }
}
