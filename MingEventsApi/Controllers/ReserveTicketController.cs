using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MingEventsApi.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MingEventsApi.Controllers
{
    public class ReserveTicketController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/ReserveTicket
        [HttpGet]
        [Route("api/ReserveTicket")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetReserveTickets()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var tickets = await db.Reserve_Ticket
                .Select(r => new
                {
                    r.armchair_id,
                    r.user_id,
                    r.reservation_date,
                    r.event_id
                })
                .ToListAsync();

            return Ok(tickets);
        }

        // GET: api/ReserveTicket/{armchair_id}/{user_id}/{event_id}
        [HttpGet]
        [Route("api/ReserveTicket/{armchair_id:int}/{user_id:int}/{event_id:int}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetReserveTicket(int armchair_id, int user_id, int event_id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var ticket = await db.Reserve_Ticket
                .Where(r => r.armchair_id == armchair_id && r.user_id == user_id && r.event_id == event_id)
                .Select(r => new
                {
                    r.armchair_id,
                    r.user_id,
                    r.reservation_date,
                    r.event_id
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        // GET: api/ReserveTicket/user/{user_id:int}
        [HttpGet]
        [Route("api/ReserveTicket/user/{user_id:int}")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetReserveTicketsByUser(int user_id)
        {
            db.Configuration.LazyLoadingEnabled = false;


            var query = await (
                from r in db.Reserve_Ticket
                where r.user_id == user_id
                join e in db.Event on r.event_id equals e.event_id into eventGroup
                from e in eventGroup.DefaultIfEmpty()
                select new
                {
                    r.armchair_id,
                    r.user_id,
                    r.reservation_date,
                    r.event_id,
                    e.start_date
                })
                .ToListAsync();

            var tickets = query
                .Where(t => t.start_date == null ||
                            (DateTime.Parse(t.start_date) >= DateTime.Now.Date))
                .Select(t => new
                {
                    t.armchair_id,
                    t.user_id,
                    t.reservation_date,
                    t.event_id
                })
                .ToList();

            return Ok(tickets);
        }

        // GET: api/ReserveTicket/ReservedSeatsByEvent/{event_id}
        [HttpGet]
        [Route("api/ReserveTicket/ReservedSeatsByEvent/{event_id:int}")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetReservedSeatsByEvent(int event_id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var reservedSeats = await db.Reserve_Ticket
                .Where(r => r.event_id == event_id)
                .Include(r => r.Armchair)
                .Select(r => new
                {
                    rows = r.Armchair.rows,
                    columns = r.Armchair.columns,
                    armchair_id = r.armchair_id
                })
                .ToListAsync();

            if (reservedSeats == null || !reservedSeats.Any())
                return NotFound();
            return Ok(reservedSeats);
        }
        // POST: api/ReserveTicket
        [HttpPost]
        [Route("api/ReserveTicket")]
        [ResponseType(typeof(Reserve_Ticket))]
        public async Task<IHttpActionResult> PostReserveTicket(Reserve_Ticket ticket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Reserve_Ticket.Add(ticket);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { armchair_id = ticket.armchair_id, user_id = ticket.user_id, event_id = ticket.event_id }, ticket);
        }

        // PUT: api/ReserveTicket/{armchair_id}/{user_id}/{event_id}
        [HttpPut]
        [Route("api/ReserveTicket/{armchair_id:int}/{user_id:int}/{event_id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReserveTicket(int armchair_id, int user_id, int event_id, Reserve_Ticket ticket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (armchair_id != ticket.armchair_id || user_id != ticket.user_id || event_id != ticket.event_id)
                return BadRequest("ID mismatch");

            var existing = await db.Reserve_Ticket
                .FirstOrDefaultAsync(r => r.armchair_id == armchair_id && r.user_id == user_id && r.event_id == event_id);

            if (existing == null)
                return NotFound();

            // Actualiza solo los campos modificables
            existing.reservation_date = ticket.reservation_date;

            try
            {
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/ReserveTicket/{armchair_id}/{user_id}/{event_id}
        [HttpDelete]
        [Route("api/ReserveTicket/{armchair_id:int}/{user_id:int}/{event_id:int}")]
        [ResponseType(typeof(Reserve_Ticket))]
        public async Task<IHttpActionResult> DeleteReserveTicket(int armchair_id, int user_id, int event_id)
        {
            var ticket = await db.Reserve_Ticket
                .FirstOrDefaultAsync(r => r.armchair_id == armchair_id && r.user_id == user_id && r.event_id == event_id);

            if (ticket == null)
                return NotFound();

            db.Reserve_Ticket.Remove(ticket);
            await db.SaveChangesAsync();

            return Ok(ticket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}