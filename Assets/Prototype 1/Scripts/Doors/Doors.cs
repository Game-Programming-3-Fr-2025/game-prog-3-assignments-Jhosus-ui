using UnityEngine;

public class Doors : MonoBehaviour
{
    [System.Serializable]
    public class DoorLock
    {
        public GameObject keyObject;
        public GameObject doorObject;
        public AudioClip unlockSound;
        public bool keyCollected = false;
        public bool isUnlocked = false;
    }

    public DoorLock[] doorLocks;
    public AudioSource audioSource;
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 2f;

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        CheckKeysCollection();
        CheckDoorInteraction();
    }

    void CheckKeysCollection()
    {
        foreach (DoorLock doorLock in doorLocks)
        {
            if (!doorLock.keyCollected && doorLock.keyObject != null && !doorLock.keyObject.activeInHierarchy)
                doorLock.keyCollected = true;
        }
    }

    void CheckDoorInteraction()
    {
        if (player == null) return;

        foreach (DoorLock doorLock in doorLocks)
        {
            if (doorLock.doorObject != null && doorLock.keyCollected && !doorLock.isUnlocked)
            {
                float distance = Vector2.Distance(player.transform.position, doorLock.doorObject.transform.position);
                if (distance <= interactionDistance && Input.GetKeyDown(interactKey))
                    UnlockDoor(doorLock);
            }
        }
    }

    void UnlockDoor(DoorLock doorLock)
    {
        doorLock.isUnlocked = true;
        if (doorLock.unlockSound != null && audioSource != null)
            audioSource.PlayOneShot(doorLock.unlockSound);

        doorLock.doorObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (doorLocks == null) return;

        foreach (DoorLock doorLock in doorLocks)
        {
            if (doorLock.keyObject != null && doorLock.doorObject != null)
            {
                Gizmos.color = doorLock.isUnlocked ? Color.green : (doorLock.keyCollected ? Color.yellow : Color.red);
                Gizmos.DrawLine(doorLock.keyObject.transform.position, doorLock.doorObject.transform.position);
                Gizmos.DrawWireCube(doorLock.keyObject.transform.position, Vector3.one * 0.3f);
                Gizmos.DrawWireCube(doorLock.doorObject.transform.position, Vector3.one * 0.5f);

                if (doorLock.keyCollected && !doorLock.isUnlocked)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(doorLock.doorObject.transform.position, interactionDistance);
                }
            }
        }
    }
}