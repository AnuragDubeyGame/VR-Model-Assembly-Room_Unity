using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_button : MonoBehaviour
{
    public string ModelName;
    public TextMeshProUGUI ObjName;
    public Button DownloadButton;
    public AWS_ObjectLoader objLoader;

    private void Start()
    {
        objLoader = FindObjectOfType<AWS_ObjectLoader>();
    }

    public void DisplayName()
    {
        ObjName.text = ModelName;
        DownloadButton.onClick.AddListener(delegate { objLoader.LoadContent(ModelName); } );
    }
}
