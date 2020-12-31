using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Nancy.Hosting.Self;

namespace FFXIVZoomHack.WPF
{
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window
    {
        private NancyHost nancy;

        public MainView()
        {
            this.InitializeComponent();

            this.Closing += (_, __) =>
            {
                if (this.nancy != null)
                {
                    this.nancy.Stop();
                    this.nancy.Dispose();
                }

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

                this.nancy = new NancyHost(
                    new Uri(Properties.Settings.Default.RESTApiUri));
                this.nancy.Start();
            };
        }

        public void ShowMessage(
            string message)
        {
            this.MessageLabel.Content = message;

            Dispatcher.BeginInvoke((Action)(async () =>
            {
                var durations = 100 * message.Length;

                await Task.Delay(TimeSpan.FromMilliseconds(durations));
                this.MessageLabel.Content = string.Empty;
            }));
        }

        public MainViewModel ViewModel => this.DataContext as MainViewModel;
    }
}
