using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public DateTime DateUploaded { get; set; } = DateTime.Now;
    }
}