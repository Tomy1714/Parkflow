using LibParkFlow.Entidades;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public class ResumenCuentaPresentacionDto
    {
        public bool Ok { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Celda { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public int MinutosEstadia { get; set; }
        public decimal CostoParqueadero { get; set; }
        public List<ServicioLavadoDto> Facturables { get; set; } = new();
        public List<ServicioLavadoDto> EnCurso { get; set; } = new();
        public decimal Total { get; set; }
        public string EstadoRegistro { get; set; } = string.Empty;
    }

    public class ServicioLavadoDto
    {
        public int IdServicio { get; set; }
        public string TipoLavado { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public string EstadoLavado { get; set; } = string.Empty;
    }

    public class LiquidarPresentacionDto
    {
        public string Placa { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = "Efectivo";
        public CanalPago Canal { get; set; } = CanalPago.CAJA;
        public decimal ValorRecibido { get; set; }
        public string? Referencia { get; set; }
        public int? IdCaja { get; set; }
        public DateTime? Ahora { get; set; }
    }

    public interface ILiquidacionNegocioPresentacion
    {
        ResumenCuentaPresentacionDto ConsultarCuenta(string placa, DateTime? ahora = null);
        ComprobantePago Liquidar(LiquidarPresentacionDto datosCobro);
        bool AutorizarSalida(string placa, DateTime? ahora = null);
    }
}
