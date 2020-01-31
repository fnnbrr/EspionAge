using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Light mainLight;
    public Color imageDarkenedColor;
    public List<Image> imagesToDarknen;
    private List<Color> imagesOriginalColor = new List<Color>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMain()
    {
        SceneManager.LoadScene(Constants.SCENE_MAIN);
    }

    public void OnStartButtonEnter()
    {
        mainLight.enabled = false;

        imagesOriginalColor.Clear();
        imagesToDarknen.ForEach(i => {
            imagesOriginalColor.Add(i.color);
            i.color = imageDarkenedColor; 
        });
    }

    public void OnStartButtonExit()
    {
        mainLight.enabled = true;

        imagesToDarknen.ForEach(i => {
            i.color = imagesOriginalColor[imagesToDarknen.IndexOf(i)];
        });
    }
}
