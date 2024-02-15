using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicatiionController : MonoBehaviour
{
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ClientSingleton clientPrefab;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        LunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            
        }
        else
        {
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
           bool authenticated = await clientSingleton.CreateClient();
            
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            await hostSingleton.CreateClient();

            if (authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }

}
