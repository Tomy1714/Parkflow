using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class VehiculosNegocio : IVehiculosNegocio
    {
        private readonly IConexion _conexion;

        public VehiculosNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<Vehiculo> Consultar()
        {
            return _conexion.Vehiculos!
                .Include(v => v.Propietario)
                .OrderBy(v => v.Placa)
                .ToList();
        }

        public List<Vehiculo> ConsultarPorUsuario(int idUsuario)
        {
            return _conexion.Vehiculos!
                .Include(v => v.HistorialRegistros)
                .Include(v => v.HistorialReservas)
                .Where(v => v.IdUsuarioPropietario == idUsuario)
                .OrderBy(v => v.Placa)
                .ToList();
        }

        public Vehiculo? BuscarPorPlaca(string placa)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            return _conexion.Vehiculos!
                .Include(v => v.Propietario)
                .Include(v => v.HistorialRegistros)
                .Include(v => v.HistorialReservas)
                .FirstOrDefault(v => v.Placa == pNorm);
        }

        public Vehiculo Guardar(Vehiculo vehiculo)
        {
            vehiculo.Placa = (vehiculo.Placa ?? "").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(vehiculo.Placa) || vehiculo.Placa.Length < 6)
                throw new ArgumentException("Por favor ingrese un número de placa válido antes de continuar");

            var existente = BuscarPorPlaca(vehiculo.Placa);
            if (existente == null)
            {
                _conexion.Vehiculos!.Add(vehiculo);
            }
            else
            {
                existente.TipoVehiculo = vehiculo.TipoVehiculo;
                existente.Marca = vehiculo.Marca;
                existente.Modelo = vehiculo.Modelo;
                if (vehiculo.IdUsuarioPropietario.HasValue)
                {
                    existente.IdUsuarioPropietario = vehiculo.IdUsuarioPropietario;
                }
            }

            _conexion.SaveChanges();
            return vehiculo;
        }

        public Vehiculo RegistrarVehiculoCliente(string placa, int idUsuario, string tipoVehiculo = "Automóvil")
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(pNorm) || pNorm.Length < 6)
                throw new ArgumentException("Por favor ingrese un número de placa válido antes de continuar");

            var existente = BuscarPorPlaca(pNorm);
            if (existente != null)
            {
                if (existente.IdUsuarioPropietario.HasValue && existente.IdUsuarioPropietario.Value != idUsuario)
                {
                    throw new InvalidOperationException($"La placa {pNorm} ya se encuentra registrada por otro cliente.");
                }
                existente.IdUsuarioPropietario = idUsuario;
                _conexion.SaveChanges();
                return existente;
            }

            var nuevo = new Vehiculo
            {
                Placa = pNorm,
                TipoVehiculo = tipoVehiculo,
                IdUsuarioPropietario = idUsuario
            };

            _conexion.Vehiculos!.Add(nuevo);
            _conexion.SaveChanges();
            return nuevo;
        }

        public bool Borrar(string placa)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            var existente = BuscarPorPlaca(pNorm);
            if (existente == null) return false;
            _conexion.Vehiculos!.Remove(existente);
            _conexion.SaveChanges();
            return true;
        }
    }
}
