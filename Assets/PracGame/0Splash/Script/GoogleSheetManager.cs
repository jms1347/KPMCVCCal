using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetManager : Singleton<GoogleSheetManager>
{
    const string hitterPlayerDbURL = "https://docs.google.com/spreadsheets/d/1zqPY0yaMBbi5B-4Rs-S7Qp6-AomWHGqAU_7PZjBOuhY/export?format=tsv&gid=0&range=A2:W";
    public CharacterSo characterSo;

    void Awake()
    {
       // DontDestroyOnLoad(this.gameObject);

        CheckGetAllGSData();
    }

    async void CheckGetAllGSData()
    {
        string result = await GetGSDataToURL(hitterPlayerDbURL);
        await Task.Run(() => SetCharacterData(result));

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

    #region 장수 데이터 넣기
    void SetCharacterData(string data)
    {
        if (characterSo.characterDataList != null) characterSo.characterDataList = null;

        int lineSize;
        string[] line = data.Split('\n');
        lineSize = line.Length;
        for (int i = 0; i < lineSize; i++)
        {
            Character character = new Character();
            string[] row = line[i].Split('\t');

            //character.index = i;


            //hitter.year = row[4] != ""? int.Parse(row[4]): 0;

            
        }
    }
    #endregion
    
}
