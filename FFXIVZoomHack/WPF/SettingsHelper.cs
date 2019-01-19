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

        private Settings settings = Settings.Load();

        public void Save() => this.settings?.Save();

        public bool AutoApply
        {
            get => this.settings.AutoApply;
            set => this.SetProperty(
                x => this.settings.AutoApply = x,
                this.settings.AutoApply,
                value);
        }

        public float DesiredFov
        {
            get => this.settings.DesiredFov;
            set => this.SetProperty(
                x => this.settings.DesiredFov = x,
                this.settings.DesiredFov,
                value);
        }

        public float DesiredZoom
        {
            get => this.settings.DesiredZoom;
            set => this.SetProperty(
                x => this.settings.DesiredZoom = x,
                this.settings.DesiredZoom,
                value);
        }

        public int[] DX9_StructureAddress
        {
            get => this.settings.DX9_StructureAddress;
            set => this.SetProperty(
                x => this.settings.DX9_StructureAddress = x,
                this.settings.DX9_StructureAddress,
                value);
        }

        public int DX9_ZoomCurrent
        {
            get => this.settings.DX9_ZoomCurrent;
            set => this.SetProperty(
                x => this.settings.DX9_ZoomCurrent = x,
                this.settings.DX9_ZoomCurrent,
                value);
        }

        public int DX9_ZoomMax
        {
            get => this.settings.DX9_ZoomMax;
            set => this.SetProperty(
                x => this.settings.DX9_ZoomMax = x,
                this.settings.DX9_ZoomMax,
                value);
        }

        public int DX9_FovCurrent
        {
            get => this.settings.DX9_FovCurrent;
            set => this.SetProperty(
                x => this.settings.DX9_FovCurrent = x,
                this.settings.DX9_FovCurrent,
                value);
        }

        public int DX9_FovMax
        {
            get => this.settings.DX9_FovMax;
            set => this.SetProperty(
                x => this.settings.DX9_FovMax = x,
                this.settings.DX9_FovMax,
                value);
        }

        public int[] DX11_StructureAddress
        {
            get => this.settings.DX11_StructureAddress;
            set => this.SetProperty(
                x => this.settings.DX11_StructureAddress = x,
                this.settings.DX11_StructureAddress,
                value);
        }

        public int DX11_ZoomCurrent
        {
            get => this.settings.DX11_ZoomCurrent;
            set => this.SetProperty(
                x => this.settings.DX11_ZoomCurrent = x,
                this.settings.DX11_ZoomCurrent,
                value);
        }

        public int DX11_ZoomMax
        {
            get => this.settings.DX11_ZoomMax;
            set => this.SetProperty(
                x => this.settings.DX11_ZoomMax = x,
                this.settings.DX11_ZoomMax,
                value);
        }

        public int DX11_FovCurrent
        {
            get => this.settings.DX11_FovCurrent;
            set => this.SetProperty(
                x => this.settings.DX11_FovCurrent = x,
                this.settings.DX11_FovCurrent,
                value);
        }

        public int DX11_FovMax
        {
            get => this.settings.DX11_FovMax;
            set => this.SetProperty(
                x => this.settings.DX11_FovMax = x,
                this.settings.DX11_FovMax,
                value);
        }

        public string OffsetUpdateLocation
        {
            get => this.settings.OffsetUpdateLocation;
            set => this.SetProperty(
                x => this.settings.OffsetUpdateLocation = x,
                this.settings.OffsetUpdateLocation,
                value);
        }

        public string LastUpdate
        {
            get => this.settings.LastUpdate;
            set => this.SetProperty(
                x => this.settings.LastUpdate = x,
                this.settings.LastUpdate,
                value);
        }

        private bool SetProperty<T>(
            Action<T> set,
            T oldValue,
            T newValue)
        {
            if (object.Equals(oldValue, newValue))
            {
                return false;
            }

            set(newValue);

            return true;
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
