using Microsoft.AspNetCore.Mvc; 
namespace ParkingSystem.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class ParkingController : ControllerBase
    {
        // Semáforo que controla el acceso al estacionamiento, inicializado con capacidad máxima de 3 vehículos
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(3);


        // Lista para almacenar los IDs de los vehículos que están actualmente en el estacionamiento
        private static List<int> _vehicles = new List<int>();

        // Objeto para manejar la sincronización de acceso a la lista de vehículos
        private static readonly object _lock = new object();

        // Método para ingresar un vehículo al estacionamiento
        [HttpPost("enter")] 
        public IActionResult EnterVehicle([FromBody] int vehicleId)
        {
            lock (_lock) // Bloquea el acceso al código dentro del bloque a un solo hilo a la vez
            {
                // Verifica si el vehículo ya está en el estacionamiento
                if (_vehicles.Contains(vehicleId))
                {
                    return BadRequest(new { message = $"El vehículo ID {vehicleId} ya está en el estacionamiento." });
                }

                // Verifica si hay espacio disponible en el estacionamiento
                if (_semaphore.CurrentCount == 0)
                {
                    return BadRequest(new { message = "El estacionamiento está lleno. No se puede ingresar el vehículo." });
                }

                // Agrega el ID del vehículo a la lista de vehículos en el estacionamiento
                _vehicles.Add(vehicleId);
                _semaphore.Wait(); // Adquiere un permiso del semáforo, bloqueando el hilo si no hay permisos disponibles

                // Retorna una respuesta de éxito
                return Ok(new { message = $"Vehículo ID {vehicleId} ha ingresado al estacionamiento." });
            }
        }

        // Método para salir un vehículo del estacionamiento
        [HttpPost("exit")] // Define la ruta para salir vehículos
        public IActionResult ExitVehicle([FromBody] int vehicleId)
        {
            lock (_lock) // Bloquea el acceso al código dentro del bloque a un solo hilo a la vez
            {
                // Verifica si el vehículo está en el estacionamiento
                if (!_vehicles.Contains(vehicleId))
                {
                    return BadRequest(new { message = $"El vehículo ID {vehicleId} no está en el estacionamiento." });
                }

                // Elimina el ID del vehículo de la lista de vehículos en el estacionamiento
                _vehicles.Remove(vehicleId);
                _semaphore.Release(); // Libera un permiso del semáforo, permitiendo que otro vehículo ingrese

                // Retorna una respuesta de éxito
                return Ok(new { message = $"Vehículo ID {vehicleId} ha salido del estacionamiento." });
            }
        }

        // Método para obtener el estado del estacionamiento
        [HttpGet("status")] // Define la ruta para obtener el estado
        public IActionResult Status()
        {
            // Retorna el total de vehículos y la capacidad máxima
            return Ok(new { totalVehicles = _vehicles.Count, capacity = 3 });
        }

        // Método para obtener los IDs de los vehículos en el estacionamiento
        [HttpGet("vehicles")] // Define la ruta para obtener la lista de vehículos
        public IActionResult GetVehicles()
        {
            lock (_lock) // Bloquea el acceso al código dentro del bloque a un solo hilo a la vez
            {
                // Retorna la lista de IDs de vehículos en el estacionamiento
                return Ok(_vehicles);
            }
        }
    }
}
