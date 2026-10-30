using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabConnectionTest : MonoBehaviour
{
    private void Start()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = "HistoreonTestPlayer",
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(
            request,
            OnLoginSuccess,
            OnLoginFailure
        );
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PLAYFAB CONNECTION SUCCESS!");
        Debug.Log("Player ID: " + result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PLAYFAB CONNECTION FAILED!");
        Debug.LogError(error.GenerateErrorReport());
    }
}