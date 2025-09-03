using UnityEngine;
using System.Collections;

public class FFT1 : MonoBehaviour
{
    public GameObject[] interactiveObjects; 
    public GameObject[] falseFloors;       
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 2f;

    [Header("Efecto de Luz")]
    public Color glowColor = Color.yellow;
    public float glowDuration = 0.5f;
    public float waveSpeed = 0.2f;
    public float maxIntensity = 2f;

    private GameObject player;
    private bool canInteract = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (canInteract && player != null)
        {
            CheckInteractions();
        }
    }

    void CheckInteractions()
    {
        foreach (GameObject obj in interactiveObjects)
        {
            if (obj != null)
            {
                float distance = Vector2.Distance(player.transform.position, obj.transform.position);
                if (distance <= interactionDistance && Input.GetKeyDown(interactKey))
                {
                    StartCoroutine(ActivateLightWave());
                    break;
                }
            }
        }
    }

    IEnumerator ActivateLightWave()
    {
        canInteract = false;

        foreach (GameObject floor in falseFloors)
        {
            if (floor != null)
            {
                StartCoroutine(GlowEffect(floor));
                yield return new WaitForSeconds(waveSpeed);
            }
        }

        yield return new WaitForSeconds(glowDuration * 2f);
        canInteract = true;
    }

    IEnumerator GlowEffect(GameObject floor)
    {
        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer == null) yield break;

        Material mat = renderer.material;
        Color originalColor = mat.color;

        // Fase de aumento
        for (float t = 0; t < glowDuration; t += Time.deltaTime)
        {
            float intensity = Mathf.Lerp(1f, maxIntensity, t / glowDuration);
            mat.color = originalColor * intensity;
            mat.SetColor("_EmissionColor", glowColor * intensity);
            yield return null;
        }

        // Fase de disminución
        for (float t = 0; t < glowDuration; t += Time.deltaTime)
        {
            float intensity = Mathf.Lerp(maxIntensity, 1f, t / glowDuration);
            mat.color = originalColor * intensity;
            mat.SetColor("_EmissionColor", glowColor * intensity);
            yield return null;
        }

        mat.color = originalColor;
        mat.SetColor("_EmissionColor", Color.black);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (GameObject obj in interactiveObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawWireSphere(obj.transform.position, interactionDistance);
            }
        }
    }
}