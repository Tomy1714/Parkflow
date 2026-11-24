using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StrategyPago
{
    public static class FabricaMetodoPago
    {
        public static IMetodoPagoStrategy ObtenerEstrategia(string metodo)
        {
            return (metodo ?? "").Trim().ToLower() switch
            {
                "efectivo" => new PagoEfectivo(),
                "transferencia" => new PagoTransferencia(),
                "tarjeta" => new PagoTarjeta(),
                _ => new PagoEfectivo()
            };
        }
    }
}
