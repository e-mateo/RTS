using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public Action<ETeam> OnGameOver;
    public ETeam FirstTeamColor = ETeam.Neutral;
    public ETeam SecondTeamColor = ETeam.Neutral;
    public bool IsGameOver { get; private set; }

    #region Team and scoring methods

    // Scores are based on the number of factories per team
    int[] TeamScores = new int[2];
    public int[] GetTeamScores { get { return TeamScores; } }
    public ETeam GetOpponent(ETeam team)
    {
        if (team == FirstTeamColor)
            return SecondTeamColor;
        return FirstTeamColor;
    }
    public void IncreaseTeamScore(ETeam team)
    {
        if (team >= ETeam.Neutral)
            return;

        ++TeamScores[(int)team];
    }
    public void DecreaseTeamScore(ETeam team)
    {
        if (team >= ETeam.Neutral)
            return;

        --TeamScores[(int)team];

        if (TeamScores[(int)team] <= 0)
            OnGameOver(GetOpponent(team));
    }
    #endregion

    #region MonoBehaviour methods
    void Start()
    {
    }

    #endregion
}
