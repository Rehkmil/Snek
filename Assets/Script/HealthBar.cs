using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerController player;
    public Slider slider;

    void Start()
    {
        slider.maxValue = player.maxHealth;
        slider.value = player.currentHealth;
    }

    void Update()
    {
        slider.value = player.currentHealth;
    }
}
