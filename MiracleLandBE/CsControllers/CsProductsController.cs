using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE;
using MiracleLandBE.Models;
using MiracleLandBE.MinimalModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MiracleLandBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsProductsController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly string _jwtKey;

        public CsProductsController(TsmgbeContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CsProducts>>> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new CsProducts
                {
                    Pid = p.Pid,
                    Pname = p.Pname,
                    Pprice = p.Pprice,
                    Pquantity = p.Pquantity,
                    Pinfo = p.Pinfo,
                    Pimg = p.Pimg,
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/CsProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CsProducts>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var reproduct = new CsProducts();
            reproduct.Pid = product.Pid;
            reproduct.Pname = product.Pname;
            reproduct.Pprice = product.Pprice;
            reproduct.Pquantity = product.Pquantity;
            reproduct.Pinfo = product.Pinfo;
            reproduct.Pimg = product.Pimg;

            return Ok(reproduct);
        }

        [HttpPost("UpdateShoppingCart")]
        public async Task<ActionResult> UpdateShoppingCart([FromBody] CsProductsToCart productToCart)
        {
            if (string.IsNullOrEmpty(productToCart.token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(productToCart.token, new TokenValidationParameters
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

                var existingCartItem = await _context.ShoppingCarts.FirstOrDefaultAsync(cart => cart.Uid == uid && cart.Pid == productToCart.Pid);

                var product = await _context.Products.FindAsync(productToCart.Pid);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (existingCartItem == null)
                {
                    if (productToCart.Pquantity > 0)
                    {
                        if (product.Pquantity < productToCart.Pquantity)
                        {
                            return BadRequest("Not enough product in stock.");
                        }

                        var newCartItem = new ShoppingCart
                        {
                            Cartitemid = Guid.NewGuid(),
                            Uid = uid,
                            Pid = productToCart.Pid,
                            Pquantity = productToCart.Pquantity
                        };

                        product.Pquantity -= productToCart.Pquantity;

                        _context.ShoppingCarts.Add(newCartItem);
                    }
                }
                else
                {
                    if (productToCart.Pquantity == 0)
                    {
                        product.Pquantity += existingCartItem.Pquantity;
                        _context.ShoppingCarts.Remove(existingCartItem);
                    }
                    else
                    {
                        if (product.Pquantity + existingCartItem.Pquantity < productToCart.Pquantity)
                        {
                            return BadRequest("Not enough product in stock.");
                        }

                        var quantityDifference = productToCart.Pquantity - existingCartItem.Pquantity;
                        product.Pquantity -= quantityDifference;

                        if (existingCartItem.Pquantity + quantityDifference <= 0)
                        {
                            _context.ShoppingCarts.Remove(existingCartItem);
                        }
                        else
                        {
                            existingCartItem.Pquantity += quantityDifference;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok("Shopping cart updated successfully.");
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

        [HttpGet("GetShoppingCartItems")]
        public async Task<ActionResult<IEnumerable<CsProducts>>> GetShoppingCartItems([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                // Validate token and extract UID
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidClaim = principal.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                if (string.IsNullOrEmpty(uidClaim) || !Guid.TryParse(uidClaim, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                // Get all items in the user's shopping cart
                var cartItems = await _context.ShoppingCarts
                    .Where(cart => cart.Uid == uid)
                    .Join(_context.Products,
                        cart => cart.Pid,
                        product => product.Pid,
                        (cart, product) => new CsProducts
                        {
                            Pid = product.Pid,
                            Pname = product.Pname,
                            Pprice = product.Pprice,
                            Pquantity = cart.Pquantity,
                            Pinfo = product.Pinfo,
                            Pimg = product.Pimg
                        })
                    .ToListAsync();

                return Ok(cartItems);
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

        [HttpGet("shoppingcart")]
        public async Task<ActionResult<IEnumerable<ShoppingCartsGet>>> GetUserShoppingCart([FromHeader] string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            // Decode the token to get the user's ID
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var uidClaim = principal.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            if (string.IsNullOrEmpty(uidClaim) || !Guid.TryParse(uidClaim, out var uid))
            {
                return Unauthorized("Invalid token.");
            }

            // Retrieve shopping cart items for the user
            var shoppingCartItems = await _context.ShoppingCarts
                .Where(cart => cart.Uid == uid)
                .Select(cart => new
                {
                    cart.Cartitemid,
                    cart.Pid,
                    cart.Pquantity
                })
                .ToListAsync();

            if (!shoppingCartItems.Any())
            {
                return Ok("Shopping cart is empty.");
            }

            // Build the result with product price lookup
            var cartWithPrices = new List<ShoppingCartsGet>();
            foreach (var item in shoppingCartItems)
            {
                var product = await _context.Products.FindAsync(item.Pid);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.Pid} not found.");
                }

                cartWithPrices.Add(new ShoppingCartsGet
                {
                    Cartitemid = item.Cartitemid,
                    Pid = item.Pid,
                    Pquantity = item.Pquantity,
                    Pprice = product.Pprice
                });
            }

            return Ok(cartWithPrices);
        }

    }
}
