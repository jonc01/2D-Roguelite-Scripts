using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("Health Numbers (optional)")]
    [SerializeField] TextMeshProUGUI healthNumbers;
    [SerializeField] bool isEnemy = true;

    [Header("Health Bars")]
    [SerializeField] private float health;
    private float lerpTimer;
    public float maxHealth = 50f;
    public float chipSpeed = 2f;
    public Slider mainHealthBar;
    public Slider trailHealthBar;
    public Image trailHealthBarImage;
    Color trailColor;

    private void Start()
    {
        //SetHealth();
        //if (isEnemy) trailColor = Color.HSVToRGB(0.083f, 1f, 1f); //hsv(38, 100%, 100%) orange
        if (isEnemy) trailColor = Color.white; //hsv(38, 100%, 100%) orange
        else trailColor = Color.red;
    }

    public void SetHealth(float setMaxHP)
    {
        maxHealth = setMaxHP;
        health = maxHealth;
    }

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        float fillMain = mainHealthBar.value;
        float fillTrail = trailHealthBar.value;
        float hFraction = health / maxHealth;

        if(fillTrail > hFraction) //Damage taken
        {
            trailHealthBarImage.color = trailColor;
            mainHealthBar.value = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            trailHealthBar.value = Mathf.Lerp(fillTrail, hFraction, percentComplete);
        }
        if(fillMain < hFraction)
        {
            trailHealthBarImage.color = Color.green;
            trailHealthBar.value = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            mainHealthBar.value = Mathf.Lerp(fillMain, trailHealthBar.value, percentComplete);
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        health = currentHealth;
        lerpTimer = 0f;

        if(healthNumbers != null)
            healthNumbers.text = currentHealth + "/" + maxHealth;
    }
}
