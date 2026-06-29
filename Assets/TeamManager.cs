using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }

    public const int MaxCatchers = 5;
    public const int MaxGhosts = 1;

    [Header("Prefaby T˝m˘")]
    [SerializeField] private GameObject catcherPrefab;
    [SerializeField] private GameObject ghostPrefab;

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

        ulong clientId = player.OwnerClientId;
        Vector3 spawnPosition = player.transform.position;
        Quaternion spawnRotation = player.transform.rotation;

        GameObject prefabToSpawn = (requested == Team.Catcher) ? catcherPrefab : ghostPrefab;
        if (prefabToSpawn != null)
        {
            ReleaseTeam(player.CurrentTeam.Value);

            GameObject newPlayerObj = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
            NetworkObject newNetworkObject = newPlayerObj.GetComponent<NetworkObject>();
            newNetworkObject.SpawnAsPlayerObject(clientId, true);

            PlayerTeam newPlayerTeam = newPlayerObj.GetComponent<PlayerTeam>();
            if (newPlayerTeam != null)
            {
                newPlayerTeam.CurrentTeam.Value = requested;

                // KLÕ»OV¡ OPRAVA: explicitnÌ RPC p¯Ìmo vlastnÌkovi, aù schov· UI.
                // NetworkVariable nastaven· o ¯·dek v˝ö m˘ûe klientovi dorazit se
                // starou hodnotou ve spawn zpr·vÏ (zn·mÈ chov·nÌ NGO) - RPC ne.
                newPlayerTeam.NotifyTeamReadyRpc();
            }

            StartCoroutine(DestroyOldPlayerRoutine(player.NetworkObject));

            if (requested == Team.Catcher) CatcherCount.Value++;
            else if (requested == Team.Ghost) GhostCount.Value++;

            return true;
        }
        return false;
    }

    private IEnumerator DestroyOldPlayerRoutine(NetworkObject oldNetworkObject)
    {
        yield return new WaitForEndOfFrame();
        if (oldNetworkObject != null && oldNetworkObject.IsSpawned)
        {
            oldNetworkObject.Despawn(true);
        }
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