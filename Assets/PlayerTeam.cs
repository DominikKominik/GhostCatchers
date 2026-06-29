using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerTeam : NetworkBehaviour
{
    [Header("Vizuál")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material catcherMaterial;
    [SerializeField] private Material ghostMaterial;

    [Header("Výchozí nastavení týmu pro tento PREFAB")]
    [SerializeField] private Team initialPrefabTeam = Team.None;

    public NetworkVariable<Team> CurrentTeam = new NetworkVariable<Team>(
        Team.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        CurrentTeam.OnValueChanged += OnTeamChanged;

        UpdateVisual(CurrentTeam.Value != Team.None ? CurrentTeam.Value : initialPrefabTeam);

        if (IsOwner)
        {
            if (initialPrefabTeam != Team.None)
            {
                // Pojistka pro hotový Catcher/Ghost prefab - hlavní mechanismus
                // schování UI je ale NotifyTeamReadyRpc() níže.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (TeamSelectionUI.Instance != null)
                {
                    TeamSelectionUI.Instance.Hide();
                }
                return;
            }

            // Lobby placeholder (None) - ukážeme menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            TryShowUI();
        }
    }

    private async void TryShowUI()
    {
        while (TeamSelectionUI.Instance == null)
        {
            await Task.Delay(100);
            if (this == null) return; // objekt mohl být mezitím despawnutý
        }
        if (this == null) return;
        TeamSelectionUI.Instance.Show(this);
    }

    public override void OnNetworkDespawn()
    {
        CurrentTeam.OnValueChanged -= OnTeamChanged;
    }

    public void RequestTeam(Team team)
    {
        RequestTeamRpc(team);
    }

    [Rpc(SendTo.Server)]
    private void RequestTeamRpc(Team requestedTeam)
    {
        if (TeamManager.Instance == null) return;
        TeamManager.Instance.TryAssignTeam(this, requestedTeam);
    }

    // Server tohle zavolá hned po SpawnAsPlayerObject + nastavení CurrentTeam.Value.
    // Na rozdíl od NetworkVariable.OnValueChanged tomu NEHROZÍ race condition se
    // spawn zprávou - je to pøímá, jednorázová zpráva konkrétnímu klientovi.
    [Rpc(SendTo.Owner)]
    public void NotifyTeamReadyRpc()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (TeamSelectionUI.Instance != null)
        {
            TeamSelectionUI.Instance.Hide();
        }
    }

    private void OnTeamChanged(Team oldTeam, Team newTeam)
    {
        UpdateVisual(newTeam);

        // Záložní mechanismus - kdyby RPC z nìjakého dùvodu nedorazilo
        if (IsOwner && newTeam != Team.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (TeamSelectionUI.Instance != null)
            {
                TeamSelectionUI.Instance.Hide();
            }
        }
    }

    private void UpdateVisual(Team team)
    {
        if (bodyRenderer == null) return;
        if (team == Team.Catcher && catcherMaterial != null)
            bodyRenderer.material = catcherMaterial;
        else if (team == Team.Ghost && ghostMaterial != null)
            bodyRenderer.material = ghostMaterial;
    }
}