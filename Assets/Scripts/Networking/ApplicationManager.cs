using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;

    [SerializeField] private HostSingleton hostPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Dedicated servers don't have a person playing
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }


    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if(isDedicatedServer)
        {

        }
        else
        {
            PlayFabManager.Instance.ProgressTxt.text = "Connecting to servers...";
            PlayFabManager.Instance.LoadingBar.fillAmount = 0.8f;

            HostSingleton hostSingleton = Instantiate(hostPrefab);

            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            
            bool authenticated = await clientSingleton.CreateClient();

            PlayFabManager.Instance.LoadingBar.fillAmount = 1f;

            if(authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }
}
