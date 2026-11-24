using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StrategyPago
{
    public class PagoEfectivo : IMetodoPagoStrategy
    {
        public string NombreMetodo => "Efectivo";

        public ResultadoPago Procesar(decimal monto, DatosPago? datos)
        {
            if (datos == null || datos.ValorRecibido < monto)
            {
                return new ResultadoPago
                {
                    Aprobado = false,
                    Mensaje = $"Valor recibido insuficiente. Total: ${monto:N0}, recibido: ${datos?.ValorRecibido ?? 0:N0}",
                    Cambio = 0
                };
            }

            decimal cambio = datos.ValorRecibido - monto;
            return new ResultadoPago
            {
                Aprobado = true,
                Mensaje = "Pago en efectivo procesado con éxito.",
                Cambio = cambio,
                Referencia = "EFECTIVO-" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
        }
    }
}
