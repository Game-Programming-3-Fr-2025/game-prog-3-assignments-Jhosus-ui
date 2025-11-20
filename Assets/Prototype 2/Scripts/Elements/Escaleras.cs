//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//using System.Collections;

//public class Escaleras : MonoBehaviour
//{
//    [Header("Referencias")]
//    [SerializeField] private Image fadeImage;
//    [SerializeField] private Transform puntoDestino;

//    [Header("Configuración")]
//    [SerializeField] private float duracionFadeIn = 0.5f;
//    [SerializeField] private float duracionOscuro = 1f;
//    [SerializeField] private float duracionFadeOut = 1.5f;

//    private bool jugadorEnRango;
//    private bool enTransicion;
//    private Transform jugador;
//    private Player2 playerController;
//    private Rigidbody2D playerRb;
//    private Vector2 velocidadAnterior;

//    void Start()
//    {
//        ConfigurarVisuales(0f);
//    }

//    void Update()
//    {
//        bool inputInteraccion = Input.GetKeyDown(KeyCode.E) ||
//                              (Gamepad.current != null && Gamepad.current.yButton.wasPressedThisFrame);

//        if (jugadorEnRango && !enTransicion && inputInteraccion)
//            StartCoroutine(RealizarTransicion());
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            jugadorEnRango = true;
//            jugador = other.transform;
//            playerController = other.GetComponent<Player2>();
//            playerRb = other.GetComponent<Rigidbody2D>();
//        }
//    }

//    void OnTriggerExit2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            jugadorEnRango = false;
//        }
//    }

//    private IEnumerator RealizarTransicion()
//    {
//        if (puntoDestino == null || jugador == null) yield break;

//        enTransicion = true;

//        // Bloquear movimiento del jugador
//        BloquearMovimientoJugador(true);

//        // Oscurecer
//        yield return AnimarFade(0f, 1f, duracionFadeIn);

//        // Mantener oscuro
//        yield return new WaitForSeconds(duracionOscuro);

//        // Teletransportar
//        jugador.position = puntoDestino.position;

//        // Aclarar
//        yield return AnimarFade(1f, 0f, duracionFadeOut);

//        // Restaurar movimiento del jugador (GARANTIZADO)
//        BloquearMovimientoJugador(false);

//        // Resetear estado
//        ConfigurarVisuales(0f);
//        enTransicion = false;
//    }

//    private void BloquearMovimientoJugador(bool bloquear)
//    {
//        if (playerRb != null)
//        {
//            if (bloquear)
//            {
//                // Guardar velocidad anterior y congelar
//                velocidadAnterior = playerRb.linearVelocity;
//                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
//                playerRb.linearVelocity = Vector2.zero;

//                // Desactivar script de movimiento
//                if (playerController != null)
//                    playerController.enabled = false;
//            }
//            else
//            {
//                // Restaurar constraints y velocidad
//                playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
//                playerRb.linearVelocity = velocidadAnterior;

//                // Reactivar script de movimiento
//                if (playerController != null)
//                    playerController.enabled = true;
//            }
//        }
//    }

//    private IEnumerator AnimarFade(float inicio, float fin, float duracion)
//    {
//        float tiempo = 0f;

//        while (tiempo < duracion)
//        {
//            tiempo += Time.deltaTime;
//            SetearAlphaFade(Mathf.Lerp(inicio, fin, tiempo / duracion));
//            yield return null;
//        }

//        SetearAlphaFade(fin);
//    }

//    private void SetearAlphaFade(float alpha)
//    {
//        if (fadeImage != null)
//        {
//            Color color = fadeImage.color;
//            color.a = alpha;
//            fadeImage.color = color;
//        }
//    }

//    private void ConfigurarVisuales(float alpha)
//    {
//        SetearAlphaFade(alpha);
//    }

//    // Asegurar que se restaure el movimiento si el objeto se destruye
//    private void OnDestroy()
//    {
//        if (enTransicion)
//        {
//            BloquearMovimientoJugador(false);
//        }
//    }
//}