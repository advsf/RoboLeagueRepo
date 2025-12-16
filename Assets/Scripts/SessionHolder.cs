using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "SessionHolder", menuName = "Scriptable Objects/SessionHolder")]
public class SessionHolder : ScriptableObject
{
    // stores the session information
    public ISession ActiveSession;

    [NonSerialized]
    private HashSet<string> bannedSessionIds;

    private void OnEnable()
    {
        if (bannedSessionIds == null)
            bannedSessionIds = new HashSet<string>();
    }

    public void BanSession(string sessionId)
    {
        bannedSessionIds.Add(sessionId);
    }

    public bool IsSessionBanned(ISessionInfo info)
    {
        return bannedSessionIds.Contains(info.Id);
    }

    public bool IsSessionBanned(ISession info)
    {
        return bannedSessionIds.Contains(info.Id);
    }
}
