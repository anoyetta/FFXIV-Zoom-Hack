using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FFXIVZoomHack.WPF
{
    public class SettingsHelper :
        INotifyPropertyChanged
    {
        #region Singleton

        private static SettingsHelper instance;

        public static SettingsHelper Instance =>
            instance ?? (instance = new SettingsHelper());

        private SettingsHelper()
        {
        }

        #endregion Singleton

        public Settings InnerSettings => this.LazySettings.Value;

        private readonly Lazy<Settings> LazySettings = new Lazy<Settings>(() => Settings.Load());

        public void Save() => this.LazySettings.Value?.Save();

        public bool AutoApply
        {
            get => this.LazySettings.Value.AutoApply;
            set => this.SetProperty(
                x => this.LazySettings.Value.AutoApply = x,
                this.LazySettings.Value.AutoApply,
                value);
        }

        public bool AutoQuit
        {
            get => this.LazySettings.Value.AutoQuit;
            set => this.SetProperty(
                x => this.LazySettings.Value.AutoQuit = x,
                this.LazySettings.Value.AutoQuit,
                value);
        }

        public float DesiredFov
        {
            get => this.LazySettings.Value.DesiredFov;
            set => this.SetProperty(
                x => this.LazySettings.Value.DesiredFov = x,
                this.LazySettings.Value.DesiredFov,
                value);
        }

        public float DesiredZoom
        {
            get => this.LazySettings.Value.DesiredZoom;
            set => this.SetProperty(
                x => this.LazySettings.Value.DesiredZoom = x,
                this.LazySettings.Value.DesiredZoom,
                value);
        }

        public int[] DX9_StructureAddress
        {
            get => this.LazySettings.Value.DX9_StructureAddress;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX9_StructureAddress = x,
                this.LazySettings.Value.DX9_StructureAddress,
                value);
        }

        public int DX9_ZoomCurrent
        {
            get => this.LazySettings.Value.DX9_ZoomCurrent;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX9_ZoomCurrent = x,
                this.LazySettings.Value.DX9_ZoomCurrent,
                value);
        }

        public int DX9_ZoomMax
        {
            get => this.LazySettings.Value.DX9_ZoomMax;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX9_ZoomMax = x,
                this.LazySettings.Value.DX9_ZoomMax,
                value);
        }

        public int DX9_FovCurrent
        {
            get => this.LazySettings.Value.DX9_FovCurrent;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX9_FovCurrent = x,
                this.LazySettings.Value.DX9_FovCurrent,
                value);
        }

        public int DX9_FovMax
        {
            get => this.LazySettings.Value.DX9_FovMax;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX9_FovMax = x,
                this.LazySettings.Value.DX9_FovMax,
                value);
        }

        public int[] DX11_StructureAddress
        {
            get => this.LazySettings.Value.DX11_StructureAddress;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX11_StructureAddress = x,
                this.LazySettings.Value.DX11_StructureAddress,
                value);
        }

        public int DX11_ZoomCurrent
        {
            get => this.LazySettings.Value.DX11_ZoomCurrent;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX11_ZoomCurrent = x,
                this.LazySettings.Value.DX11_ZoomCurrent,
                value);
        }

        public int DX11_ZoomMax
        {
            get => this.LazySettings.Value.DX11_ZoomMax;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX11_ZoomMax = x,
                this.LazySettings.Value.DX11_ZoomMax,
                value);
        }

        public int DX11_FovCurrent
        {
            get => this.LazySettings.Value.DX11_FovCurrent;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX11_FovCurrent = x,
                this.LazySettings.Value.DX11_FovCurrent,
                value);
        }

        public int DX11_FovMax
        {
            get => this.LazySettings.Value.DX11_FovMax;
            set => this.SetProperty(
                x => this.LazySettings.Value.DX11_FovMax = x,
                this.LazySettings.Value.DX11_FovMax,
                value);
        }

        public string OffsetUpdateLocation
        {
            get => this.LazySettings.Value.OffsetUpdateLocation;
            set => this.SetProperty(
                x => this.LazySettings.Value.OffsetUpdateLocation = x,
                this.LazySettings.Value.OffsetUpdateLocation,
                value);
        }

        public string LastUpdate
        {
            get => this.LazySettings.Value.LastUpdate;
            set => this.SetProperty(
                x => this.LazySettings.Value.LastUpdate = x,
                this.LazySettings.Value.LastUpdate,
                value);
        }

        private bool SetProperty<T>(
            Action<T> set,
            T oldValue,
            T newValue,
            [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(oldValue, newValue))
            {
                return false;
            }

            set(newValue);

            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
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
