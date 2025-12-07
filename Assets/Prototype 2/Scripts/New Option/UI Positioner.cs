using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIPositioner : MonoBehaviour
{
    public SplitScreenManager splitScreen;
    public RectTransform player1UIPanel;
    public RectTransform player2UIPanel;

    // Referencias a los textos de cada jugador
    public TMP_Text p1CoinsText, p1DistText, p1ScoreText;
    public TMP_Text p2CoinsText, p2DistText, p2ScoreText;

    void Start()
    {
        PositionUI();
        ConnectPlayerUI();
    }

    void PositionUI()
    {
        if (splitScreen == null) return;

        if (player1UIPanel != null)
        {
            Rect uiSpace1 = splitScreen.GetPlayer1UISpace();
            player1UIPanel.anchorMin = new Vector2(uiSpace1.x, uiSpace1.y);
            player1UIPanel.anchorMax = new Vector2(uiSpace1.x + uiSpace1.width, uiSpace1.y + uiSpace1.height);
            player1UIPanel.offsetMin = Vector2.zero;
            player1UIPanel.offsetMax = Vector2.zero;
        }

        if (player2UIPanel != null)
        {
            Rect uiSpace2 = splitScreen.GetPlayer2UISpace();
            player2UIPanel.anchorMin = new Vector2(uiSpace2.x, uiSpace2.y);
            player2UIPanel.anchorMax = new Vector2(uiSpace2.x + uiSpace2.width, uiSpace2.y + uiSpace2.height);
            player2UIPanel.offsetMin = Vector2.zero;
            player2UIPanel.offsetMax = Vector2.zero;
        }
    }

    void ConnectPlayerUI()
    {
        // Buscar todos los jugadores
        PlayerScore[] players = FindObjectsOfType<PlayerScore>();

        foreach (PlayerScore player in players)
        {
            if (player.isPlayer1)
            {
                player.coinsText = p1CoinsText;
                player.distanceText = p1DistText;
                player.totalText = p1ScoreText;
            }
            else
            {
                player.coinsText = p2CoinsText;
                player.distanceText = p2DistText;
                player.totalText = p2ScoreText;
            }

            // Actualizar UI inicial
            player.UpdateUI();
        }
    }
}