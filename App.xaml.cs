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

            try
                {
                    IDatabase databaseAdapter = new MySqlDatabaseAdapter();
                    IBiometricAdapter biometricAdapter = new SourceAFISAdapter();
                    ICaptahuellasService captahuellasAdapter = new FutronicCaptahuellasAdapter();

                    var databaseService = new DatabaseService(databaseAdapter);
                    var biometricService = new BiometricService(biometricAdapter);
                    var captahuellasService = new CaptahuellasService(captahuellasAdapter);

                    AppInstance = new MyApp(databaseService, biometricService, captahuellasService);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falla crítica al inicializar la aplicación:\n{ex.Message}",
                                    "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }
    }
}
