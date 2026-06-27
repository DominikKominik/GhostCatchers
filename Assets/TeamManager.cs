using Unity.Netcode;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }

    public const int MaxCatchers = 5;
    public const int MaxGhosts = 1;

    public NetworkVariable<int> CatcherCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> GhostCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    public bool TryAssignTeam(PlayerTeam player, Team requested)
    {
        if (!IsServer) return false;

        if (requested == Team.Catcher && CatcherCount.Value >= MaxCatchers) return false;
        if (requested == Team.Ghost && GhostCount.Value >= MaxGhosts) return false;

        // Pokud hráè už døív nìjaký tým mìl, uvolníme mu místo (pro budoucí pøepínání)
        ReleaseTeam(player.CurrentTeam.Value);

        if (requested == Team.Catcher) CatcherCount.Value++;
        else if (requested == Team.Ghost) GhostCount.Value++;

        return true;
    }

    public void HandleDisconnect(Team team)
    {
        if (!IsServer) return;
        ReleaseTeam(team);
    }

    private void ReleaseTeam(Team team)
    {
        if (team == Team.Catcher) CatcherCount.Value = Mathf.Max(0, CatcherCount.Value - 1);
        else if (team == Team.Ghost) GhostCount.Value = Mathf.Max(0, GhostCount.Value - 1);
    }
}