using UnityEngine;

public enum ETeam
{
    Blue = 0,
    Red = 1,
    //Green,

    Neutral
}

[RequireComponent(typeof(GameState))]
public class GameServices : MonoBehaviour
{
    [SerializeField, Tooltip("Generic material used for 3D models, in the following order : blue, red and green")]
    Material[] TeamMaterials = new Material[3];

    [SerializeField, Tooltip("Unplayable terrain border size")]
    float NonPlayableBorder = 100f;

    [SerializeField, Tooltip("Playable bounds size if no terrain is found")]
    float DefaultPlayableBoundsSize = 100f;

    static GameServices Instance = null;

    UnitController[] ControllersArray;
    TargetBuilding[] TargetBuildingArray;
    GameState CurrentGameState = null;

    Terrain CurrentTerrain = null;
    Bounds PlayableBounds;

    #region Static methods
    public static GameServices GetGameServices()
    {
        return Instance;
    }
    public static GameState GetGameState()
    {
        return Instance.CurrentGameState;
    }
    public static UnitController GetControllerByTeam(ETeam team)
    {
        if (Instance.ControllersArray.Length < (int)team)
            return null;
        return Instance.ControllersArray[(int)team];
    }
    public static Material GetTeamMaterial(ETeam team)
    {
        return Instance.TeamMaterials[(int)team];
    }
    public static ETeam GetOpponent(ETeam team)
    {
        return Instance.CurrentGameState.GetOpponent(team);
    }

    public static TargetBuilding[] GetTargetBuildings() { return Instance.TargetBuildingArray; }

    // return RGB color struct for each team
    public static Color GetTeamColor(ETeam team)
    {
        switch(team)
        {
            case ETeam.Blue:
                return Color.blue;
            case ETeam.Red:
                return Color.red;
            //case Team.Green:
            //    return Color.green;
            default:
                return Color.grey;
        }
    }
    public static float GetNonPlayableBorder { get { return Instance.NonPlayableBorder; } }
    public static Terrain GetTerrain { get { return Instance.CurrentTerrain; } }
    public static Bounds GetPlayableBounds()
    {
        return Instance.PlayableBounds;
    }
    public static Vector3 GetTerrainSize()
    {
        return Instance.TerrainSize;
    }
    public static bool IsPosInPlayableBounds(Vector3 pos)
    {
        if (GetPlayableBounds().Contains(pos))
            return true;

        return false;
    }
    public Vector3 TerrainSize
    {
        get
        {
            if (CurrentTerrain)
                return CurrentTerrain.terrainData.bounds.size;
            return new Vector3(DefaultPlayableBoundsSize, 10.0f, DefaultPlayableBoundsSize);
        }
    }

    #endregion

    #region MonoBehaviour methods
    void Awake()
    {
        Instance = this;

        // Retrieve controllers from scene for each team
        ControllersArray = new UnitController[2];
        foreach (UnitController controller in FindObjectsOfType<UnitController>())
        {
            ControllersArray[(int)controller.GetTeam()] = controller;
        }

        // Store TargetBuildings
        TargetBuildingArray = FindObjectsOfType<TargetBuilding>();

        // Store GameState ref
        if (CurrentGameState == null)
            CurrentGameState = GetComponent<GameState>();

        // Assign first found terrain
        foreach (Terrain terrain in FindObjectsOfType<Terrain>())
        {
            CurrentTerrain = terrain;
            //Debug.Log("terrainData " + CurrentTerrain.terrainData.bounds.ToString());
            break;
        }

        if (CurrentTerrain)
        {
            PlayableBounds = CurrentTerrain.terrainData.bounds;
            Vector3 clampedOne = new Vector3(1f, 0f, 1f);
            Vector3 heightReduction = Vector3.up * 0.1f; // $$ hack : this is to prevent selectioning / building in high areas
            PlayableBounds.SetMinMax(PlayableBounds.min + clampedOne * NonPlayableBorder / 2f, PlayableBounds.max - clampedOne * NonPlayableBorder / 2f - heightReduction);
        }
        else
        {
            Debug.LogWarning("could not find terrain asset in scene, setting default PlayableBounds");
            Vector3 clampedOne = new Vector3(1f, 0f, 1f);
            PlayableBounds.SetMinMax(   new Vector3(-DefaultPlayableBoundsSize, -10.0f, -DefaultPlayableBoundsSize) + clampedOne * NonPlayableBorder / 2f,
                                        new Vector3(DefaultPlayableBoundsSize, 10.0f, DefaultPlayableBoundsSize) - clampedOne * NonPlayableBorder / 2f);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(PlayableBounds.center, PlayableBounds.size);
    }
    #endregion
}
