using System;
using System.Windows;
using ControlAcceso.Application;
using ControlAcceso.Biometrics;
using ControlAcceso.Database;
using ControlAcceso.Hardware;
using ControlAcceso.Services;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class App : System.Windows.Application
    {
        public static MyApp AppInstance { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IDatabase databaseAdapter = new MySqlDatabaseAdapter();
            IBiometricAdapter biometricAdapter = new SourceAFISAdapter();
            ICaptahuellasService captahuellasAdapter = new FutronicCaptahuellasAdapter();

            var databaseService = new DatabaseService(databaseAdapter);
            var biometricService = new BiometricService(biometricAdapter);
            var captahuellasService = new CaptahuellasService(captahuellasAdapter);

            AppInstance = new MyApp(databaseService, biometricService, captahuellasService);

            var mainController = new MainController(AppInstance);
            mainController.IniciarAplicacion();
        }
    }
}
