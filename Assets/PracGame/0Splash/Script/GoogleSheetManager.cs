using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetManager : Singleton<GoogleSheetManager>
{
    const string charDbURL = "https://docs.google.com/spreadsheets/d/1DwZA44mcZJ0FT08cZuRZ9KVMirfqqXAXmbhhAY2NvAA/export?format=tsv&gid=254390684&range=A2:G";
    public CharacterSo characterSo;

    void Awake()
    {
       // DontDestroyOnLoad(this.gameObject);

        CheckGetAllGSData();
    }

    async void CheckGetAllGSData()
    {
        string result = await GetGSDataToURL(charDbURL);
        await Task.Run(() => SetCharacterData(result));

        LoadingSceneManager.LoadScene("MainScene_busy");
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

    #region 장수 데이터 넣기
    void SetCharacterData(string data)
    {
        if (characterSo.characterDataList.Count > 0) characterSo.characterDataList.Clear();

        int lineSize;
        string[] line = data.Split('\n');
        lineSize = line.Length;
        for (int i = 0; i < lineSize; i++)
        {
            Character character = new Character();
            string[] row = line[i].Split('\t');

            character.code = i;
            switch (row[0])
            {
                case "촉":
                    character.nation = NationType.Green;
                    break;
                case "위":
                    character.nation = NationType.Blue;
                    break;
                case "오":
                    character.nation = NationType.Red;
                    break;
                case "군웅":
                    character.nation = NationType.Black;
                    break;
                default:
                    character.nation = NationType.Purple;
                    break;

            }
            character.Name = row[1];

            switch (row[2])
            {
                case "넉다운":
                    character.NoSkillType = SkillType.Knockdown;
                    break;
                case "에어본":
                    character.NoSkillType = SkillType.Airborne;
                    break;
                case "격퇴":
                    character.NoSkillType = SkillType.Repel;
                    break;
                case "반격":
                    character.NoSkillType = SkillType.Counterattack;
                    break;
                case "연소":
                    character.NoSkillType = SkillType.Fire;
                    break;
                case "자염":
                    character.NoSkillType = SkillType.Flame;
                    break;
                case "매혹":
                    character.NoSkillType = SkillType.Love;
                    break;
                case "회복":
                    character.NoSkillType = SkillType.Heal;
                    break;
                case "소환":
                    character.NoSkillType = SkillType.Summon;
                    break;
                case "무적":
                    character.NoSkillType = SkillType.Invincible;
                    break;
                case "우길":
                    character.NoSkillType = SkillType.Ugil;
                    break;
                case "CC":
                    character.NoSkillType = SkillType.CC;
                    break;
                default:
                    character.NoSkillType = SkillType.None;
                    break;
            }
            switch (row[3])
            {
                case "넉다운":
                    character.BoSkillType = SkillType.Knockdown;
                    break;
                case "에어본":
                    character.BoSkillType = SkillType.Airborne;
                    break;
                case "격퇴":
                    character.BoSkillType = SkillType.Repel;
                    break;
                default:
                    character.NoSkillType = SkillType.None;
                    break;
            }
            switch (row[4])
            {
                case "넉다운":
                    character.ChuConnectableSkillType = SkillType.Knockdown;
                    break;
                case "에어본":
                    character.ChuConnectableSkillType = SkillType.Airborne;
                    break;
                case "격퇴":
                    character.ChuConnectableSkillType = SkillType.Repel;
                    break;
                case "추격":
                    character.ChuConnectableSkillType = SkillType.Counterattack;
                    break;

                default:
                    character.ChuConnectableSkillType = SkillType.None;
                    break;
            }
            switch (row[5])
            {
                case "넉다운":
                    character.Chu2OutputSkillType = SkillType.Knockdown;
                    break;
                case "에어본":
                    character.Chu2OutputSkillType = SkillType.Airborne;
                    break;
                case "격퇴":
                    character.Chu2OutputSkillType = SkillType.Repel;
                    break;
                case "All":
                    character.Chu2OutputSkillType = SkillType.All;
                    break;

                default:
                    character.Chu2OutputSkillType = SkillType.None;
                    break;
            }
            character.passiveSkill = row[6];

            characterSo.characterDataList.Add(character);
        }
    }
    #endregion
    
}
