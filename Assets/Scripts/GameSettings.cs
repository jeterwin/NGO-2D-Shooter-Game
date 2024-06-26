using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Singletons/GameSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] private string gameVersion = "0.0.0";

    public string GameVersion
    {
        get { return gameVersion; }
    }

    [SerializeField] private string nickName = "Erwin";
    public string NickName
    {
        get 
        {
            int number = Random.Range(0, 9999);
            return nickName + number.ToString(); 
        } 
    }

    [SerializeField] private string appId = "a990540c-fc2b-44b2-87a4-b454477a8a07";

    public string AppId
    {
        get { return appId; }
    }
}
