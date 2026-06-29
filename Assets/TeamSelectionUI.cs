using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    public static TeamSelectionUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Button catcherButton;
    [SerializeField] private Button ghostButton;

    private PlayerTeam localPlayer;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        catcherButton.onClick.AddListener(() => {
            if (localPlayer != null)
            {
                localPlayer.RequestTeam(Team.Catcher);
            }
        });

        ghostButton.onClick.AddListener(() => {
            if (localPlayer != null)
            {
                localPlayer.RequestTeam(Team.Ghost);
            }
        });
    }

    public void Show(PlayerTeam player)
    {
        localPlayer = player;
        panel.SetActive(true);

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

        catcherButton.interactable = catchers < TeamManager.MaxCatchers;
        ghostButton.interactable = ghosts < TeamManager.MaxGhosts;
    }
}