using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;

namespace FFXIVZoomHack.WPF
{
    public class MainViewModel :
        INotifyPropertyChanged
    {
        private static MainViewModel currentInstance;

        public MainViewModel()
        {
            currentInstance = this;

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
        }

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

                MessageBox.Show(
                    Application.Current.MainWindow,
                    "Completed.",
                    "Apply Settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }));

        private async Task ApplyChangesAsync(IEnumerable<int> pids = null) => await Task.Run(() =>
        {
            foreach (var pid in (pids ?? this.PIDList))
            {
                Memory.Apply(this.Config.InnerSettings, pid);
            }
        });

        private async Task UpdateOffsetAsync()
        {
            try
            {
                var offsets = Settings.Load(this.Config.OffsetUpdateLocation);

                if (string.Equals(this.Config.LastUpdate, offsets.LastUpdate))
                {
                    MessageBox.Show("No new update found");
                    return;
                }

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

                MessageBox.Show(
                    Application.Current.MainWindow,
                    "Updated: " + this.Config.LastUpdate,
                    "Update Offset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Application.Current.MainWindow,
                    "Error: " + ex,
                    "Update Offset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static readonly System.Threading.Timer Timer = new System.Threading.Timer(
            TimerCallback,
            null,
            TimeSpan.FromMilliseconds(100),
            Timeout.InfiniteTimeSpan);

        private static async void TimerCallback(object state)
        {
            try
            {
                var activePIDs = Memory.GetPids().ToArray();
                var currentPIDs = currentInstance.PIDList.ToArray();

                var newPIDs = activePIDs.Except(currentPIDs).ToArray();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var id in newPIDs)
                    {
                        currentInstance.PIDList.Add(id);
                    }

                    var toRemove = currentInstance.PIDList
                        .Where(x => !activePIDs.Contains(x))
                        .ToArray();

                    foreach (var id in toRemove)
                    {
                        currentInstance.PIDList.Remove(id);
                    }

                    currentInstance.PID = currentInstance.PIDList.FirstOrDefault();
                });

                if (currentInstance.Config.AutoApply &&
                    newPIDs.Any())
                {
                    await currentInstance.ApplyChangesAsync(currentInstance.PIDList);
                }
            }
            catch
            {
                /* something went wrong on the background thread, should find a way to log this..*/
            }
            finally
            {
                Timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
            }
        }

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
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
