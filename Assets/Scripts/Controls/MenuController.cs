using System;
using UnityEngine.UI;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    Transform FactoryMenuCanvas = null;

    [SerializeField]
    Transform FormationMenuCanvas = null;
    public GraphicRaycaster BuildMenuRaycaster { get; private set; }
    public GraphicRaycaster FormationMenuRaycaster { get; private set; }


    UnitController Controller = null;
    GameObject FactoryMenuPanel = null;
    Text BuildPointsText = null;
    Text CapturedTargetsText = null;
    Button[] BuildUnitButtons = null;
    Button[] BuildFactoryButtons = null;
    Button CancelBuildButton = null;
    Text[] BuildQueueTexts = null;

    public void HideFactoryMenu()
    {
        if (FactoryMenuPanel)
            FactoryMenuPanel.SetActive(false);
    }
    public void ShowFactoryMenu()
    {
        if (FactoryMenuPanel)
            FactoryMenuPanel.SetActive(true);
    }
    public void UpdateBuildPointsUI()
    {
        if (BuildPointsText != null)
            BuildPointsText.text = "Build Points : " + Controller.TotalBuildPoints;
    }
    public void UpdateCapturedTargetsUI()
    {
        if (CapturedTargetsText != null)
            CapturedTargetsText.text = "Captured Targets : " + Controller.CapturedTargets;
    }
    public void UpdateFactoryBuildQueueUI(int i, Factory selectedFactory)
    {
        if (selectedFactory == null)
            return;
        int queueCount = selectedFactory.GetQueuedCount(i);
        if (queueCount > 0)
        {
            BuildQueueTexts[i].text = "+" + queueCount;
            BuildQueueTexts[i].enabled = true;
        }
        else
        {
            BuildQueueTexts[i].enabled = false;
        }
    }
    public void HideAllFactoryBuildQueue()
    {
        foreach (Text text in BuildQueueTexts)
        {
            if (text)
                text.enabled = false;
        }
    }
    public void UnregisterBuildButtons(int availableUnitsCount, int availableFactoriesCount)
    {
        // unregister build buttons
        for (int i = 0; i < availableUnitsCount; i++)
        {
            BuildUnitButtons[i].onClick.RemoveAllListeners();
        }
        for (int i = 0; i < availableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].onClick.RemoveAllListeners();
        }
    }

    public void UpdateFactoryMenu(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod, Action<int> enterFactoryBuildModeMethod)
    {
        ShowFactoryMenu();

        // Unit build buttons
        // register available buttons
        int i = 0;
        for (; i < selectedFactory.AvailableUnitsCount; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildUnitButtons[i].onClick.AddListener(() =>
            {
                if (requestUnitBuildMethod(index))
                    UpdateFactoryBuildQueueUI(index, selectedFactory);
            });

            Text[] buttonTextArray = BuildUnitButtons[i].GetComponentsInChildren<Text>();
            Text buttonText = buttonTextArray[0];//BuildUnitButtons[i].GetComponentInChildren<Text>();
            UnitDataScriptable data = selectedFactory.GetBuildableUnitData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";

            // Update queue count UI
            BuildQueueTexts[i] = buttonTextArray[1];
            UpdateFactoryBuildQueueUI(i, selectedFactory);
        }
        // hide remaining buttons
        for (; i < BuildUnitButtons.Length; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(false);
        }

        // activate Cancel button
        CancelBuildButton.onClick.AddListener(  () =>
                                                {
                                                    selectedFactory?.CancelCurrentBuild();
                                                    HideAllFactoryBuildQueue();
                                                });

        // Factory build buttons
        // register available buttons
        i = 0;
        for (; i < selectedFactory.AvailableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildFactoryButtons[i].onClick.AddListener(() =>
            {
                enterFactoryBuildModeMethod(index);
            });

            Text buttonText = BuildFactoryButtons[i].GetComponentInChildren<Text>();
            FactoryDataScriptable data = selectedFactory.GetBuildableFactoryData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";
        }
        // hide remaining buttons
        for (; i < BuildFactoryButtons.Length; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(false);
        }
    }
    void Awake()
    {
        if (FactoryMenuCanvas == null)
        {
            Debug.LogWarning("FactoryMenuCanvas not assigned in inspector");
        }
        else
        {
            Transform FactoryMenuPanelTransform = FactoryMenuCanvas.Find("FactoryMenu_Panel");
            if (FactoryMenuPanelTransform)
            {
                FactoryMenuPanel = FactoryMenuPanelTransform.gameObject;
                FactoryMenuPanel.SetActive(false);
            }
            BuildMenuRaycaster = FactoryMenuCanvas.GetComponent<GraphicRaycaster>();
            Transform BuildPointsTextTransform = FactoryMenuCanvas.Find("BuildPointsText");
            if (BuildPointsTextTransform)
            {
                BuildPointsText = BuildPointsTextTransform.GetComponent<Text>();
            }
            Transform CapturedTargetsTextTransform = FactoryMenuCanvas.Find("CapturedTargetsText");
            if (CapturedTargetsTextTransform)
            {
                CapturedTargetsText = CapturedTargetsTextTransform.GetComponent<Text>();
            }
        }

        if(FormationMenuCanvas)
            FormationMenuRaycaster = FormationMenuCanvas.GetComponent<GraphicRaycaster>();

        Controller = GetComponent<UnitController>();
    }
    void Start()
    {
        BuildUnitButtons = FactoryMenuPanel.transform.Find("BuildUnitMenu_Panel").GetComponentsInChildren<Button>();
        BuildFactoryButtons = FactoryMenuPanel.transform.Find("BuildFactoryMenu_Panel").GetComponentsInChildren<Button>();
        CancelBuildButton = FactoryMenuPanel.transform.Find("Cancel_Button").GetComponent<Button>();
        BuildQueueTexts = new Text[BuildUnitButtons.Length];
    }
}

        
