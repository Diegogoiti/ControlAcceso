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

            // Red de seguridad global: ninguna excepción no controlada debe
            // tumbar la aplicación sin avisar al usuario.
            RegistrarManejadoresGlobalesDeExcepciones();

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

        /// <summary>
        /// Captura excepciones no controladas en el hilo de UI, en hilos de
        /// fondo y en tareas async para mostrar un mensaje amigable en lugar
        /// de que la aplicación falle en silencio.
        /// </summary>
        private void RegistrarManejadoresGlobalesDeExcepciones()
        {
            // Errores en el hilo de UI (WPF). Al marcar e.Handled = true la
            // aplicación sigue viva y el usuario puede guardar su trabajo.
            DispatcherUnhandledException += (_, args) =>
            {
                args.Handled = true;
                MostrarErrorGlobal(args.Exception, "Ocurrió un error inesperado en la interfaz");
            };

            // Errores en hilos de fondo (Task.Run, etc.). Aquí la app sigue
            // corriendo; el manejador solo informa.
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                args.SetObserved();
                MostrarErrorGlobal(args.Exception, "Ocurrió un error en una tarea en segundo plano");
            };

            // Último recurso: excepciones fatales del proceso.
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MostrarErrorGlobal(ex, "Error crítico de la aplicación");
                }
            };
        }

        private static void MostrarErrorGlobal(Exception ex, string titulo)
        {
            try
            {
                System.Windows.MessageBox.Show(
                    $"{titulo}:\n\n{ex.Message}\n\nSi el problema persiste, contacta al administrador del sistema.",
                    titulo,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            catch
            {
                // Si ni siquiera podemos mostrar el diálogo, no queda nada más que hacer.
            }
        }
    }
}
