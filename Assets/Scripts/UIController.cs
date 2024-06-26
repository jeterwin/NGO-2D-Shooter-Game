using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    [SerializeField] private Slider healthImage;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;


    private void Awake()
    {
        Instance = this;
    }
    public void UpdateHealth(int health)
    {
        healthImage.value = health;
    }
    public void UpdateStatsDisplay()
    {

    }
}
