using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI txtLevelText;
    public int LevelNumber { get; private set; }
    
    private const int ResetLevelNumber = 1;
    
    private int _clearedLines = 0;
    
    public const int NumberOfLinesToLevelUp = 2;
    
    private const int SoftDropBonus = 1;
    private const int HardDropBonus = 2;
    private const int SingleLineClear = 100;
    private const int DoubleLineClear = 300;
    private const int TripleLineClear = 500;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetLevelCounter();
        if (txtLevelText != null)
            txtLevelText.text = LevelNumber.ToString();
    }
    
    
    public void ResetLevelCounter()
    {
        LevelNumber = ResetLevelNumber;
        if (txtLevelText != null)
            txtLevelText.text = LevelNumber.ToString();
    }
    
    public void IncreaseLevelCounter()
    {
        LevelNumber += 1;
        txtLevelText.text = LevelNumber.ToString();
    }

    public void addClearedLines()
    {
        _clearedLines++;
        Debug.Log("Cleared Lines: "+_clearedLines);
        if(_clearedLines==NumberOfLinesToLevelUp)
        {
            IncreaseLevelCounter();
            _clearedLines = 0;
        }
    }
}
