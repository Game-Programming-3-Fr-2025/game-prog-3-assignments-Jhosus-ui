using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIPositioner : MonoBehaviour
{
    public SplitScreenManager splitScreen;
    public RectTransform player1UIPanel;
    public RectTransform player2UIPanel;

    private List<LifeSystem> lifeSystems = new List<LifeSystem>();

    void Start()
    {
        FindAllLifeSystems();
        PositionUIElements();
        NotifyLifeSystems();
    }

    void FindAllLifeSystems()
    {
        // Buscar todos los LifeSystem en la escena
        LifeSystem[] foundSystems = FindObjectsOfType<LifeSystem>();
        lifeSystems.Clear();
        lifeSystems.AddRange(foundSystems);

        Debug.Log($"Encontrados {lifeSystems.Count} sistemas de vida");
    }

    void PositionUIElements()
    {
        if (splitScreen == null) return;

        // Posicionar UI del Player 1 (Área derecha superior)
        if (player1UIPanel != null)
        {
            Rect uiSpace1 = splitScreen.GetPlayer1UISpace();
            player1UIPanel.anchorMin = new Vector2(uiSpace1.x, uiSpace1.y);
            player1UIPanel.anchorMax = new Vector2(uiSpace1.x + uiSpace1.width, uiSpace1.y + uiSpace1.height);
            player1UIPanel.offsetMin = Vector2.zero;
            player1UIPanel.offsetMax = Vector2.zero;
        }

        // Posicionar UI del Player 2 (Área izquierda inferior)  
        if (player2UIPanel != null)
        {
            Rect uiSpace2 = splitScreen.GetPlayer2UISpace();
            player2UIPanel.anchorMin = new Vector2(uiSpace2.x, uiSpace2.y);
            player2UIPanel.anchorMax = new Vector2(uiSpace2.x + uiSpace2.width, uiSpace2.y + uiSpace2.height);
            player2UIPanel.offsetMin = Vector2.zero;
            player2UIPanel.offsetMax = Vector2.zero;
        }
    }

    void NotifyLifeSystems()
    {
        foreach (LifeSystem lifeSystem in lifeSystems)
        {
            if (lifeSystem != null)
            {
                lifeSystem.UpdateUIPosition();
            }
        }
    }

    // Método para añadir un LifeSystem manualmente
    public void RegisterLifeSystem(LifeSystem lifeSystem)
    {
        if (!lifeSystems.Contains(lifeSystem))
        {
            lifeSystems.Add(lifeSystem);
            lifeSystem.UpdateUIPosition();
        }
    }

    // Método público para forzar actualización
    public void RefreshUI()
    {
        FindAllLifeSystems();
        PositionUIElements();
        NotifyLifeSystems();
    }

}