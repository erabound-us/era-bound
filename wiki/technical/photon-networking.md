# Photon Networking

## Existing DLLs (confirmed in build)
- `PhotonUnityNetworking.dll` — PUN2 core
- `PhotonRealtime.dll` — matchmaking & rooms
- `PhotonChat.dll` — in-game chat
- `PhotonVoice.dll` + `PhotonVoice.PUN.dll` — proximity voice
- `Photon3Unity3D.dll` — low-level transport

## Room Flow for Treasure Royale
```
Player opens Transporter → selects era/date
  → PhotonNetwork.CreateRoom(roomName, RoomOptions, TypedLobby)
  → Other players join via matchmaking filter on "era" property
  → MasterClient sets universeSeed when room is full / timer expires
  → All clients load era scene via PhotonNetwork.LoadLevel()
```

## Artifact Sync Pattern
```csharp
// ArtifactPickup.cs
[PunRPC]
void RPC_PickupArtifact(string artifactId, int playerId) {
    // Remove from world, add to player inventory
    ArtifactManager.Instance.ClaimArtifact(artifactId, playerId);
}

// Call via:
photonView.RPC("RPC_PickupArtifact", RpcTarget.AllBuffered, artifact.id, PhotonNetwork.LocalPlayer.ActorNumber);
```

## Voice Chat
- Photon Voice already bundled — enable proximity voice per era map
- Reduces comms range in large maps to add tension
