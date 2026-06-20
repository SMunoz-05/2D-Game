using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI component for a single charm slot in the inventory grid.
/// Attach this to the slot prefab and wire the child Image and frame objects.
/// </summary>
public class CharmSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject equippedFrame;
    [SerializeField] private GameObject lockedOverlay;

    private CharmData charmData;
    private CharmInventoryUI parentUI;

    public CharmData Charm => charmData;

    /// <summary>Initialize slot with data and parent reference.</summary>
    public void Initialize(CharmData data, CharmInventoryUI parent)
    {
        charmData = data;
        parentUI = parent;

        if (iconImage != null)
            iconImage.sprite = data != null ? data.icon : null;

        UpdateVisuals();
    }

    /// <summary>Set visual state depending on equipped/locked.</summary>
    public void UpdateVisuals()
    {
        bool isEquipped = false;
        if (charmData != null && GameManager.Instance != null)
            isEquipped = GameManager.Instance.equippedCharms.Contains(charmData);

        if (equippedFrame != null) equippedFrame.SetActive(isEquipped);
        if (lockedOverlay != null) lockedOverlay.SetActive(charmData == null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        parentUI?.OnSlotClicked(this);
    }
}
