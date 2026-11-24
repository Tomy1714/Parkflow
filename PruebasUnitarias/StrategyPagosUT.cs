using LibParkFlow.Implementaciones.Patrones.StrategyPago;
using LibParkFlow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class StrategyPagosUT
    {
        [TestMethod]
        public void PagoEfectivo_ConMontoSuficiente_DebeAprobarYCalcularCambio()
        {
            var strategy = new PagoEfectivo();
            var datos = new DatosPago { ValorRecibido = 50000m };

            var res = strategy.Procesar(31000m, datos);

            Assert.IsTrue(res.Aprobado);
            Assert.AreEqual(19000m, res.Cambio);
        }

        [TestMethod]
        public void PagoEfectivo_ConMontoInsuficiente_DebeRechazar()
        {
            var strategy = new PagoEfectivo();
            var datos = new DatosPago { ValorRecibido = 20000m };

            var res = strategy.Procesar(31000m, datos);

            Assert.IsFalse(res.Aprobado);
            StringAssert.Contains(res.Mensaje, "insuficiente");
        }

        [TestMethod]
        public void PagoTransferencia_ConReferenciaValida_DebeAprobar()
        {
            var strategy = new PagoTransferencia();
            var datos = new DatosPago { Referencia = "TRF-987654" };

            var res = strategy.Procesar(31000m, datos);

            Assert.IsTrue(res.Aprobado);
            Assert.AreEqual("TRF-987654", res.Referencia);
        }

        [TestMethod]
        public void PagoTarjeta_SimulacionAutorizacion_DebeGenerarCodigoAuth()
        {
            var strategy = new PagoTarjeta();
            var datos = new DatosPago();

            var res = strategy.Procesar(31000m, datos);

            Assert.IsTrue(res.Aprobado);
            Assert.IsNotNull(res.Referencia);
            StringAssert.StartsWith(res.Referencia, "AUTH-");
        }
    }
}
