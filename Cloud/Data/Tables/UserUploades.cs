using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Data.Tables
{
    public class UserUploades
    {
        [Key]
        public int Id { get; set; }
        public string PageId { get; set; }
        public string? Filename { get; set; }
        public string? Text { get; set; }
        public string UserId { get; set; }
        public bool DeleteAfterDownload { get; set; }//TODO create enum contitions(status)
        public string? FileUrl { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
