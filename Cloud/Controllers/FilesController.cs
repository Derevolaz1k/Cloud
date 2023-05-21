using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private string SaveFile(IFormFile file)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine("C:\\Users\\79655\\Desktop\\Новая папка", uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return uniqueFileName;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            try
            {
                using (var stream = System.IO.File.Create(Path.Combine("C:\\Users\\79655\\Desktop\\Новая папка", file.FileName)))
                {
                    await file.CopyToAsync(stream);
                    
                }
            }
            catch(Exception ex) { return BadRequest("Erorr"); }
            string fileUrl = Url.Action("Download", "Files", new { fileName = SaveFile(file) }, Request.Scheme);
            return Ok(fileUrl);
        }

        [HttpGet("download")]
        public IActionResult Download(string fileName)
        {
            string filePath = Path.Combine("C:\\Users\\79655\\Desktop\\Новая папка", fileName);

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
