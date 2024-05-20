using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEntity : MonoBehaviour, ISelectable, IDamageable, IRepairable
{
    [SerializeField]
    protected ETeam Team;

    protected EntityVisibility _Visibility;
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

    protected int HP = 0;
    protected Action OnHpUpdated;
    protected GameObject SelectedSprite = null;
    protected Text HPText = null;
    protected bool IsInitialized = false;
    protected UnityEngine.UI.Image MinimapImage;

    public Action OnDeadEvent;
    public bool IsSelected { get; protected set; }
    public bool IsAlive { get; protected set; }
    virtual public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        Team = _team;

        if (Visibility) { Visibility.Team = _team; }

        Transform minimapTransform = transform.Find("MinimapCanvas");
        if (minimapTransform != null)
        {
            MinimapImage = minimapTransform.GetComponentInChildren<UnityEngine.UI.Image>();
            MinimapImage.color = GameServices.GetTeamColor(Team);
        }

        IsInitialized = true;
    }
    public Color GetColor()
    {
        return GameServices.GetTeamColor(GetTeam());
    }
    void UpdateHpUI()
    {
        if (HPText != null)
            HPText.text = "HP : " + HP.ToString();
    }

    #region ISelectable
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        SelectedSprite?.SetActive(IsSelected);
    }
    public ETeam GetTeam()
    {
        return Team;
    }
    #endregion

    #region IDamageable
    public void AddDamage(int damageAmount)
    {
        if (IsAlive == false)
            return;

        HP -= damageAmount;

        OnHpUpdated?.Invoke();

        if (HP <= 0)
        {
            IsAlive = false;
            OnDeadEvent?.Invoke();
            Debug.Log("Entity " + gameObject.name + " died");
        }
    }
    public void Destroy()
    {
        AddDamage(HP);
    }
    #endregion

    #region IRepairable
    virtual public bool NeedsRepairing()
    {
        return true;
    }
    virtual public void Repair(int amount)
    {
        OnHpUpdated?.Invoke();
    }
    virtual public void FullRepair()
    {
    }
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        IsAlive = true;

        SelectedSprite = transform.Find("SelectedSprite")?.gameObject;
        SelectedSprite?.SetActive(false);

        Transform hpTransform = transform.Find("Canvas/HPText");
        if (hpTransform)
            HPText = hpTransform.GetComponent<Text>();

        OnHpUpdated += UpdateHpUI;
    }
    virtual protected void Start()
    {
        Init(GetTeam());
        UpdateHpUI();
    }
    virtual protected void Update()
    {
    }
    #endregion
}
