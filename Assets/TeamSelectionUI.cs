using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // DÙLEŽITÉ: Pøidáno pro ovládání kurzoru v novém Input Systemu

public class TeamSelectionUI : MonoBehaviour
{
    public static TeamSelectionUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Button catcherButton;
    [SerializeField] private Button ghostButton;

    // ODSTRANÌNO: Nefunkèní texty, které házely NullReferenceException

    private PlayerTeam localPlayer;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        catcherButton.onClick.AddListener(() => localPlayer?.RequestTeam(Team.Catcher));
        ghostButton.onClick.AddListener(() => localPlayer?.RequestTeam(Team.Ghost));
    }

    public void Show(PlayerTeam player)
    {
        localPlayer = player;
        panel.SetActive(true);

        // Zpøístupníme myš pro klikání v menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.CatcherCount.OnValueChanged += OnCountsChanged;
            TeamManager.Instance.GhostCount.OnValueChanged += OnCountsChanged;
            RefreshUI();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);

        // NOVÝ INPUT SYSTEM OPRAVA: Po výbìru týmu zamkneme myš zpìt do hry, aby šlo chodit a otáèet se
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.CatcherCount.OnValueChanged -= OnCountsChanged;
            TeamManager.Instance.GhostCount.OnValueChanged -= OnCountsChanged;
        }
    }

    private void OnCountsChanged(int oldValue, int newValue)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (TeamManager.Instance == null) return;

        int catchers = TeamManager.Instance.CatcherCount.Value;
        int ghosts = TeamManager.Instance.GhostCount.Value;

        // Tlaèítka se vypnou pouze v pøípadì, že je tým už plný
        catcherButton.interactable = catchers < TeamManager.MaxCatchers;
        ghostButton.interactable = ghosts < TeamManager.MaxGhosts;
    }
}