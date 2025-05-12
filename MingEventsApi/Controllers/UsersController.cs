using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MingEventsApi.Models;
using System.Linq;
using System.Net.Http;

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
            existing.photo = user.photo;
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
        [HttpPost]
        [Route("api/Users/{id:int}/UploadProfilePicture")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UploadProfilePicture(int id)
        {
            // Verifica si la petición es multipart
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("No es una petición multipart válida.");

            var user = await db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Ruta física donde se guardará la imagen
            var root = System.Web.Hosting.HostingEnvironment.MapPath("~//Images/ProfilePictures");
            if (!System.IO.Directory.Exists(root))
                System.IO.Directory.CreateDirectory(root);

            // Usa el espacio de nombres correcto
            var provider = new System.Net.Http.MultipartFormDataStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                // Solo se espera un archivo
                var file = provider.FileData.FirstOrDefault();
                if (file == null)
                    return BadRequest("No se recibió ningún archivo.");

                // Obtiene la extensión original
                var originalFileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var extension = System.IO.Path.GetExtension(originalFileName);

                // Nuevo nombre: id del usuario + extensión
                var newFileName = id + extension;
                var newFilePath = System.IO.Path.Combine(root, newFileName);

                // Si ya existe una imagen previa, elimínala
                if (System.IO.File.Exists(newFilePath))
                    System.IO.File.Delete(newFilePath);

                // Renombra el archivo subido
                System.IO.File.Move(file.LocalFileName, newFilePath);

                // Guarda la URL relativa en el campo photo
                user.photo = $"/Images/ProfilePictures/{newFileName}";
                await db.SaveChangesAsync();

                return Ok(new { photoUrl = user.photo });
            }
            catch
            {
                return InternalServerError();
            }
        }
        [HttpGet]
        [Route("api/Users/{id:int}/ProfilePicture")]
        [ResponseType(typeof(void))]
        public IHttpActionResult GetProfilePicture(int id)
        {
            var user = db.Users.Find(id);
            if (user == null || string.IsNullOrEmpty(user.photo))
                return NotFound();

            // Ruta física del archivo
            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~" + user.photo);

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

    }
}
