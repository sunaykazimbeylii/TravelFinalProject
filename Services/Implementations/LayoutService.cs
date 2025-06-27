using TravelFinalProject.DAL;

namespace TravelFinalProject.Services.Implementations
{
    public class LayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }
        public void GetSettings()
        {

        }
    }
}
