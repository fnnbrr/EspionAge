using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public GameObject textBoxContainer;
    public GameObject canvas;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI conversationText;

    private RectTransform textBoxRect;
    private Vector2 tbAnchor;
    private float anchorX;
    private float anchorY;
    
    private Vector3 textPosition;

    void Start()
    {
        textBoxRect = Utils.GetRequiredComponent<RectTransform>(textBoxContainer);
        Hide();
    }

    public void Update()
    {
        textPosition = Camera.main.WorldToScreenPoint(transform.position);
        textBoxContainer.transform.position = textPosition;

        tbAnchor = textBoxRect.anchoredPosition;
        anchorX = tbAnchor.x;
        anchorY = tbAnchor.y;

        anchorX = Mathf.Clamp(anchorX, 0, Mathf.Max(0, Screen.width - textBoxRect.sizeDelta.x));
        anchorY = Mathf.Clamp(anchorY, -Screen.height + textBoxRect.sizeDelta.y, 0);

        tbAnchor.x = anchorX;
        tbAnchor.y = anchorY;

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
