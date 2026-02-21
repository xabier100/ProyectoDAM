using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpButton : MonoBehaviour
{
    public Button popUpButton;

    public GameObject popUp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        popUpButton.onClick.AddListener(Accept);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Accept()
    {
        popUp.SetActive(false);
    }
}
