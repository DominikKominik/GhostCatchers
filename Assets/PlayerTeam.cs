using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks; // DÙLEŽITÉ: Pøidáno pro fungování Task.Delay

public enum Team
{
    None,    // dosud nevybráno
    Catcher, // lidé - èervený válec
    Ghost    // duch - zelený válec
}

public class PlayerTeam : NetworkBehaviour
{
    [Header("Vizuál")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material catcherMaterial;
    [SerializeField] private Material ghostMaterial;

    public NetworkVariable<Team> CurrentTeam = new NetworkVariable<Team>(
        Team.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        CurrentTeam.OnValueChanged += OnTeamChanged;
        UpdateVisual(CurrentTeam.Value);

        if (IsOwner)
        {
            // Dokud si hráè nevybere tým, kurzor je volný kvùli klikání v menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Spustíme asynchronní bezpeèné otevøení menu (poèkej na inicializaci UI)
            TryShowUI();
        }
    }

    private async void TryShowUI()
    {
        // Pokud UI na scénì ještì neexistuje (klient se naèetl rychleji než se probudil Canvas),
        // poèkáme 100 milisekund a zkusíme to znovu.
        while (TeamSelectionUI.Instance == null)
        {
            await Task.Delay(100);
        }

        // Jakmile už UI prokazatelnì žije, bezpeènì ho klientovi ukážeme
        TeamSelectionUI.Instance.Show(this);
    }

    public override void OnNetworkDespawn()
    {
        CurrentTeam.OnValueChanged -= OnTeamChanged;

        if (IsServer && TeamManager.Instance != null)
        {
            TeamManager.Instance.HandleDisconnect(CurrentTeam.Value);
        }
    }

    public void RequestTeam(Team team)
    {
        RequestTeamRpc(team);
    }

    // RPC upraveno pro èistou funkènost v Unity 6
    [Rpc(SendTo.Server)]
    private void RequestTeamRpc(Team requestedTeam)
    {
        if (TeamManager.Instance == null) return;

        if (TeamManager.Instance.TryAssignTeam(this, requestedTeam))
        {
            CurrentTeam.Value = requestedTeam;
        }
    }

    private void OnTeamChanged(Team oldTeam, Team newTeam)
    {
        UpdateVisual(newTeam);

        if (IsOwner && newTeam != Team.None)
        {
            // Tým vybrán - zavøeme menu (které v sobì už má schování a zamknutí kurzoru)
            TeamSelectionUI.Instance?.Hide();
        }
    }

    private void UpdateVisual(Team team)
    {
        if (bodyRenderer == null) return;

        if (team == Team.Catcher)
            bodyRenderer.material = catcherMaterial;
        else if (team == Team.Ghost)
            bodyRenderer.material = ghostMaterial;
    }
}