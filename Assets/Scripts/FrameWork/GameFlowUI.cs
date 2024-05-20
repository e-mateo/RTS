using UnityEngine;
using UnityEngine.UI;

public class GameFlowUI : MonoBehaviour
{
    [SerializeField] Text GameOverText;
    [SerializeField] Text WinnerText;
    void Start()
    {
        GameOverText?.gameObject.SetActive(false);
        WinnerText?.gameObject.SetActive(false);

        GameServices.GetGameState().OnGameOver += ShowGameResults;
    }
    void ShowGameResults(ETeam winner)
    {
        GameOverText?.gameObject.SetActive(true);
        WinnerText?.gameObject.SetActive(true);

        if(WinnerText)
        {
            WinnerText.color = GameServices.GetTeamColor(winner);
            WinnerText.text = "Winner is " + winner.ToString() + " team";
        }
    }
}
