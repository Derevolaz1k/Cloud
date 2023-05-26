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
        /// <summary>
        /// Saving a file and assigning a unique name
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Unique name</returns>
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
        /// <summary>
        /// Creates a user upload object
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Redirect on view</returns>
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
                userUpload.DeleteAfterView = model.DeletedAfterDownload; userUpload.PageId = Guid.NewGuid().ToString(); userUpload.FileUrl = fileUrl;

                _context.Uploads.Add(userUpload);
                await _context.SaveChangesAsync();
                return Redirect($"/api/files/upload?pageId={userUpload.PageId}");
            }
            catch
            {
                return BadRequest();
            }
        }
        /// <summary>
        /// Checking for the contents of the user upload object in the database.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns>Showing the view.</returns>
        [HttpGet("upload")]
        public async Task<IActionResult> Upload(string pageId)
        {
            var userUpload = await _context.Uploads.FirstOrDefaultAsync(x => x.PageId == pageId);
            if (userUpload != null)
            {
                if(userUpload.DeleteAfterView)
                {
                    if(userUpload.ViewsCounter>1)
                    {
                        await Delete(userUpload.PageId);
                        return NotFound();
                    }
                }
                userUpload.ViewsCounter++;
                await _context.SaveChangesAsync();
                return View(new DownloadModel {PageId=userUpload.PageId,Text=userUpload.Text,FileUrl=userUpload.FileUrl });
            }
            return NotFound();
        }
        /// <summary>
        /// All user uploads.
        /// </summary>
        /// <returns>Filenames, texts and id pages in format idPage_text(idPage_filename).</returns>
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
        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("download")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_pathFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    throw new Exception("File not found");
                }
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var userUpload = _context.Uploads.Where(x => x.Filename == fileName).FirstOrDefault();
                if (userUpload.DeleteAfterView)
                {
                    await Delete(userUpload.PageId);
                }
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch(Exception ex) 
            {
                return NotFound("File not found:(");
            } 
        }
        /// <summary>
        /// Deleting a database entry and file
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [Authorize]
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