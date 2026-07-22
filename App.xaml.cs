using System;
using System.Windows;
using ControlAcceso.Application;
using ControlAcceso.Biometrics;
using ControlAcceso.Database;
using ControlAcceso.Hardware;
using ControlAcceso.Services;

namespace ControlAcceso
{
    /// <summary>
    /// Lógica de interacción para App.xaml.
    /// Inicia los servicios de infraestructura y expone la instancia global de MyApp.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Acceso global estático a la instancia orquestadora MyApp.
        /// Permite acceder a los casos de uso desde cualquier ventana mediante App.AppInstance.
        /// </summary>
        public static MyApp AppInstance { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Instanciar los adaptadores concretos de infraestructura
            IDatabase databaseAdapter = new MySqlDatabaseAdapter();
            IBiometricAdapter biometricAdapter = new SourceAFISAdapter();
            ICaptahuellasService captahuellasAdapter = new FutronicCaptahuellasAdapter();

            // 2. Instanciar los servicios de la capa de aplicación wrapped sobre las interfaces
            var databaseService = new DatabaseService(databaseAdapter);
            var biometricService = new BiometricService(biometricAdapter);
            var captahuellasService = new CaptahuellasService(captahuellasAdapter);

            // 3. Inicializar el orquestador global (Singleton)
            AppInstance = new MyApp(databaseService, biometricService, captahuellasService);
        }
    }
}
