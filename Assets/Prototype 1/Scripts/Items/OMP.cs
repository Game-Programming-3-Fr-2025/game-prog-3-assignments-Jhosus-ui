using UnityEngine;

public class OMP : MonoBehaviour
{
    [System.Serializable]
    public class ProjectileLauncher
    {
        public GameObject projectileObject; 
        public Vector2 shootDirection;      
        public float maxDistance = 10f;   
    }

    [System.Serializable]
    public class TrapZone
    {
        public GameObject triggerObject;   
        public ProjectileLauncher[] launchers; 
        public bool hasTriggered = false;
    }

    [Header("Configuración de Trampa")]
    public TrapZone[] trapZones;         
    public float shootForce = 10f;        
    public float triggerDistance = 3f;     

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player != null) CheckAllTraps();
    }

    void CheckAllTraps()
    {
        foreach (TrapZone zone in trapZones)
        {
            if (zone.triggerObject != null && !zone.hasTriggered)
            {
                float distance = Vector2.Distance(zone.triggerObject.transform.position, player.transform.position);
                if (distance <= triggerDistance)
                {
                    ShootProjectiles(zone);
                    zone.hasTriggered = true;
                }
            }
        }
    }

    void ShootProjectiles(TrapZone zone)
    {
        foreach (ProjectileLauncher launcher in zone.launchers)
        {
            if (launcher.projectileObject != null)
            {
                StartCoroutine(MoveProjectile(launcher));
            }
        }
    }

    System.Collections.IEnumerator MoveProjectile(ProjectileLauncher launcher)
    {
        GameObject projectile = launcher.projectileObject;
        Vector3 startPos = projectile.transform.position;
        Vector3 endPos = startPos + (Vector3)launcher.shootDirection.normalized * launcher.maxDistance;

        float journey = 0f;
        float speed = shootForce / launcher.maxDistance;

        projectile.SetActive(true);

        while (journey < 1f)
        {
            journey += Time.deltaTime * speed;
            projectile.transform.position = Vector3.Lerp(startPos, endPos, journey);
            yield return null;
        }

        projectile.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        foreach (TrapZone zone in trapZones)  //Tener en cuenta, aunque salga error es porque hay que colocar un objeto
        {
            if (zone.triggerObject != null)
            {
                Gizmos.color = zone.hasTriggered ? Color.gray : Color.red;
                Gizmos.DrawWireSphere(zone.triggerObject.transform.position, triggerDistance);

                if (zone.launchers != null)
                {
                    foreach (ProjectileLauncher launcher in zone.launchers)
                    {
                        if (launcher.projectileObject != null)
                        {
                            Vector3 startPos = launcher.projectileObject.transform.position;
                            Vector3 endPos = startPos + (Vector3)launcher.shootDirection.normalized * launcher.maxDistance;

                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(startPos, endPos);
                            Gizmos.DrawWireSphere(endPos, 0.3f);
                        }
                    }
                }
            }
        }
    }
}