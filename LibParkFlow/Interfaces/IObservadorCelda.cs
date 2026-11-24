namespace LibParkFlow.Interfaces
{
    using LibParkFlow.Entidades;

    /// <summary>
    /// Interfaz del Observador (GoF Observer) para reaccionar a cambios en las celdas
    /// </summary>
    public interface IObservadorCelda
    {
        void Actualizar(Celda celda);
    }
}
