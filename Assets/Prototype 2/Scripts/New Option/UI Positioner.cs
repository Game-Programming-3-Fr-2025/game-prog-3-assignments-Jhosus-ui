using UnityEngine;
using UnityEngine.UI;

public class UIPositioner : MonoBehaviour
{
    public SplitScreenManager splitScreen;
    public RectTransform player1UIPanel;
    public RectTransform player2UIPanel;

    void Start()
    {
        PositionUIElements();
    }

    void PositionUIElements()
    {
        if (splitScreen == null) return;

        // Posicionar UI del Player 1 (área derecha superior)
        Rect uiSpace1 = splitScreen.GetPlayer1UISpace();
        player1UIPanel.anchorMin = new Vector2(uiSpace1.x, uiSpace1.y);
        player1UIPanel.anchorMax = new Vector2(uiSpace1.x + uiSpace1.width, uiSpace1.y + uiSpace1.height);

        // Posicionar UI del Player 2 (área izquierda inferior)  
        Rect uiSpace2 = splitScreen.GetPlayer2UISpace();
        player2UIPanel.anchorMin = new Vector2(uiSpace2.x, uiSpace2.y);
        player2UIPanel.anchorMax = new Vector2(uiSpace2.x + uiSpace2.width, uiSpace2.y + uiSpace2.height);
    }
}