'use strict';

/* =====================================================================
   ParkFlow Central — Front del MVP
   Sin backend ni base de datos: el estado vive en memoria y se reinicia
   al recargar la página. Las reglas (RN-01 a RN-05) y los criterios de
   aceptación de HU-01 a HU-04 siguen lo definido en el Entregable 1.
   ===================================================================== */

/* ---------- Reglas de negocio y constantes ---------- */
const TARIFA_HORA = 3000;                    // $3.000 por hora
const TARIFA_FRACCION = TARIFA_HORA / 4;     // RN-01: fracción de 15 minutos
const MINUTOS_FRACCION = 15;
const UMBRAL_FRACCION = 5;                   // RN-01: desde 5 min se cobra fracción completa
const TOLERANCIA_RESERVA = 15;               // RN-03: tolerancia de 15 minutos
const PRECIO_LAVADO_GENERAL = 25000;

const PERMISOS = {
  Administrador: ['dashboard', 'ingreso', 'reservas', 'lavado', 'caja'],
  Operador: ['dashboard', 'ingreso', 'reservas', 'lavado'],
  Cajero: ['dashboard', 'caja'],
  Lavador: ['lavado'],
  Cliente: ['c-panel', 'c-reservas', 'c-lavado', 'c-cuenta']
};
const CODIGOS = { Administrador: 'AD-1001', Operador: 'OP-2094', Cajero: 'CJ-3310', Lavador: 'LV-4125', Cliente: 'CL-5001' };
const PANTALLAS = {
  dashboard: ['Dashboard Operativo', 'Mapa de celdas en tiempo real y estadísticas del día'],
  ingreso:   ['Registro de Ingreso', 'Registra la entrada del vehículo y asígnale una celda'],
  reservas:  ['Reservas Activas', 'Control de reservas y vencimiento por tolerancia'],
  lavado:    ['Control de Lavado', 'Seguimiento del servicio: Pendiente → En Lavado → Listo'],
  caja:      ['Liquidación & Caja', 'Cobro consolidado de parqueadero y lavado'],
  'c-panel':    ['Mi Panel', 'Estado de tus vehículos y de tus servicios'],
  'c-reservas': ['Mis Reservas', 'Crea, consulta y cancela tus reservas de celda'],
  'c-lavado':   ['Mi Lavado', 'Solicita el lavado de tu vehículo y sigue su avance'],
  'c-cuenta':   ['Mi Cuenta y Pagos', 'Consulta tu cuenta y págala en línea']
};
const ETIQUETA_CELDA = { DISPONIBLE: 'DISPONIBLE', OCUPADA: 'OCUPADA', RESERVADA: 'RESERVADA', MANTENIMIENTO: 'MANTENIMIENTO' };

/* ---------- Utilidades ---------- */
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const normPlaca = (v) => (v || '').toUpperCase().replace(/[^A-Z0-9]/g, '');
const dinero = (n) => '$' + String(Math.round(n)).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
const dos = (n) => String(n).padStart(2, '0');
function hora12(d) {
  let h = d.getHours();
  const ap = h >= 12 ? 'PM' : 'AM';
  h = h % 12 || 12;
  return dos(h) + ':' + dos(d.getMinutes()) + ' ' + ap;
}
const hora24 = (d) => dos(d.getHours()) + ':' + dos(d.getMinutes()) + ':' + dos(d.getSeconds());
const duracion = (min) => Math.floor(min / 60) + ' h ' + dos(min % 60) + ' min';

/* ---------- Reloj del sistema (modo demostración) ---------- */
const Reloj = {
  desfase: 0,
  iniciar() { const t = new Date(); t.setHours(9, 42, 0, 0); this.desfase = t.getTime() - Date.now(); },
  ahora() { return new Date(Date.now() + this.desfase); },
  avanzar(min) { this.desfase += min * 60000; },
  hoy(h, m) { const d = this.ahora(); d.setHours(h, m, 0, 0); return d; }
};

/* ---------- Estado en memoria ---------- */
let estado;
let ui = { pantalla: 'dashboard', celdaSel: null, cuentaPlaca: '', metodo: 'Efectivo' };

function crearEstadoInicial() {
  const celdas = [];
  [['A', 12], ['B', 10], ['C', 8]].forEach(([zona, n]) => {
    for (let i = 1; i <= n; i++) celdas.push({ id: zona + '-' + dos(i), zona, estado: 'DISPONIBLE', registroId: null, reservaId: null });
  });
  const e = { usuario: null, clientes: [{ codigo: 'CL-5001', placas: ['ABC123', 'XYZ789'] }], celdas, registros: [], reservas: [], auditoria: [], lavados: [], comprobantes: [], caja: { apertura: Reloj.ahora(), movimientos: [] }, seq: { registro: 1, reserva: 1, lavado: 1, factura: 1 } };

  const celda = (id) => e.celdas.find((c) => c.id === id);
  const ocupar = (placa, tipo, celdaId, h, m) => {
    const r = { id: e.seq.registro++, placa, tipo, celdaId, entrada: Reloj.hoy(h, m), salida: null, pagadoEn: null, estado: 'ACTIVO', reservaId: null };
    e.registros.push(r); celda(celdaId).estado = 'OCUPADA'; celda(celdaId).registroId = r.id; return r;
  };
  const reg = {
    DEF456: ocupar('DEF456', 'Automóvil', 'A-02', 9, 10),
    KTM456: ocupar('KTM456', 'Moto', 'B-01', 8, 30),
    GHI789: ocupar('GHI789', 'Automóvil', 'B-03', 8, 50)
  };
  ocupar('JKL012', 'Automóvil', 'B-05', 9, 20);
  ocupar('MNB321', 'Moto', 'A-07', 9, 30);
  ocupar('PLM908', 'Automóvil', 'A-09', 8, 5);
  ocupar('RTY567', 'Automóvil', 'C-04', 9, 0);

  const reservar = (placa, celdaId, h, m) => {
    const r = { id: e.seq.reserva++, placa, celdaId, hora: Reloj.hoy(h, m), estado: 'ACTIVA' };
    e.reservas.push(r); celda(celdaId).estado = 'RESERVADA'; celda(celdaId).reservaId = r.id;
  };
  reservar('PQR321', 'A-05', 10, 0);
  reservar('XYZ789', 'C-01', 10, 0);
  reservar('MNO456', 'B-02', 14, 0);

  celda('A-10').estado = 'MANTENIMIENTO';
  celda('B-08').estado = 'MANTENIMIENTO';

  const lavar = (placa, estadoL, h, m) => {
    const l = { id: e.seq.lavado++, placa, registroId: reg[placa].id, tipo: 'Lavado General', precio: PRECIO_LAVADO_GENERAL, estado: estadoL, solicitado: Reloj.hoy(h, m), inicio: null, fin: null, facturado: false };
    if (estadoL !== 'PENDIENTE') l.inicio = Reloj.hoy(h, m + 10);
    if (estadoL === 'LISTO') l.fin = Reloj.hoy(h + 1, m);
    e.lavados.push(l);
  };
  lavar('GHI789', 'LISTO', 8, 55);
  lavar('KTM456', 'EN_LAVADO', 8, 35);
  lavar('DEF456', 'PENDIENTE', 9, 15);
  return e;
}

/* =====================================================================
   LÓGICA DE NEGOCIO
   ===================================================================== */

/** RN-01: cobro por hora y fracciones de 15 min; desde 5 min se cobra fracción completa. */
function calcularTarifa(entrada, salida) {
  const minutos = Math.max(0, Math.floor((salida - entrada) / 60000));
  const horas = Math.floor(minutos / 60);
  const resto = minutos % 60;
  const fracciones = resto >= UMBRAL_FRACCION ? Math.ceil(resto / MINUTOS_FRACCION) : 0;
  return { minutos, horas, fracciones, valor: horas * TARIFA_HORA + fracciones * TARIFA_FRACCION };
}

const buscarCelda = (id) => estado.celdas.find((c) => c.id === id);
const registroActivo = (placa) => estado.registros.find((r) => r.placa === placa && r.estado === 'ACTIVO');
const registroAbierto = (placa) => [...estado.registros].reverse().find((r) => r.placa === placa && r.estado !== 'FINALIZADO');
/** Reserva activa de la placa que todavía está dentro del margen de tolerancia (RN-03). */
function reservaVigente(placa) {
  const limite = TOLERANCIA_RESERVA * 60000;
  return estado.reservas.find((r) => r.placa === placa && r.estado === 'ACTIVA' && Reloj.ahora() - r.hora <= limite) || null;
}

/* ---------- HU-01: Registro de ingreso ---------- */
function registrarIngreso({ placa, tipo, celdaId }) {
  placa = normPlaca(placa);
  if (placa.length < 6) return { ok: false, campo: 'placa', msg: 'Por favor ingrese un número de placa válido antes de continuar' };
  if (registroActivo(placa)) return { ok: false, campo: 'placa', msg: 'El vehículo ya tiene un ingreso activo' };

  const reserva = reservaVigente(placa);
  if (reserva) celdaId = reserva.celdaId;                      // Criterio 4: autocompleta la celda reservada
  const celda = buscarCelda(celdaId);
  if (!celda) return { ok: false, campo: 'celda', msg: 'Seleccione una celda' };
  if (!reserva && celda.estado !== 'DISPONIBLE') return { ok: false, campo: 'celda', msg: 'La celda seleccionada no está disponible' };   // Criterio 2

  const r = { id: estado.seq.registro++, placa, tipo, celdaId: celda.id, entrada: Reloj.ahora(), salida: null, pagadoEn: null, estado: 'ACTIVO', reservaId: reserva ? reserva.id : null };
  estado.registros.push(r);
  celda.estado = 'OCUPADA'; celda.registroId = r.id; celda.reservaId = null;
  if (reserva) reserva.estado = 'ATENDIDA';                    // la reserva queda vinculada y atendida
  return { ok: true, registro: r, reserva };
}

/* ---------- HU-02: Reservas ---------- */
function crearReserva({ placa, celdaId, hora }) {
  placa = normPlaca(placa);
  if (placa.length < 6) return { ok: false, msg: 'Ingrese una placa válida (mínimo 6 caracteres)' };
  if (estado.reservas.some((r) => r.placa === placa && r.estado === 'ACTIVA')) return { ok: false, msg: 'La placa ya tiene una reserva activa' };
  const celda = buscarCelda(celdaId);
  if (!celda || celda.estado !== 'DISPONIBLE') return { ok: false, msg: 'La celda seleccionada no está disponible' };
  const [h, m] = (hora || '').split(':').map(Number);
  if (isNaN(h) || isNaN(m)) return { ok: false, msg: 'Indique la hora de la reserva' };
  const fecha = Reloj.hoy(h, m);
  if (Reloj.ahora() - fecha > TOLERANCIA_RESERVA * 60000) return { ok: false, msg: 'La hora de la reserva ya pasó' };
  const r = { id: estado.seq.reserva++, placa, celdaId, hora: fecha, estado: 'ACTIVA' };
  estado.reservas.push(r);
  celda.estado = 'RESERVADA'; celda.reservaId = r.id;
  return { ok: true, reserva: r };
}

function cancelarReserva(id, quien) {
  const r = estado.reservas.find((x) => x.id === id);
  if (!r || r.estado !== 'ACTIVA') return false;
  r.estado = 'CANCELADA_MANUAL';
  liberarCeldaReservada(r);
  estado.auditoria.unshift({ fecha: Reloj.ahora(), placa: r.placa, celdaId: r.celdaId, evento: quien === 'cliente' ? 'Cancelada por el cliente' : 'Cancelada manualmente' });
  return true;
}

function liberarCeldaReservada(r) {
  const c = buscarCelda(r.celdaId);
  if (c && c.estado === 'RESERVADA') { c.estado = 'DISPONIBLE'; c.reservaId = null; }
}

/** Criterio 1 de HU-02: cancela por inasistencia las reservas que superan la tolerancia. Devuelve las canceladas. */
function verificarVencimientos() {
  const limite = TOLERANCIA_RESERVA * 60000;
  const ahora = Reloj.ahora();
  const vencidas = estado.reservas.filter((r) => r.estado === 'ACTIVA' && ahora - r.hora > limite);
  vencidas.forEach((r) => {
    r.estado = 'CANCELADA_INASISTENCIA';
    liberarCeldaReservada(r);
    estado.auditoria.unshift({ fecha: ahora, placa: r.placa, celdaId: r.celdaId, evento: 'Cancelada automáticamente por Sistema' });
  });
  return vencidas;
}

/* ---------- HU-03: Lavado ---------- */
const SIGUIENTE_ESTADO = { PENDIENTE: 'EN_LAVADO', EN_LAVADO: 'LISTO' };

function solicitarLavado(placaRaw) {
  const placa = normPlaca(placaRaw);
  const reg = placa.length >= 6 ? registroActivo(placa) : null;
  if (!reg) return { ok: false, msg: 'No se puede asignar un lavado a un vehículo que no cuenta con un registro de parqueadero activo' };   // Criterio 2 (RN-05)
  if (estado.lavados.some((l) => l.registroId === reg.id && l.estado !== 'LISTO')) return { ok: false, msg: 'El vehículo ya tiene un lavado en curso' };
  const l = { id: estado.seq.lavado++, placa, registroId: reg.id, tipo: 'Lavado General', precio: PRECIO_LAVADO_GENERAL, estado: 'PENDIENTE', solicitado: Reloj.ahora(), inicio: null, fin: null, facturado: false };
  estado.lavados.push(l);
  return { ok: true, lavado: l };
}

/** Criterio 3: solo se permite la transición secuencial Pendiente → En Lavado → Listo. */
function avanzarLavado(id) {
  const l = estado.lavados.find((x) => x.id === id);
  if (!l || !SIGUIENTE_ESTADO[l.estado]) return { ok: false };
  l.estado = SIGUIENTE_ESTADO[l.estado];
  if (l.estado === 'EN_LAVADO') l.inicio = Reloj.ahora();
  if (l.estado === 'LISTO') l.fin = Reloj.ahora();         // Criterio 1: registra hora de finalización y notifica a caja
  return { ok: true, lavado: l };
}

/* ---------- HU-04: Liquidación y cobro ---------- */
function cuentaDe(placa) {
  const reg = registroAbierto(placa);
  if (!reg) return null;
  const fin = reg.estado === 'PAGADO' ? reg.pagadoEn : Reloj.ahora();
  const parqueadero = calcularTarifa(reg.entrada, fin);
  const lavados = estado.lavados.filter((l) => l.registroId === reg.id);
  const facturables = lavados.filter((l) => l.estado === 'LISTO' && !l.facturado);
  const enCurso = lavados.filter((l) => l.estado !== 'LISTO');
  const total = parqueadero.valor + facturables.reduce((s, l) => s + l.precio, 0);
  return { registro: reg, fin, parqueadero, facturables, enCurso, total };
}

function confirmarPago(placa, metodo, canal) {
  canal = canal || 'Caja';
  const c = cuentaDe(placa);
  if (!c) return { ok: false, msg: 'No existe una cuenta abierta para la placa ' + placa };
  if (c.registro.estado === 'PAGADO') return { ok: false, msg: 'Esta cuenta ya fue pagada' };
  if (c.enCurso.length) return { ok: false, msg: 'Hay un lavado en curso: debe estar en estado Listo antes de cobrar' };
  const comp = {
    factura: 'FAC-' + String(estado.seq.factura++).padStart(4, '0'),
    registroId: c.registro.id, placa, fecha: Reloj.ahora(), metodo, canal,
    parqueadero: c.parqueadero, lavados: c.facturables.map((l) => ({ tipo: l.tipo, precio: l.precio })), total: c.total
  };
  estado.comprobantes.push(comp);
  c.facturables.forEach((l) => { l.facturado = true; });
  c.registro.estado = 'PAGADO'; c.registro.pagadoEn = comp.fecha;
  const celda = buscarCelda(c.registro.celdaId);          // Criterio 4: la celda pasa a Disponible
  celda.estado = 'DISPONIBLE'; celda.registroId = null;
  estado.caja.movimientos.unshift({ factura: comp.factura, placa, total: comp.total, metodo: metodo + (canal === 'En línea' ? ' (en línea)' : ''), fecha: comp.fecha });
  return { ok: true, comprobante: comp };
}

/** Criterio 2 (RN-04): no se autoriza la salida si la cuenta no está pagada. */
function autorizarSalida(placa) {
  const reg = registroAbierto(placa);
  if (!reg) return { ok: false, msg: 'No existe una cuenta abierta para la placa ' + placa };
  if (reg.estado !== 'PAGADO') return { ok: false, msg: 'Imposible registrar salida: Cuenta con saldo pendiente de pago' };
  reg.salida = Reloj.ahora(); reg.estado = 'FINALIZADO';
  return { ok: true, registro: reg };
}

/* ---------- HU-05: Autogestión del cliente ---------- */
const clienteActual = () => estado.clientes.find((c) => c.codigo === estado.usuario.codigo);
const duenoDe = (placa) => estado.clientes.find((c) => c.placas.includes(placa)) || null;
const reservasDeCliente = (cli) => estado.reservas.filter((r) => cli.placas.includes(r.placa)).sort((a, b) => a.hora - b.hora);
const lavadosDeCliente = (cli) => estado.lavados.filter((l) => cli.placas.includes(l.placa)).sort((a, b) => b.solicitado - a.solicitado);

function agregarVehiculo(cliente, placaRaw) {
  const placa = normPlaca(placaRaw);
  if (placa.length < 6) return { ok: false, msg: 'Ingrese una placa válida (mínimo 6 caracteres)' };
  if (cliente.placas.includes(placa)) return { ok: false, msg: 'Este vehículo ya está en tu lista' };
  if (duenoDe(placa)) return { ok: false, msg: 'La placa ya está asociada a otro cliente' };
  cliente.placas.push(placa);
  return { ok: true, placa };
}

/** Criterio 2: el cliente solo puede cancelar reservas de sus propios vehículos. */
function cancelarReservaCliente(cliente, id) {
  const r = estado.reservas.find((x) => x.id === id);
  if (!r || !cliente.placas.includes(r.placa)) return false;
  return cancelarReserva(id, 'cliente');
}

function estadoVehiculo(placa) {
  const reg = registroAbierto(placa);
  const reserva = estado.reservas.find((r) => r.placa === placa && r.estado === 'ACTIVA') || null;
  const lavados = reg ? estado.lavados.filter((l) => l.registroId === reg.id) : [];
  return { reg, reserva, lavado: lavados.length ? lavados[lavados.length - 1] : null };
}

/* =====================================================================
   INTERFAZ
   ===================================================================== */
let toastTimer;
function toast(msg, tipo) {
  const t = $('toast');
  t.textContent = msg;
  t.className = 'toast show' + (tipo === 'error' ? ' error' : '');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { t.className = 'toast'; }, 3200);
}

function mostrarError(id, msg) {
  const el = $(id);
  el.textContent = msg || '';
  el.hidden = !msg;
}

/* ---------- Navegación y sesión ---------- */
function irA(pantalla) {
  if (!estado.usuario || !PERMISOS[estado.usuario.rol].includes(pantalla)) return;
  ui.pantalla = pantalla;
  document.querySelectorAll('.screen').forEach((s) => s.classList.toggle('active', s.id === 'pantalla-' + pantalla));
  document.querySelectorAll('.nav-item').forEach((b) => b.classList.toggle('active', b.dataset.pantalla === pantalla));
  $('titulo-pantalla').textContent = PANTALLAS[pantalla][0];
  $('subtitulo-pantalla').textContent = PANTALLAS[pantalla][1];
  render();
}

function iniciarSesion() {
  const rol = document.querySelector('#roles .role-chip.sel').dataset.rol;
  const codigo = $('login-codigo').value.trim();
  const clave = $('login-clave').value;
  if (!codigo || !clave) { mostrarError('login-error', 'Ingrese su código y contraseña para continuar'); return; }
  mostrarError('login-error', '');
  estado.usuario = { rol, codigo };
  if (rol === 'Cliente' && !estado.clientes.some((c) => c.codigo === codigo)) estado.clientes.push({ codigo, placas: [] });
  ui = { pantalla: 'dashboard', celdaSel: null, cuentaPlaca: '', metodo: 'Efectivo', metodoCli: 'Tarjeta' };
  $('vista-login').hidden = true;
  $('vista-app').hidden = false;
  $('user-nombre').textContent = rol;
  $('user-codigo').textContent = codigo;
  $('user-avatar').textContent = rol[0];
  document.querySelectorAll('.nav-item').forEach((b) => { b.hidden = !PERMISOS[rol].includes(b.dataset.pantalla); });
  irA(rol === 'Lavador' ? 'lavado' : rol === 'Cliente' ? 'c-panel' : 'dashboard');
}

function reiniciarDemo() {
  Reloj.iniciar();
  estado = crearEstadoInicial();
  toast('Datos de demostración reiniciados');
}

function cerrarSesion() {
  estado.usuario = null;
  $('vista-app').hidden = true;
  $('vista-login').hidden = false;
}

/* ---------- Render: dashboard ---------- */
function renderDashboard() {
  const cuenta = (est) => estado.celdas.filter((c) => c.estado === est).length;
  const total = estado.celdas.length;
  const kpi = (cls, lbl, n) => '<div class="kpi ' + cls + '"><div class="lbl">' + lbl + '</div><div class="val">' + n +
    (lbl === 'TOTAL CELDAS' ? '' : '<span class="pct">(' + Math.round((n / total) * 100) + '%)</span>') + '</div></div>';
  $('kpis').innerHTML = kpi('', 'TOTAL CELDAS', total) + kpi('k-green', 'DISPONIBLE', cuenta('DISPONIBLE')) +
    kpi('k-blue', 'OCUPADA', cuenta('OCUPADA')) + kpi('k-amber', 'RESERVADA', cuenta('RESERVADA')) + kpi('k-gray', 'MANTENIMIENTO', cuenta('MANTENIMIENTO'));

  $('mapa').innerHTML = ['A', 'B', 'C'].map((z) =>
    '<div class="zona"><div class="zona-title">ZONA ' + z + '</div><div class="cell-grid">' +
    estado.celdas.filter((c) => c.zona === z).map((c) =>
      '<div class="cell ' + c.estado + (ui.celdaSel === c.id ? ' sel' : '') + '" data-celda="' + c.id + '"><div class="code">' + c.id + '</div><div class="st">' + ETIQUETA_CELDA[c.estado] + '</div></div>').join('') +
    '</div></div>').join('');

  const d = $('detalle-celda');
  const c = ui.celdaSel && buscarCelda(ui.celdaSel);
  if (!c) { d.textContent = 'Selecciona una celda para ver su detalle.'; return; }
  let txt = '<b>Celda ' + c.id + '</b> · ' + c.estado.toLowerCase();
  if (c.estado === 'OCUPADA') {
    const r = estado.registros.find((x) => x.id === c.registroId);
    if (r) txt += ' — ' + esc(r.placa) + ' (' + r.tipo + '), ingreso ' + hora12(r.entrada);
  } else if (c.estado === 'RESERVADA') {
    const r = estado.reservas.find((x) => x.id === c.reservaId);
    if (r) txt += ' — reserva de ' + esc(r.placa) + ' para las ' + hora12(r.hora);
  }
  d.innerHTML = txt;
}

/* ---------- Render: ingreso ---------- */
function renderIngreso() {
  const sel = $('ing-celda');
  const previa = sel.value || 'A-12';
  sel.innerHTML = estado.celdas.map((c) => '<option value="' + c.id + '">' + c.id + ' · ' + c.estado.charAt(0) + c.estado.slice(1).toLowerCase() + '</option>').join('');
  sel.value = previa;
  actualizarReservaChip();

  const ult = [...estado.registros].reverse().slice(0, 6);
  $('ultimos-ingresos').innerHTML = ult.length ?
    '<div class="table-wrap"><table><tr><th>PLACA</th><th>CELDA</th><th>INGRESO</th><th>ESTADO</th></tr>' +
    ult.map((r) => '<tr><td><b>' + esc(r.placa) + '</b></td><td>' + r.celdaId + '</td><td>' + hora12(r.entrada) + '</td><td>' + tagRegistro(r.estado) + '</td></tr>').join('') +
    '</table></div>' : '<div class="empty">Aún no hay ingresos registrados.</div>';
}

function tagRegistro(e) {
  return { ACTIVO: '<span class="tag tag-blue">En parqueadero</span>', PAGADO: '<span class="tag tag-green">Pagado</span>', FINALIZADO: '<span class="tag tag-gray">Salida registrada</span>' }[e];
}

/** Criterio 4 de HU-01: al escribir una placa con reserva vigente, se autocompleta y bloquea la celda. */
function actualizarReservaChip() {
  const placa = normPlaca($('ing-placa').value);
  const chip = $('ing-reserva');
  const sel = $('ing-celda');
  const r = placa.length >= 6 ? reservaVigente(placa) : null;
  if (r) {
    sel.value = r.celdaId; sel.disabled = true;
    chip.hidden = false;
    chip.textContent = 'Reserva activa encontrada: celda ' + r.celdaId + ' · ' + hora12(r.hora) + '. Se vinculará al ingreso.';
  } else {
    sel.disabled = false; chip.hidden = true;
  }
}

function limpiarIngreso() {
  $('ing-placa').value = '';
  $('ing-placa').classList.remove('invalid'); $('ing-celda').classList.remove('invalid');
  mostrarError('ing-placa-err', ''); mostrarError('ing-celda-err', '');
  $('ing-celda').value = 'A-12';
  actualizarReservaChip();
}

function confirmarIngreso() {
  mostrarError('ing-placa-err', ''); mostrarError('ing-celda-err', '');
  $('ing-placa').classList.remove('invalid'); $('ing-celda').classList.remove('invalid');
  const res = registrarIngreso({ placa: $('ing-placa').value, tipo: $('ing-tipo').value, celdaId: $('ing-celda').value });
  if (!res.ok) {
    $(res.campo === 'placa' ? 'ing-placa' : 'ing-celda').classList.add('invalid');
    mostrarError(res.campo === 'placa' ? 'ing-placa-err' : 'ing-celda-err', res.msg);
    return;
  }
  const r = res.registro;
  limpiarIngreso();
  render();
  abrirModal('<div class="ticket"><div class="cap">TICKET DE INGRESO</div><div class="plate">' + esc(r.placa) + '</div>' +
    '<div class="row"><span>Celda</span><b>' + r.celdaId + '</b></div><div class="row"><span>Tipo</span><b>' + r.tipo + '</b></div>' +
    '<div class="row"><span>Hora de ingreso</span><b>' + hora12(r.entrada) + '</b></div>' +
    (res.reserva ? '<div class="row"><span>Reserva</span><b>Vinculada · Atendida</b></div>' : '') +
    '<div class="row"><span>Estado de la celda</span><b style="color:var(--blue)">Ocupada</b></div></div>' +
    '<div class="btn-row"><button class="btn btn-outline" data-action="imprimir">Imprimir</button><button class="btn btn-primary" data-action="cerrar-modal">Cerrar</button></div>');
  toast('Ingreso registrado correctamente');
}

function abrirModal(html) { $('modal').innerHTML = '<div class="modal-box">' + html + '</div>'; $('modal').hidden = false; }
function cerrarModal() { $('modal').hidden = true; $('modal').innerHTML = ''; }

/* ---------- Render: reservas ---------- */
const ETIQUETA_RESERVA = {
  ACTIVA: '<span class="tag tag-amber">Activa</span>',
  ATENDIDA: '<span class="tag tag-green">Atendida</span>',
  CANCELADA_INASISTENCIA: '<span class="tag tag-red">Cancelada por Inasistencia</span>',
  CANCELADA_MANUAL: '<span class="tag tag-gray">Cancelada</span>'
};
function renderReservas() {
  const filas = [...estado.reservas].sort((a, b) => a.hora - b.hora);
  $('tabla-reservas').innerHTML = filas.length ?
    '<div class="table-wrap"><table><tr><th>PLACA</th><th>CELDA</th><th>HORA</th><th>ESTADO</th><th></th></tr>' +
    filas.map((r) => '<tr><td><b>' + esc(r.placa) + '</b></td><td>' + r.celdaId + '</td><td>' + hora12(r.hora) + '</td><td>' + ETIQUETA_RESERVA[r.estado] + '</td><td>' +
      (r.estado === 'ACTIVA' ? '<button class="btn btn-danger btn-sm" data-action="cancelar-reserva" data-id="' + r.id + '">Cancelar</button>' : '') + '</td></tr>').join('') +
    '</table></div>' : '<div class="empty">No hay reservas registradas.</div>';

  const sel = $('res-celda');
  const previa = sel.value;
  sel.innerHTML = estado.celdas.filter((c) => c.estado === 'DISPONIBLE').map((c) => '<option value="' + c.id + '">' + c.id + '</option>').join('');
  if (previa) sel.value = previa;

  $('tabla-auditoria').innerHTML = estado.auditoria.length ?
    '<div class="table-wrap"><table><tr><th>FECHA Y HORA</th><th>PLACA</th><th>CELDA LIBERADA</th><th>EVENTO</th></tr>' +
    estado.auditoria.map((a) => '<tr><td>' + hora24(a.fecha) + '</td><td><b>' + esc(a.placa) + '</b></td><td>' + a.celdaId + '</td><td>' + a.evento + '</td></tr>').join('') +
    '</table></div>' : '<div class="empty">Sin eventos todavía. Las cancelaciones aparecerán aquí con su sello de tiempo.</div>';
}

/* ---------- Render: lavado ---------- */
function renderLavado() {
  const cols = [['PENDIENTE', 'PENDIENTE', 'amber'], ['EN_LAVADO', 'EN LAVADO', 'blue'], ['LISTO', 'LISTO', 'green']];
  $('kanban').innerHTML = cols.map(([est, titulo, color]) => {
    const items = estado.lavados.filter((l) => l.estado === est).sort((a, b) => a.solicitado - b.solicitado);
    return '<div class="kcol"><div class="kcol-title ' + color + '"><span>● ' + titulo + '</span><span>' + items.length + '</span></div>' +
      (items.map((l) => {
        let info = 'Solicitado ' + hora12(l.solicitado);
        if (l.inicio && est !== 'PENDIENTE') info += ' · Inicio ' + hora12(l.inicio);
        if (l.fin) info += ' · Fin ' + hora12(l.fin);
        const accion = est === 'PENDIENTE' ? '<button class="btn btn-outline btn-sm btn-block" data-action="avanzar-lavado" data-id="' + l.id + '">Iniciar lavado</button>' :
          est === 'EN_LAVADO' ? '<button class="btn btn-primary btn-sm btn-block" data-action="avanzar-lavado" data-id="' + l.id + '">Marcar como Listo</button>' :
          '<span class="tag tag-green">Enviado a Caja</span>';
        return '<div class="kcard"><div class="plate">' + esc(l.placa) + '</div><div class="svc">' + l.tipo + '<br>' + info + '</div>' + accion + '</div>';
      }).join('') || '<div class="empty">Sin servicios.</div>') + '</div>';
  }).join('');
}

/* ---------- Render: caja ---------- */
function renderCaja() {
  let html = '';
  const placa = ui.cuentaPlaca;
  if (placa) {
    const c = cuentaDe(placa);
    if (!c) html += '<div class="msg-error">No existe una cuenta abierta para la placa ' + esc(placa) + '</div>';
    else html += vistaCuenta(c);
  }
  const abiertas = estado.registros.filter((r) => r.estado !== 'FINALIZADO');
  html += '<div class="cuenta"><div class="card-head"><h3>Cuentas abiertas</h3></div>' + (abiertas.length ?
    '<div class="table-wrap"><table><tr><th>PLACA</th><th>CELDA</th><th>INGRESO</th><th>ESTADO</th><th></th></tr>' +
    abiertas.map((r) => '<tr><td><b>' + esc(r.placa) + '</b></td><td>' + r.celdaId + '</td><td>' + hora12(r.entrada) + '</td><td>' + tagRegistro(r.estado) + '</td><td><button class="btn btn-outline btn-sm" data-action="ver-cuenta" data-placa="' + esc(r.placa) + '">Consultar</button></td></tr>').join('') +
    '</table></div>' : '<div class="empty">No hay cuentas abiertas.</div>') + '</div>';
  $('cuenta').innerHTML = html;

  const mov = estado.caja.movimientos;
  const totalCaja = mov.reduce((s, m) => s + m.total, 0);
  $('arqueo').innerHTML =
    '<div class="stat"><span>Apertura de caja</span><b>' + hora12(estado.caja.apertura) + '</b></div>' +
    '<div class="stat"><span>Comprobantes emitidos</span><b>' + mov.length + '</b></div>' +
    '<div class="stat"><span>Total recaudado</span><b>' + dinero(totalCaja) + '</b></div>' +
    (mov.length ? '<div class="table-wrap" style="margin-top:14px"><table><tr><th>FACTURA</th><th>PLACA</th><th>MÉTODO</th><th>VALOR</th></tr>' +
      mov.map((m) => '<tr><td>' + m.factura + '</td><td>' + esc(m.placa) + '</td><td>' + m.metodo + '</td><td>' + dinero(m.total) + '</td></tr>').join('') + '</table></div>'
      : '<div class="empty" style="margin-top:10px">Aún no hay movimientos en esta caja.</div>');
}

function vistaCuenta(c) {
  const r = c.registro;
  const p = c.parqueadero;
  const detalleTiempo = p.horas + ' h' + (p.fracciones ? ' + ' + p.fracciones + ' fracción' + (p.fracciones > 1 ? 'es' : '') + ' de 15 min' : '');
  const cab = '<div class="cuenta"><div class="cuenta-head"><span class="plate-big">' + esc(r.placa) + '</span>' + tagRegistro(r.estado) + '</div>' +
    '<div class="meta">' + r.tipo + ' · Celda ' + r.celdaId + ' · Ingreso ' + hora12(r.entrada) + ' · ' + (r.estado === 'PAGADO' ? 'Pago ' : 'Ahora ') + hora12(c.fin) + ' · Estadía ' + duracion(p.minutos) + '</div>';

  if (r.estado === 'PAGADO') {
    const comp = estado.comprobantes.find((x) => x.registroId === r.id);
    return cab + '<div class="line"><span>Comprobante</span><b>' + comp.factura + '</b></div>' +
      '<div class="line"><span>Método de pago</span><b>' + comp.metodo + '</b></div>' +
      '<div class="line"><span>Parqueadero<small>' + detalleTiempo + '</small></span><span>' + dinero(comp.parqueadero.valor) + '</span></div>' +
      comp.lavados.map((l) => '<div class="line"><span>' + l.tipo + '</span><span>' + dinero(l.precio) + '</span></div>').join('') +
      '<div class="total"><span>Total pagado</span><span>' + dinero(comp.total) + '</span></div>' +
      '<div class="btn-row"><button class="btn btn-primary" data-action="autorizar-salida" data-placa="' + esc(r.placa) + '">Autorizar Salida</button>' +
      '<button class="btn btn-outline" data-action="imprimir">Imprimir comprobante</button></div></div>';
  }
  return cab + '<div class="line"><span>Parqueadero<small>' + detalleTiempo + '</small></span><span>' + dinero(p.valor) + '</span></div>' +
    c.facturables.map((l) => '<div class="line"><span>' + l.tipo + '</span><span>' + dinero(l.precio) + '</span></div>').join('') +
    (c.enCurso.length ? '<div class="msg-error">Hay un lavado en curso (' + c.enCurso[0].estado.replace('_', ' ').toLowerCase() + '). Debe estar en estado Listo antes de cobrar.</div>' : '') +
    '<div class="total"><span>Total a pagar</span><span>' + dinero(c.total) + '</span></div>' +
    '<div class="label-caps">MÉTODO DE PAGO</div><div class="pay-methods">' +
    ['Efectivo', 'Transferencia', 'Tarjeta'].map((m) => '<div class="pay-chip' + (ui.metodo === m ? ' sel' : '') + '" data-metodo="' + m + '">' + m + '</div>').join('') + '</div>' +
    '<div class="btn-row"><button class="btn btn-primary" data-action="confirmar-pago" data-placa="' + esc(r.placa) + '"' + (c.enCurso.length ? ' disabled' : '') + '>Confirmar pago</button>' +
    '<button class="btn btn-outline" data-action="autorizar-salida" data-placa="' + esc(r.placa) + '">Autorizar Salida</button></div></div>';
}

function render() {
  if (!estado || !estado.usuario) return;
  if (ui.pantalla === 'dashboard') renderDashboard();
  if (ui.pantalla === 'ingreso') renderIngreso();
  if (ui.pantalla === 'reservas') renderReservas();
  if (ui.pantalla === 'lavado') renderLavado();
  if (ui.pantalla === 'caja') renderCaja();
  if (ui.pantalla === 'c-panel') renderClientePanel();
  if (ui.pantalla === 'c-reservas') renderClienteReservas();
  if (ui.pantalla === 'c-lavado') renderClienteLavado();
  if (ui.pantalla === 'c-cuenta') renderClienteCuenta();
}

/* ---------- Render: cliente ---------- */
const ETIQUETA_LAVADO = {
  PENDIENTE: '<span class="tag tag-amber">Pendiente</span>',
  EN_LAVADO: '<span class="tag tag-blue">En Lavado</span>',
  LISTO: '<span class="tag tag-green">Listo</span>'
};
function opcionesPlacas(cli) { return cli.placas.map((p) => '<option value="' + esc(p) + '">' + esc(p) + '</option>').join(''); }

function renderClientePanel() {
  const cli = clienteActual();
  $('c-vehiculos').innerHTML = cli.placas.length ? cli.placas.map((placa) => {
    const { reg, reserva, lavado } = estadoVehiculo(placa);
    let tag, detalle;
    if (reg && reg.estado === 'ACTIVO') {
      tag = '<span class="tag tag-blue">En parqueadero</span>';
      detalle = 'Celda ' + reg.celdaId + ' · Ingreso ' + hora12(reg.entrada) + ' · Estadía ' + duracion(Math.floor((Reloj.ahora() - reg.entrada) / 60000));
    } else if (reg && reg.estado === 'PAGADO') {
      tag = '<span class="tag tag-green">Pagado</span>';
      detalle = 'Cuenta pagada. Falta que el operador autorice tu salida.';
    } else if (reserva) {
      tag = '<span class="tag tag-amber">Reserva activa</span>';
      detalle = 'Celda ' + reserva.celdaId + ' a las ' + hora12(reserva.hora) + ' (tolerancia de 15 min)';
    } else {
      tag = '<span class="tag tag-gray">Fuera del parqueadero</span>';
      detalle = 'Sin ingreso ni reserva activa';
    }
    return '<div class="veh-card"><div class="veh-top"><span class="plate-big">' + esc(placa) + '</span>' + tag + '</div><div class="meta">' + detalle + '</div>' +
      (lavado ? '<div class="veh-lavado">Lavado: ' + ETIQUETA_LAVADO[lavado.estado] + '</div>' : '') + '</div>';
  }).join('') : '<div class="empty">Aún no tienes vehículos registrados. Agrega tu placa para empezar.</div>';
}

function renderClienteReservas() {
  const cli = clienteActual();
  const mias = reservasDeCliente(cli);
  $('c-tabla-reservas').innerHTML = mias.length ?
    '<div class="table-wrap"><table><tr><th>PLACA</th><th>CELDA</th><th>HORA</th><th>ESTADO</th><th></th></tr>' +
    mias.map((r) => '<tr><td><b>' + esc(r.placa) + '</b></td><td>' + r.celdaId + '</td><td>' + hora12(r.hora) + '</td><td>' + ETIQUETA_RESERVA[r.estado] + '</td><td>' +
      (r.estado === 'ACTIVA' ? '<button class="btn btn-danger btn-sm" data-action="cancelar-reserva-cliente" data-id="' + r.id + '">Cancelar</button>' : '') + '</td></tr>').join('') +
    '</table></div>' : '<div class="empty">Todavía no tienes reservas.</div>';
  const placa = $('cr-placa'), celda = $('cr-celda');
  const p0 = placa.value, c0 = celda.value;
  placa.innerHTML = opcionesPlacas(cli); if (p0) placa.value = p0;
  celda.innerHTML = estado.celdas.filter((c) => c.estado === 'DISPONIBLE').map((c) => '<option value="' + c.id + '">' + c.id + '</option>').join('');
  if (c0) celda.value = c0;
}

function renderClienteLavado() {
  const cli = clienteActual();
  const sel = $('cl-placa'); const previa = sel.value;
  sel.innerHTML = opcionesPlacas(cli); if (previa) sel.value = previa;
  const mios = lavadosDeCliente(cli);
  $('c-tabla-lavados').innerHTML = mios.length ?
    '<div class="table-wrap"><table><tr><th>PLACA</th><th>SERVICIO</th><th>SOLICITADO</th><th>AVANCE</th></tr>' +
    mios.map((l) => '<tr><td><b>' + esc(l.placa) + '</b></td><td>' + l.tipo + '</td><td>' + hora12(l.solicitado) + '</td><td>' + ETIQUETA_LAVADO[l.estado] +
      (l.fin ? ' <span class="hint">terminó ' + hora12(l.fin) + '</span>' : '') + '</td></tr>').join('') +
    '</table></div>' : '<div class="empty">No has solicitado lavados todavía.</div>';
}

function vistaCuentaCliente(c) {
  const r = c.registro, p = c.parqueadero;
  const detalle = p.horas + ' h' + (p.fracciones ? ' + ' + p.fracciones + ' fracción' + (p.fracciones > 1 ? 'es' : '') + ' de 15 min' : '');
  const cab = '<div class="cuenta-card"><div class="cuenta-head"><span class="plate-big">' + esc(r.placa) + '</span>' + tagRegistro(r.estado) + '</div>' +
    '<div class="meta">Celda ' + r.celdaId + ' · Ingreso ' + hora12(r.entrada) + ' · Estadía ' + duracion(p.minutos) + '</div>';
  if (r.estado === 'PAGADO') {
    const comp = estado.comprobantes.find((x) => x.registroId === r.id);
    return cab + '<div class="line"><span>Comprobante</span><b>' + comp.factura + '</b></div>' +
      '<div class="line"><span>Pagado con</span><b>' + comp.metodo + ' (' + comp.canal.toLowerCase() + ')</b></div>' +
      '<div class="total"><span>Total pagado</span><span>' + dinero(comp.total) + '</span></div>' +
      '<div class="aviso">Pago registrado. Presenta tu comprobante al operador para que autorice tu salida.</div></div>';
  }
  return cab + '<div class="line"><span>Parqueadero<small>' + detalle + '</small></span><span>' + dinero(p.valor) + '</span></div>' +
    c.facturables.map((l) => '<div class="line"><span>' + l.tipo + '</span><span>' + dinero(l.precio) + '</span></div>').join('') +
    (c.enCurso.length ? '<div class="msg-error">Tu lavado sigue en curso (' + c.enCurso[0].estado.replace('_', ' ').toLowerCase() + '). Podrás pagar cuando esté Listo.</div>' : '') +
    '<div class="total"><span>Total a pagar</span><span>' + dinero(c.total) + '</span></div>' +
    '<div class="label-caps">PAGO EN LÍNEA</div><div class="pay-methods">' +
    ['Transferencia', 'Tarjeta'].map((m) => '<div class="pay-chip' + (ui.metodoCli === m ? ' sel' : '') + '" data-cmetodo="' + m + '">' + m + '</div>').join('') + '</div>' +
    '<button class="btn btn-primary" data-action="pagar-linea" data-placa="' + esc(r.placa) + '"' + (c.enCurso.length ? ' disabled' : '') + '>Pagar en línea</button></div>';
}

function renderClienteCuenta() {
  const cli = clienteActual();
  const abiertas = cli.placas.map((p) => cuentaDe(p)).filter(Boolean);
  $('c-cuentas').innerHTML = abiertas.length ? abiertas.map(vistaCuentaCliente).join('') :
    '<div class="empty">No tienes cuentas abiertas. Aparecerán aquí cuando tu vehículo ingrese al parqueadero.</div>';
  const comps = estado.comprobantes.filter((x) => cli.placas.includes(x.placa)).reverse();
  $('c-historial').innerHTML = comps.length ?
    '<div class="table-wrap"><table><tr><th>FACTURA</th><th>PLACA</th><th>MÉTODO</th><th>TOTAL</th></tr>' +
    comps.map((x) => '<tr><td>' + x.factura + '</td><td>' + esc(x.placa) + '</td><td>' + x.metodo + '</td><td>' + dinero(x.total) + '</td></tr>').join('') + '</table></div>' :
    '<div class="empty">Aún no tienes comprobantes.</div>';
}

/* ---------- Eventos ---------- */
document.addEventListener('click', (ev) => {
  const t = ev.target;
  const rol = t.closest('#roles .role-chip');
  if (rol) {
    document.querySelectorAll('#roles .role-chip').forEach((c) => c.classList.remove('sel'));
    rol.classList.add('sel');
    $('lbl-codigo').textContent = 'Código de ' + rol.dataset.rol;
    $('login-codigo').value = CODIGOS[rol.dataset.rol];
    return;
  }
  const nav = t.closest('.nav-item');
  if (nav) { irA(nav.dataset.pantalla); return; }
  const celda = t.closest('.cell');
  if (celda) { ui.celdaSel = celda.dataset.celda; renderDashboard(); return; }
  const pago = t.closest('.pay-chip');
  if (pago) {
    if (pago.dataset.cmetodo) { ui.metodoCli = pago.dataset.cmetodo; renderClienteCuenta(); }
    else { ui.metodo = pago.dataset.metodo; renderCaja(); }
    return;
  }

  const btn = t.closest('[data-action]');
  if (!btn) return;
  const id = Number(btn.dataset.id);
  const placa = btn.dataset.placa;
  switch (btn.dataset.action) {
    case 'login': iniciarSesion(); break;
    case 'logout': cerrarSesion(); break;
    case 'reiniciar': reiniciarDemo(); break;
    case 'avanzar': {
      Reloj.avanzar(Number(btn.dataset.min));
      actualizarReloj(); revisarVencimientos(); render();
      break;
    }
    case 'confirmar-ingreso': confirmarIngreso(); break;
    case 'limpiar-ingreso': limpiarIngreso(); break;
    case 'cerrar-modal': cerrarModal(); break;
    case 'imprimir': toast('Documento enviado a la impresora'); break;
    case 'crear-reserva': {
      const r = crearReserva({ placa: $('res-placa').value, celdaId: $('res-celda').value, hora: $('res-hora').value });
      mostrarError('res-error', r.ok ? '' : r.msg);
      if (r.ok) { $('res-placa').value = ''; toast('Reserva creada para ' + r.reserva.placa + ' en la celda ' + r.reserva.celdaId); render(); }
      break;
    }
    case 'cancelar-reserva': if (cancelarReserva(id)) { toast('Reserva cancelada y celda liberada'); render(); } break;
    case 'solicitar-lavado': {
      const r = solicitarLavado($('lav-placa').value);
      mostrarError('lav-error', r.ok ? '' : r.msg);
      if (r.ok) { $('lav-placa').value = ''; toast('Lavado solicitado para ' + r.lavado.placa); render(); }
      break;
    }
    case 'avanzar-lavado': {
      const r = avanzarLavado(id);
      if (r.ok) { toast(r.lavado.estado === 'LISTO' ? 'Servicio listo: notificado al módulo de caja' : 'Lavado iniciado'); render(); }
      break;
    }
    case 'agregar-vehiculo': {
      const r = agregarVehiculo(clienteActual(), $('cv-placa').value);
      mostrarError('cv-error', r.ok ? '' : r.msg);
      if (r.ok) { $('cv-placa').value = ''; toast('Vehículo ' + r.placa + ' agregado a tu lista'); render(); }
      break;
    }
    case 'crear-reserva-cliente': {
      if (!$('cr-placa').value) { mostrarError('cr-error', 'Primero agrega un vehículo en Mi Panel'); break; }
      const r = crearReserva({ placa: $('cr-placa').value, celdaId: $('cr-celda').value, hora: $('cr-hora').value });
      mostrarError('cr-error', r.ok ? '' : r.msg);
      if (r.ok) { toast('Reserva creada para ' + r.reserva.placa + ' en la celda ' + r.reserva.celdaId); render(); }
      break;
    }
    case 'cancelar-reserva-cliente': if (cancelarReservaCliente(clienteActual(), id)) { toast('Reserva cancelada y celda liberada'); render(); } break;
    case 'solicitar-lavado-cliente': {
      if (!$('cl-placa').value) { mostrarError('cl-error', 'Primero agrega un vehículo en Mi Panel'); break; }
      const r = solicitarLavado($('cl-placa').value);
      mostrarError('cl-error', r.ok ? '' : r.msg);
      if (r.ok) { toast('Lavado solicitado para ' + r.lavado.placa); render(); }
      break;
    }
    case 'pagar-linea': {
      const r = confirmarPago(placa, ui.metodoCli, 'En línea');
      if (r.ok) { toast('Pago en línea confirmado · ' + r.comprobante.factura + ' · ' + dinero(r.comprobante.total)); render(); } else toast(r.msg, 'error');
      break;
    }
    case 'buscar-cuenta': ui.cuentaPlaca = normPlaca($('caja-placa').value); renderCaja(); break;
    case 'ver-cuenta': ui.cuentaPlaca = placa; $('caja-placa').value = placa; renderCaja(); break;
    case 'confirmar-pago': {
      const r = confirmarPago(placa, ui.metodo);
      if (r.ok) { toast('Pago confirmado · ' + r.comprobante.factura + ' · ' + dinero(r.comprobante.total)); render(); } else toast(r.msg, 'error');
      break;
    }
    case 'autorizar-salida': {
      const r = autorizarSalida(placa);
      if (r.ok) { toast('Salida autorizada para ' + placa); ui.cuentaPlaca = ''; render(); } else toast(r.msg, 'error');
      break;
    }
  }
});

$('ing-placa').addEventListener('input', () => { $('ing-placa').value = normPlaca($('ing-placa').value); actualizarReservaChip(); });
['res-placa', 'lav-placa', 'caja-placa', 'cv-placa'].forEach((id) => $(id).addEventListener('input', () => { $(id).value = normPlaca($(id).value); }));
document.addEventListener('keydown', (ev) => {
  if (ev.key !== 'Enter') return;
  if (!$('vista-login').hidden) iniciarSesion();
  else if (ev.target.id === 'caja-placa') { ui.cuentaPlaca = normPlaca($('caja-placa').value); renderCaja(); }
});

/* ---------- Reloj en vivo y vencimiento automático de reservas ---------- */
function actualizarReloj() { if (estado) $('reloj').textContent = hora24(Reloj.ahora()); }
function revisarVencimientos() {
  const vencidas = verificarVencimientos();
  if (vencidas.length) {
    toast('Reserva ' + vencidas.map((r) => r.placa).join(', ') + ' cancelada por inasistencia: celda liberada', 'error');
    render();     // Criterio 3 de HU-02: el mapa se refresca solo, sin recargar la página
  }
}
Reloj.iniciar();
estado = crearEstadoInicial();
estado.usuario = null;
setInterval(() => { if (estado.usuario) { actualizarReloj(); revisarVencimientos(); } }, 1000);

/* Se expone para pruebas manuales desde la consola del navegador */
window.ParkFlow = { get estado() { return estado; }, Reloj, registrarIngreso, crearReserva, cancelarReserva, verificarVencimientos, solicitarLavado, avanzarLavado, cuentaDe, confirmarPago, autorizarSalida, calcularTarifa, agregarVehiculo, cancelarReservaCliente, reservasDeCliente, lavadosDeCliente, reiniciarDemo };
