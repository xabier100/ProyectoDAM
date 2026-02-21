using Scipts.PostLogin;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Button playButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playButton.onClick.AddListener(Play);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Play()
    {
        VerifyRequirementsController instance = VerifyRequirementsController.Instance;
            instance.IsVerified(
                onSuccess: emailVerified =>
                {
                    if (emailVerified==1)
                        SceneManager.LoadScene("Tetraversus");
                    else
                        Debug.Log("Please verify your account to play.");
                },
                onError: ex => Debug.LogError("An error occurred while verifying requirements: " + ex.Message)
            );
    }
}
