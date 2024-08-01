using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayFab.ClientModels;
using UnityEngine.UI;
using PlayFab;
using System;

public class PlayerNameChange : MonoBehaviour
{
    [SerializeField] private TMP_InputField newNameField;

    [SerializeField] private TextMeshProUGUI successfulNameChangeTxt; // This text will write whether or not we changed our name or not

    [SerializeField] private Button changeNameBtn;

    [SerializeField] private int minNewNameLength = 2;
    [SerializeField] private int maxNewNameLength = 16;

    public void HandleNewName()
    {
        changeNameBtn.interactable = newNameField.text.Length >= minNewNameLength && newNameField.text.Length <= maxNewNameLength;

        successfulNameChangeTxt.text = changeNameBtn.interactable ? "" : "Your new name must be between 2 and 16 characters!" ;
    }

    public void ChangeUserName()
    {
        if(PlayerPrefs.GetFloat("NameChangeTimer", 0) > 0f)
        {
            successfulNameChangeTxt.text = "You can change your name in " + "time";

        }

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerName", newNameField.text }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, onUpdatedName, onError);
    }

    private void onUpdatedName(UpdateUserDataResult result)
    {
        successfulNameChangeTxt.text = "Successfully changed your name!";
        //PlayerPrefs.SetFloat("NameChangeTimer", 262974383); // 1 Month in seconds lmfao
    }

    private void onError(PlayFabError error)
    {
        successfulNameChangeTxt.text = "Something went wrong.";
    }
}
