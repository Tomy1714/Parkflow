using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.FactoryMethodRoles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PruebasUnitarias
{
    [TestClass]
    public class FactoryMethodRolesUT
    {
        [TestMethod]
        public void CreadoresRol_DebenInstanciarSubclaseCorrespondiente()
        {
            var cAdmin = new CreadorRolAdministrador();
            var rolAdmin = cAdmin.CrearRol();
            Assert.IsInstanceOfType(rolAdmin, typeof(RolAdministrador));
            Assert.AreEqual(RolUsuario.ADMINISTRADOR, rolAdmin.Tipo);
            CollectionAssert.Contains(rolAdmin.OperacionesPermitidas(), "consultarIngresosDiarios");

            var cOperador = new CreadorRolOperador();
            var rolOperador = cOperador.CrearRol();
            Assert.IsInstanceOfType(rolOperador, typeof(RolOperador));
            Assert.AreEqual(RolUsuario.OPERADOR, rolOperador.Tipo);
            CollectionAssert.Contains(rolOperador.OperacionesPermitidas(), "registrarIngresoVehiculo");

            var cCajero = new CreadorRolCajero();
            var rolCajero = cCajero.CrearRol();
            Assert.IsInstanceOfType(rolCajero, typeof(RolCajero));
            Assert.AreEqual(RolUsuario.CAJERO, rolCajero.Tipo);
            CollectionAssert.Contains(rolCajero.OperacionesPermitidas(), "procesarCobro");

            var cLavador = new CreadorRolLavador();
            var rolLavador = cLavador.CrearRol();
            Assert.IsInstanceOfType(rolLavador, typeof(RolLavador));
            Assert.AreEqual(RolUsuario.LAVADOR, rolLavador.Tipo);
            CollectionAssert.Contains(rolLavador.OperacionesPermitidas(), "actualizarEstadoLavado");

            var cCliente = new CreadorRolCliente();
            var rolCliente = cCliente.CrearRol();
            Assert.IsInstanceOfType(rolCliente, typeof(RolCliente));
            Assert.AreEqual(RolUsuario.CLIENTE, rolCliente.Tipo);
            CollectionAssert.Contains(rolCliente.OperacionesPermitidas(), "pagarEnLinea");
        }
    }
}
