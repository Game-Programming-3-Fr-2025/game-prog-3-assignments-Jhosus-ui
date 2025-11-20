using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera player1Camera; // Arriba
    public Camera player2Camera; // Abajo

    [Header("Configuración")]
    [Range(0.5f, 0.8f)]
    public float cameraWidthPercent = 0.7f; // Qué % del ancho ocupa cada cámara

    public Color backgroundColor = Color.black;

    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start()
    {
        SetupCameras();
        UpdateCameraPositions();
    }

    void Update()
    {
        // Solo recalcular si cambia la resolución
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraPositions();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void SetupCameras()
    {
        player1Camera.backgroundColor = backgroundColor;
        player2Camera.backgroundColor = backgroundColor;
        player2Camera.gameObject.SetActive(true);
    }

    void UpdateCameraPositions()
    {
        float halfHeight = 0.5f;

        // PLAYER 1 (ARRIBA) - Pegado a la IZQUIERDA, espacio libre a la DERECHA
        player1Camera.rect = new Rect(
            0f,                    // x: pegado al borde izquierdo
            halfHeight,            // y: mitad superior
            cameraWidthPercent,    // width: ocupa 70% del ancho
            halfHeight             // height: mitad de la pantalla
        );

        // PLAYER 2 (ABAJO) - Pegado a la DERECHA, espacio libre a la IZQUIERDA
        player2Camera.rect = new Rect(
            1f - cameraWidthPercent, // x: pegado al borde derecho
            0f,                      // y: mitad inferior
            cameraWidthPercent,      // width: ocupa 70% del ancho
            halfHeight               // height: mitad de la pantalla
        );
    }

    // Obtener los espacios libres para UI (en coordenadas de viewport 0-1)
    public Rect GetPlayer1UISpace()
    {
        // Espacio DERECHO de player 1
        return new Rect(cameraWidthPercent, 0.5f, 1f - cameraWidthPercent, 0.5f);
    }

    public Rect GetPlayer2UISpace()
    {
        // Espacio IZQUIERDO de player 2
        return new Rect(0f, 0f, 1f - cameraWidthPercent, 0.5f);
    }

    // Cambiar el ancho de las cámaras en tiempo real
    public void SetCameraWidth(float percent)
    {
        cameraWidthPercent = Mathf.Clamp(percent, 0.5f, 1f);
        UpdateCameraPositions();
    }
}