using System;
using MySql.Data.MySqlClient;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;

namespace pruebaa_2
{
    class Program
    {
        static string conexionString = "Server=localhost;Database=prueba1;User Id=root;Password=Mitocondria#1;";

        public static void Main(string[] args)
        {
            // Opciones del menú
            Console.WriteLine("Seleccione una opción:");
            Console.WriteLine("1. Listar top 10 usuarios");
            Console.WriteLine("2. Generar archivo CSV de usuarios");
            Console.WriteLine("3. Agregar nuevo usuario");
            Console.WriteLine("4. Actualizar salario de usuario");

            int opcion;
            while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > 4)
            {
                Console.WriteLine("Por favor, ingrese un número válido de opción (1 a 4).");
            }

            switch (opcion)
            {
                case 1:
                    top10();
                    break;
                case 2:
                    generarCSV();
                    break;
                case 3:
                    agregarUsuario();
                    break;
                case 4:
                    actualizarSalario();
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }

        public static void top10()
        {
            // Listar top 10 usuarios de la base antes creada
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexionString))
                {
                    Console.WriteLine("\nTOP 10 usuarios");

                    string sql = "SELECT * FROM usuarios ORDER BY userId ASC LIMIT 10";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string userId = reader["userId"].ToString();
                                string login = reader["Login"].ToString();
                                string nombre = reader["Nombre"].ToString();
                                string paterno = reader["Paterno"].ToString();
                                string materno = reader["Materno"].ToString();
                                Console.WriteLine($"[{userId}, {login}, {nombre}, {paterno}, {materno}]");
                            }
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();
        }

        public static void generarCSV()
        {
            // Generar un archivo csv con las siguientes campos con su información(Login, Nombre completo, sueldo, fecha Ingreso)
            string rutaArchivoCsv = @"C:\Users\Megahabilidades\Desktop\usuarios.csv";
            string consultaSql = "SELECT u.Login, CONCAT (u.Nombre, ' ', u.Paterno, ' ', u.Materno) AS NombreCompleto, e.Sueldo, e.FechaIngreso FROM usuarios u JOIN empleados e On u.userId = e.userId LIMIT 50;";

            try
            {
                using (var writer = new StreamWriter(rutaArchivoCsv))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var usuarios = new List<Usuario>();
                    using (var connection = new MySqlConnection(conexionString))
                    {
                        connection.Open();

                        using (var command = new MySqlCommand(consultaSql, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var usuario = new Usuario
                                    {
                                        Login = reader["Login"].ToString(),
                                        Nombre = reader["NombreCompleto"].ToString(),
                                        Sueldo = reader["Sueldo"].ToString(),
                                        FechaIngreso = reader["FechaIngreso"].ToString()
                                    };
                                    usuarios.Add(usuario);
                                }
                            }
                        }
                    }
                    csv.WriteRecords(usuarios);
                }
                Console.WriteLine($"Se ha generado el archivo CSV en: {rutaArchivoCsv}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en el archivo CSV: {ex.Message}");
            }
        }

        public static void actualizarSalario()
        {
            // Actualizar el sueldo de un usuario según su ID
            Console.Write("Introduce el ID del usuario: ");
            string login = Console.ReadLine();
            using (MySqlConnection connection = new MySqlConnection(conexionString))
            {
                Console.Write("Introduce el nuevo sueldo del usuario: ");
                string sueldoAct = Console.ReadLine();
                string consultaSql = $"UPDATE empleados SET Sueldo={sueldoAct} WHERE userId={login};";
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(consultaSql, connection))
                {
                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Usuario actualizado exitosamente.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró un usuario con el login especificado.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al actualizar el usuario: {ex.Message}");
                    }
                }
            }
        }

        public static void agregarUsuario()
        {
            // Agregar un nuevo usuario con salario y fecha de ingreso
            Console.Write("Introduce el userId del usuario: ");
            string userId = Console.ReadLine();

            Console.Write("Introduce el salario del usuario: ");
            decimal salario;
            while (!decimal.TryParse(Console.ReadLine(), out salario))
            {
                Console.WriteLine("Por favor, introduce un valor numérico para el salario.");
                Console.Write("Introduce el salario del usuario: ");
            }

            DateTime fechaIngreso = DateTime.Today;

            string consultaSql = "INSERT INTO empleados (userId, Sueldo, FechaIngreso) " +
                                 "VALUES (@userId, @salario, @fechaIngreso);";

            using (MySqlConnection connection = new MySqlConnection(conexionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(consultaSql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@salario", salario);
                        command.Parameters.AddWithValue("@fechaIngreso", fechaIngreso);

                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            Console.WriteLine("Usuario agregado exitosamente.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo agregar el usuario.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al agregar el usuario: {ex.Message}");
                }
            }
        }
    }

    public class Usuario
    {
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string Sueldo { get; set; }
        public string FechaIngreso { get; set; }
    }
}
