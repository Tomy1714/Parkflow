using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StrategyPago
{
    public class PagoTransferencia : IMetodoPagoStrategy
    {
        public string NombreMetodo => "Transferencia";

        public ResultadoPago Procesar(decimal monto, DatosPago? datos)
        {
            if (datos?.SimularRechazo == true)
            {
                return new ResultadoPago
                {
                    Aprobado = false,
                    Mensaje = "Transferencia rechazada por la entidad bancaria."
                };
            }

            string refOperacion = !string.IsNullOrWhiteSpace(datos?.Referencia)
                ? datos.Referencia
                : "TRF-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            return new ResultadoPago
            {
                Aprobado = true,
                Mensaje = "Transferencia bancaria validada con éxito.",
                Referencia = refOperacion
            };
        }
    }
}
