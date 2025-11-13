using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class P2Life : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Interfaz de Corazones")]
    public List<Image> heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private void Start()
    {
        // Inicializar la vida al máximo
        currentHealth = maxHealth;

        // Actualizar la interfaz de corazones
        UpdateHeartsUI();
    }

    // Función para recibir daño
    public void TakeDamage(int damageAmount)
    {
        // Reducir la vida
        currentHealth -= damageAmount;

        // Asegurarse de que la vida no sea menor a 0
        currentHealth = Mathf.Max(0, currentHealth);

        // Actualizar la interfaz
        UpdateHeartsUI();

        // Verificar si el jugador murió
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Función para curar/recuperar vida
    public void Heal(int healAmount)
    {
        // Aumentar la vida
        currentHealth += healAmount;

        // Asegurarse de que la vida no exceda el máximo
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Actualizar la interfaz
        UpdateHeartsUI();
    }

    // Función para actualizar los corazones en la UI
    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                // Mostrar corazón lleno
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                // Mostrar corazón vacío
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    // Función que se llama cuando el jugador muere
    private void Die()
    {
        Debug.Log("¡Jugador 2 ha muerto!");
    }

    // Función para reiniciar la vida al máximo
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
    }
}