using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Receive a payload of all the players on a team when joining a new room
public class StarBossAllTeamsUpdateMessage
{
    public int teamIndex;
    public string[] clients;
}
