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
    
    private Vector3 textPosition;

    private void Awake()
    {
        textBoxRect = Utils.GetRequiredComponent<RectTransform>(textBoxContainer);
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

        tbAnchor.x = Mathf.Clamp(tbAnchor.x, 0, Mathf.Max(0, Screen.width - textBoxRect.sizeDelta.x));
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
