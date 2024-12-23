using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE.Models;

namespace MiracleLandBE.CsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsOrderDetailsController : ControllerBase
    {
        private readonly TsmgbeContext _context;

        public CsOrderDetailsController(TsmgbeContext context)
        {
            _context = context;
        }

        // GET: api/CsOrderDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CsOrderDetail>>> GetCsOrderDetails()
        {
            return await _context.CsOrderDetails.ToListAsync();
        }

        // GET: api/CsOrderDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CsOrderDetail>> GetCsOrderDetail(Guid id)
        {
            var csOrderDetail = await _context.CsOrderDetails.FindAsync(id);

            if (csOrderDetail == null)
            {
                return NotFound();
            }

            return csOrderDetail;
        }

        // PUT: api/CsOrderDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCsOrderDetail(Guid id, CsOrderDetail csOrderDetail)
        {
            if (id != csOrderDetail.Odid)
            {
                return BadRequest();
            }

            _context.Entry(csOrderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CsOrderDetailExists(id))
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

        // POST: api/CsOrderDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CsOrderDetail>> PostCsOrderDetail(CsOrderDetail csOrderDetail)
        {
            _context.CsOrderDetails.Add(csOrderDetail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CsOrderDetailExists(csOrderDetail.Odid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCsOrderDetail", new { id = csOrderDetail.Odid }, csOrderDetail);
        }

        // DELETE: api/CsOrderDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCsOrderDetail(Guid id)
        {
            var csOrderDetail = await _context.CsOrderDetails.FindAsync(id);
            if (csOrderDetail == null)
            {
                return NotFound();
            }

            _context.CsOrderDetails.Remove(csOrderDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CsOrderDetailExists(Guid id)
        {
            return _context.CsOrderDetails.Any(e => e.Odid == id);
        }
    }
}
