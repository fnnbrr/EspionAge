using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class MinigameInstantiateLater
{
    public Sprite sprite;
    public string name;

    public MinigameInstantiateLater(Sprite s, string n)
    {
        sprite = s;
        name = n;
    }
}

[ExecuteAlways]
public class MinigameBarSetup : MonoBehaviour
{
    public Transform staminaFillTransform;

    [Header("Preset Assets")]
    public GameObject segmentBasePrefab;

    public Sprite startSegmentSprite;
    private readonly string startSegmentName = "StartSegment";
    public Sprite middleSegmentSprite;
    private readonly string middleSegmentName = "MiddleSegment";
    public Sprite endSegmentSprite;
    private readonly string endSegmentName = "EndSegment";

    public enum MinigameStaminaType
    {
        None,
        Time,
        Target
    }
    public MinigameStaminaType staminaType;
    public Sprite segmentSprite;
    public List<Image> childImages;

    private void OnValidate()
    {
        UpdateListOfChildren();
    }

    void UpdateListOfChildren()
    {
        if (staminaType == MinigameStaminaType.None)
        {
            return;
        }
        else if (staminaType == MinigameStaminaType.Time)
        {
            childImages.Clear();
            foreach (Transform child in staminaFillTransform)
            {
                // because we cannot destroy objects in OnValidate, we wait until the end of the frame for this:
                // https://answers.unity.com/questions/1318576/destroy-child-objects-onvalidate.html
                StartCoroutine(DestroyAtEndOfFrame(child.gameObject));
            }
        }
        else
        {
            List<MinigameInstantiateLater> toInstantiateLater = new List<MinigameInstantiateLater>();

            List<Image> list = new List<Image>();
            Image[] kids = staminaFillTransform.GetComponentsInChildren<Image>(true);
            foreach (Image k in kids)
            {
                if (k.transform.parent == staminaFillTransform)
                {
                    list.Add(k);
                }
            }
            if (list.Count > 0)
            {
                if (list[0].sprite != startSegmentSprite)
                {
                    list[0].sprite = startSegmentSprite;
                }
                list[0].name = startSegmentName;

                if (list.Count > 1)
                {
                    for (int i = 1; i < list.Count - 1; i++)
                    {
                        if (list[i].sprite != middleSegmentSprite)
                        {
                            list[i].sprite = middleSegmentSprite;
                        }
                        list[i].name = middleSegmentName;
                    }

                    if (list[list.Count - 1].sprite != endSegmentSprite)
                    {
                        list[list.Count - 1].sprite = endSegmentSprite;
                    }
                    list[list.Count - 1].name = endSegmentName;
                }
                else
                {
                    toInstantiateLater.Add(CreateEndSegment());
                }
            }
            else
            {
                toInstantiateLater.Add(CreateStartSegment());
                toInstantiateLater.Add(CreateEndSegment());
            }

            if (toInstantiateLater.Count > 0)
            {
                // because we cannot instantiate objects in OnValidate (because it called SendMessage in the background), 
                //  we wait until the end of the frame for this (like deleting objects):
                // https://issuetracker.unity3d.com/issues/sendmessage-cannot-be-called-during-awake-without-using-sendmessage
                StartCoroutine(InstantiateAtEndOfFrame(toInstantiateLater));
            }

            childImages = new List<Image>(list.ToArray());
        }
    }

    IEnumerator InstantiateAtEndOfFrame(List<MinigameInstantiateLater> instLater)
    {
        yield return new WaitForEndOfFrame();
        foreach (MinigameInstantiateLater toInst in instLater)
        {
            ImageCreateImage(toInst.sprite, toInst.name);
        }
        UpdateListOfChildren();
    }

    IEnumerator DestroyAtEndOfFrame(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }

    private MinigameInstantiateLater CreateStartSegment()
    {
        return new MinigameInstantiateLater(startSegmentSprite, startSegmentName);
    }

    private MinigameInstantiateLater CreateMiddleSegment()
    {
        return new MinigameInstantiateLater(middleSegmentSprite, middleSegmentName);
    }

    private MinigameInstantiateLater CreateEndSegment()
    {
        return new MinigameInstantiateLater(endSegmentSprite, endSegmentName);
    }

    private Image ImageCreateImage(Sprite sprite, string name)
    {
        GameObject segment = Instantiate(segmentBasePrefab, staminaFillTransform);
        segment.name = name;
        Image segmentImage = segment.GetComponent<Image>();
        segmentImage.sprite = sprite;
        
        if (staminaType.Equals(MinigameStaminaType.Target))
        {
            Image segmentSubSprite = segmentImage.gameObject.GetComponentInChildren<Image>();
            segmentSubSprite.sprite = segmentSprite;
        }
        return segmentImage;
    }
}
