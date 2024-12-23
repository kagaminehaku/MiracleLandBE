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

namespace MiracleLandBE.CsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsUserAccountsController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly LoginTokenGenerate _loginTokenGenerate;

        public CsUserAccountsController(TsmgbeContext context,LoginTokenGenerate loginTokenGenerate)
        {
            _context = context;
            _loginTokenGenerate = loginTokenGenerate;
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

        // GET: api/CsUserAccounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAccount>> GetUserAccount(Guid id)
        {
            var userAccount = await _context.UserAccounts.FindAsync(id);

            if (userAccount == null)
            {
                return NotFound();
            }

            return userAccount;
        }

        // PUT: api/CsUserAccounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAccount(Guid id, UserAccount userAccount)
        {
            if (id != userAccount.Uid)
            {
                return BadRequest();
            }

            _context.Entry(userAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CsUserAccounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserAccount>> PostUserAccount(UserAccount userAccount)
        {
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

            return CreatedAtAction("GetUserAccount", new { id = userAccount.Uid }, userAccount);
        }

        private bool UserAccountExists(Guid id)
        {
            return _context.UserAccounts.Any(e => e.Uid == id);
        }
    }
}
