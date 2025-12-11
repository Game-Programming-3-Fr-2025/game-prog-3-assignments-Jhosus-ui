using UnityEngine;

public class SplitScreenAudioManager : MonoBehaviour
{
    private AudioListener listener1;
    private AudioListener listener2;
    private LifeSystem life1;
    private LifeSystem life2;

    void Start()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();

        foreach (AudioListener listener in allListeners)
        {
            LifeSystem life = listener.GetComponent<LifeSystem>();

            if (life != null)
            {
                if (life.playerType == PlayerType.Player1)
                {
                    listener1 = listener;
                    life1 = life;
                }
                else if (life.playerType == PlayerType.Player2)
                {
                    listener2 = listener;
                    life2 = life;
                }
            }
        }

        if (listener1 != null) listener1.enabled = true;
        if (listener2 != null) listener2.enabled = true;
    }

    void Update()
    {
        if (listener1 != null && life1 != null)
            listener1.enabled = life1.currentHealth > 0;

        if (listener2 != null && life2 != null)
            listener2.enabled = life2.currentHealth > 0;
    }
}