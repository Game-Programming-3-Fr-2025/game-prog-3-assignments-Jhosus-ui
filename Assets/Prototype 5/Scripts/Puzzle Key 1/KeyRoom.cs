using UnityEngine;

public class KeyRoom : MonoBehaviour
{
    public GameObject keyImageUI;
    public bool isKeySpecial = false;

    private bool isActive = true;

    void Start()
    {
        if (isKeySpecial)
        {
            gameObject.SetActive(false);
            isActive = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Cambiar esta línea - solo verificar si el objeto está activo en escena
        if (other.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            if (keyImageUI != null)
            {
                keyImageUI.SetActive(true);
                TimeLoopManager.Instance.RegisterActivatedUI(keyImageUI);
            }

            gameObject.SetActive(false);
            isActive = false;
            TimeLoopManager.Instance.RegisterCollectedItem(gameObject);
        }
    }

    // Método para activar llaves especiales desde Fathers.cs
    public void ActivateSpecialKey()
    {
        if (isKeySpecial)
        {
            gameObject.SetActive(true);
            isActive = true;
        }
    }
}