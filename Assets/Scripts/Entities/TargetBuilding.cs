using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TargetBuilding : MonoBehaviour
{
    [SerializeField]
    float CaptureGaugeStart = 100f;
    [SerializeField]
    float CaptureGaugeSpeed = 1f;
    [SerializeField]
    int BuildPoints = 5;
    [SerializeField]
    float TimerGivingPoint = 10f;
    [SerializeField]
    Material BlueTeamMaterial = null;
    [SerializeField]
    Material RedTeamMaterial = null;

    Material NeutralMaterial = null;
    MeshRenderer BuildingMeshRenderer = null;
    Image GaugeImage;
    Image MinimapImage;

    int[] TeamScore;
    float CaptureGaugeValue;
    float CurrentTimerGivingPoint;
    ETeam OwningTeam = ETeam.Neutral;
    ETeam CapturingTeam = ETeam.Neutral;
    public ETeam GetTeam() { return OwningTeam; }

    public List<Squad> AISquadsCapturing = new List<Squad>();

    private EntityVisibility _Visibility;
    public EntityVisibility Visibility
    {
        get
        {
            if (_Visibility == null)
            {
                _Visibility = GetComponent<EntityVisibility>();
            }
            return _Visibility;
        }
    }

    List<Unit> playerUnitsAroundLab = new List<Unit>();

    public List<Unit> PlayerUnitsAroundLab { get { return playerUnitsAroundLab; } }


    #region MonoBehaviour methods
    void Start()
    {
        BuildingMeshRenderer = GetComponentInChildren<MeshRenderer>();
        NeutralMaterial = BuildingMeshRenderer.material;

        GaugeImage = GetComponentInChildren<Image>();
        if (GaugeImage)
            GaugeImage.fillAmount = 0f;
        CaptureGaugeValue = CaptureGaugeStart;
        TeamScore = new int[2];
        TeamScore[0] = 0;
        TeamScore[1] = 0;

        Transform minimapTransform = transform.Find("MinimapCanvas");
        if (minimapTransform != null)
            MinimapImage = minimapTransform.GetComponentInChildren<Image>();
    }
    void Update()
    {
        for(int i = 0; i < playerUnitsAroundLab.Count; i++)
        {
            if (playerUnitsAroundLab[i] == null)
                playerUnitsAroundLab.RemoveAt(i);
        }

        if(OwningTeam != ETeam.Neutral)
        {
            CurrentTimerGivingPoint -= Time.deltaTime;
            if (CurrentTimerGivingPoint < 0)
            {
                GameServices.GetControllerByTeam(OwningTeam).TotalBuildPoints += 1;
                CurrentTimerGivingPoint = TimerGivingPoint;
            }
        }

        if (CapturingTeam == OwningTeam || CapturingTeam == ETeam.Neutral)
            return;

        CaptureGaugeValue -= TeamScore[(int)CapturingTeam] * CaptureGaugeSpeed * Time.deltaTime;

        GaugeImage.fillAmount = 1f - CaptureGaugeValue / CaptureGaugeStart;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(CapturingTeam);
        }
    }
    #endregion

    #region Capture methods
    public void StartCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] += unit.Cost;

        if (CapturingTeam == ETeam.Neutral)
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] == 0)
            {
                CapturingTeam = unit.GetTeam();
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
        else
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] > 0)
                ResetCapture();
        }
    }
    public void StopCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] -= unit.Cost;
        if (TeamScore[(int)unit.GetTeam()] == 0)
        {
            ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
            if (TeamScore[(int)opponentTeam] == 0)
            {
                ResetCapture();
            }
            else
            {
                CapturingTeam = opponentTeam;
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
    }
    void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        CapturingTeam = ETeam.Neutral;
        GaugeImage.fillAmount = 0f;
    }
    void OnCaptured(ETeam newTeam)
    {
        Debug.Log("target captured by " + newTeam.ToString());
        if (OwningTeam != newTeam)
        {
            UnitController teamController = GameServices.GetControllerByTeam(newTeam);
            if (teamController != null)
                teamController.CaptureTarget(BuildPoints);

            if (OwningTeam != ETeam.Neutral)
            {
                // remove points to previously owning team
                teamController = GameServices.GetControllerByTeam(OwningTeam);
                if (teamController != null)
                    teamController.LoseTarget(BuildPoints);
            }
        }

        ResetCapture();
        OwningTeam = newTeam;
        if (Visibility) { Visibility.Team = OwningTeam; }
        if (MinimapImage) { MinimapImage.color = GameServices.GetTeamColor(OwningTeam); }
        BuildingMeshRenderer.material = newTeam == ETeam.Blue ? BlueTeamMaterial : RedTeamMaterial;
        WorldState.Instance.TakeOverLab(this, newTeam);
        CurrentTimerGivingPoint = TimerGivingPoint;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit && unit.GetTeam() == ETeam.Blue)
            playerUnitsAroundLab.Add(unit);
    }

    private void OnTriggerExit(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit && unit.GetTeam() == ETeam.Blue)
            playerUnitsAroundLab.Remove(unit);
    }
}
