using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int index;
    public string teamID;
    public int positionCode;
    public string name;
    public string teamName;
    public string grade;
    public int year;
    public int level;
    public int addPoint;
    public int addPoint_A;
    public int addPoint_B;
    public int addPoint_C;
    //public int bPoint_2;
    //public int cPoint_1;
    //public int cPoint_2;
    //public int cPoint_seongu;
    //public int dPoint;
    //public int ePoint;
    //public string handType;
    //public enum PreferredBattingOrder
    //{
    //    top = 0,
    //    cleanup = 1,
    //    down = 2,
    //    balance = 3
    //}
    //public PreferredBattingOrder preferredBattingOrder;
    //public string preferredBattingOrder_Detail;
}

[CreateAssetMenu(fileName = "ClanTeamPlayerDataSo", menuName = "ScriptableObject/ClanTeamPlayerDataSO")]
public class MyClanDataSo : ScriptableObject
{
    public List<PlayerData> clanTeamPlayerDataList = new List<PlayerData>();

    public List<PlayerData> GetGroupIDData(string pId)
    {
        List<PlayerData> tempData = new List<PlayerData>();
        Debug.Log("pid : " + pId);
        tempData = clanTeamPlayerDataList?.Where((temp) => temp.teamID == pId).ToList();

        return tempData;
    }
}
