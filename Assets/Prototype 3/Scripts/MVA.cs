using UnityEngine;

public class MVA : MonoBehaviour
{
    [Header("Movement Area Settings")]
    public Vector2 areaSize = new Vector2(50f, 50f);
    public bool showAreaGizmo = true;
    public Color areaColor = new Color(0, 0.5f, 1f, 0.3f);
    public Color borderColor = Color.blue;

    private static MVA instance;
    public static MVA Instance => instance ?? FindObjectOfType<MVA>();

    void Start() => instance = this;

    public Vector2 GetRandomPositionInArea()
    {
        Vector2 center = (Vector2)transform.position;
        Vector2 randomOffset = new Vector2(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );
        return center + randomOffset;
    }

    void OnDrawGizmos()
    {
        if (!showAreaGizmo) return;
        Vector3 center = transform.position;

        Gizmos.color = areaColor;
        Gizmos.DrawCube(center, new Vector3(areaSize.x, areaSize.y, 0.1f));

        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0.1f));
    }
}