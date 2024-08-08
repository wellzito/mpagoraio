using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioZone : MonoBehaviour
{
    public delegate void AudioZoneEventHandler(AudioZone zone, object character);
    public static event AudioZoneEventHandler OnUserEnteredZone;
    public static event AudioZoneEventHandler OnUserExitedZone;

    private List<object> charactersInZone;
    private string localPlayerUID = null;
    private string id = Guid.NewGuid().ToString();

    public string Id { get => id; }
    public List<object> GetCharactersInZone() => charactersInZone;

    private static Dictionary<string, AudioZone> zones;

    public static Dictionary<string, AudioZone> ZonesMap { get => zones; }

    private void Awake()
    {
        AddZoneToList();

        charactersInZone = new List<object>();
        //NetworkRoomController.onRemoveNetworkEntity += RemoveCharacter;
        AgoraController.OnLocalUserJoinedAgoraChannel += LocalUserJoined;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //NetworkRoomController.onRemoveNetworkEntity -= RemoveCharacter;
        AgoraController.OnLocalUserJoinedAgoraChannel -= LocalUserJoined;
    }

    private void AddZoneToList()
    {
        if (zones == null)
            zones = new Dictionary<string, AudioZone>();
        zones.Add(id, this);
    }

    /*private void RemoveCharacter(NetworkEntity entity, NetworkEntityView view)
    {
        var character = view.GetComponent<Character>();
        if (character)
        {
            charactersInZone.Remove(character);
            character.RemoveFromAudioZone(this);
        }
        else
            Debug.Log("Could not remove character from a zone");
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(localPlayerUID))
            return;

        /*if (!other.TryGetComponent(out Character character))
            return;*/

        //Debug.Log($"Character with UID = {character.UID} entered zone");
       // charactersInZone.Add(character);
       // OnUserEnteredZone?.Invoke(this, character);
    }
    private void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(localPlayerUID))
            return;

        /*if (!other.TryGetComponent(out Character character))
            return;*/

        /*Debug.Log($"Character with UID = {character.UID} exited zone");
        charactersInZone.Remove(character);
        OnUserExitedZone?.Invoke(this, character);*/
    }

    private void LocalUserJoined(string uid)
    {
        if (string.IsNullOrEmpty(localPlayerUID))
        {
            localPlayerUID = uid;
            gameObject.SetActive(true);
        }
    }

    public string GetId() => id;
}
