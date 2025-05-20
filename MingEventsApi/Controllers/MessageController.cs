using MingEventsApi.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MingEventsApi.Controllers
{
    public class MessageController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/Message
        [HttpGet]
        [Route("api/Message")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetMessage()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var Message = await db.Message
                .Select(m => new
                {
                    m.message_id,
                    m.sender_id,
                    m.content,
                    m.send_at,
                    m.is_read,
                    m.chat_id
                })
                .ToListAsync();

            return Ok(Message);
        }

        // GET: api/Message/{id}
        [HttpGet]
        [Route("api/Message/{id}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetMessage(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var message = await db.Message
                .Where(m => m.message_id == id)
                .Select(m => new
                {
                    m.message_id,
                    m.sender_id,
                    m.content,
                    m.send_at,
                    m.is_read,
                    m.chat_id
                })
                .FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        // PUT: api/Message/{id}
        [HttpPut]
        [Route("api/Message/{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMessage(int id, Message Message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != Message.message_id)
                return BadRequest("ID mismatch");

            var existing = await db.Message.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualitzem només els camps modificables
            existing.sender_id = Message.sender_id;
            existing.content = Message.content;
            existing.send_at = Message.send_at;
            existing.is_read = Message.is_read;
            existing.chat_id = Message.chat_id;

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

        // POST: api/Message
        [HttpPost]
        [Route("api/Message")]
        [ResponseType(typeof(Message))]
        public async Task<IHttpActionResult> PostMessage(Message Message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Message.Add(Message);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = Message.message_id }, Message);
        }

        // DELETE: api/Message/{id}
        [HttpDelete]
        [Route("api/Message/{id:int}")]
        [ResponseType(typeof(Message))]
        public async Task<IHttpActionResult> DeleteMessage(int id)
        {
            var message = await db.Message.FindAsync(id);
            if (message == null)
                return NotFound();

            db.Message.Remove(message);
            await db.SaveChangesAsync();

            return Ok(message);
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