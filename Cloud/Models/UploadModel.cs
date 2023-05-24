using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Models
{
    public class UploadModel
    {
        public IFormFile? File { get; set; }

        public string? Text { get; set; }

        public bool DeletedAfterDownload { get; set; }
        
    }
}
