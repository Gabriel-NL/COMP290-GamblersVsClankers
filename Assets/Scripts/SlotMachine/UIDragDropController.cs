using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Image = UnityEngine.UI.Image;
/// Old Input System only (Input.GetMouseButton*).
/// External controller: attach to a neutral "Scripts" GameObject.
public class UIDragDropController : MonoBehaviour
{
    [Header("Source (click/hold here)")]
    [MustBeAssigned][SerializeField] private Canvas staticCanvas;   // canvas that holds the source image
    [MustBeAssigned][SerializeField] private Image sourceImage;    // the specific image you must click

    [Header("Dragged (follows the mouse)")]
    [MustBeAssigned][SerializeField] private Canvas dynamicCanvas;  // canvas that holds the draggable image
    [MustBeAssigned][SerializeField] private RectTransform draggableImage; // the image to move (its RectTransform)
    private Image draggableImageTexture;

    [Header("Options")]
    [Tooltip("Hide draggable image until a drag begins.")]
    [SerializeField] private bool startHidden = true;

    [Tooltip("Keep the cursor exactly on the image pivot while dragging.")]
    [SerializeField] private bool cursorSnapsToPivot = true;

    [Tooltip("Optional clamp so the draggable stays inside the dynamic canvas rect.")]
    [SerializeField] private bool clampInsideDynamicCanvas = false;

    [Header("Events")]
    public UnityEvent onDragStart;
    public UnityEvent onDragTick;
    public UnityEvent onDragEnd;   // fires on mouse release

    private bool isDragging = false;
    private Vector2 grabOffsetLocal; // cursor offset (in dynamic canvas local space)

    void Awake()
    {
        draggableImageTexture = draggableImage.GetComponent<Image>();

        if (startHidden && draggableImage) draggableImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Press: begin drag only if the press is over the source image (on the static canvas).
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverSourceImage() && sourceImage.sprite != null)
            {
                draggableImageTexture.sprite = sourceImage.sprite;
                BeginDrag();
            }
        }

        // While dragging: move the draggable image to cursor position (on dynamic canvas).
        if (isDragging)
        {
            MoveDraggableToCursor();

            onDragTick?.Invoke();

            // Release: end drag
            if (Input.GetMouseButtonUp(0) || !Application.isFocused)
            {
                EndDrag();
            }
        }
    }

    // --- Core helpers ---

    private bool IsPointerOverSourceImage()
    {
        if (!sourceImage || !staticCanvas) return false;

        Camera cam =
            staticCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : staticCanvas.worldCamera;

        // Fast hit test without EventSystem:
        return RectTransformUtility.RectangleContainsScreenPoint(
            sourceImage.rectTransform,
            Input.mousePosition,
            cam
        );
    }

    private void BeginDrag()
    {
        isDragging = true;

        if (draggableImage && !draggableImage.gameObject.activeSelf)
            draggableImage.gameObject.SetActive(true);

        // Compute initial grab offset so the image doesn't "jump" under the cursor,
        // unless we want it to snap exactly to its pivot.
        grabOffsetLocal = Vector2.zero;

        if (!cursorSnapsToPivot)
        {
            if (TryScreenToDynamicLocal(Input.mousePosition, out var localPoint))
            {
                grabOffsetLocal = localPoint - draggableImage.anchoredPosition;
            }
        }

        MoveDraggableToCursor(); // place immediately on frame 0
        onDragStart?.Invoke();
    }

    private void EndDrag()
    {
        isDragging = false;
        onDragEnd?.Invoke();
        // If you want the dragged image to hide after drop, uncomment:
        // if (draggableImage) draggableImage.gameObject.SetActive(false);
    }

    private void MoveDraggableToCursor()
    {
        if (!draggableImage || !dynamicCanvas) return;

        if (TryScreenToDynamicLocal(Input.mousePosition, out var localPoint))
        {
            var target = cursorSnapsToPivot ? localPoint : (localPoint - grabOffsetLocal);

            if (clampInsideDynamicCanvas)
            {
                var canvasRect = dynamicCanvas.transform as RectTransform;
                target = ClampToRect(canvasRect, draggableImage, target);
            }

            draggableImage.anchoredPosition = target;
        }
    }

    private bool TryScreenToDynamicLocal(Vector3 screenPos, out Vector2 localPoint)
    {
        var dynRect = dynamicCanvas.transform as RectTransform;
        Camera cam = dynamicCanvas.worldCamera;

        RectTransform dynamicCanvasRectTransform = dynamicCanvas.transform as RectTransform;

        Vector3 screenPosition = screenPos;
        Camera cameraForConversion =
            dynamicCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : dynamicCanvas.worldCamera;

        bool isInsideDynamicCanvasRectangle = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dynamicCanvasRectTransform,
            screenPosition,
            cameraForConversion,
            out localPoint
        );

        // Retornar o resultado booleano indicando se o ponto em tela está dentro do retângulo do canvas dinâmico.
        return isInsideDynamicCanvasRectangle;
    }

    private static Vector2 ClampToRect(RectTransform container, RectTransform item, Vector2 desired)
    {
        if (!container || !item) return desired;

        // Get container rect in its own local space
        Rect cr = container.rect;

        // Compute item extents relative to its pivot to keep the whole element inside
        Vector2 size = item.rect.size;
        Vector2 pivot = item.pivot;
        float left = desired.x - size.x * pivot.x;
        float right = desired.x + size.x * (1f - pivot.x);
        float bottom = desired.y - size.y * pivot.y;
        float top = desired.y + size.y * (1f - pivot.y);

        float dx = 0f, dy = 0f;
        if (left < cr.xMin) dx = cr.xMin - left;
        if (right > cr.xMax) dx = cr.xMax - right;
        if (bottom < cr.yMin) dy = cr.yMin - bottom;
        if (top > cr.yMax) dy = cr.yMax - top;

        return desired + new Vector2(dx, dy);
    }
}
