using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetManager : Singleton<GoogleSheetManager>
{
    const string hitterPlayerDbURL = "https://docs.google.com/spreadsheets/d/1zqPY0yaMBbi5B-4Rs-S7Qp6-AomWHGqAU_7PZjBOuhY/export?format=tsv&gid=0&range=A2:W";
    const string pitcherPlayerDbURL = "https://docs.google.com/spreadsheets/d/1zqPY0yaMBbi5B-4Rs-S7Qp6-AomWHGqAU_7PZjBOuhY/export?format=tsv&gid=958478835&range=A2:R";
    const string clanTeamPlayerDbURL = "https://docs.google.com/spreadsheets/d/1zqPY0yaMBbi5B-4Rs-S7Qp6-AomWHGqAU_7PZjBOuhY/export?format=tsv&gid=1530723360&range=A2:T";
    public HitterPlayerSo hitterPlayerSo;
    public PitcherPlayerSo pitcherPlayerSo;
    public MyClanDataSo clanTeamPlayerSo;

    void Awake()
    {
       // DontDestroyOnLoad(this.gameObject);

        CheckGetAllGSData();
    }

    async void CheckGetAllGSData()
    {
        string result = await GetGSDataToURL(hitterPlayerDbURL);
        await Task.Run(() => SetHitterData(result));
        result = await GetGSDataToURL(pitcherPlayerDbURL);
        await Task.Run(() => SetPitcherData(result));
        result = await GetGSDataToURL(clanTeamPlayerDbURL);
        await Task.Run(() => SetClanTeamPlayerData(result));

        LoadingSceneManager.LoadScene("MainScene_v2");
    }


    async UniTask<string> GetGSDataToURL(string url)
    {
        try
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            await www.SendWebRequest();
            string data = www.downloadHandler.text;
            return data;
        }
        catch
        {
            return "Error";
        }       
    }
    #region 클랜 팀 선수 데이터 넣기
    void SetClanTeamPlayerData(string data)
    {
        if (clanTeamPlayerSo.clanTeamPlayerDataList != null) clanTeamPlayerSo.clanTeamPlayerDataList = null;
        int lineSize;
        string[] line = data.Split('\n');
        lineSize = line.Length;
        for (int i = 0; i < lineSize; i++)
        {
            PlayerData player = new PlayerData();
            string[] row = line[i].Split('\t');

            player.index = i;
            player.teamID = row[0];
            player.positionCode = int.Parse(row[1]);
            player.name = row[2];
            player.teamName = row[3];
            player.grade = row[4];
            player.year = row[5] != "" ? int.Parse(row[5]) : 0;
            player.level = row[6] != "" ? int.Parse(row[6]) : 0;
            player.addPoint = row[7] != "" ? int.Parse(row[7]) : 0;
            player.aPoint_1 = row[8] != "" ? int.Parse(row[8]) : 0;
            player.aPoint_2 = row[9] != "" ? int.Parse(row[9]) : 0;
            player.bPoint_1 = row[10] != "" ? int.Parse(row[10]) : 0;
            player.bPoint_2 = row[11] != "" ? int.Parse(row[11]) : 0;
            player.cPoint_1 = row[12] != "" ? int.Parse(row[12]): 0;
            player.cPoint_2 = row[13] != "" ? int.Parse(row[13]) : 0;
            player.cPoint_seongu = row[14] != "" ? int.Parse(row[14]) : 0;
            player.dPoint = row[15] != "" ? int.Parse(row[15]) : 0;
            player.ePoint = row[16] != "" ? int.Parse(row[16]) : 0;
            player.handType = row[17];
            int temp4 = 0;
            switch (row[18])
            {
                case "상위":
                    temp4 = 0;
                    break;
                case "클린업":
                    temp4 = 1;
                    break;
                case "하위":
                    temp4 = 2;
                    break;
                case "밸런스":
                    temp4 = 3;
                    break;
            }
            player.preferredBattingOrder = (PlayerData.PreferredBattingOrder)temp4;
            player.preferredBattingOrder_Detail = row[19];
            if (clanTeamPlayerSo.clanTeamPlayerDataList == null)
                clanTeamPlayerSo.clanTeamPlayerDataList = new System.Collections.Generic.List<PlayerData>();

            clanTeamPlayerSo.clanTeamPlayerDataList.Add(player);
        }
    }
    #endregion
    #region 타자 데이터 넣기
    void SetHitterData(string data)
    {
        if (hitterPlayerSo.hitterPlayerDataList != null) hitterPlayerSo.hitterPlayerDataList = null;
        int lineSize;
        string[] line = data.Split('\n');
        lineSize = line.Length;
        for (int i = 0; i < lineSize; i++)
        {
            HitterPlayerDB hitter = new HitterPlayerDB();
            string[] row = line[i].Split('\t');

            hitter.index = i;
            hitter.name = row[0];
            hitter.teamName = row[1];
            hitter.grade = row[2];
            hitter.overlol = int.Parse(row[3]);
            hitter.year = row[4] != ""? int.Parse(row[4]): 0;
            hitter.hit = int.Parse(row[5]);
            hitter.hit_right = int.Parse(row[6]);
            hitter.hit_left = int.Parse(row[7]);
            hitter.hit_under = int.Parse(row[8]);
            hitter.longHit = int.Parse(row[9]);
            hitter.longHit_right = int.Parse(row[10]);
            hitter.longHit_left = int.Parse(row[11]);
            hitter.longHit_under = int.Parse(row[12]);
            hitter.sungu = int.Parse(row[13]);
            hitter.sungu_right = int.Parse(row[14]);
            hitter.sungu_left = int.Parse(row[15]);
            hitter.sungu_under = int.Parse(row[16]);
            hitter.speed = int.Parse(row[17]);
            hitter.defence = int.Parse(row[18]);
            int temp2 = 0;
            switch (row[19])
            {
                case "포수":
                    temp2 = 0;
                    break;                
                case "1루수":
                    temp2 = 1;
                    break;
                case "2루수":
                    temp2 = 2;
                    break;
                case "3루수":
                    temp2 = 3;
                    break;
                case "유격수":
                    temp2 = 4;
                    break;
                case "외야수":
                    temp2 = 5;
                    break;
            }
            hitter.position = (HitterPlayerDB.Position)temp2;
            int temp3 = 0;
            switch (row[20])
            {
                case "우사양타":
                case "우투양타":
                    temp3 = 2;
                    break;
                case "우투우타":
                    temp3 = 1;
                    break;
                case "우투좌타":
                case "좌투좌타":
                    temp3 = 0;
                    break;
            }
            hitter.hand = (HitterPlayerDB.Hand)temp3;
            int temp4 = 0;
            switch (row[21])
            {
                case "상위":
                    temp4 = 0;
                    break;
                case "클린업":
                    temp4 = 1;
                    break;
                case "하위":
                    temp4 = 2;
                    break;
                case "밸런스":
                    temp4 = 3;
                    break;
            }
            hitter.preferredBattingOrder = (HitterPlayerDB.PreferredBattingOrder)temp4;
            hitter.preferredBattingOrder_Detail = row[22];

            if (hitterPlayerSo.hitterPlayerDataList == null)
                hitterPlayerSo.hitterPlayerDataList = new System.Collections.Generic.List<HitterPlayerDB>();

            hitterPlayerSo.hitterPlayerDataList.Add(hitter);
        }
    }
    #endregion
    #region 투수 데이터 넣기
    void SetPitcherData(string data)
    {
        if (pitcherPlayerSo.pitcherPlayerDataList != null) pitcherPlayerSo.pitcherPlayerDataList = null;
        int lineSize;
        string[] line = data.Split('\n');
        lineSize = line.Length;
        for (int i = 0; i < lineSize; i++)
        {
            PitcherPlayerDB pitcher = new PitcherPlayerDB();
            string[] row = line[i].Split('\t');
            pitcher.index = i;

            pitcher.name = row[0];
            pitcher.teamName = row[1];
            pitcher.grade = row[2]; 
            pitcher.overlol = int.Parse(row[3]);
            pitcher.year = row[4] != "" ? int.Parse(row[4]) : 0;
            pitcher.changeball = int.Parse(row[5]);
            pitcher.changeball_right = int.Parse(row[6]);
            pitcher.changeball_left = int.Parse(row[7]);
            pitcher.ninth = int.Parse(row[8]);
            pitcher.ninth_right = int.Parse(row[9]);
            pitcher.ninth_left = int.Parse(row[10]);
            pitcher.powerball = int.Parse(row[11]);
            pitcher.powerball_right = int.Parse(row[12]);
            pitcher.powerball_left = int.Parse(row[13]);
            pitcher.mental = int.Parse(row[14]);
            pitcher.health = int.Parse(row[15]);
            int temp2 = 0;
            switch (row[16])
            {
                case "선발":
                    temp2 = 0;
                    break;
                case "계투":
                    temp2 = 1;
                    break;
                case "마무리":
                    temp2 = 2;
                    break;

            }
            pitcher.position = (PitcherPlayerDB.Position)temp2;
            int temp3 = 0;
            switch (row[17])
            {
                case "우사양타":
                case "우사우타":
                case "우사좌타":
                    temp3 = 3;
                    break;
                case "우투양타":
                case "우투우타":
                case "우투좌타":
                    temp3 = 1;
                    break;
                case "좌사좌타":
                    temp3 = 2;
                    break;
                case "좌투우타":
                case "좌투좌타":
                    temp3 = 0;
                    break;
            }
            pitcher.hand = (PitcherPlayerDB.Hand)temp3;

            if (pitcherPlayerSo.pitcherPlayerDataList == null)
                pitcherPlayerSo.pitcherPlayerDataList = new System.Collections.Generic.List<PitcherPlayerDB>();
            pitcherPlayerSo.pitcherPlayerDataList.Add(pitcher);
        }
    }
    #endregion
}
