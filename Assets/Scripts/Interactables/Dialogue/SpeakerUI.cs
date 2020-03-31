using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public GameObject aButton;
    public GameObject speechBubbleOutline;
    public GameObject textBoxContainer;
    public GameObject canvas;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI conversationText;
    public bool isRescalable = true;
    public bool isClampToScreen = true;


    private RectTransform textBoxRect;
    private RectTransform bubbleOutlineRect;
    private Vector2 tbAnchor;
    private VerticalLayoutGroup verticalLayoutGroup;
    
    private Vector3 textPosition;

    // Used for padding on right side of screen for repositioning when resizing
    public float extraPadding = 20f;

    private void Awake()
    {
        // textBoxRect is needed to for the anchor position and outlineRect is used to find the width of
        // the resizable speech bubble because textBoxRect is the maxWidth
        textBoxRect = Utils.GetRequiredComponent<RectTransform>(textBoxContainer);
        bubbleOutlineRect = Utils.GetRequiredComponent<RectTransform>(speechBubbleOutline);
        verticalLayoutGroup = Utils.GetRequiredComponent<VerticalLayoutGroup>(speechBubbleOutline);
    }

    void Start()
    {
        Hide();
    }

    public void Update()
    {
        if (isRescalable)
        {
            RescaleSpeechBubble();
        }

        textPosition = Camera.main.WorldToScreenPoint(transform.position);
        textBoxContainer.transform.position = textPosition;

        if (isClampToScreen)
        {
            tbAnchor = textBoxRect.anchoredPosition;

            tbAnchor.x = Mathf.Clamp(tbAnchor.x, 0, Mathf.Max(0, Screen.width - bubbleOutlineRect.sizeDelta.x));
            tbAnchor.y = Mathf.Clamp(tbAnchor.y, Mathf.Min(-Screen.height + textBoxRect.sizeDelta.y, 0), 0);

            if (tbAnchor.x <= 0)
            {
                tbAnchor.x += extraPadding;
            }
            else
            {
                if (tbAnchor.x + bubbleOutlineRect.sizeDelta.x >= Screen.width - extraPadding / 2)
                {
                    tbAnchor.x -= extraPadding;
                }
            }

            if (tbAnchor.y >= 0)
            {
                tbAnchor.y -= extraPadding;
            }
            else
            {
                if (tbAnchor.y - bubbleOutlineRect.sizeDelta.y <= -Screen.height + extraPadding / 2)
                {
                    tbAnchor.y += extraPadding;
                }
            }

            textBoxRect.anchoredPosition = tbAnchor;
        }
    }

    public void SetActiveAButton(bool setActive)
    {
        aButton.SetActive(setActive);

        if (setActive)
        {
            verticalLayoutGroup.padding.right = Constants.RIGHT_PADDING_WITH_ABUTTON;
        }
        else
        {
            verticalLayoutGroup.padding.right = Constants.RIGHT_PADDING_WITHOUT_ABUTTON;
        }
    }

    public void Show()
    {
        canvas.SetActive(true);
    }

    public void Hide()
    {
        canvas.SetActive(false);
    }

    private void RescaleSpeechBubble()
    {
        float scale = Mathf.Lerp(Constants.MIN_SCALE_SPEECHBUBBLE, Constants.MAX_SCALE_SPEECHBUBBLE,
            Constants.DISTANCE_FOR_SPEECHBUBBLE_SCALING/GetDistanceFromPlayer());
        textBoxRect.localScale = new Vector3(scale, scale, scale);
    }

    private float GetDistanceFromPlayer()
    {
        return Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);
    }

    public void SetIsClamp(bool isClamp)
    {
        isClampToScreen = isClamp;
    }
}
