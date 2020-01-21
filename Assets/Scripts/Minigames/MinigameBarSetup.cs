using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MinigameBarSetup : MonoBehaviour
{
    public Transform staminaFillTransform;

    [Header("Preset Assets")]
    public GameObject segmentBasePrefab;
    public Sprite segmentSprite;
    private readonly string segmentName = "Segment";

    public enum MinigameStaminaType
    {
        None,
        Time,
        Target
    }
    public MinigameStaminaType staminaType;
    public Sprite segmentSubSprite;
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
            int toInstantiateLater = 0;

            List<Image> list = new List<Image>();
            foreach (Transform k in staminaFillTransform)
            {
                Image kImage = k.GetComponent<Image>();
                if (kImage)
                {
                    list.Add(kImage);
                }
                else
                {
                    // Destroy anything that does not have an image component
                    DestroyAtEndOfFrame(k.gameObject);
                }
            }

            // Let's force the count to be at least length 2
            if (list.Count < 2)
            {
                while(list.Count + toInstantiateLater < 2)
                {
                    toInstantiateLater++;
                }
            } 
            // Otherwise, check to make sure all have the correct sprite and name
            else
            {
                foreach(Image image in list)
                {
                    image.sprite = segmentSprite;
                    image.name = segmentName;
                }
            }

            if (toInstantiateLater > 0)
            {
                // because we cannot instantiate objects in OnValidate (because it called SendMessage in the background), 
                //  we wait until the end of the frame for this (like deleting objects):
                // https://issuetracker.unity3d.com/issues/sendmessage-cannot-be-called-during-awake-without-using-sendmessage
                StartCoroutine(InstantiateAtEndOfFrame(toInstantiateLater));
            }

            childImages = new List<Image>(list.ToArray());
        }
    }

    IEnumerator InstantiateAtEndOfFrame(int num)
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < num; i++)
        {
            ImageCreateImage();
        }
        UpdateListOfChildren();
    }

    IEnumerator DestroyAtEndOfFrame(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }

    private Image ImageCreateImage()
    {
        GameObject segment = Instantiate(segmentBasePrefab, staminaFillTransform);
        segment.name = segmentName;
        Image segmentImage = segment.GetComponent<Image>();
        segmentImage.sprite = segmentSprite;
        
        if (staminaType.Equals(MinigameStaminaType.Target))
        {
            Image segmentSubImage = segmentImage.gameObject.GetComponentInChildren<Image>();
            segmentSubImage.sprite = segmentSubSprite;
        }
        return segmentImage;
    }
}
