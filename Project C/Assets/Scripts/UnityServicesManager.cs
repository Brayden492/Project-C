using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;

public class UnityServicesManager : MonoBehaviour
{
    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            // Initialize Unity Gaming Services
            await UnityServices.InitializeAsync();

            // Sign the player in anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Unity Services initialized!");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Unity Services error: " + exception.Message);
        }
    }
}
