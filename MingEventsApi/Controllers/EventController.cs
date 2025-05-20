using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MingEventsApi.Models;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;

namespace MingEventsApi.Controllers
{
    public class EventController : ApiController
    {
        private MingsEventsEntities1 db = new MingsEventsEntities1();

        // GET: api/Event
        [HttpGet]
        [Route("api/Event")]
        [ResponseType(typeof(IEnumerable<object>))]
        public async Task<IHttpActionResult> GetEvent()
        {
            db.Configuration.LazyLoadingEnabled = false;

            var events = await db.Event
                .Select(e => new
                {
                    e.event_id,
                    e.name,
                    e.price,
                    e.reserved_places,
                    e.photo,
                    e.start_date,
                    e.end_date,
                    e.seating,
                    e.descripcion,
                    e.establish_id
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Event/{id}
        [HttpGet]
        [Route("api/Event/{id}")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> GetEvent(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var ev = await db.Event
                .Where(e => e.event_id == id)
                .Select(e => new
                {
                    e.event_id,
                    e.name,
                    e.price,
                    e.reserved_places,
                    e.photo,
                    e.start_date,
                    e.end_date,
                    e.seating,
                    e.descripcion,
                    e.establish_id
                })
                .FirstOrDefaultAsync();

            if (ev == null)
            {
                return NotFound();
            }

            return Ok(ev);
        }

        // POST: api/Event
        [HttpPost]
        [Route("api/Event")]
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> PostEvent(Event ev)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Event.Add(ev);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = ev.event_id }, ev);
        }

        // PUT: api/Event/{id}
        [HttpPut]
        [Route("api/Event/{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEvent(int id, Event ev)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != ev.event_id)
                return BadRequest("ID mismatch");

            var existing = await db.Event.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualiza solo los campos modificables
            existing.name = ev.name;
            existing.price = ev.price;
            existing.reserved_places = ev.reserved_places;
            existing.photo = ev.photo;
            existing.start_date = ev.start_date;
            existing.end_date = ev.end_date;
            existing.seating = ev.seating;
            existing.descripcion = ev.descripcion;
            existing.establish_id = ev.establish_id;

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

        // DELETE: api/Event/{id}
        [HttpDelete]
        [Route("api/Event/{id}")]
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> DeleteEvent(int id)
        {
            var ev = await db.Event.FindAsync(id);
            if (ev == null)
                return NotFound();

            db.Event.Remove(ev);
            await db.SaveChangesAsync();

            return Ok(ev);
        }

        // POST: api/Event/{id}/UploadPhoto
        [HttpPost]
        [Route("api/Event/{id:int}/UploadPhoto")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UploadPhoto(int id)
        {
            // Verifica si la petición es multipart
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("No es una petición multipart válida.");

            var ev = await db.Event.FindAsync(id);
            if (ev == null)
                return NotFound();

            // Ruta física donde se guardará la imagen
            var root = System.Web.Hosting.HostingEnvironment.MapPath("~//Images/EventPhotos");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                // Solo se espera un archivo
                var file = provider.FileData.FirstOrDefault();
                if (file == null)
                    return BadRequest("No se recibió ningún archivo.");

                // Obtiene la extensión original
                var originalFileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var extension = Path.GetExtension(originalFileName);

                // Nuevo nombre: id del evento + extensión
                var newFileName = id + extension;
                var newFilePath = Path.Combine(root, newFileName);

                // Si ya existe una imagen previa, elimínala
                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);

                // Renombra el archivo subido
                File.Move(file.LocalFileName, newFilePath);

                // Guarda la URL relativa en el campo photo
                ev.photo = $"/Images/EventPhotos/{newFileName}";
                await db.SaveChangesAsync();

                return Ok(new { photoUrl = ev.photo });
            }
            catch
            {
                return InternalServerError();
            }
        }

        // GET: api/Event/{id}/Photo
        [HttpGet]
        [Route("api/Event/{id:int}/Photo")]
        [ResponseType(typeof(void))]
        public IHttpActionResult GetPhoto(int id)
        {
            var ev = db.Event.Find(id);
            if (ev == null || string.IsNullOrEmpty(ev.photo))
                return NotFound();

            // Ruta física del archivo
            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~" + ev.photo);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            // Obtiene el tipo MIME según la extensión
            var extension = System.IO.Path.GetExtension(filePath).ToLower();
            string mimeType;
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                default:
                    mimeType = "application/octet-stream";
                    break;
            }

            var result = new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StreamContent(new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            };
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

            // Devuelve el archivo como respuesta HTTP
            return ResponseMessage(result);
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