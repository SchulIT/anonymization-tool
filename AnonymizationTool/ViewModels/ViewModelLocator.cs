using AnonymizationTool.Anonymization;
using AnonymizationTool.Data;
using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Data.SchILD;
using AnonymizationTool.Export;
using AnonymizationTool.Export.Csv;
using AnonymizationTool.Export.SchulIT.Idp;
using AnonymizationTool.Settings;
using AnonymizationTool.UI;
using Autofac;
using GalaSoft.MvvmLight.Messaging;
using static AnonymizationTool.Settings.IEmailSettings;

namespace AnonymizationTool.ViewModels
{
    public class ViewModelLocator
    {
        private static IContainer container;

        static ViewModelLocator()
        {
            RegisterServices();
        }

        public static void RegisterServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AccessOdbcSchILDDataSource>().AsSelf().Keyed<ISchILDDataSource>(DatabaseType.Access).SingleInstance();
            builder.RegisterType<MssqlSchILDDataSource>().AsSelf().Keyed<ISchILDDataSource>(DatabaseType.MSSQL).SingleInstance();
            builder.RegisterType<MysqlSchILDDataSource>().AsSelf().Keyed<ISchILDDataSource>(DatabaseType.MySQL).SingleInstance();
            builder.RegisterType<SchILDDataSourceResolverDataSource>().As<ISchILDDataSource>().SingleInstance();

            builder.RegisterType<SqlDataSource>().As<IPersistentDataSource>().SingleInstance();
            builder.RegisterType<JsonSettingsService>().As<ISettingsService>().SingleInstance().OnActivating(s => s.Instance.LoadSettings());

            builder.RegisterType<FirstnameLastnameEmailFaker>().AsSelf().Keyed<IEmailFaker>(AnonymizationType.FirstnameLastname).SingleInstance();
            builder.RegisterType<FLastnameEmailFaker>().Keyed<IEmailFaker>(AnonymizationType.FLastname).SingleInstance();
            builder.RegisterType<RandomEmailFaker>().Keyed<IEmailFaker>(AnonymizationType.Random).SingleInstance();

            builder.RegisterType<BogusNameFaker>().As<INameFaker>().SingleInstance();

            builder.RegisterType<StudentFaker>().As<IStudentFaker>().SingleInstance();

            builder.RegisterType<IdpExportService>().Named<IExportService>("schulit_idp");
            builder.RegisterType<CsvExportService>().Named<IExportService>("csv");

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<UIDispatcher>().As<IDispatcher>().SingleInstance();

            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<AboutViewModel>().AsSelf().SingleInstance();

            container = builder.Build();
        }

        public IMessenger Messenger { get { return container.Resolve<IMessenger>(); } }

        public MainViewModel Main { get { return container.Resolve<MainViewModel>(); } }

        public SettingsViewModel Settings { get { return container.Resolve<SettingsViewModel>(); } }

        public AboutViewModel About { get { return container.Resolve<AboutViewModel>(); } }
    }
}
