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
    public class ArmchairController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/Armchair
        [HttpGet]
        [Route("api/Armchair")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetArmchairs()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var armchairs = await db.Armchair
                .Select(a => new
                {
                    a.armchair_id,
                    a.columns,
                    a.rows,
                    a.user_id,
                    a.establish_id
                })
                .ToListAsync();

            return Ok(armchairs);
        }

        // GET: api/Armchair/{id}
        [HttpGet]
        [Route("api/Armchair/{id:int}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetArmchair(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var armchair = await db.Armchair
                .Where(a => a.armchair_id == id)
                .Select(a => new
                {
                    a.armchair_id,
                    a.columns,
                    a.rows,
                    a.user_id,
                    a.establish_id
                })
                .FirstOrDefaultAsync();

            if (armchair == null)
                return NotFound();

            return Ok(armchair);
        }

        // GET: api/Armchair/establishment/{establish_id}
        [HttpGet]
        [Route("api/Armchair/establishment/{establish_id:int}")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetArmchairsByEstablishment(int establish_id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var armchairs = await db.Armchair
                .Where(a => a.establish_id == establish_id)
                .Select(a => new
                {
                    a.armchair_id,
                    a.columns,
                    a.rows,
                    a.user_id,
                    a.establish_id
                })
                .ToListAsync();

            if (armchairs == null || !armchairs.Any())
                return NotFound();

            return Ok(armchairs);
        }   

        // POST: api/Armchair
        [HttpPost]
        [Route("api/Armchair")]
        [ResponseType(typeof(Armchair))]
        public async Task<IHttpActionResult> PostArmchair(Armchair armchair)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Armchair.Add(armchair);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = armchair.armchair_id }, armchair);
        }

        // PUT: api/Armchair/{id}
        [HttpPut]
        [Route("api/Armchair/{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutArmchair(int id, Armchair armchair)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != armchair.armchair_id)
                return BadRequest("ID mismatch");

            var existing = await db.Armchair.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.columns = armchair.columns;
            existing.rows = armchair.rows;
            existing.user_id = armchair.user_id;
            existing.establish_id = armchair.establish_id;

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

        // DELETE: api/Armchair/{id}
        [HttpDelete]
        [Route("api/Armchair/{id:int}")]
        [ResponseType(typeof(Armchair))]
        public async Task<IHttpActionResult> DeleteArmchair(int id)
        {
            var armchair = await db.Armchair.FindAsync(id);
            if (armchair == null)
                return NotFound();

            db.Armchair.Remove(armchair);
            await db.SaveChangesAsync();

            return Ok(armchair);
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