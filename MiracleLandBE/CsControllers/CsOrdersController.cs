using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiracleLandBE.MinimalModels;
using MiracleLandBE.Models;

namespace MiracleLandBE.CsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsOrdersController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly string _jwtKey;

        public CsOrdersController(TsmgbeContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] CsOrdersRequest orderRequest)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var principal = tokenHandler.ValidateToken(orderRequest.token, new TokenValidationParameters
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

            var order = new CsOrder
            {
                Uid = uid,
                Orderid = Guid.NewGuid(),
                Total = orderRequest.Total,
                IsPayment = orderRequest.IsPayment,
                ShipId = orderRequest.ShipId,
                CsOrderDetails = new List<CsOrderDetail>()
            };

            if (orderRequest.CsOrderDetails == null || !orderRequest.CsOrderDetails.Any())
            {
                return BadRequest("Order details are missing.");
            }

            foreach (var detailRequest in orderRequest.CsOrderDetails)
            {
                var detail = new CsOrderDetail
                {
                    Odid = Guid.NewGuid(),
                    Orderid = order.Orderid,
                    Pid = detailRequest.Pid,
                    Quantity = detailRequest.Quantity
                };
                order.CsOrderDetails.Add(detail);
            }

            _context.CsOrders.Add(order);
            await _context.SaveChangesAsync();

            var userCartItems = _context.ShoppingCarts.Where(cart => cart.Uid == uid);
            _context.ShoppingCarts.RemoveRange(userCartItems);
            await _context.SaveChangesAsync();

            return Ok("Order placed successfully and cart cleared.");
        }




        [HttpGet("Orders")]
        public async Task<IActionResult> GetUserOrders(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
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

            if (!Guid.TryParse(uidString, out var uid))
            {
                return Unauthorized("Invalid token.");
            }

            var orders = await _context.CsOrders
                .Where(o => o.Uid == uid)
                .Select(o => new CsOrdersRequest
                {
                    Orderid = o.Orderid,
                    Uid = o.Uid,
                    Total = o.Total,
                    IsPayment = o.IsPayment,
                    ShipId = o.ShipId
                })
                .ToListAsync();

            return Ok(orders);
        }


        // GET: api/Orders/details/{orderId}
        [HttpGet("order-details/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            var orderDetails = await _context.CsOrderDetails
                .Where(od => od.Orderid == orderId)
                .Join(
                    _context.Products, 
                    od => od.Pid,     
                    p => p.Pid,      
                    (od, p) => new CsOrderDetailRequest
                    {
                        Odid = od.Odid,
                        Orderid = od.Orderid,
                        Pid = od.Pid,
                        Pname = p.Pname,  
                        Pimg = p.Pimg,    
                        Quantity = od.Quantity
                    })
                .ToListAsync();

            return Ok(orderDetails);
        }


        private async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _context.CsOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}
