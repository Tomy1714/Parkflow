using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StrategyPago
{
    public class PagoTarjeta : IMetodoPagoStrategy
    {
        public string NombreMetodo => "Tarjeta";

        public ResultadoPago Procesar(decimal monto, DatosPago? datos)
        {
            if (datos?.SimularRechazo == true)
            {
                return new ResultadoPago
                {
                    Aprobado = false,
                    Mensaje = "Transacción con tarjeta denegada: Fondos insuficientes o tarjeta inválida."
                };
            }

            string authCode = "AUTH-" + new Random().Next(100000, 999999);
            return new ResultadoPago
            {
                Aprobado = true,
                Mensaje = "Pago con tarjeta aprobado por la pasarela.",
                Referencia = authCode
            };
        }
    }
}
