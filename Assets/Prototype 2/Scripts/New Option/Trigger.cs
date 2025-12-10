using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour
{
    public bool esModoArriba = false;
    public Transform puntoAjuste;

    [Header("Configuración por Jugador")]
    public string nombreCamaraPlayer1 = "MainCameraP1";
    public string nombreCamaraPlayer2 = "MainCameraP2";

    private bool player1Activado = false;
    private bool player2Activado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer1 = other.CompareTag("Player 1");
        bool isPlayer2 = other.CompareTag("Player 2");

        if (!isPlayer1 && !isPlayer2) return;

        if ((isPlayer1 && player1Activado) || (isPlayer2 && player2Activado))
            return;

        if (isPlayer1) player1Activado = true;
        if (isPlayer2) player2Activado = true;

        string nombreCamaraBuscada = isPlayer1 ? nombreCamaraPlayer1 : nombreCamaraPlayer2;
        GameObject camaraObj = GameObject.Find(nombreCamaraBuscada);

        if (camaraObj == null)
        {
            Debug.LogError($"No se encontró cámara: {nombreCamaraBuscada}");
            return;
        }

        FC2 camara = camaraObj.GetComponent<FC2>();
        if (camara == null)
        {
            Debug.LogError($"La cámara {nombreCamaraBuscada} no tiene componente FC2");
            return;
        }

        Vector3 posicion = puntoAjuste != null ? puntoAjuste.position : transform.position;

        if (esModoArriba)
            camara.CambiarAModoVerticalArriba(posicion);
        else
            camara.CambiarAModoHorizontal(posicion);

        StartCoroutine(DesactivarTemporalmenteParaJugador(other));

        if (player1Activado && player2Activado)
            gameObject.SetActive(false);
    }

    IEnumerator DesactivarTemporalmenteParaJugador(Collider2D playerCollider)
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            yield return new WaitForSeconds(0.5f);

            if (!(player1Activado && player2Activado))
                triggerCollider.enabled = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = esModoArriba ? Color.cyan : Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);

        if (puntoAjuste != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, puntoAjuste.position);
            Gizmos.DrawWireSphere(puntoAjuste.position, 0.3f);
        }
    }
}