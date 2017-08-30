using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using LiteDB;

namespace Televic.Mycro.Documents
{
    [RoutePrefix("api/documents")]
    public class DocumentsController : ApiController
    {
        private readonly LiteRepository _repository;

        public DocumentsController(LiteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage GetDocument(string id)
        {
            var fileInfo = _repository.FileStorage.FindById(id);
            
            if (fileInfo != null)
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(fileInfo.OpenRead())
                };
                result.Content.Headers.ContentLength = fileInfo.Length;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(fileInfo.MimeType);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileInfo.Filename
                };
                return result;
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [Route("")]
        [SwaggerForm("Document","Upload a document")]
        public async Task<IHttpActionResult> PostDocument()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();
            var fileData = filesReadToProvider.Contents.FirstOrDefault();

            if (fileData != null)
            {
                var fileName = fileData.Headers.ContentDisposition.FileName?.Trim('"');
                var metaData = _repository.FileStorage.Upload(Guid.NewGuid().ToString(), fileName, await fileData.ReadAsStreamAsync());
                return Ok(metaData);
            }

            return BadRequest();
        }
    }
}
