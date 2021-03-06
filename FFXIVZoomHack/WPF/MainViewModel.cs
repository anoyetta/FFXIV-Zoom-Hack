using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;

namespace FFXIVZoomHack.WPF
{
    public class MainViewModel :
        INotifyPropertyChanged,
        IDisposable
    {
        public static MainViewModel Current;

        public MainViewModel()
        {
            Current = this;

            this.Config.PropertyChanged += async (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.DesiredZoom):
                    case nameof(Settings.DesiredFov):
                        lock (this)
                        {
                            this.Config.Save();
                        }

                        await this.ApplyChangesAsync();
                        break;
                }
            };

            this.thread = new Thread(new ThreadStart(this.DoWork))
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
            };

            this.thread.Start();
        }

        public void Dispose()
        {
            if (this.thread != null)
            {
                this.thread.Abort();
                this.thread = null;
            }
        }

        public MainView MainView { get; set; }

        public SettingsHelper Config => SettingsHelper.Instance;

        public ObservableCollection<int> PIDList { get; } = new ObservableCollection<int>();

        private int pid;

        public int PID
        {
            get => this.pid;
            set => this.SetProperty(ref this.pid, value);
        }

        private ICommand setDefaultUpdateLocationCommand;

        public ICommand SetDefaultUpdateLocationCommand =>
            this.setDefaultUpdateLocationCommand ?? (this.setDefaultUpdateLocationCommand = new DelegateCommand(() =>
            {
                this.Config.OffsetUpdateLocation = @"https://raw.githubusercontent.com/jayotterbein/FFXIV-Zoom-Hack/master/Offsets.xml";
            }));

        private ICommand updateOffsetCommand;

        public ICommand UpdateOffsetCommand =>
            this.updateOffsetCommand ?? (this.updateOffsetCommand = new DelegateCommand(async () => await UpdateOffsetAsync()));

        private ICommand checkAutoApplyCommand;

        public ICommand CheckAutoApplyCommand =>
            this.checkAutoApplyCommand ?? (this.checkAutoApplyCommand = new DelegateCommand(async () =>
            {
                if (this.Config.AutoApply)
                {
                    await this.ApplyChangesAsync();
                }
            }));

        private ICommand setDefaultCommand;

        public ICommand SetDefaultCommand =>
            this.setDefaultCommand ?? (this.setDefaultCommand = new DelegateCommand(async () =>
            {
                this.Config.DesiredZoom = 20.0f;
                this.Config.DesiredFov = 0.78f;
                this.Config.Save();
                await this.ApplyChangesAsync();
            }));

        private ICommand applyCommand;

        public ICommand ApplyCommand =>
            this.applyCommand ?? (this.applyCommand = new DelegateCommand(async () =>
            {
                await this.ApplyChangesAsync();

                this.MainView.ShowMessage(
                    "The settings have been applied.");
            }));

        private DelegateCommand showCommand;

        public DelegateCommand ShowCommand =>
            this.showCommand ?? (this.showCommand = new DelegateCommand(this.ExecuteShowCommand));

        private void ExecuteShowCommand()
        {
            this.MainView.Show();
            this.MainView.WindowState = WindowState.Normal;
            this.MainView.NotifyIcon.Visibility = Visibility.Collapsed;

            this.MainView.Activate();
        }

        private DelegateCommand hideCommand;

        public DelegateCommand HideCommand =>
            this.hideCommand ?? (this.hideCommand = new DelegateCommand(this.ExecuteHideCommand));

        private void ExecuteHideCommand()
        {
            this.MainView.NotifyIcon.Visibility = Visibility.Visible;
            this.MainView.Hide();
        }

        private DelegateCommand exitCommand;

        public DelegateCommand ExitCommand =>
            this.exitCommand ?? (this.exitCommand = new DelegateCommand(() => this.MainView?.Close()));

        public async Task ApplyChangesAsync(IEnumerable<int> pids = null) => await Task.Run(() =>
        {
            const bool isOnlyMax = true;

            foreach (var pid in (pids ?? this.PIDList))
            {
                Memory.Apply(this.Config.InnerSettings, pid, isOnlyMax);
                Thread.Sleep(1);
            }
        });

        private async Task<Settings> GetOffsetsAsync()
        {
            var temp = Path.GetTempFileName();
            File.Delete(temp);

            using (var web = new WebClient())
            {
                await web.DownloadFileTaskAsync(this.Config.OffsetUpdateLocation, temp);
            }

            var offsets = Settings.Load(temp);

            return offsets;
        }

        private async Task UpdateOffsetAsync()
        {
            try
            {
                var offsets = await this.GetOffsetsAsync();

                this.Config.DX11_StructureAddress = offsets.DX11_StructureAddress;
                this.Config.DX11_ZoomCurrent = offsets.DX11_ZoomCurrent;
                this.Config.DX11_ZoomMax = offsets.DX11_ZoomMax;
                this.Config.DX11_FovCurrent = offsets.DX11_FovCurrent;
                this.Config.DX11_FovMax = offsets.DX11_FovMax;
                this.Config.DX9_StructureAddress = offsets.DX9_StructureAddress;
                this.Config.DX9_ZoomCurrent = offsets.DX9_ZoomCurrent;
                this.Config.DX9_ZoomMax = offsets.DX9_ZoomMax;
                this.Config.DX9_FovCurrent = offsets.DX9_FovCurrent;
                this.Config.DX9_FovMax = offsets.DX9_FovMax;
                this.Config.LastUpdate = offsets.LastUpdate;
                this.Config.Save();

                if (this.Config.AutoApply)
                {
                    await this.ApplyChangesAsync();
                }

                this.MainView.ShowMessage(
                    $"Offset has been updated. Updated: {this.Config.LastUpdate}");
            }
            catch (Exception ex)
            {
                this.MainView.ShowMessage(
                    $"An error occurred while updating the offset.");

                MessageBox.Show(
                    Application.Current.MainWindow,
                    "Error: " + ex,
                    "Update Offset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task<bool> TryUpdateOffsetAsync()
        {
            var offsets = await this.GetOffsetsAsync();

            var t1 = DateTime.MinValue.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(this.Config.LastUpdate) &&
                this.Config.LastUpdate.Length >= 10)
            {
                t1 = this.Config.LastUpdate.Substring(0, 10);
            }

            if (string.IsNullOrEmpty(offsets.LastUpdate) ||
                this.Config.LastUpdate.Length < 10)
            {
                return false;
            }

            var t2 = offsets.LastUpdate.Substring(0, 10);

            if (DateTime.TryParse(t1, out DateTime current) &&
                DateTime.TryParse(t2, out DateTime remote))
            {
                if (remote > current)
                {
                    this.Config.DX11_StructureAddress = offsets.DX11_StructureAddress;
                    this.Config.DX11_ZoomCurrent = offsets.DX11_ZoomCurrent;
                    this.Config.DX11_ZoomMax = offsets.DX11_ZoomMax;
                    this.Config.DX11_FovCurrent = offsets.DX11_FovCurrent;
                    this.Config.DX11_FovMax = offsets.DX11_FovMax;
                    this.Config.DX9_StructureAddress = offsets.DX9_StructureAddress;
                    this.Config.DX9_ZoomCurrent = offsets.DX9_ZoomCurrent;
                    this.Config.DX9_ZoomMax = offsets.DX9_ZoomMax;
                    this.Config.DX9_FovCurrent = offsets.DX9_FovCurrent;
                    this.Config.DX9_FovMax = offsets.DX9_FovMax;
                    this.Config.LastUpdate = offsets.LastUpdate;
                    this.Config.Save();

                    return true;
                }
            }

            return false;
        }

        private Thread thread;

        private async void DoWork()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));

            await this.TryUpdateOffsetAsync();

            while (true)
            {
                try
                {
                    var activePIDs = Memory.GetPids().ToArray();
                    var currentPIDs = this.PIDList.ToArray();

                    var newPIDs = activePIDs.Except(currentPIDs).ToArray();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var id in newPIDs)
                        {
                            this.PIDList.Add(id);
                        }

                        var toRemove = this.PIDList
                            .Where(x => !activePIDs.Contains(x))
                            .ToArray();

                        foreach (var id in toRemove)
                        {
                            this.PIDList.Remove(id);
                        }

                        this.PID = this.PIDList.FirstOrDefault();
                    });

                    if (this.Config.AutoApply && newPIDs.Any())
                    {
                        await this.ApplyChangesAsync(newPIDs);
                    }

                    if (!activePIDs.Any() &&
                        currentPIDs.Any() &&
                        this.Config.AutoQuit)
                    {
                        Application.Current.Shutdown(0);
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromSeconds(8));
                }
            }
        }

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }
}
