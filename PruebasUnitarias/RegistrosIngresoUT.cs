using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class RegistrosIngresoUT
    {
        [TestMethod]
        public void RegistrarIngreso_ConMarcaYModelo_GuardaEnVehiculoYRetornaDto()
        {
            var options = new DbContextOptionsBuilder<Conexion>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var ctx = new Conexion(options);
            ctx.Database.EnsureCreated();
            InicializadorDatos.Inicializar(ctx);

            var registrosNegocio = new RegistrosNegocio(ctx);

            var ticket = registrosNegocio.RegistrarIngreso(
                placa: "NEW999",
                tipoVehiculo: "Automóvil",
                numeroCelda: "A-03",
                ahora: DateTime.Now,
                marca: "Mazda",
                modelo: "CX-30"
            );

            Assert.IsNotNull(ticket);
            Assert.AreEqual("NEW999", ticket.Placa);
            Assert.AreEqual("Mazda", ticket.Marca);
            Assert.AreEqual("CX-30", ticket.Modelo);
            Assert.AreEqual("A-03", ticket.NumeroCelda);

            // Validar en la base de datos (tabla Vehiculos)
            var vehiculo = ctx.Vehiculos!.FirstOrDefault(v => v.Placa == "NEW999");
            Assert.IsNotNull(vehiculo);
            Assert.AreEqual("Mazda", vehiculo.Marca);
            Assert.AreEqual("CX-30", vehiculo.Modelo);
            Assert.AreEqual("Automóvil", vehiculo.TipoVehiculo);
        }

        [TestMethod]
        public void RegistrarIngreso_VehiculoExistenteSinMarca_ActualizaMarcaYModelo()
        {
            var options = new DbContextOptionsBuilder<Conexion>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var ctx = new Conexion(options);
            ctx.Database.EnsureCreated();
            InicializadorDatos.Inicializar(ctx);

            // Supongamos un vehículo sin marca
            var vehiculoInicial = new Vehiculo
            {
                Placa = "TEST01",
                TipoVehiculo = "Automóvil",
                Marca = null,
                Modelo = null
            };
            ctx.Vehiculos!.Add(vehiculoInicial);
            ctx.SaveChanges();

            var registrosNegocio = new RegistrosNegocio(ctx);

            var ticket = registrosNegocio.RegistrarIngreso(
                placa: "TEST01",
                tipoVehiculo: "Automóvil",
                numeroCelda: "A-06",
                ahora: DateTime.Now,
                marca: "Chevrolet",
                modelo: "Onix"
            );

            Assert.IsNotNull(ticket);
            Assert.AreEqual("Chevrolet", ticket.Marca);
            Assert.AreEqual("Onix", ticket.Modelo);

            var vActualizado = ctx.Vehiculos!.FirstOrDefault(v => v.Placa == "TEST01");
            Assert.IsNotNull(vActualizado);
            Assert.AreEqual("Chevrolet", vActualizado.Marca);
            Assert.AreEqual("Onix", vActualizado.Modelo);
        }
    }
}
