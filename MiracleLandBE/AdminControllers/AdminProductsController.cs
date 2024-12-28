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
        private readonly ImgUploader _imgUploader;

        public AdminProductsController(TsmgbeContext context,IConfiguration configuration, ImgUploader imgUploader)
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

        [HttpPatch("UpdateProductImage")]
        public async Task<IActionResult> UpdateProductImage([FromBody] EditProductImage model)
        {
            if (model == null || model.Pid == Guid.Empty || string.IsNullOrWhiteSpace(model.ProductImgContent))
            {
                return BadRequest("Invalid input.");
            }

            try
            {
                var product = await _context.Products.FindAsync(model.Pid);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (!string.IsNullOrEmpty(model.ProductImgContent))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(model.ProductImgContent);
                        string imagePath = Path.GetTempFileName();
                        await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

                        string avatarUrl = await _imgUploader.UploadImageAsync(imagePath);
                        product.Pimg = avatarUrl;

                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"Image upload failed: {ex.Message}");
                    }
                }

                // Save changes to the database
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok("Product image updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the product image.");
            }
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

        [HttpPost("PP")]
        public async Task<ActionResult> PutProduct(PostPutProduct productUpdate)
        {
            Console.WriteLine(productUpdate.Pid);
            Console.WriteLine(productUpdate.Pname);
            Console.WriteLine(productUpdate.Pprice);
            Console.WriteLine(productUpdate.Pquantity);
            Console.WriteLine(productUpdate.Pinfo);
            Console.WriteLine(productUpdate.PimgContent);
            Console.WriteLine("Fine");
            var existingProduct = await _context.Products.FindAsync(productUpdate.Pid);

            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }


            existingProduct.Pname = productUpdate.Pname;
            existingProduct.Pprice = productUpdate.Pprice;
            existingProduct.Pquantity = productUpdate.Pquantity;
            existingProduct.Pinfo = productUpdate.Pinfo;
            existingProduct.Pimg = string.Empty;

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

            //_context.Entry(existingProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productUpdate.Pid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
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
