namespace LibParkFlow.Interfaces
{
    public class DatosPago
    {
        public decimal ValorRecibido { get; set; }
        public string? Referencia { get; set; }
        public string? NumeroTarjeta { get; set; }
        public bool SimularRechazo { get; set; } = false;
    }

    public class ResultadoPago
    {
        public bool Aprobado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public decimal Cambio { get; set; } = 0;
    }

    /// <summary>
    /// Interfaz de Estrategia (GoF Strategy) para el procesamiento de métodos de pago
    /// </summary>
    public interface IMetodoPagoStrategy
    {
        string NombreMetodo { get; }
        ResultadoPago Procesar(decimal monto, DatosPago? datos);
    }
}
