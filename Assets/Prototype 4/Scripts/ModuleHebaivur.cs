using UnityEngine;

public class ModuleBehavior : MonoBehaviour
{
    public int moduleLevel = 1;
    public Sprite[] levelSprites; // Sprites para diferentes niveles

    void Start()
    {
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && levelSprites != null && moduleLevel <= levelSprites.Length)
        {
            sr.sprite = levelSprites[moduleLevel - 1];
        }
    }

    public void SetLevel(int level)
    {
        moduleLevel = level;
        UpdateVisuals();
    }
}