using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContentLayoutHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loreText;
    [SerializeField] private ScrollRect scrollRect;

    private string lastText = "";

    private void LateUpdate()
    {
        if (loreText != null && loreText.text != lastText)
        {
            lastText = loreText.text;
            RebuildLayout();
        }
    }

    public void RebuildLayout()
    {
        if (loreText != null)
        {
            RectTransform contentRect = (RectTransform)loreText.transform.parent;

            // Force rebuild the text to get proper layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(loreText.GetComponent<RectTransform>());

            // Rebuild the content container
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            // Adjust content height based on text
            float preferredHeight = LayoutUtility.GetPreferredHeight(loreText.GetComponent<RectTransform>());
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
