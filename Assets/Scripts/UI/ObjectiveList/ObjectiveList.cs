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
    void Start()
    {
        background = GetComponentInChildren<Image>();
        objectiveText = GetComponentInChildren<TMP_Text>();
        root =  GetComponentInChildren<Animator>();
        
        HideObjectiveList();
    }

    public void DisplayObjectiveList()
    {
        root.gameObject.SetActive(true);
    }

    public void DisplayObjectiveText(string[] tags, string[] objectives)
    {
        DisplayObjectiveList();
        
        // if (objectiveText != null) 
        // {   
        //     objectiveText.text = textToSet;
            
        // }
    }

    public void CrossOutObjectiveText()
    {
        objectiveText.text = "<s>" + objectiveText.text + "</s>";
    }

    public void HideObjectiveList()
    {
        root.gameObject.SetActive(false);
    }
}
