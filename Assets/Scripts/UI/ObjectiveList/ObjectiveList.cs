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
    private bool slideOut;

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponentInChildren<Image>();
        objectiveText = GetComponentInChildren<TMP_Text>();
        root =  GetComponentInChildren<Animator>();
        HideObjectiveList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !root.GetBool("slideOut"))
        {
            root.SetBool("slideOut", true); 
        } else if (Input.GetKeyDown(KeyCode.Space) && root.GetBool("slideOut"))
        {
            root.SetBool("slideOut", false); 
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
    }

    public void HideObjectiveList()
    {
        root.gameObject.SetActive(false);
    }

    
}
