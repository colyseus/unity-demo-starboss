using System;
using System.Collections.Generic;
using LucidSightTools;

[Serializable]
public class StarBossTeam
{
    public int teamIndex = -1;
    public List<string> clientsOnTeam = new List<string>();

    public bool AddPlayer(string clientID)
    {
        if (ContainsClient(clientID))
        {
            LSLog.LogError($"Team {teamIndex} already has a client with ID {clientID}! Will not add");
            return false;
        }

        clientsOnTeam.Add(clientID);
        return true;
    }

    public bool RemovePlayer(string clientID)
    {
        if (!ContainsClient(clientID))
        {
            LSLog.LogError($"Team {teamIndex} does not have a client with ID {clientID}! Will not remove them");
            return false;
        }

        clientsOnTeam.Remove(clientID);
        return true;
    }

    public bool ContainsClient(string clientID)
    {
        return clientsOnTeam.Contains(clientID);
    }
}
