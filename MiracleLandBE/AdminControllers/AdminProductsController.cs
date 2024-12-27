using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE;
using MiracleLandBE.LogicalServices;
using MiracleLandBE.MinimalModels;
using MiracleLandBE.Models;
using static MiracleLandBE.LogicalServices.ImageUploader;

namespace MiracleLandBE.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminProductsController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly string _jwtKey;
        private readonly ImageUploader.ImgUploader _imgUploader;

        public AdminProductsController(TsmgbeContext context,IConfiguration configuration, ImageUploader.ImgUploader imgUploader)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
            _imgUploader = imgUploader;
        }

        // GET: api/AdminProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/AdminProducts/5
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

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(PostPutProduct productInput)
        {
            var newProduct = new Product
            {
                Pid = productInput.Pid,
                Pname = productInput.Pname,
                Pprice = productInput.Pprice,
                Pquantity = productInput.Pquantity,
                Pinfo = productInput.Pinfo,
                Pimg = string.Empty // Placeholder, updated if PimgContent is provided
            };

            if (!string.IsNullOrEmpty(productInput.PimgContent))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(productInput.PimgContent);
                    string imagePath = Path.GetTempFileName();
                    await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

                    string imageUrl = await _imgUploader.UploadImageAsync(imagePath);
                    newProduct.Pimg = imageUrl;

                    System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Image upload failed: {ex.Message}");
                }
            }

            _context.Products.Add(newProduct);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(newProduct.Pid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = newProduct.Pid }, newProduct);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, PostPutProduct productUpdate)
        {
            if (id != productUpdate.Pid)
            {
                return BadRequest();
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Pname = productUpdate.Pname;
            existingProduct.Pprice = productUpdate.Pprice;
            existingProduct.Pquantity = productUpdate.Pquantity;
            existingProduct.Pinfo = productUpdate.Pinfo;
            existingProduct.Pimg = existingProduct.Pimg;

            if (!string.IsNullOrEmpty(productUpdate.PimgContent))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(productUpdate.PimgContent);
                    string imagePath = Path.GetTempFileName();
                    await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

                    string imageUrl = await _imgUploader.UploadImageAsync(imagePath);
                    existingProduct.Pimg = imageUrl;

                    System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Image upload failed: {ex.Message}");
                }
            }

            _context.Entry(existingProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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


        // DELETE: api/AdminProducts/5
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
