using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class DragAndDropNog : MonoBehaviour
{
    [MustBeAssigned] public Image rewardSlotImage;
    [MustBeAssigned] public Canvas dynamicCanvas;
    [MustBeAssigned] public GameObject tilesParent;

    //private
    private SpriteRenderer[] allTiles;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetupRewardSlotDragging()
    {

        // Remove existing DragDrop component if present
        var existingDrag = rewardSlotImage.GetComponent<DragDrop>();
        if (existingDrag != null) DestroyImmediate(existingDrag);

        // Add DragDrop component and configure it
        var dragDrop = rewardSlotImage.gameObject.AddComponent<DragDrop>();
        dragDrop.canvas = dynamicCanvas;

        // Ensure CanvasGroup exists for drag functionality
        var canvasGroup = rewardSlotImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = rewardSlotImage.gameObject.AddComponent<CanvasGroup>();

        // Enable raycast target for dragging
        rewardSlotImage.raycastTarget = true;

        //Debug.Log($"Reward slot image is now draggable for {rolledCharacter.soldierType.name}");
    }
}
