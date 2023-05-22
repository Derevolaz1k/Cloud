using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Data.Tables
{
    public class UserFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string UserId { get; set; }
        public bool Deleted { get; set; }
        public bool DeletedAfterDownload { get; set; }
    }
}
