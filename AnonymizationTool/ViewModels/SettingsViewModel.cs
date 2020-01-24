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

        private bool isConnectionSchILD;

        /// <summary>
        /// Flag whether we are currently connecting to the SchILD database
        /// </summary>
        public bool IsConnectingSchILD
        {
            get { return isConnectionSchILD; }
            set
            {
                Set(() => IsConnectingSchILD, ref isConnectionSchILD, value);

                TestDataSourceConnectionCommand?.RaiseCanExecuteChanged();
                TestSchILDConnectionCommand?.RaiseCanExecuteChanged();
                ConnectDataSourceCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool isConnectionInternal;

        /// <summary>
        /// Flag whether we are currently connecting to the internal database
        /// </summary>
        public bool IsConnectingInternal
        {
            get { return isConnectionSchILD; }
            set
            {
                Set(() => IsConnectingInternal, ref isConnectionInternal, value);

                TestDataSourceConnectionCommand?.RaiseCanExecuteChanged();
                TestSchILDConnectionCommand?.RaiseCanExecuteChanged();
                ConnectDataSourceCommand?.RaiseCanExecuteChanged();
            }
        }

        public List<DatabaseType> SchILDDatabaseTypes { get; } = new List<DatabaseType>();

        public List<DatabaseType> InternalDatabaseTypes { get; } = new List<DatabaseType>();

        public List<AnonymizationType> AnonymizationTypes { get; } = new List<AnonymizationType>();

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        #endregion

        #region Commands

        public RelayCommand TestSchILDConnectionCommand { get; private set; }

        public RelayCommand TestDataSourceConnectionCommand { get; private set; }

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

            TestSchILDConnectionCommand = new RelayCommand(TestConnectSchILD, CanTestConnectSchILD);
            TestDataSourceConnectionCommand = new RelayCommand(TestConnectDataSource, TestCanConnectDataSource);
            ConnectDataSourceCommand = new RelayCommand(ConnectDataSource, CanConnectDataSource);

            AddDatabaseTypes();
            AddAnonymizationTypes();

            LoadSettings();

            settingsService.Changed += delegate
            {
                TestSchILDConnectionCommand?.RaiseCanExecuteChanged();
                TestDataSourceConnectionCommand?.RaiseCanExecuteChanged();
                ConnectDataSourceCommand?.RaiseCanExecuteChanged();
            };
        }

        private async void TestConnectSchILD()
        {
            try
            {
                IsConnectingSchILD = true;

                await schILDDataSource.TestConnectionAsync(settingsService.Settings.SchILDConnection.Type, settingsService.Settings.SchILDConnection.ConnectionString);

                Messenger.Send(new DialogMessage { Title = "Verbindung erfolgreich", Header = "Verbindung erfolgreich", Text = "Es wurde erfolgreich eine Verbindung zur SchILD-Datenbank aufgebaut" });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Verbindung nicht erfolgreich", Header = "Fehler beim Verbinden", Text = "Es konnte keine Verbindung zur SchILD-Datenbank aufgebaut werden. Bitte die Verbindungszeichenfolge überprüfen." });
            }
            finally
            {
                IsConnectingSchILD = false;
            }
        }

        private bool CanTestConnectSchILD()
        {
            return IsConnectingSchILD == false && IsConnectingInternal == false && schILDDataSource.CanConnect;
        }

        private async void TestConnectDataSource()
        {
            try
            {
                IsConnectingInternal = true;

                await persistentDataSource.TestConnectionAsync(settingsService.Settings.DatabaseConnection.Type, settingsService.Settings.DatabaseConnection.ConnectionString);

                Messenger.Send(new DialogMessage { Title = "Verbindung erfolgreich", Header = "Verbindung erfolgreich", Text = "Es wurde erfolgreich eine Verbindung zur Datenbank aufgebaut" });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Verbindung nicht erfolgreich", Header = "Fehler beim Verbinden", Text = "Es konnte keine Verbindung zur Datenbank aufgebaut werden. Bitte die Verbindungszeichenfolge überprüfen." });
            }
            finally
            {
                IsConnectingInternal = false;
            }
        }

        private bool TestCanConnectDataSource()
        {
            return IsConnectingSchILD == false && IsConnectingInternal == false && persistentDataSource.CanConnect;
        }

        private async void ConnectDataSource()
        {
            try
            {
                IsConnectingInternal = true;

                await persistentDataSource.ConnectAsync();

                Messenger.Send(new DialogMessage { Title = "Verbindung erfolgreich", Header = "Verbindung erfolgreich", Text = "Es wurde erfolgreich eine Verbindung zur Datenbank aufgebaut" });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Verbindung nicht erfolgreich", Header = "Fehler beim Verbinden", Text = "Es konnte keine Verbindung zur Datenbank aufgebaut werden. Bitte die Verbindungszeichenfolge überprüfen." });
            }
            finally
            {
                IsConnectingInternal = false;
            }
        }

        private bool CanConnectDataSource()
        {
            return persistentDataSource.CanConnect;
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
    }
}
