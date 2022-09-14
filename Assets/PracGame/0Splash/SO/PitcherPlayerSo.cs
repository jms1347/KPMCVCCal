using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PitcherPlayerDB
{
    public int index;

    public string name;
    public string teamName;
    public string grade;
    public int overlol;
    public int year;
    public int changeball;
    public int changeball_right;
    public int changeball_left;
    public int ninth;
    public int ninth_right;
    public int ninth_left;
    public int powerball;
    public int powerball_right;
    public int powerball_left;
    public int mental;
    public int health;
    public int level = 0;
    public int addpoint = 0;
    public int positionCode = 0;
    public enum Position
    {
        first = 0,
        middle = 1,
        final =2
    }
    public Position position;
    public enum Hand
    {
        leftHand = 0,
        rightHand = 1,
        leftSideHand = 2,
        rightSideHand = 3
    }
    public Hand hand;
    public enum PreferredBattingOrder
    {
        top = 0,
        cleanup = 1,
        down = 2,
        balance = 3
    }
}

[CreateAssetMenu(fileName = "PitcherPlayerSO", menuName = "ScriptableObject/PitcherPlayerSO")]
public class PitcherPlayerSo : ScriptableObject
{
    public List<PitcherPlayerDB> pitcherPlayerDataList = new List<PitcherPlayerDB>();

    public PitcherPlayerDB FindPitcherPlayer(string pName, string pTeamName, string pGrade, int pYear)
    {
        for (int i = 0; i < pitcherPlayerDataList.Count; i++)
        {
            if (pitcherPlayerDataList[i].name == pName &&
                pitcherPlayerDataList[i].teamName == pTeamName &&
                pitcherPlayerDataList[i].grade == pGrade &&
                pitcherPlayerDataList[i].year == pYear)
            {
                return pitcherPlayerDataList[i];
            }
        }

        return null;
    }
}