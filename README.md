# Juego Multijugador UDP - Proyecto de Redes

Juego multijugador desarrollado en **Unity 2022.3** con arquitectura **Cliente-Servidor** utilizando **UDP**.

El proyecto se ha centrado en la **lógica de red y sincronización**. El gameplay es básico (movimiento) ya que la prioridad ha sido la implementación de red.

---

## Arquitectura

```
            ┌──────────────┐
            │   SERVIDOR   │
            │  (Relay UDP) │
            └──────┬───────┘
                   │
         ┌─────────┴─────────┐
         ▼                   ▼
   ┌──────────┐        ┌──────────┐
   │ CLIENTE 1│        │ CLIENTE 2│
   └──────────┘        └──────────┘
```

- **Protocolo:** UDP
- **Puerto:** 9000
- **Máx. jugadores:** 2

---

## Tipos de Mensajes

| Mensaje | Descripción |
|---------|-------------|
| `POSITION` | Posición y rotación del jugador |
| `ANIMATIONSTATE` | Estado de animación |
| `SHOOT` | Disparo de arma |
| `HITPLAYER` | Impacto en jugador |
| `HITENEMY` | Impacto en enemigo |
| `KILL` | Muerte de jugador |
| `REVIVE` | Revivir jugador |
| `PING/PONG` | Medición de latencia |
| `PAUSE/UNPAUSE` | Control de pausa |
| `RESET` | Reinicio de partida |
| `ACKNOWLEDGEMENTS` | Confirmación de recepción (ACKs) |

---

## Fiabilidad sobre UDP

### Sistema de ACKs
- Cada mensaje tiene un **ID único**
- Los mensajes no confirmados se **reenvían automáticamente**

### Gestión de paquetes fuera de orden
- **Timestamps** en mensajes de posición
- Paquetes antiguos se descartan

### Interpolación
- Buffer de posiciones para **movimiento suave**
- `Vector3.Lerp` y `Quaternion.Slerp` entre posiciones

---

## Cómo Ejecutar

1. Ejecutar `Server.exe`
2. Ejecutar `Client.exe` (Jugador 1) → Introducir IP y Puerto → Conectar
3. Ejecutar `Client.exe` (Jugador 2) → Introducir IP y Puerto → Conectar

**Importante:** El juego NO inicia hasta que ambos clientes estén conectados (2/2 jugadores).

---

## Controles

| Tecla | Acción |
|-------|--------|
| WASD | Movimiento |
| Ratón | Rotar cámara |

---

## Requisitos Cumplidos

| Requisito | Estado |
|-----------|--------|
| UDP | Sí |
| Replication Model | Sí |
| Mínimo 3 tipos de datos | Sí (+10 tipos) |
| Replication Manager | Sí |
| Mínimo 2 clientes | Sí |
| Gestión de fiabilidad (ACKs) | Sí |
| Paquetes fuera de orden | Sí |


Made by:
Ao Yunqian / Bernat Cifuentes / Luis Fernández / Pau Hernández
