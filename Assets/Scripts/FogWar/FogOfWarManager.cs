using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class FogOfWarManager : MonoBehaviour
{
    static FogOfWarManager _Instance = null;
    static public FogOfWarManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<FogOfWarManager>();
            return _Instance;
        }
    }


    private UnitController Controller;
	public ETeam Team => Controller.GetTeam();

	[SerializeField]
    private FogOfWarSystem FOWSystem;

	public FogOfWarSystem GetFogOfWarSystem
	{
		get { return FOWSystem; }
	}

	[SerializeField]
	private float UpdateFrequency = 0.05f;

	private float LastUpdateDate = 0f;


    void Start()
    {
        Controller = FindObjectOfType<PlayerController>();
        FOWSystem.Init();
    }

    private void Update()
    {
		if ((Time.time - LastUpdateDate) > UpdateFrequency)
		{
			LastUpdateDate = Time.time;
			UpdateVisibilityTextures();
			UpdateFactoriesVisibility();
			UpdateUnitVisibility();
            UpdateBuildingVisibility();
        }
    }
           

	private void UpdateVisibilityTextures()
	{
		FOWSystem.ClearVisibility();
		FOWSystem.UpdateVisions(FindObjectsOfType<EntityVisibility>());
		FOWSystem.UpdateTextures(1 << (int)Team);
	}

	private void UpdateUnitVisibility()
	{
		foreach (Unit unit in GameServices.GetControllerByTeam(Team).UnitList)
		{
            if (unit.Visibility == null) { continue; }

            unit.Visibility.SetVisible(true);
		}

		foreach (Unit unit in GameServices.GetControllerByTeam(Team.GetOpponent()).UnitList)
		{
			if (unit.Visibility == null) { continue; }

			if (FOWSystem.IsVisible(1 << (int)Team, unit.Visibility.Position))
			{
				unit.Visibility.SetVisible(true);
			}
			else
			{
                unit.Visibility.SetVisible(false);
            }
        }
	}

	private void UpdateBuildingVisibility()
	{
		foreach (TargetBuilding building in GameServices.GetTargetBuildings())
		{
			if (building.Visibility == null) { continue; }

            if (FOWSystem.IsVisible(1 << (int)Team, building.Visibility.Position))
			{
				building.Visibility.SetVisibleUI(true);
			}
			else
			{
				building.Visibility.SetVisibleUI(false);
			}

			if (FOWSystem.WasVisible(1 << (int)Team, building.Visibility.Position))
			{
                building.Visibility.SetVisibleDefault(true);
            }
			else
			{
				building.Visibility.SetVisible(false);
            }
        }
	}

	private void UpdateFactoriesVisibility()
	{
		foreach (Factory factory in GameServices.GetControllerByTeam(Team).GetFactoryList)
		{
			factory.Visibility?.SetVisible(true);
		}

		foreach (Factory factory in GameServices.GetControllerByTeam(Team.GetOpponent()).GetFactoryList)
		{
			if (FOWSystem.IsVisible(1 << (int)Team, factory.Visibility.Position))
			{
				factory.Visibility.SetVisibleUI(true);
			}
			else
			{
                factory.Visibility.SetVisibleUI(false);
            }

            if (FOWSystem.WasVisible(1 << (int)Team, factory.Visibility.Position))
			{
                factory.Visibility.SetVisibleDefault(true);
            }
            else
			{
                factory.Visibility.SetVisible(false);
            }
        }
	}

	public bool IsFactoryVisible(ETeam team, Factory factory)
	{
		return FOWSystem.IsVisible(1 << (int)team, factory.Visibility.Position);
    }

    public bool IsLabVisible(ETeam team, TargetBuilding lab)
    {
        return FOWSystem.IsVisible(1 << (int)team, lab.Visibility.Position);
    }
}
