using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MingEventsApi.Models;
using System.Linq;
using System.Collections.Generic;


namespace MingEventsApi.Controllers
{
    public class ChatController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/Chat
        [HttpGet]
        [Route("api/Chat")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetChat()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var Chat = await db.Chat
                .Select(c => new
                {
                    c.chat_id,
                    c.send_date,
                    c.user1_id,
                    c.user2_id
                })
                .ToListAsync();

            return Ok(Chat);
        }

        // GET: api/Chat/{id}
        [HttpGet]
        [Route("api/Chat/{id}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetChat(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var chat = await db.Chat
                .Where(c => c.chat_id == id)
                .Select(c => new
                {
                    c.chat_id,
                    c.user1_id,
                    c.user2_id
                })
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound();
            }

            return Ok(chat);
        }


        // GET: api/Chat/user/{id}
        [HttpGet]
        [Route("api/Chat/user/{id:int}")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetChatByUser(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var Chat = await db.Chat
                .Where(c => c.user1_id == id || c.user2_id == id)
                .Select(c => new
                {
                    c.chat_id,
                    c.user1_id,
                    c.user2_id
                })
                .ToListAsync();

            if (Chat == null || !Chat.Any())
            {
                return NotFound();
            }

            return Ok(Chat);
        }

        // PUT: api/Chat/{id}
        [HttpPut]
        [Route("api/Chat/{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutChat(int id, Chat chat)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != chat.chat_id)
                return BadRequest("ID mismatch");

            var existing = await db.Chat.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualitzem només els camps modificables
            existing.user1_id = chat.user1_id;
            existing.user2_id = chat.user2_id;

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

        // POST: api/Chat
        [HttpPost]
        [Route("api/Chat")]
        [ResponseType(typeof(Chat))]
        public async Task<IHttpActionResult> PostChat(Chat chat)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Chat.Add(chat);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = chat.chat_id }, chat);
        }

        // DELETE: api/Chat/{id}
        [HttpDelete]
        [Route("api/Chat/{id}")]
        [ResponseType(typeof(Chat))]
        public async Task<IHttpActionResult> DeleteChat(int id)
        {
            var chat = await db.Chat.FindAsync(id);
            if (chat == null)
                return NotFound();

            db.Chat.Remove(chat);
            await db.SaveChangesAsync();

            return Ok(chat);
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
