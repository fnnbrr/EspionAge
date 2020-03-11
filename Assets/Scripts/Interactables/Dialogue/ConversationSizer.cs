using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConversationSizer : MonoBehaviour
{ 
    public TMP_Text text;
    public RectTransform rt;
    Vector2 preferredSize;
    public Vector2 padding;

    // Start is called before the first frame update
    void Start()
    {
        SetConversationBoxSize();

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetConversationBoxSize()
    {
        preferredSize = text.GetPreferredValues(Mathf.Infinity, Mathf.Infinity);
        text.rectTransform.SetPositionAndRotation(new Vector3(text.rectTransform.position.x + padding.x, text.rectTransform.position.y + padding.y, 0f), text.rectTransform.rotation);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x + padding.x * 2);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y + padding.y * 2);
    }
}
