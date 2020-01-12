﻿using AnonymizationTool.Anonymization;
using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Data.SchILD;
using AnonymizationTool.Export;
using AnonymizationTool.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AnonymizationTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                Set(() => IsBusy, ref isBusy, value);

                AnonymizeCommand?.RaiseCanExecuteChanged();
                LoadStudentsCommand?.RaiseCanExecuteChanged();
                SyncCommand?.RaiseCanExecuteChanged();
                RemoveCommand?.RaiseCanExecuteChanged();
            }
        }

        private string busyMessage;

        public string BusyMessage
        {
            get { return busyMessage; }
            set { Set(() => BusyMessage, ref busyMessage, value); }
        }

        private double? busyProgress;

        public double? BusyProgress
        {
            get { return busyProgress; }
            set { Set(() => BusyProgress, ref busyProgress, value); }
        }

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        #endregion

        public ObservableCollection<AnonymousStudent> Students { get; } = new ObservableCollection<AnonymousStudent>();

        public RelayCommand LoadStudentsCommand { get; private set; }
        public RelayCommand SyncCommand { get; private set; }
        public RelayCommand AnonymizeCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public RelayCommand ExportCommand { get; private set; }


        private IStudentFaker studentFaker;
        private readonly IPersistentDataSource dataSource;
        private readonly ISchILDDataSource schILDDataSource;
        private readonly IExportService exportService;
        private readonly ILogger<MainViewModel> logger;

        public MainViewModel(IStudentFaker studentFaker, IPersistentDataSource dataSource, ISchILDDataSource schILDDataSource, IExportService exportService, ILogger<MainViewModel> logger, IMessenger messenger)
            : base(messenger)
        {
            this.studentFaker = studentFaker;
            this.dataSource = dataSource;
            this.schILDDataSource = schILDDataSource;
            this.exportService = exportService;
            this.logger = logger;

            LoadStudentsCommand = new RelayCommand(LoadStudents, CanLoadStudents);
            SyncCommand = new RelayCommand(Sync, CanSync);
            AnonymizeCommand = new RelayCommand(Anonymize, CanAnonymize);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
            ExportCommand = new RelayCommand(Export, CanExport);

            Students.CollectionChanged += (sender, args) =>
            {
                AnonymizeCommand?.RaiseCanExecuteChanged();
                RemoveCommand?.RaiseCanExecuteChanged();
                ExportCommand?.RaiseCanExecuteChanged();
            };

            dataSource.ConnectionStateChanged += (sender, args) =>
            {
                RunOnUI(() =>
                {
                    LoadStudentsCommand?.RaiseCanExecuteChanged();
                    SyncCommand?.RaiseCanExecuteChanged();
                    AnonymizeCommand?.RaiseCanExecuteChanged();
                });
            };
        }

        private async void Export()
        {
            var msg = new SelectDirectoryDialogMessage();
            Messenger.Send(msg);

            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Exportiere CSV-Dateien...";

                await exportService.ExportAsync(msg.Path, Students);
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Exportieren", Text = "Beim Exportieren ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExport()
        {
            return Students.Count > 0;
        }

        private async void Remove()
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Lösche Schüler aus der internen Datenbank...";

                await Task.Run(() =>
                {
                    var students = Students.Where(s => s.IsMissingInSchILD == true);

                    foreach (var student in students)
                    {
                        dataSource.RemoveStudent(student);
                    }
                });

                await dataSource.SaveChangesAsync();

                LoadStudents();                
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Löschen der Schüler", Text = "Beim Löschen der Schüler aus der internen Datenbank ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRemove()
        {
            return !IsBusy && dataSource.IsConnected && Students.Count > 0 && Students.Any(s => s.IsMissingInSchILD == true);
        }

        private async void Anonymize()
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Anonymisiere Schüler...";

                // Create a working copy of all students (because we want to update the UI afterwards)
                var databaseStudents = Students.ToList();
                Students.Clear();

                await Task.Run(() =>
                {
                    var currentStudentIdx = 0;
                    var studentsCount = (double)databaseStudents.Count;

                    foreach (var student in databaseStudents)
                    {
                        int attempt = 0;
                        do
                        {
                            studentFaker.FakeStudent(student, attempt);
                            attempt++;
                        } while (Students.Count(x => x.AnonymousEmail == student.AnonymousEmail) > 1); // Ensure email addresses are unique!

                        dataSource.UpdateStudent(student);

                        // Update progressbar
                        RunOnUI(() => BusyProgress = (currentStudentIdx / studentsCount) * 100);
                        currentStudentIdx++;
                    }
                });

                BusyProgress = null;
                BusyMessage = "Speichere Änderungen in der internen Datenbank...";

                await dataSource.SaveChangesAsync();

                foreach (var student in databaseStudents)
                {
                    Students.Add(student);
                }
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Anonymisieren der Schüler", Text = "Beim Anonymisieren der Schüler ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanAnonymize()
        {
            return !IsBusy && dataSource.IsConnected && Students.Count > 0;
        }

        private void RunOnUI(Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }

        public async void LoadStudents()
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;

                Students.Clear();

                if (!dataSource.IsConnected)
                {
                    BusyMessage = "Verbinde mit der internen Datenbank...";
                    await dataSource.ConnectAsync();
                }

                BusyMessage = "Lade Schüler aus der internen Datenbank...";
                var students = await dataSource.LoadStudentsAsync();

                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var student in students)
                    {
                        Students.Add(student);
                    }
                });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Laden der Schüler", Text = "Beim Laden der Schüler aus der internen Datenbank ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLoadStudents()
        {
            return !IsBusy && dataSource.IsConnected;
        }

        private async void Sync()
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Verbinde zur SchILD Datenbank...";

                await schILDDataSource.ConnectAsync();

                BusyMessage = "Lade Schüler aus Datenbank...";

                var schildStudents = await schILDDataSource.LoadStudentsAsync();

                BusyMessage = "Aktualisiere Schüler in der internen Datenbank...";

                // Create a working copy of all students (because we want to update the UI afterwards)
                var databaseStudents = Students.ToList();
                Students.Clear();

                await Task.Run(() =>
                {
                    // Mark all students as missing in SchILD (this property gets updated if an student is found in SchILD
                    foreach (var student in databaseStudents)
                    {
                        student.IsMissingInSchILD = true;
                    }

                    var currentStudentIdx = 0;
                    var studentsCount = (double)schildStudents.Count();

                    foreach (var schildStudent in schildStudents)
                    {
                        var anonymousStudent = databaseStudents.FirstOrDefault(x => x.SchILDId == schildStudent.Id);

                        if (anonymousStudent == null)
                        {
                            anonymousStudent = new AnonymousStudent
                            {
                                SchILDId = schildStudent.Id
                            };

                            databaseStudents.Add(anonymousStudent);
                            dataSource.AddStudent(anonymousStudent);
                        }
                        else
                        {
                            dataSource.UpdateStudent(anonymousStudent);
                        }

                        anonymousStudent.IsMissingInSchILD = false;
                        PopulateAnonymousStudent(anonymousStudent, schildStudent);

                        // Update progressbar
                        RunOnUI(() => BusyProgress = (currentStudentIdx / studentsCount) * 100);
                        currentStudentIdx++;
                    }
                });

                BusyProgress = null;
                BusyMessage = "Speichere Änderungen in der internen Datenbank...";
                await dataSource.SaveChangesAsync();

                foreach(var student in databaseStudents)
                {
                    Students.Add(student);
                }
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Laden der Schüler", Text = "Beim Laden der Schüler aus SchILD ist ein Fehler aufgetreten." });
            }
            finally
            {
                await schILDDataSource.DisconnectAsync();
                IsBusy = false;
            }
        }

        private bool CanSync()
        {
            return dataSource.IsConnected && !IsBusy;
        }

        private void PopulateAnonymousStudent(AnonymousStudent anonymousStudent, Student student)
        {
            anonymousStudent.FirstName = student.FirstName;
            anonymousStudent.LastName = student.LastName;
            anonymousStudent.Grade = student.Grade;
            anonymousStudent.Email = student.Email;
        }
    }
}