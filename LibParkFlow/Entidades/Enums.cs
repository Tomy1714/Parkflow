namespace LibParkFlow.Entidades
{
    public enum EstadoCelda
    {
        DISPONIBLE,
        OCUPADA,
        RESERVADA,
        MANTENIMIENTO
    }

    public enum EstadoLavado
    {
        PENDIENTE,
        EN_LAVADO,
        LISTO
    }

    public enum EstadoReserva
    {
        ACTIVA,
        ATENDIDA,
        CANCELADA_INASISTENCIA,
        CANCELADA_MANUAL
    }

    public enum EstadoRegistro
    {
        ACTIVO,
        PAGADO,
        FINALIZADO
    }

    public enum CanalPago
    {
        CAJA,
        EN_LINEA
    }

    public enum RolUsuario
    {
        ADMINISTRADOR,
        OPERADOR,
        CAJERO,
        LAVADOR,
        CLIENTE
    }

    public enum EstadoCaja
    {
        ABIERTA,
        CERRADA
    }
}
