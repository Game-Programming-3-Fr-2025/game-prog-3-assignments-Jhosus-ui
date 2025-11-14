using UnityEngine;
using System.Collections;

public class PIA : MonoBehaviour
{
    [Header("Configuración de Pinchos")]
    public GameObject[] spikes;         
    public float showTime = 2f;          
    public float hideTime = 3f;          
    public float initialDelay = 0f;      
    public bool startVisible = false;    

    void Start()
    {
        SetSpikesState(startVisible);
        StartCoroutine(SpikesCycle());
    }

    IEnumerator SpikesCycle() //Demostrar sistema de pinchos
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            SetSpikesState(true);
            yield return new WaitForSeconds(showTime);

            SetSpikesState(false);
            yield return new WaitForSeconds(hideTime);
        }
    }

    void SetSpikesState(bool state)
    {
        foreach (GameObject spike in spikes)
        {
            if (spike != null)
            {
                spike.SetActive(state);
            }
        }
    }

    // Debug visual en el Editor
    void OnDrawGizmosSelected()
    {
        if (spikes == null) return;

        Gizmos.color = Color.red;
        foreach (GameObject spike in spikes)
        {
            if (spike != null)
            {
                Gizmos.DrawWireCube(spike.transform.position, Vector3.one * 0.5f);
                Gizmos.DrawLine(transform.position, spike.transform.position);
            }
        }
    }
}