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
    private VerticalLayoutGroup verticalLayoutGroup;

    // Used for padding on right side of screen for repositioning when resizing
    private float extraPadding = 40f;

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

        textBoxContainer.transform.position = Camera.main.WorldToScreenPoint(transform.position);

        if (isClampToScreen)
        {
            var refResolution = Utils.GetRequiredComponent<RectTransform>(canvas).sizeDelta;
            float scale = GetScale(Screen.width, Screen.height, refResolution, 0.5f);
            float padding = extraPadding / scale;
            bool needsToBeRepositioned = false;
            Vector2 tbAnchor = textBoxRect.anchoredPosition;

            // Too Far Left
            if (tbAnchor.x < padding)
            {
                tbAnchor.x = padding;
                needsToBeRepositioned = true;
            }

            // Too Far Right
            if (tbAnchor.x + bubbleOutlineRect.sizeDelta.x > Screen.width/scale - padding)
            {
                tbAnchor.x = Screen.width/scale - bubbleOutlineRect.sizeDelta.x - padding;
                needsToBeRepositioned = true;
            }

            // Too Far Top
            if (tbAnchor.y > -padding)
            {
                tbAnchor.y = -padding;
                needsToBeRepositioned = true;
            }

            // Too Far Bottom 
            if (tbAnchor.y - bubbleOutlineRect.sizeDelta.y - padding < -Screen.height/scale)
            {
                tbAnchor.y = -Screen.height/scale + bubbleOutlineRect.sizeDelta.y + padding;
                needsToBeRepositioned = true;
            }

            if (needsToBeRepositioned)
            {
                textBoxRect.anchoredPosition = tbAnchor;
            }
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

    private float GetScale(int width, int height, Vector2 scalerReferenceResolution, float scalerMatchWidthOrHeight)
    {
        return Mathf.Pow(width / scalerReferenceResolution.x, 1f - scalerMatchWidthOrHeight) *
               Mathf.Pow(height / scalerReferenceResolution.y, scalerMatchWidthOrHeight);
    }
}
