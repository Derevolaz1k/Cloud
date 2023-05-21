using Microsoft.EntityFrameworkCore;

namespace Cloud.Data.Tables
{
    public class UserFiles
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public int UserId { get; set; }
        public bool Deleted { get; set; }
        public bool DeletedAfterDownload { get; set; }
    }
}
