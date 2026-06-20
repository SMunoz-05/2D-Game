using UnityEngine;

[CreateAssetMenu(fileName = "CharmData", menuName = "Metroidvania/Charm", order = 100)]
public class CharmData : ScriptableObject
{
    [Header("Identidad")]
    public string charmID;

    [Tooltip("Nombre mostrado del amuleto")]
    public string charmName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite icon;

    [Header("Modificadores de Estadísticas")]
    public float speedMultiplier = 1f;       // Ej: 1.2f aumentaría un 20% la velocidad
    public float attackRangeMultiplier = 1f; // Ej: 1.3f aumentaría un 30% el rango del tajo
    public float soulGainMultiplier = 1f;    // Ej: 1.5f aumentaría la ganancia de alma

    [Header("Coste")]
    public int notchCost = 1;
}
