using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class PuntuationController : MonoBehaviour
{
    public static PuntuationController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI myTextElement;
    public int Score { get; private set; }
    
    private const int RESET_SCORE = 0;
    
    private const int SOFT_DROP_BONUS = 1;
    private const int HARD_DROP_BONUS = 2;
    
    private AppConfig _appConfig;

    private async Task Awake()
    {
        _appConfig = await AppConfigLoader.LoadAsync();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (myTextElement != null)
            myTextElement.text = Score.ToString();
    }

    public void AddSoftDrop()
    {
        Score += SOFT_DROP_BONUS;
        if (myTextElement != null)
            myTextElement.text = Score.ToString();
        Debug.Log(Score.ToString());
    }

    public void AddHardDrop()
    {
        Score += HARD_DROP_BONUS;
        if (myTextElement != null)
            myTextElement.text = Score.ToString();
        Debug.Log(Score.ToString());
    }
    
    public void ResetScore()
    {
        Score = RESET_SCORE;
        if (myTextElement != null)
            myTextElement.text = Score.ToString();
        Debug.Log(Score.ToString());
    }
}