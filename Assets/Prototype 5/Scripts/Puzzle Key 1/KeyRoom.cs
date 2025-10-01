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
        // Cambiar esta l�nea - solo verificar si el objeto est� activo en escena
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

    // M�todo para activar llaves especiales desde Fathers.cs
    public void ActivateSpecialKey()
    {
        if (isKeySpecial)
        {
            gameObject.SetActive(true);
            isActive = true;
        }
    }
}