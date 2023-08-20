using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite sprite;

    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite()
    {
        return sprite;
    }

    public UnityEngine.Color GetColor(LobbyManager.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManager.PlayerCharacter.White:    return UnityEngine.Color.white;
            case LobbyManager.PlayerCharacter.Brown:    return new UnityEngine.Color32(91,46,33, 255);
            case LobbyManager.PlayerCharacter.Red:      return UnityEngine.Color.red;
            case LobbyManager.PlayerCharacter.Purple:   return new UnityEngine.Color32(231, 0, 195, 255);
            case LobbyManager.PlayerCharacter.Blue:     return UnityEngine.Color.blue;
            case LobbyManager.PlayerCharacter.Cyan:     return new UnityEngine.Color32(0, 231, 227, 255);
            case LobbyManager.PlayerCharacter.Green:    return UnityEngine.Color.green;
        }
    }

}