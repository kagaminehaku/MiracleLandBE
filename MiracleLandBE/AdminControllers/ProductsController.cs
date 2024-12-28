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
using static MiracleLandBE.LogicalServices.ImageUploader;

namespace MiracleLandBE.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly TsmgbeContext _context;

        public ProductsController(TsmgbeContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromForm] PostPutProductNoImage postPutProduct)
        {
            if (string.IsNullOrEmpty(postPutProduct.Pid.ToString()))
            {
                return BadRequest("Product is required.");
            }
            try
            {
                var existingProduct = await _context.Products.FirstOrDefaultAsync(u => u.Pid == postPutProduct.Pid);
                if (existingProduct == null)
                {
                    return NotFound("Product not found.");
                }

                existingProduct.Pname = postPutProduct.Pname;
                existingProduct.Pprice = postPutProduct.Pprice;
                existingProduct.Pquantity = postPutProduct.Pquantity;
                existingProduct.Pinfo = postPutProduct.Pinfo;
                existingProduct.Pimg = existingProduct.Pimg;


                _context.Entry(existingProduct).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("Product information updated successfully.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Failed: {ex.Message}");
            }
        }


        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.Pid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = product.Pid }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Pid == id);
        }
    }
}
