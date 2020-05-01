using AnonymizationTool.Anonymization;
using AnonymizationTool.Collections;
using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Data.SchILD;
using AnonymizationTool.Export;
using AnonymizationTool.Messages;
using AnonymizationTool.UI;
using Autofac.Features.Indexed;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
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
                RemoveNonActiveStudentsCommand?.RaiseCanExecuteChanged();
                AnonymizeSelectedStudentsCommand?.RaiseCanExecuteChanged();
                RemoveSelectedStudentsCommand?.RaiseCanExecuteChanged();
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

        private bool isNotPersistentDatabase = false;

        /// <summary>
        /// Flag whether the user is using a SQLite in-memory database
        /// </summary>
        public bool IsNotPersistentDatabase
        {
            get { return isNotPersistentDatabase; }
            set { Set(() => IsNotPersistentDatabase, ref isNotPersistentDatabase, value); }
        }

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        #endregion

        public RangeObservableCollection<AnonymousStudent> Students { get; } = new RangeObservableCollection<AnonymousStudent>();

        public RangeObservableCollection<AnonymousStudent> SelectedStudents { get; } = new RangeObservableCollection<AnonymousStudent>();

        public RelayCommand LoadStudentsCommand { get; private set; }
        public RelayCommand SyncCommand { get; private set; }
        public RelayCommand AnonymizeCommand { get; private set; }

        public RelayCommand AnonymizeSelectedStudentsCommand { get; private set; }

        public RelayCommand RemoveNonActiveStudentsCommand { get; private set; }

        public RelayCommand RemoveSelectedStudentsCommand { get; private set; }

        public RelayCommand ExportCsvCommand { get; private set; }

        public RelayCommand ExportSchulITIdpCommand { get; private set; }

        private IStudentFaker studentFaker;
        private readonly IPersistentDataSource dataSource;
        private readonly ISchILDDataSource schILDDataSource;
        private readonly IIndex<string, IExportService> exportServices;
        private readonly IDispatcher dispatcher;

        public MainViewModel(IStudentFaker studentFaker, IPersistentDataSource dataSource, ISchILDDataSource schILDDataSource, IIndex<string, IExportService> exportServices, IDispatcher dispatcher, IMessenger messenger)
            : base(messenger)
        {
            this.studentFaker = studentFaker;
            this.dataSource = dataSource;
            this.schILDDataSource = schILDDataSource;
            this.exportServices = exportServices;
            this.dispatcher = dispatcher;

            LoadStudentsCommand = new RelayCommand(LoadStudents, CanLoadStudents);
            SyncCommand = new RelayCommand(Sync, CanSync);
            AnonymizeCommand = new RelayCommand(Anonymize, CanAnonymize);
            AnonymizeSelectedStudentsCommand = new RelayCommand(AnomyizeSelectedStudents, CanAnonymizeSelectedStudents);
            RemoveNonActiveStudentsCommand = new RelayCommand(RemoveNonActive, CanRemoveNonActive);
            RemoveSelectedStudentsCommand = new RelayCommand(RemoveSelected, CanRemoveSelected);
            ExportCsvCommand = new RelayCommand(ExportCsv, CanExport);
            ExportSchulITIdpCommand = new RelayCommand(ExportSchulITIdp, CanExport);

            Students.CollectionChanged += (sender, args) =>
            {
                AnonymizeCommand?.RaiseCanExecuteChanged();
                RemoveNonActiveStudentsCommand?.RaiseCanExecuteChanged();
                ExportCsvCommand?.RaiseCanExecuteChanged();
                ExportSchulITIdpCommand?.RaiseCanExecuteChanged();
            };

            SelectedStudents.CollectionChanged += (sender, args) =>
            {
                RemoveSelectedStudentsCommand?.RaiseCanExecuteChanged();
                AnonymizeSelectedStudentsCommand?.RaiseCanExecuteChanged();
            };

            dataSource.ConnectionStateChanged += (sender, args) =>
            {
                dispatcher.RunOnUI(() =>
                {
                    if (args.IsConnected == false)
                    {
                        // Clear all students in case we get disconnected
                        Students.Clear();
                        SelectedStudents.Clear();
                    }

                    IsNotPersistentDatabase = sender.IsInMemory;
                    LoadStudentsCommand?.RaiseCanExecuteChanged();
                    SyncCommand?.RaiseCanExecuteChanged();
                    AnonymizeCommand?.RaiseCanExecuteChanged();
                });
            };
        }

        private void UpdateProgress(IExportService exportService, ProgressChangedEventArgs args)
        {
            dispatcher.RunOnUI(() =>
            {
                if(!string.IsNullOrEmpty(args.Details))
                {
                    BusyMessage = args.Details;
                }

                if (args.Completed == null || args.Total == null)
                {
                    BusyProgress = null;
                }
                else if (args.Total > 0)
                {
                    BusyProgress = (double)args.Completed / args.Total * 100;
                }
                else
                {
                    BusyProgress = 0;
                }
            });
        }

        private async void ExportCsv()
        {
            var exportService = exportServices["csv"];

            try
            {    
                exportService.ProgressChanged += UpdateProgress;
                IsBusy = true;
                BusyProgress = 0;

                await exportService.ExportAsync(Students);

                Messenger.Send(new DialogMessage { Title = "Vorgang erfolgreich", Header = "Export erfolgreich", Text = $"Der Export war erfolgreich." });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Exportieren", Text = "Beim Exportieren ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
                exportService.ProgressChanged -= UpdateProgress;
            }
        }

        private async void ExportSchulITIdp()
        {
            var exportService = exportServices["schulit_idp"];

            try
            {
                exportService.ProgressChanged += UpdateProgress;
                IsBusy = true;
                BusyProgress = 0;
                BusyMessage = "Exportiere zum SchulIT Idp...";

                await exportService.ExportAsync(Students);

                Messenger.Send(new DialogMessage { Title = "Vorgang abgeschlose", Header = "Export erfolgreich", Text = $"Der Export war erfolgreich." });
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Exportieren", Text = "Beim Exportieren ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
                exportService.ProgressChanged -= UpdateProgress;
            }
        }

        private bool CanExport()
        {
            return Students.Count > 0;
        }

        private async void RemoveNonActive()
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Lösche Schüler aus der internen Datenbank...";

                await Task.Factory.StartNew(() =>
                {
                    var students = Students.Where(s => s.IsMissingInSchILD == true);

                    foreach (var student in students)
                    {
                        dataSource.RemoveStudent(student);
                    }
                }, TaskCreationOptions.LongRunning);

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

        private bool CanRemoveNonActive()
        {
            return !IsBusy && dataSource.IsConnected && Students.Count > 0 && Students.Any(s => s.IsMissingInSchILD == true);
        }

        private async void RemoveSelected()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Ausgewählte Schüler löschen...";
                BusyProgress = 0;

                await Task.Factory.StartNew(() =>
                {
                    var currentStudentIdx = 0;
                    var studentsCount = SelectedStudents.Count;

                    foreach (var student in SelectedStudents)
                    {
                        dataSource.RemoveStudent(student);

                        // Update progressbar
                        dispatcher.RunOnUI(() => BusyProgress = (currentStudentIdx / studentsCount) * 100);
                        currentStudentIdx++;
                    }
                }, TaskCreationOptions.LongRunning);

                BusyProgress = null;
                BusyMessage = "Speichere Änderungen in der internen Datenbank...";

                await dataSource.SaveChangesAsync();

                SelectedStudents.Clear();
                LoadStudents();
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Title = "Fehler", Header = "Fehler beim Löschen der Schüler", Text = "Beim Löschen der Schüler ausgewählten ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        private bool CanRemoveSelected()
        {
            return SelectedStudents != null && SelectedStudents.Count > 0;
        }

        private void Anonymize()
        {
            InternalAnonymize(Students.ToList());
        }

        private bool CanAnonymize()
        {
            return !IsBusy && dataSource.IsConnected && Students.Count > 0;
        }

        private async void InternalAnonymize(IEnumerable<AnonymousStudent> students)
        {
            try
            {
                IsBusy = true;
                BusyProgress = null;
                BusyMessage = "Anonymisiere ausgewählte Schüler...";

                // Create a working copy of all students (because we want to update the UI afterwards)
                var allStudents = Students.ToList();
                var selectedStudents = SelectedStudents.ToList();

                SelectedStudents.Clear();
                Students.Clear();

                await Task.Factory.StartNew(() =>
                {
                    var currentStudentIdx = 0;
                    var studentsCount = (double)students.Count();

                    foreach (var student in students)
                    {
                        int attempt = 0;
                        do
                        {
                            studentFaker.FakeStudent(student, attempt);
                            attempt++;
                        } while (Students.Count(x => x.AnonymousEmail == student.AnonymousEmail) > 1); // Ensure email addresses are unique!

                        dataSource.UpdateStudent(student);

                        // Update progressbar
                        dispatcher.RunOnUI(() => BusyProgress = (currentStudentIdx / studentsCount) * 100);
                        currentStudentIdx++;
                    }
                }, TaskCreationOptions.LongRunning);

                BusyProgress = null;
                BusyMessage = "Speichere Änderungen in der internen Datenbank...";

                await dataSource.SaveChangesAsync();

                Students.AddRange(allStudents);
                SelectedStudents.AddRange(selectedStudents);
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

        private void AnomyizeSelectedStudents()
        {
            InternalAnonymize(SelectedStudents.ToList());
        }

        private bool CanAnonymizeSelectedStudents()
        {
            return !IsBusy && dataSource.IsConnected && SelectedStudents.Count > 0;
        }

        public async void LoadStudents()
        {
            if(dataSource.CanConnect == false)
            {
                Messenger.Send(new DialogMessage { Title = "Datenbankverbindung", Header = "Datenbankverbindung", Text = "Bitte die Einstellungen für die interne Datenbank überprüfen." });
                return;
            }

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

                Students.AddRange(students);
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
                BusyMessage = "Lade Schüler aus Datenbank...";

                var schildStudents = await schILDDataSource.LoadStudentsAsync();

                BusyMessage = "Aktualisiere Schüler in der internen Datenbank...";

                // Create a working copy of all students (because we want to update the UI afterwards)
                var databaseStudents = Students.ToList();
                Students.Clear();

                await Task.Factory.StartNew(() =>
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
                        dispatcher.RunOnUI(() => BusyProgress = (currentStudentIdx / studentsCount) * 100);
                        currentStudentIdx++;
                    }
                }, TaskCreationOptions.LongRunning);

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
