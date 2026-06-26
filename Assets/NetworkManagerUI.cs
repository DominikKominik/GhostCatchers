using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    private string joinCode = ""; // Sem se ukládá kód, který zadává klient

    async void Start()
    {
        // 1. Inicializujeme online služby Unity
        await UnityServices.InitializeAsync();

        // 2. Anonymnì pøihlásíme hráèe k serveru (bez nutnosti registrace/hesla)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Vykreslení testovacího menu na obrazovku
    private void OnGUI()
    {
        // Vytvoøí okénko v levém horním rohu (X: 20, Y: 20, Šíøka: 300, Výška: 250)
        GUILayout.BeginArea(new Rect(20, 20, 300, 250));

        // Pokud hra ještì nebìží (nejsme ani host, ani klient)
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            // TLAÈÍTKO PRO HOSTA
            if (GUILayout.Button("Založit hru (Vygenerovat kód)"))
            {
                StartRelayHost();
            }

            GUILayout.Space(20); // Mezera mezi tlaèítky

            // TEXTOVÉ POLÍÈKO A TLAÈÍTKO PRO KLIENTA
            GUILayout.Label("Zadej kód od kamaráda:");
            joinCode = GUILayout.TextField(joinCode); // Tady klient píše nebo vkládá kód

            if (GUILayout.Button("Pøipojit se do hry"))
            {
                StartRelayClient(joinCode);
            }
        }
        else
        {
            // Pokud už hra bìží, ukážeme aktuální stav
            GUILayout.Label($"Status: {(NetworkManager.Singleton.IsHost ? "Hostuješ hru" : "Jsi pøipojen jako klient!")}");

            // Ukážeme kód pokoje i za bìhu, kdyby ho host zapomnìl
            if (NetworkManager.Singleton.IsHost)
            {
                GUILayout.Label($"Kód tvého pokoje: {joinCode}");
            }
        }

        GUILayout.EndArea();
    }

    // Logika pro založení hry pøes internet
    private async void StartRelayHost()
    {
        try
        {
            // Požádáme Relay server o alokaci místa pro 4 hráèe
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            // Vygenerujeme unikátní Join Code
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCode = code;

            // Kód automaticky zkopírujeme do schránky (v poèítaèi funguje jako Ctrl+C)
            GUIUtility.systemCopyBuffer = code;
            Debug.Log($"Hra založena! Kód pokoje: {code} (Zkopírováno do schránky)");

            // Pøedáme data o serveru do komponentu UnityTransport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData
            );

            // Nastartujeme hostování hry
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Chyba pøi zakládání Relay: {e}");
        }
    }

    // Logika pro pøipojení klienta pomocí kódu
    private async void StartRelayClient(string codeToJoin)
    {
        if (string.IsNullOrEmpty(codeToJoin))
        {
            Debug.LogWarning("Nemùžeš se pøipojit s prázdným kódem!");
            return;
        }

        try
        {
            Debug.Log($"Pøipojování ke kódu: {codeToJoin}");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codeToJoin);

            // Pøedáme získaná internetová data transportu klienta
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData
            );

            // Nastartujeme klienta a spojíme se pøes internet
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Chyba pøi pøipojování k Relay: {e}");
        }
    }
}