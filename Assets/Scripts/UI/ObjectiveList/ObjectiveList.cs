using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveList : Singleton<ObjectiveList>
{
    private Image background;
    private TMP_Text objectiveText;
    private Animator root;

    private float previousAxis;
    private float currentAxis;
    private const float DPAD_PRESS_THRESHOLD = 0.75f;

    // Start is called before the first frame update
    void Awake()
    {
        background = GetComponentInChildren<Image>();
        objectiveText = GetComponentInChildren<TMP_Text>();
        root =  GetComponentInChildren<Animator>();
        HideObjectiveList();
    }

    void Update()
    {
        previousAxis = currentAxis; 
        currentAxis = Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL_DPAD);
        if ((currentAxis < - LEFT_DPAD) && (previousAxis >= LEFT_DPAD) && !root.GetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE))
        {
            root.SetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE, true); 
        } 
        else if ((currentAxis < - LEFT_DPAD) && (previousAxis >= LEFT_DPAD) && root.GetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE))
        {
            root.SetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE, false); 
        }
    }

    public void DisplayObjectiveList()
    {
        root.gameObject.SetActive(true);
    }

    public void SlideOutObjectTextForSeconds(float seconds)
    {
        if (!root.GetBool("slideOut"))
        {
            root.SetBool("slideOut", true);
        }

        StartCoroutine(WaitToSlideObjectiveListIn(seconds));
    }

    public void DisplayObjectiveText(string textToSet)
    {
        DisplayObjectiveList();
        
        if (objectiveText != null) 
        {   
            objectiveText.text = textToSet;
            
        }
    }

    public void CrossOutObjectiveText()
    {
        if (!root.GetBool("slideOut"))
        {
            root.SetBool("slideOut", true); 
        }
        objectiveText.text = "<s>" + objectiveText.text + "</s>";

        StartCoroutine(WaitToSlideObjectiveListIn(5f));
    }

    IEnumerator WaitToSlideObjectiveListIn(float time)
    {
        yield return new WaitForSeconds(time);
        root.SetBool("slideOut", false); 
    }

    public void HideObjectiveList()
    {
        root.gameObject.SetActive(false);
    }
}
