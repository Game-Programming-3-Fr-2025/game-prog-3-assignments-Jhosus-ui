using UnityEngine;
using UnityEngine.InputSystem;

public class FC2 : MonoBehaviour
{
    [Header("Desplazamiento Horizontal")]
    public float desplazamientoHorizontal = 2f;
    public float suavizadoMovimiento = 0.1f;
    public float suavizadoTransicion = 2f;

    [Header("Control Manual Vertical")]
    public float velocidadDesplazamientoVertical = 1f;
    public float maxDesplazamientoVertical = 2f;

    private Vector3 velocidadCamara;
    private float desplazamientoActual;
    private float velocidadDesplazamiento;
    private float desplazamientoVerticalManual;
    private float velocidadVerticalManual;
    private Gamepad gamepad;

    void Start()
    {
        // Verificar si hay un mando conectado
        gamepad = Gamepad.current;
    }

    void LateUpdate()
    {
        // Obtener la posición del padre (jugador)
        Transform jugador = transform.parent;

        if (jugador == null) return;

        ProcesarInputVertical();
        ActualizarDesplazamientoAutomatico(jugador);
        CalcularYActualizarPosicion(jugador);
    }

    void ProcesarInputVertical()
    {
        float inputVertical = 0f;

        // Input de teclado (W/S)
        if (Input.GetKey(KeyCode.W))
            inputVertical += 1f;
        if (Input.GetKey(KeyCode.S))
            inputVertical -= 1f;

        // Input de mando (stick derecho vertical)
        if (gamepad != null)
        {
            float stickVertical = gamepad.rightStick.y.ReadValue();
            // Aplicar deadzone para evitar drift
            if (Mathf.Abs(stickVertical) > 0.1f)
                inputVertical += stickVertical;
        }

        // Input alternativo con botones del mando (opcional)
        if (gamepad != null)
        {
            if (gamepad.dpad.up.isPressed)
                inputVertical += 1f;
            if (gamepad.dpad.down.isPressed)
                inputVertical -= 1f;
        }

        // Limitar el input a -1 a 1
        inputVertical = Mathf.Clamp(inputVertical, -1f, 1f);

        // Suavizar el desplazamiento vertical manual
        desplazamientoVerticalManual = Mathf.SmoothDamp(
            desplazamientoVerticalManual,
            inputVertical * maxDesplazamientoVertical,
            ref velocidadVerticalManual,
            suavizadoMovimiento
        );
    }

    void ActualizarDesplazamientoAutomatico(Transform jugador)
    {
        // Determinar dirección objetivo automática
        bool mirandoDerecha = jugador.localScale.x >= 0;
        float desplazamientoObjetivo = mirandoDerecha ? desplazamientoHorizontal : -desplazamientoHorizontal;

        // Suavizar la transición del desplazamiento automático
        desplazamientoActual = Mathf.SmoothDamp(
            desplazamientoActual,
            desplazamientoObjetivo,
            ref velocidadDesplazamiento,
            suavizadoTransicion
        );
    }

    void CalcularYActualizarPosicion(Transform jugador)
    {
        // Calcular posición objetivo (horizontal automático + vertical manual)
        Vector3 posicionObjetivo = jugador.position +
                                 (Vector3.right * desplazamientoActual) +
                                 (Vector3.up * desplazamientoVerticalManual);
        posicionObjetivo.z = -10f;

        // Aplicar movimiento suavizado de la cámara
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionObjetivo,
            ref velocidadCamara,
            suavizadoMovimiento
        );
    }

    // Método para verificar si hay mando conectado (útil para debug)
    void OnGUI()
    {
        if (gamepad != null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Mando conectado: {gamepad.name}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Stick derecho Y: {gamepad.rightStick.y.ReadValue():F2}");
        }
    }
}