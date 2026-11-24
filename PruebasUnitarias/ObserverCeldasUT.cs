using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.ObserverCeldas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class ObserverCeldasUT
    {
        [TestMethod]
        public void Celda_AlCambiarEstado_DebeNotificarObservador()
        {
            var celda = new Celda
            {
                NumeroCelda = "A-01",
                Zona = "A",
                Estado = EstadoCelda.DISPONIBLE
            };

            var observador = new MapaCeldasObservador();
            celda.Suscribir(observador);

            celda.Ocupar();
            Assert.AreEqual(EstadoCelda.OCUPADA, celda.Estado);
            StringAssert.Contains(observador.UltimoCambio, "A-01");
            StringAssert.Contains(observador.UltimoCambio, "OCUPADA");

            celda.Liberar();
            Assert.AreEqual(EstadoCelda.DISPONIBLE, celda.Estado);
            StringAssert.Contains(observador.UltimoCambio, "DISPONIBLE");
        }
    }
}
