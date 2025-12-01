using UnityEngine;

public class Trigger : MonoBehaviour
{
    public bool esModoArriba = false;
    public Transform puntoAjuste;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player 1")) return;

        FC2 camara = FindObjectOfType<FC2>();
        if (camara == null) return;

        Vector3 posicion = puntoAjuste != null ? puntoAjuste.position : transform.position;

        if (esModoArriba)
        {
            camara.CambiarAModoVerticalArriba(posicion);
            Debug.Log($"Trigger Arriba activado en Y={transform.position.y}");
        }
        else
        {
            camara.CambiarAModoHorizontal(posicion);
        }

        gameObject.SetActive(false);
    }
}