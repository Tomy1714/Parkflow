using LibParkFlow.Entidades;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IVehiculosNegocioPresentacion
    {
        List<Vehiculo> Consultar();
        List<Vehiculo> ConsultarPorUsuario(int idUsuario);
        Vehiculo? BuscarPorPlaca(string placa);
        Vehiculo Guardar(Vehiculo vehiculo);
        Vehiculo RegistrarVehiculoCliente(string placa, int idUsuario, string tipoVehiculo = "Automóvil");
        bool Borrar(string placa);
    }
}
