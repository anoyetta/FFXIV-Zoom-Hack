using FFXIVZoomHack.WPF;
using Nancy;

namespace FFXIVZoomHack
{
    public class ZoomHackNancyModule : NancyModule
    {
        public ZoomHackNancyModule()
        {
            this.Get("/apply", async _ =>
            {
                await MainViewModel.Current?.ApplyChangesAsync();
                return null;
            });
        }
    }
}
