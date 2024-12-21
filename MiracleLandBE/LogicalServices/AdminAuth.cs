using MiracleLandBE.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MiracleLandBE.LogicalServices
{
    public class AdminAuth
    {
        private readonly TsmgbeContext _context;

        public AdminAuth(TsmgbeContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserAdminAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.Name);
            if (userId == null) return false;

            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Uid.ToString() == userId);

            return userAccount != null && userAccount.Type.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
