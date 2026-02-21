using Scipts.PostLogin;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModifyButton : MonoBehaviour
{
    public Button modifyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modifyButton.onClick.AddListener(Modify);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Modify()
    {
        VerifyRequirementsController instance = VerifyRequirementsController.Instance;
        instance.IsVerified(
            onSuccess: emailVerified =>
            {
                if (emailVerified==1)
                    SceneManager.LoadScene("UserMenu");
                else
                    Debug.Log("Please verify your account to modify data.");
            },
            onError: ex => Debug.LogError("An error occurred while verifying requirements: " + ex.Message)
        );
    }
}
