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

        TimeLoopManager.Instance.OnLoopReset += ResetKeyState;
    }

    void OnDestroy()
    {
        if (TimeLoopManager.Instance != null)
            TimeLoopManager.Instance.OnLoopReset -= ResetKeyState;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pickup);

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

    public void ActivateSpecialKey() //Encargarse de activar la llave especial
    {
        if (isKeySpecial)
        {
            gameObject.SetActive(true);
            isActive = true;
        }
    }

    void ResetKeyState() //Reiniciar el estado de la llave en cada loop
    {
        if (isKeySpecial)
        {
            isActive = true;
        }
    }
}