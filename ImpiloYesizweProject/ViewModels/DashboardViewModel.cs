using ImpiloYesizweProject.Models;

namespace ImpiloYesizweProject.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMessages { get; set; }

        public int TotalDonations { get; set; }

        public int TotalGalleryImages { get; set; }

        // NEW
        public List<ContactMessage> RecentMessages { get; set; } = new();

        public List<Donation> RecentDonations { get; set; } = new();
        public decimal TotalDonationAmount { get; set; }
        
    }
}