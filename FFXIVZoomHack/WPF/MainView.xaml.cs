using System.Windows;

namespace FFXIVZoomHack.WPF
{
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            this.InitializeComponent();

            this.Closing += (_, __) =>
            {
                this.ViewModel?.Dispose();
            };
        }

        public MainViewModel ViewModel => this.DataContext as MainViewModel;
    }
}
