using Cloud.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Cloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private string _pathFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles");
        private string SaveFile(IFormFile file)
        {
            if (!Path.Exists(_pathFolder))
            {
                Directory.CreateDirectory(_pathFolder);
            }
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(_pathFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return uniqueFileName;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string fileUrl = string.Empty;
            try
            {
                fileUrl = Url.Action("Download", "Files", new { fileName = SaveFile(file) }, Request.Scheme) ?? throw new Exception("Bad url");
            }
            catch
            {
                return BadRequest();
            }
            return Ok(fileUrl);
        }

        [HttpGet("download")]
        public IActionResult Download(string fileName)
        {
            string filePath = Path.Combine(_pathFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // DELETE api/<UploadController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        
    }
}
