using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class VehiculosModel : PageModel
    {
        private readonly IVehiculosNegocioPresentacion _vehiculosNegocioPresentacion;

        [BindProperty]
        public List<Vehiculo>? Lista { get; set; }

        [BindProperty]
        public Vehiculo? Vehiculo { get; set; }

        [BindProperty]
        public bool Borrando { get; set; }

        public VehiculosModel()
        {
            _vehiculosNegocioPresentacion = new VehiculosNegocioPresentacion();
        }

        public IActionResult OnGet()
        {
            OnPostBtRefrescar();
            return Page();
        }

        public void OnPostBtRefrescar()
        {
            try
            {
                Lista = _vehiculosNegocioPresentacion.Consultar();
                Vehiculo = null;
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
        }

        public void OnPostBtNuevo()
        {
            Vehiculo = new Vehiculo
            {
                TipoVehiculo = "Automóvil"
            };
            Borrando = false;
        }

        public void OnPostBtModificar(string data)
        {
            try
            {
                OnPostBtRefrescar();
                Vehiculo = Lista?.FirstOrDefault(x => x.Placa == data);
                Borrando = false;
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
        }

        public void OnPostBtBorrar(string data)
        {
            try
            {
                OnPostBtRefrescar();
                Vehiculo = Lista?.FirstOrDefault(x => x.Placa == data);
                Borrando = true;
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
        }

        public void OnPostBtGuardar()
        {
            try
            {
                if (Vehiculo == null) return;

                if (!Borrando)
                {
                    _vehiculosNegocioPresentacion.Guardar(Vehiculo);
                }
                else
                {
                    _vehiculosNegocioPresentacion.Borrar(Vehiculo.Placa);
                }

                OnPostBtRefrescar();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
        }

        public void OnPostBtCancelar()
        {
            Vehiculo = null;
            Borrando = false;
            OnPostBtRefrescar();
        }
    }
}
