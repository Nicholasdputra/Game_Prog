using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    Resolution[] resolutions;
    bool isFullscreen;
    int currentResolutionIndex;

    // Start is called before the first frame update
    void Start()
    {
        isFullscreen = true;
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
