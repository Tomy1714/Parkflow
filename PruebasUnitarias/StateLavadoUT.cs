using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.StateLavado;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class StateLavadoUT
    {
        [TestMethod]
        public void EstadoLavado_TransicionesSecuenciales_DebenFuncionarCorrectamente()
        {
            var servicio = new ServicioLavado
            {
                IdServicio = 1,
                Placa = "ABC123",
                EstadoLavado = EstadoLavado.PENDIENTE
            };

            var t0 = new DateTime(2026, 11, 1, 9, 0, 0);

            // 1. PENDIENTE -> No puede cobrarse
            Assert.IsFalse(servicio.PuedeCobrarse());
            Assert.AreEqual(EstadoLavado.PENDIENTE, servicio.EstadoLavado);

            // 2. Avanzar -> EN_LAVADO
            servicio.Avanzar(t0.AddMinutes(10));
            Assert.AreEqual(EstadoLavado.EN_LAVADO, servicio.EstadoLavado);
            Assert.AreEqual(t0.AddMinutes(10), servicio.FechaHoraInicio);
            Assert.IsFalse(servicio.PuedeCobrarse());

            // 3. Avanzar -> LISTO
            servicio.Avanzar(t0.AddMinutes(40));
            Assert.AreEqual(EstadoLavado.LISTO, servicio.EstadoLavado);
            Assert.AreEqual(t0.AddMinutes(40), servicio.FechaHoraFin);
            Assert.IsTrue(servicio.PuedeCobrarse());

            // 4. Intentar avanzar estando en LISTO -> Debe lanzar InvalidOperationException
            bool lanzoExcepcion = false;
            try
            {
                servicio.Avanzar(t0.AddMinutes(50));
            }
            catch (InvalidOperationException)
            {
                lanzoExcepcion = true;
            }
            Assert.IsTrue(lanzoExcepcion);
        }
    }
}
