namespace ImpiloYesizweProject.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public DateTime DateUploaded { get; set; } = DateTime.Now;
    }
}