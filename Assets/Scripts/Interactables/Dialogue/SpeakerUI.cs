using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public GameObject speechBubbleOutline;
    public GameObject textBoxContainer;
    public GameObject canvas;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI conversationText;

    private RectTransform textBoxRect;
    private RectTransform bubbleOutlineRect;
    private Vector2 tbAnchor;
    
    private Vector3 textPosition;

    // Used for padding on right side of screen for repositioning when resizing
    public float extraXPadding = 20f;

    private void Awake()
    {
        // textBoxRect is needed to for the anchor position and outlineRect is used to find the width of
        // the resizable speech bubble because textBoxRect is the maxWidth
        textBoxRect = Utils.GetRequiredComponent<RectTransform>(textBoxContainer);
        bubbleOutlineRect = Utils.GetRequiredComponent<RectTransform>(speechBubbleOutline);
    }

    void Start()
    {
        Hide();
    }

    public void Update()
    {
        textPosition = Camera.main.WorldToScreenPoint(transform.position);
        textBoxContainer.transform.position = textPosition;

        tbAnchor = textBoxRect.anchoredPosition;

        tbAnchor.x = Mathf.Clamp(tbAnchor.x, 0, Mathf.Max(0, Screen.width - bubbleOutlineRect.sizeDelta.x - extraXPadding));
        tbAnchor.y = Mathf.Clamp(tbAnchor.y, Mathf.Min(-Screen.height + textBoxRect.sizeDelta.y, 0), 0);

        textBoxRect.anchoredPosition = tbAnchor;
    }

    public void Show()
    {
        canvas.SetActive(true);
    }

    public void Hide()
    {
        canvas.SetActive(false);
    }
}
