using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class ReservasVencimientoUT
    {
        [TestMethod]
        public void ReglaRN03_Tolerancia15Minutos_DebeCancelarReservasExpiradas()
        {
            var options = new DbContextOptionsBuilder<Conexion>()
                .UseInMemoryDatabase("Test_RN03_Tolerancia")
                .Options;

            using var ctx = new Conexion(options);
            ctx.Database.EnsureCreated();
            InicializadorDatos.Inicializar(ctx);

            var reservasNegocio = new ReservasNegocio(ctx);
            var celdasNegocio = new CeldasNegocio(ctx);

            // Reserva PQR321 está a las 10:00 en celda A-05
            var celdaA05 = celdasNegocio.BuscarPorNumero("A-05");
            Assert.AreEqual(EstadoCelda.RESERVADA, celdaA05?.Estado);

            // A las 10:10 (10 min transcurridos) -> No debe vencer
            var t1 = DateTime.Today.AddHours(10).AddMinutes(10);
            int canceladas1 = reservasNegocio.VerificarVencimientosPorTolerancia(t1);
            Assert.AreEqual(0, canceladas1);

            // A las 10:16 (16 min transcurridos, tolerancia 15 min superada) -> Debe cancelar y liberar celda
            var t2 = DateTime.Today.AddHours(10).AddMinutes(16);
            int canceladas2 = reservasNegocio.VerificarVencimientosPorTolerancia(t2);
            Assert.IsGreaterThanOrEqualTo(1, canceladas2);

            celdaA05 = celdasNegocio.BuscarPorNumero("A-05");
            Assert.AreEqual(EstadoCelda.DISPONIBLE, celdaA05?.Estado);

            // Verificar registro de auditoría
            var auditoria = reservasNegocio.ConsultarHistorialAuditoria();
            Assert.IsTrue(auditoria.Any(a => a.Placa == "PQR321" && a.Evento.Contains("automáticamente")));
        }
    }
}
