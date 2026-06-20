using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the Charm Inventory UI panel.
/// - Shows unlocked charms in a grid and highlights equipped ones.
/// - Allows equip/unequip only while GameManager.Instance.isSittingOnBench == true.
/// </summary>
public class CharmInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform gridParent; // Grid Layout Group parent for charm slots
    [SerializeField] private GameObject slotPrefab; // Prefab with CharmSlotUI
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Transform notchParent; // visual notches container

    private List<CharmSlotUI> activeSlots = new List<CharmSlotUI>();
    private CharmSlotUI selectedSlot;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnCharmsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        GameManager.OnCharmsChanged -= RefreshUI;
    }

    private void Update()
    {
        // Toggle UI with Inventory key (I) using the new Input System
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.iKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(!panelRoot.activeSelf);
        if (panelRoot.activeSelf) RefreshUI();
    }

    public void RefreshUI()
    {
        // Clear existing slots
        foreach (var slot in activeSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        activeSlots.Clear();

        if (gridParent == null || slotPrefab == null) return;

        // Create slots for each unlocked charm
        if (GameManager.Instance == null) return;

        foreach (var charm in GameManager.Instance.unlockedCharms)
        {
            var go = Instantiate(slotPrefab, gridParent);
            var slot = go.GetComponent<CharmSlotUI>();
            if (slot != null) slot.Initialize(charm, this);
            activeSlots.Add(slot);
        }

        // Update notches (simple activation based on occupied)
        UpdateNotches();
    }

    public void OnSlotClicked(CharmSlotUI slot)
    {
        if (slot == null || slot.Charm == null) return;

        selectedSlot = slot;
        if (nameText != null) nameText.text = slot.Charm.charmName;
        if (descriptionText != null) descriptionText.text = slot.Charm.description;

        // If not at bench, block equip/unequip
        if (GameManager.Instance == null || !GameManager.Instance.isSittingOnBench)
        {
            Debug.Log("Debes descansar en un banco para cambiar tus amuletos.");
            return;
        }

        // Toggle equip state
        if (GameManager.Instance.equippedCharms.Contains(slot.Charm))
        {
            GameManager.Instance.UnequipCharm(slot.Charm);
        }
        else
        {
            GameManager.Instance.EquipCharm(slot.Charm);
        }

        RefreshUI();
    }

    private void UpdateNotches()
    {
        if (notchParent == null || GameManager.Instance == null) return;
        int total = GameManager.Instance.totalNotchSlots;
        int occupied = GameManager.Instance.occupiedNotchSlots;

        for (int i = 0; i < notchParent.childCount; i++)
        {
            var child = notchParent.GetChild(i).gameObject;
            child.SetActive(i < total);
            // visual state: occupied or free
            if (i < total)
            {
                var img = child.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = i < occupied ? Color.white : Color.gray;
                }
            }
        }
    }
}
