using AnonymizationTool.Data;
using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Data.SchILD;
using AnonymizationTool.Messages;
using AnonymizationTool.Settings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using static AnonymizationTool.Settings.IEmailSettings;

namespace AnonymizationTool.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Properties

        private ISettings settings;

        public ISettings Settings
        {
            get { return settings; }
            set { Set(() => Settings, ref settings, value); }
        }

        private bool isSaving;

        public bool IsSaving
        {
            get { return isSaving; }
            set
            {
                Set(() => IsSaving, ref isSaving, value);
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool isConnection;

        /// <summary>
        /// Flag whether we are currently connecting to our internal database
        /// </summary>
        public bool IsConnecting
        {
            get { return isConnection; }
            set
            {
                Set(() => IsConnecting, ref isConnection, value);
                ConnectDataSourceCommand?.RaiseCanExecuteChanged();
                ConnectSchILDCommand?.RaiseCanExecuteChanged();
            }
        }

        public List<DatabaseType> SchILDDatabaseTypes { get; } = new List<DatabaseType>();

        public List<DatabaseType> InternalDatabaseTypes { get; } = new List<DatabaseType>();

        public List<AnonymizationType> AnonymizationTypes { get; } = new List<AnonymizationType>();

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        #endregion

        #region Commands

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand ConnectSchILDCommand { get; private set; }

        public RelayCommand ConnectDataSourceCommand { get; private set; }

        #endregion

        #region Services

        private readonly IPersistentDataSource persistentDataSource;
        private readonly ISchILDDataSource schILDDataSource;
        private readonly ISettingsService settingsService;

        #endregion

        public SettingsViewModel(IPersistentDataSource persistentDataSource, ISchILDDataSource schILDDataSource, ISettingsService settingsService, IMessenger messenger)
            : base(messenger)
        {
            this.persistentDataSource = persistentDataSource;
            this.schILDDataSource = schILDDataSource;
            this.settingsService = settingsService;

            SaveCommand = new RelayCommand(Save, CanSave);
            ConnectSchILDCommand = new RelayCommand(ConnectSchILD, CanConnectSchILD);
            ConnectDataSourceCommand = new RelayCommand(ConnectDataSource, CanConnectDataSource);

            AddDatabaseTypes();
            AddAnonymizationTypes();

            LoadSettings();
        }

        private async void ConnectSchILD()
        {
            try
            {
                IsConnecting = true;

                await schILDDataSource.TestConnectionAsync();

                Messenger.Send(new DialogMessage { Title = "Verbindung erfolgreich", Header = "Verbindung erfolgreich", Text = "Es wurde erfolgreich eine Verbindung zur SchILD-Datenbank aufgebaut" });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Verbindung nicht erfolgreich", Header = "Fehler beim Verbinden", Text = "Es konnte keine Verbindung zur SchILD-Datenbank aufgebaut werden. Bitte die Verbindungszeichenfolge überprüfen." });
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private bool CanConnectSchILD()
        {
            return IsConnecting == false && schILDDataSource.CanConnect;
        }

        private async void ConnectDataSource()
        {
            try
            {
                IsConnecting = true;
                await persistentDataSource.ConnectAsync();

                Messenger.Send(new DialogMessage { Title = "Verbindung erfolgreich", Header = "Verbindung erfolgreich", Text = "Es wurde erfolgreich eine Verbindung zur Datenbank aufgebaut" });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Verbindung nicht erfolgreich", Header = "Fehler beim Verbinden", Text = "Es konnte keine Verbindung zur Datenbank aufgebaut werden. Bitte die Verbindungszeichenfolge überprüfen." });
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private bool CanConnectDataSource()
        {
            return IsConnecting == false && persistentDataSource.CanConnect;
        }

        private void LoadSettings()
        {
            Settings = settingsService.Settings;
        }

        private void AddDatabaseTypes()
        {
            SchILDDatabaseTypes.Clear();
            InternalDatabaseTypes.Clear();

            foreach(var type in Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>())
            {
                if(persistentDataSource.IsSupported(type))
                {
                    InternalDatabaseTypes.Add(type);
                }

                if(schILDDataSource.IsSupported(type))
                {
                    SchILDDatabaseTypes.Add(type);
                }
            }
        }

        private void AddAnonymizationTypes()
        {
            AnonymizationTypes.Clear();
            AnonymizationTypes.AddRange(Enum.GetValues(typeof(AnonymizationType)).Cast<AnonymizationType>());
        }

        private async void Save()
        {
            try
            {
                IsSaving = true;
                await settingsService.SaveAsync();
                ConnectDataSourceCommand?.RaiseCanExecuteChanged();
                ConnectSchILDCommand?.RaiseCanExecuteChanged();
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool CanSave()
        {
            return !IsSaving;
        }
    }
}
