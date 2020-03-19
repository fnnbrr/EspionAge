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
        if (((Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL_DPAD) < -0.75f) || Input.GetKeyDown("j")) && !root.GetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE))
        {
            root.SetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE, true); 
        } 
        else if (((Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL_DPAD) < -0.75f) || Input.GetKeyDown("j")) && root.GetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE))
        {
            root.SetBool(Constants.ANIMATION_OBJECTIVELIST_SLIDE, false); 
        }
    }

    public void DisplayObjectiveList()
    {
        root.gameObject.SetActive(true);
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

        StartCoroutine(WaitToSlideObjectiveListIn(5));
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
