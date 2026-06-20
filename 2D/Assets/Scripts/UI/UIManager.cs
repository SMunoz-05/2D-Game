using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Componentes de Salud (Máscaras)")]
    public GameObject maskPrefab;
    public Transform masksContainer;
    private List<GameObject> spawnedMasks = new List<GameObject>();

    [Header("Componentes de Alma (Energía)")]
    public Slider soulSlider;
    public Image fillImage;                // ¡Aquí es donde ocurre la magia visual!

    [Header("Colores del Contenedor de Alma")]
    public Color colorVacio = new Color(0.3f, 0.4f, 0.6f);      // Azul oscuro apagado
    public Color colorCuracionLista = new Color(1f, 1f, 1f);   // Blanco brillante / Resplandor
    public Color colorMaximo = new Color(1f, 0.85f, 0f);       // Amarillo Dorado de Hollow Knight

    [Header("Componentes de Feedback Técnico")]
    public GameObject chargeIndicator;

    private bool estaAlMaximo = false;
    private float tiempoParpadeo;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // Si el contenedor está lleno, hacemos que el fillImage parpadee entre el color máximo y blanco
        if (estaAlMaximo && fillImage != null)
        {
            tiempoParpadeo += Time.deltaTime * 12f; // Controla la velocidad del destello
            float interpolacion = Mathf.PingPong(tiempoParpadeo, 1f);
            fillImage.color = Color.Lerp(colorMaximo, Color.white, interpolacion);
        }
    }

    public void InitializeHealthUI(int maxHealth)
    {
        foreach (GameObject mask in spawnedMasks) Destroy(mask);
        spawnedMasks.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newMask = Instantiate(maskPrefab, masksContainer);
            spawnedMasks.Add(newMask);
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        for (int i = 0; i < spawnedMasks.Count; i++)
        {
            spawnedMasks[i].SetActive(i < currentHealth);
        }
    }

    // Actualiza la barra y evalúa los estados lógicos de color y parpadeo
    public void UpdateSoulUI(float currentSoul, float maxSoul)
    {
        if (soulSlider == null || fillImage == null) return;

        // 1. Actualizar el valor matemático del Slider
        soulSlider.value = currentSoul / maxSoul;

        // 2. Controlar los estados de color y feedback visuales sencillos
        if (currentSoul >= maxSoul)
        {
            // Estado: Lleno Total (Prende la bandera de parpadeo en el Update)
            estaAlMaximo = true;
        }
        else
        {
            estaAlMaximo = false; // Apaga el parpadeo si consume alma

            if (currentSoul >= 33f) // 33f es el soulCostToHeal de tu PlayerHealth
            {
                // Estado: Tienes suficiente energía para presionar curar
                fillImage.color = colorCuracionLista;
            }
            else
            {
                // Estado: Energía insuficiente, se ve apagada
                fillImage.color = colorVacio;
            }
        }
    }

    public void ToggleChargeIndicator(bool isReady)
    {
        if (chargeIndicator != null)
        {
            chargeIndicator.SetActive(isReady);
        }
    }
}