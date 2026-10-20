# ParkFlow Central — Front del MVP

Interfaz web del sistema de gestión de parqueadero y lavadero de vehículos (proyecto de Diseño de Sistemas de Información, ITM).

Este front funciona **sin backend ni base de datos**: los datos viven en memoria y se reinician al recargar la página. Su propósito es validar el flujo principal del MVP definido en el Entregable 1 (HU-01 a HU-04).

## Cómo ejecutarlo

1. Descargar o clonar el repositorio.
2. Abrir `index.html` en el navegador (no requiere instalar nada).

## Estructura

```
index.html        Estructura de las pantallas
css/styles.css    Estilos (tema oscuro)
js/app.js         Reglas de negocio, estado en memoria y eventos
```

## Roles y pantallas

| Rol            | Pantallas disponibles                                   |
|----------------|----------------------------------------------------------|
| Administrador  | Dashboard, Ingreso, Reservas, Lavado, Caja               |
| Operador       | Dashboard, Ingreso, Reservas, Lavado                     |
| Cajero         | Dashboard, Caja                                          |
| Lavador        | Lavado                                                   |
| Cliente        | Mi Panel, Mis Reservas, Mi Lavado, Mi Cuenta y Pagos     |

Para iniciar sesión basta con elegir un rol y escribir cualquier código y contraseña (ya vienen precargados). El cliente de demostración es `CL-5001`, dueño de los vehículos `ABC123` y `XYZ789`.

Los datos se conservan al cambiar de rol (por ejemplo, el cliente crea una reserva y el operador la ve). Se reinician con el enlace **Reiniciar datos de demostración** de la pantalla de login o al recargar la página.

## Reloj de demostración

En la parte superior hay un reloj con los botones **+15 min** y **+1 h**. Sirven para simular el paso del tiempo (vencimiento de reservas y cobro por estadía) sin tener que esperar.

## Recorrido de prueba (ejemplos del Entregable 1)

**HU-01 Registro de ingreso**
- Placa `AB` → error "Por favor ingrese un número de placa válido antes de continuar".
- Placa `NEW123` en la celda `B-05` (ocupada) → "La celda seleccionada no está disponible".
- Placa `ABC123` en `A-12` → ingreso exitoso, genera el ticket y la celda pasa a Ocupada.
- Placa `XYZ789` → se autocompleta la celda `C-01` de su reserva y la reserva queda Atendida.

**HU-02 Control de vencimiento de reserva**
- Pulsar **+15 min** hasta pasar las 10:15: la reserva `PQR321` (celda `A-05`) se cancela por inasistencia, la celda queda Disponible y se registra en el historial.
- Llevar el reloj a las 14:12 y registrar `MNO456`: ingresa dentro de la tolerancia y la reserva queda Atendida.

**HU-03 Control de lavado**
- Solicitar lavado para `ZZZ999` (sin ingreso) → mensaje de que no tiene registro de parqueadero activo.
- Solicitar lavado para `ABC123` y avanzarlo: Pendiente → En Lavado → Listo.

**HU-04 Liquidación y cobro**
- Con `ABC123` ingresada y su lavado en Listo, avanzar el reloj 2 horas: parqueadero $6.000 + Lavado General $25.000 = **$31.000**.
- Pulsar **Autorizar Salida** sin pagar → "Imposible registrar salida: Cuenta con saldo pendiente de pago".
- Confirmar el pago: se genera el comprobante, la cuenta queda Pagada y la celda pasa a Disponible.

**HU-05 Autogestión del cliente** (entrar como Cliente)
- En *Mi Panel* se ve el estado de cada vehículo (fuera, reservado, en parqueadero o pagado) y se pueden agregar placas nuevas. Una placa que ya pertenece a otro cliente se rechaza.
- En *Mis Reservas* se reserva `ABC123` en la celda `A-12` a las 11:00 AM: la reserva queda Activa y la celda Reservada. Solo se ven y cancelan las reservas propias.
- En *Mi Lavado*, pedir el lavado de `ABC123` antes de que ingrese → mensaje de que no tiene registro de parqueadero activo. Después de que el operador registre su ingreso, el pedido queda Pendiente y el cliente sigue el avance hasta Listo.
- En *Mi Cuenta y Pagos*, con 2 horas de estadía y el lavado en Listo, la cuenta suma $31.000 y se paga en línea con Transferencia o Tarjeta. Se genera el comprobante, la cuenta queda Pagada y la celda pasa a Disponible. La salida la autoriza el operador (RN-04).

## Reglas de negocio aplicadas

- **RN-01:** tarifa de $3.000 por hora, con fracciones de 15 minutos; desde 5 minutos transcurridos se cobra la fracción completa.
- **RN-03:** las reservas tienen 15 minutos de tolerancia; al vencer se cancelan y liberan la celda.
- **RN-04:** no se autoriza la salida sin el pago completo.
- **RN-05:** el lavado solo puede solicitarse para vehículos con registro de parqueadero activo.

## Fuera del alcance de este MVP

Persistencia de datos, backend, autenticación real y reportes administrativos (Release 2).
