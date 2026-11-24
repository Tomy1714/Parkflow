using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones;
using LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion;
using LibParkFlow.Implementaciones.Patrones.StrategyPago;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class LiquidacionFacadeUT
    {
        private Conexion CrearContextoEnMemoria(string dbName)
        {
            var options = new DbContextOptionsBuilder<Conexion>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var ctx = new Conexion(options);
            ctx.Database.EnsureCreated();
            InicializadorDatos.Inicializar(ctx);
            return ctx;
        }

        [TestMethod]
        public void ReglaRN01_CalculoTarifaParqueadero_DebeCalcularHorasYFracciones()
        {
            var ctx = CrearContextoEnMemoria("Test_RN01");
            var reg = new RegistroParqueadero
            {
                Placa = "TEST01",
                NumeroCelda = "A-01",
                FechaHoraEntrada = new DateTime(2026, 11, 1, 10, 0, 0),
                EstadoRegistro = EstadoRegistro.ACTIVO
            };

            // 1 hora y 7 minutos -> 1 hora ($3.000) + 1 fracción ($750) = $3.750
            var fin = new DateTime(2026, 11, 1, 11, 7, 0);
            var (minutos, horas, fracciones, total) = reg.CalcularTarifaParqueadero(fin);

            Assert.AreEqual(67, minutos);
            Assert.AreEqual(1, horas);
            Assert.AreEqual(1, fracciones);
            Assert.AreEqual(3750m, total);
        }

        [TestMethod]
        public void ReglaRN04_AutorizarSalidaSinPagar_DebeLanzarExcepcion()
        {
            var ctx = CrearContextoEnMemoria("Test_RN04");
            var facade = new LiquidacionFacade(ctx);

            // GHI789 tiene ingreso activo y lavado listo pero NO ha pagado
            bool lanzoExcepcion = false;
            try
            {
                facade.AutorizarSalida("GHI789");
            }
            catch (InvalidOperationException ex)
            {
                lanzoExcepcion = true;
                StringAssert.Contains(ex.Message, "Imposible registrar salida: Cuenta con saldo pendiente de pago");
            }
            Assert.IsTrue(lanzoExcepcion);
        }

        [TestMethod]
        public void Liquidar_ConLavadoEnCurso_DebeRechazarCobro()
        {
            var ctx = CrearContextoEnMemoria("Test_LavadoEnCurso");
            var facade = new LiquidacionFacade(ctx);

            // KTM456 tiene lavado en estado EN_LAVADO
            bool lanzoExcepcion = false;
            try
            {
                facade.Liquidar("KTM456", new PagoEfectivo(), new DatosPago { ValorRecibido = 50000 }, CanalPago.CAJA);
            }
            catch (InvalidOperationException ex)
            {
                lanzoExcepcion = true;
                StringAssert.Contains(ex.Message, "Hay un lavado en curso");
            }
            Assert.IsTrue(lanzoExcepcion);
        }

        [TestMethod]
        public void FlujoCompleto_LiquidarYAutorizarSalida_DebeLiberarCelda()
        {
            var ctx = CrearContextoEnMemoria("Test_FlujoSalida");
            var facade = new LiquidacionFacade(ctx);
            var regNegocio = new RegistrosNegocio(ctx);
            var celdasNegocio = new CeldasNegocio(ctx);

            // 1. Ingreso
            var entrada = new DateTime(2026, 11, 1, 9, 0, 0);
            regNegocio.RegistrarIngreso("FLUX01", "Automóvil", "A-12", entrada);

            var celda = celdasNegocio.BuscarPorNumero("A-12");
            Assert.AreEqual(EstadoCelda.OCUPADA, celda?.Estado);

            // 2. Liquidación
            var horaPago = entrada.AddHours(2);
            var comprobante = facade.Liquidar("FLUX01", new PagoEfectivo(), new DatosPago { ValorRecibido = 10000 }, CanalPago.CAJA, ahora: horaPago);
            Assert.IsNotNull(comprobante);
            Assert.AreEqual("Efectivo", comprobante.MetodoPago);

            // Celda sigue ocupada tras pagar
            celda = celdasNegocio.BuscarPorNumero("A-12");
            Assert.AreEqual(EstadoCelda.OCUPADA, celda?.Estado);

            // 3. Salida
            facade.AutorizarSalida("FLUX01", horaPago.AddMinutes(5));

            // Celda liberada
            celda = celdasNegocio.BuscarPorNumero("A-12");
            Assert.AreEqual(EstadoCelda.DISPONIBLE, celda?.Estado);
        }
    }
}
