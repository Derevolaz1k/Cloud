using Cloud.Data;
using Cloud.Data.Tables;
using Cloud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FilesController(ApplicationDbContext context)
        {
            _context = context;
        }
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

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm]UploadModel model)
        {
            string fileUrl = string.Empty;
            try
            {
                string fileName = SaveFile(model.File);
                UserFile userFile = new UserFile { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), Filename = fileName, Deleted = false, DeletedAfterDownload = model.DeletedAfterDownload };
                fileUrl = Url.Action("Download", "Files", new { fileName = fileName }, Request.Scheme) ?? throw new Exception("Bad url");
                _context.Files.Add(userFile);
                await _context.SaveChangesAsync();
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
            var file = _context.Files.Where(x=>x.Filename==fileName).FirstOrDefault();
            if (file.DeletedAfterDownload)
            {
                System.IO.File.Delete(filePath);
                _context.Entry(file).Property(x=>x.Deleted).CurrentValue=true;
                _context.SaveChangesAsync();
            }
            return File(fileBytes, "application/octet-stream", fileName);
        }        
    }
}
