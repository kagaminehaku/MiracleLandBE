using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE.Models;

namespace MiracleLandBE.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAccountsController : ControllerBase
    {
        private readonly TsmgbeContext _context;

        public AdminAccountsController(TsmgbeContext context)
        {
            _context = context;
        }

        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.UserAccounts
                                           .Where(u => u.Type == "Customer")
                                           .ToListAsync();

            if (!customers.Any())
            {
                return NotFound("No customers found.");
            }

            return Ok(customers);
        }

        [HttpPost("BanUser")]
        public async Task<IActionResult> BanUser([FromBody] Guid userId)
        {
            var user = await _context.UserAccounts.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsActive = false;

            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();

            return Ok("User has been banned successfully.");
        }


        private bool UserAccountExists(Guid id)
        {
            return _context.UserAccounts.Any(e => e.Uid == id);
        }
    }
}
