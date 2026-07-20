using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows; // Para mostrar alertas si usas MessageBox
using MySql.Data.MySqlClient;
using ControlAcceso.Services;

namespace ControlAcceso
{
    public class MyApp
    {
        // Instancia global de la base de datos
        public Database Db { get; } = new Database();

        // Estado global: lista de empleados cargada en memoria
        public List<Empleado> Empleados { get; set; } = new List<Empleado>();

        public List<Asistencia> HistorialAsistencias { get; set; } = new List<Asistencia>();

        public void CargarEmpleadosDesdeDb()
        {
            try
            {
                Empleados = Db.ObtenerEmpleados();
            }
            catch (MySqlException ex)
            {
                // Manejo específico si el servidor MySQL no está encendido o la red falló
                MessageBox.Show($"Error de conexión a la Base de Datos:\n{ex.Message}\n\nVerifica que MySQL esté corriendo.",
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);

                
            }
            catch (Exception ex)
            {
                // Captura de errores inesperados
                MessageBox.Show($"Ocurrió un error inesperado al cargar los empleados:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Empleados = new List<Empleado>();
            }
        }

        public Empleado? ObtenerEmpleadoPorId(int id)
        {
            return Empleados.FirstOrDefault(e => e.id == id);
        }

        public void CargarHistorialAsistencias()
        {
            try
            {
                HistorialAsistencias = Db.ObtenerAsistencias();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[ERROR CONEXIÓN]: No se pudo cargar el historial. {ex.Message}");
                HistorialAsistencias = new List<Asistencia>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR INESPERADO]: {ex.Message}");
                HistorialAsistencias = new List<Asistencia>();
            }
        }
    }
}
