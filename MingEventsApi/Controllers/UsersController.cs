using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MingEventsApi.Models;
using System.Linq;

namespace WebApplicationTgtNotes.Controllers
{
    public class UsersController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/Users
        [HttpGet]
        [Route("api/Users")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetUsers()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var users = await db.Users
                .Select(u => new {
                    u.user_id,
                    u.name,
                    u.email
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/{id}
        [HttpGet]
        [Route("api/Users/{id:int}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var user = await db.Users
                .Where(u => u.user_id == id)
                .Select(u => new {
                    u.user_id,
                    u.name,
                    u.email
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        [Route("api/Users")]
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostUser(Users user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = user.user_id }, user);
        }

        // PUT: api/Users/{id}
        [HttpPut]
        [Route("api/Users/{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUser(int id, Users user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != user.user_id)
                return BadRequest("ID mismatch");

            var existing = await db.Users.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.name = user.name;
            existing.email = user.email;
            existing.password = user.password; // Asegúrate de manejarla con seguridad

            try
            {
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch
            {
                return InternalServerError();
            }
        }

        // DELETE: api/Users/{id}
        [HttpDelete]
        [Route("api/Users/{id:int}")]
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Ok(user);
        }
        // POST: api/Users/login
        [HttpPost]
        [Route("api/Users/login")]
        public async Task<IHttpActionResult> Login([FromUri] string email, [FromUri] string password)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var user = await db.Users
                .Where(u => u.email == email && u.password == password)
                .Select(u => new
                {
                    u.user_id,
                    u.name,
                    u.second_name,
                    u.email,
                    u.phone,
                    u.password,
                    u.role_id
                    // Agrega más campos si lo necesitas
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
