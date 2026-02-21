using UnityEngine;
using UnityEngine.UI;

public class GoBack : MonoBehaviour
{
    public Button goBackButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goBackButton.onClick.AddListener(GoBackAction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoBackAction()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("PostLogin");
    }
}
