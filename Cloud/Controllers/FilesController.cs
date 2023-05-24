using Cloud.Data;
using Cloud.Data.Tables;
using Cloud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        //private string SaveText(string text)//TODO in database
        //{
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        string uniqueFileName = Guid.NewGuid().ToString() + ".txt";
        //        string filePath = Path.Combine(_pathFolder, uniqueFileName);
        //        var file = System.IO.File.Create(filePath);
        //        using (var sw = new StreamWriter(file))
        //        {
        //            sw.WriteLine(text);
        //        }
        //        return filePath;
        //    }
        //    return string.Empty;
        //}

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadModel model)
        {
            string fileUrl = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(model.Text) && model.File == null)
                {
                    throw new Exception("Doesn't exist");
                }
                UserUploades? userUpload = new UserUploades { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) };
                if (model.File != null)
                {
                    string fileName = SaveFile(model.File);
                    userUpload.Filename = fileName;
                    fileUrl = Url.Action("Download", "Files", new { fileName = fileName }, Request.Scheme) ?? throw new Exception("Bad url");
                    
                }
                if (!string.IsNullOrEmpty(model.Text))
                {
                    userUpload.Text = model.Text;
                }
                userUpload.DeleteAfterDownload = model.DeletedAfterDownload; userUpload.PageId = Guid.NewGuid().ToString(); userUpload.FileUrl = fileUrl;

                _context.Uploads.Add(userUpload);
                await _context.SaveChangesAsync();
                return Redirect($"/api/files/upload?pageId={userUpload.PageId}");//6ddf0485-c67a-473f-9a5f-8be3d4aef0b5
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet("upload")]
        public async Task<IActionResult> Upload(string pageId)
        {
            var model = await _context.Uploads.FirstOrDefaultAsync(x => x.PageId == pageId);
            if (model != null)
            {
                return View(new DownloadModel {PageId=model.PageId,Text=model.Text,FileUrl=model.FileUrl });
            }
            return NotFound();
        }

        //[Authorize]
        //[HttpGet("history")]
        //public IEnumerable<string> History() => _context.Uploads.Where(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).Select(x => x.Filename);

        [Authorize]
        [HttpGet("history")]
        public IEnumerable<string> History()
        {
            IEnumerable<string> historyFiles = _context.Uploads
                .Where(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Where(x=>x.Filename != null)
                .Select(x => x.PageId + "_" + x.Filename);//Display the file name in History

            IEnumerable<string> historyText = _context.Uploads
                .Where(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Where(x =>x.Text!=null&&x.Filename == null)
                .Select(x =>x.PageId+"_"+x.Text);//Display the text name in History
            return historyFiles.Take(historyFiles.Count()).Concat(historyText.Take(historyText.Count()));
        }

        [HttpGet("download")]
        public IActionResult Download(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_pathFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    throw new Exception("File not found");
                }
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var file = _context.Uploads.Where(x => x.Filename == fileName).FirstOrDefault();
                if (file.DeleteAfterDownload)
                {
                    System.IO.File.Delete(filePath);
                    _context.Remove(file);
                    _context.SaveChangesAsync();
                }
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            } 
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string pageId)
        {
            var userUpload = await _context.Uploads.FirstOrDefaultAsync(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && x.PageId == pageId);
            if (userUpload == null)
            {
                return NotFound();
            }

            // delete file from server
            if(!string.IsNullOrEmpty(userUpload.Filename))
            {
                var filePath = Path.Combine(_pathFolder, userUpload.Filename);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            // delete from base
            _context.Remove(userUpload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}