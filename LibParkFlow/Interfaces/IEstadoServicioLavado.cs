namespace LibParkFlow.Interfaces
{
    using LibParkFlow.Entidades;

    /// <summary>
    /// Interfaz del Estado (GoF State) para el control del ciclo de vida del lavado
    /// </summary>
    public interface IEstadoServicioLavado
    {
        EstadoLavado EstadoEnum { get; }
        void Avanzar(ServicioLavado servicio, DateTime ahora);
        bool PuedeCobrarse();
    }
}
