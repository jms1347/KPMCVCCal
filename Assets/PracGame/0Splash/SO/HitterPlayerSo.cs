using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitterPlayerDB
{
    public int index;
    public string name;
    public string teamName;
    public string grade;
    public int overlol;
    public int year;
    public int hit;
    public int hit_right;
    public int hit_left;
    public int hit_under;
    public int longHit;
    public int longHit_right;
    public int longHit_left;
    public int longHit_under;
    public int sungu;
    public int sungu_right;
    public int sungu_left;
    public int sungu_under;
    public int speed;
    public int defence;
    public int level = 0;
    public int addpoint = 0;
    public enum Position
    {
        posu = 0,
        one = 1,
        two = 2,
        three =3,
        ugyuk = 4,
        lcr = 5
    }
    public Position position;
    public enum Hand
    {
        leftHand = 0,
        rightHand = 1,
        doubleHand = 2
    }
    public Hand hand;
    public enum PreferredBattingOrder
    {
        top = 0,
        cleanup =1,
        down =2,
        balance = 3
    }
    public PreferredBattingOrder preferredBattingOrder;
    public string preferredBattingOrder_Detail;
}

[CreateAssetMenu(fileName = "HitterPlayerSO", menuName = "ScriptableObject/HitterPlayerSO")]
public class HitterPlayerSo : ScriptableObject
{
    public List<HitterPlayerDB> hitterPlayerDataList = new List<HitterPlayerDB>();

    public HitterPlayerDB FindHitterPlayer(string pName, string pTeamName, string pGrade, int pYear )
    {
        for (int i = 0; i < hitterPlayerDataList.Count; i++)
        {
            if(hitterPlayerDataList[i].name == pName && 
                hitterPlayerDataList[i].teamName == pTeamName &&
                hitterPlayerDataList[i].grade == pGrade &&
                hitterPlayerDataList[i].year == pYear)
            {
                return hitterPlayerDataList[i];
            }
        }

        return null;
    }
}