# Source: unity-multiplayer

- **Repo:** https://github.com/Nice-Wolf-Studio/unity-claude-skills
- **Path:** `skills/unity-multiplayer/` (`SKILL.md` + `references/netcode-gameobjects.md`, `references/transport-layer.md`)
- **Commit:** `a149d3e995420432f52b0899092daa4b069f441f` (2026-09-15)
- **License:** MIT (`LICENSE` copied)
- **Stars at fetch:** 40
- **Fetched:** 2026-10-16

## Why it was picked

It is a Unity 6.3 guide to Netcode for GameObjects 2.x. It uses the current `[Rpc(SendTo.*)]` attributes and still explains the legacy `ServerRpc`/`ClientRpc`. It covers NetworkVariable permissions, spawning and ownership, scene management, connection approval, Distributed Authority, Unity Transport, and Relay and Lobby with UGS. Among the community skills reviewed, it was the most complete Unity-specific multiplayer skill.

## Security review

I read all files. They are Markdown only. Strings like `"secret"` in the samples are placeholder connection-approval passwords.

## Caveats

- It covers NGO and UGS only, not Photon Fusion or Normcore. No redistributable, quality Photon or Normcore skill was found.
- For Quest co-presence (Shared Spatial Anchors, Meta Platform SDK, colocation), use the official Meta skills alongside this one.
- It is derived from documentation and not tested here. Check NGO and UGS package versions against your project.
