using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CloseSessionButton : MonoBehaviour
{
    public Button closeSessionButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeSessionButton.onClick.AddListener(CloseSession);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CloseSession()
    {
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("username");
        SceneManager.LoadScene("MainMenu");
    }
}
