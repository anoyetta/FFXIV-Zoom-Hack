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

            this.StateChanged += (_, e) =>
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.ViewModel.HideCommand.Execute();
                }
                else
                {
                    this.ViewModel.ShowCommand.Execute();
                }
            };

            this.ViewModel.MainView = this;

            this.ShowInTaskbar = false;
            this.WindowState = WindowState.Minimized;

            this.Loaded += (_, __) =>
            {
                this.ShowInTaskbar = true;
                this.ViewModel.HideCommand.Execute();
            };
        }

        public MainViewModel ViewModel => this.DataContext as MainViewModel;
    }
}
