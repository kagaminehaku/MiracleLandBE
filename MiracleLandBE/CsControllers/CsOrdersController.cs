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
    public class CsOrdersController : ControllerBase
    {
        private readonly TsmgbeContext _context;

        public CsOrdersController(TsmgbeContext context)
        {
            _context = context;
        }

        // GET: api/CsOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CsOrder>>> GetCsOrders()
        {
            return await _context.CsOrders.ToListAsync();
        }

        // GET: api/CsOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CsOrder>> GetCsOrder(Guid id)
        {
            var csOrder = await _context.CsOrders.FindAsync(id);

            if (csOrder == null)
            {
                return NotFound();
            }

            return csOrder;
        }

        // PUT: api/CsOrders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCsOrder(Guid id, CsOrder csOrder)
        {
            if (id != csOrder.Orderid)
            {
                return BadRequest();
            }

            _context.Entry(csOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CsOrderExists(id))
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

        // POST: api/CsOrders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CsOrder>> PostCsOrder(CsOrder csOrder)
        {
            _context.CsOrders.Add(csOrder);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CsOrderExists(csOrder.Orderid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCsOrder", new { id = csOrder.Orderid }, csOrder);
        }

        // DELETE: api/CsOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCsOrder(Guid id)
        {
            var csOrder = await _context.CsOrders.FindAsync(id);
            if (csOrder == null)
            {
                return NotFound();
            }

            _context.CsOrders.Remove(csOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CsOrderExists(Guid id)
        {
            return _context.CsOrders.Any(e => e.Orderid == id);
        }
    }
}
