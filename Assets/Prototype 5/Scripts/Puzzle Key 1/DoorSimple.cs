using UnityEngine;

public class DoorSimple : MonoBehaviour
{
    public float reappearTime = 5f;
    private bool canOpen = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpen)
        {
            gameObject.SetActive(false);
            canOpen = false;
            Invoke("Reappear", reappearTime);
        }
    }

    void Reappear()
    {
        gameObject.SetActive(true);
    }
}