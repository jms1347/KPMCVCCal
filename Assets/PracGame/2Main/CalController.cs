using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalController : MonoBehaviour
{
    public HitterPlayerSo hitterPlayerDB;
    public PitcherPlayerSo pitcherPlayerDB;

    [System.Serializable]
    public class HitterBox
    {
        public InputField nameInput;
        public Dropdown teamInput;
        public Dropdown gradeInput;
        
        public InputField yearInput;
        public InputField addNumberInput;
        public InputField levelInput;

    }
    public List<HitterBox> hitterBoxList = new List<HitterBox>();
    public List<HitterPlayerDB> hitterPlayers = new List<HitterPlayerDB>();

    [Header("합계변수")]
    public int sumLeftHit = 0;
    public int sumRightHit = 0;
    public int sumUnderHit = 0;

    public int sumLeftLongHit = 0;
    public int sumRightLongHit = 0;
    public int sumUnderLongHit = 0;

    public int sumLeftSungu = 0;
    public int sumRightSungu = 0;
    public int sumUnderSungu = 0;

    public int sumSpeed = 0;
    public int sumDefence = 0;
    public int sumAwakening = 0;
    public int sumLeftHitterCnt = 0;
    public int sumRightHitterCnt = 0;
    public int sumDoubleHitterCnt = 0;

    [Header("결과 팝업")]
    public GameObject resultPop;
    public Text[] resultText = new Text[11];

    public void InitSumValue()
    {
        sumLeftHit = 0;
        sumRightHit = 0;
         sumUnderHit = 0;

         sumLeftLongHit = 0;
         sumRightLongHit = 0;
         sumUnderLongHit = 0;

        sumLeftSungu = 0;
         sumRightSungu = 0;
         sumUnderSungu = 0;

        sumSpeed = 0;
         sumDefence = 0;
        sumAwakening = 0;
        sumLeftHitterCnt = 0;
       sumRightHitterCnt = 0;
        sumDoubleHitterCnt = 0;
        hitterPlayers.Clear();

        for (int i = 0; i < resultText.Length; i++)
        {
            resultText[i].text = "";
        }
    }
    public void HitterCalBtn()
    {
        //초기화
        InitSumValue();

        bool isError = false;
        //선수 검색
        for (int i = 0; i < hitterBoxList.Count; i++)
        {
            HitterPlayerDB tempHitter = hitterPlayerDB.FindHitterPlayer(hitterBoxList[i].nameInput.text,
                hitterBoxList[i].teamInput.captionText.text,
                hitterBoxList[i].gradeInput.captionText.text,
                int.Parse(hitterBoxList[i].yearInput.text));

            if(tempHitter == null)
            {
                isError = true;
                Result((i+1) + "번 입력오류");
                break;
            }
            else
            {
                hitterPlayers.Add(tempHitter);
            }
        }

        if (!isError)
        {
            //선수 계산
            for (int i = 0; i < hitterPlayers.Count; i++)
            {
                sumLeftHit += hitterPlayers[i].hit_left;
                sumRightHit += hitterPlayers[i].hit_right;
                sumUnderHit += hitterPlayers[i].hit_under;
                sumLeftLongHit += hitterPlayers[i].longHit_left;
                sumRightLongHit += hitterPlayers[i].longHit_right;
                sumUnderLongHit += hitterPlayers[i].longHit_under;
                sumLeftSungu += hitterPlayers[i].sungu_left;
                sumRightSungu += hitterPlayers[i].sungu_right;
                sumUnderSungu += hitterPlayers[i].sungu_under;
                sumSpeed += hitterPlayers[i].speed;
                sumDefence += hitterPlayers[i].defence;

                //포지션 별 추가 능력치 계싼
                if (i < 2 && (hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.top || hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    SumPlus(1);
                }
                else if ((i > 1 && i < 5) && (hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.cleanup || hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    SumPlus(1);
                }
                else if ((i > 4 && i < 9) && (hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.down || hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    SumPlus(1);
                }

                string[] split_temp_text = hitterPlayers[i].preferredBattingOrder_Detail.Split('/');

                for (int x = 0; x < split_temp_text.Length; x++)
                {
                    string temp = split_temp_text[x].Trim();

                    if (temp == i.ToString())
                    {
                        SumPlus(2);
                        break;
                    }
                }

                //좌우타 계산
                if(hitterPlayers[i].hand == HitterPlayerDB.Hand.leftHand)
                {
                    sumLeftHitterCnt += 1;
                }
                else if (hitterPlayers[i].hand == HitterPlayerDB.Hand.rightHand)
                {
                    sumRightHitterCnt += 1;
                }
                else
                {
                    sumDoubleHitterCnt += 1;
                }

                    //등급 더하기
                    if (hitterPlayers[i].grade == "라이징")
                {
                    sumLeftHit += 55;
                    sumRightHit += 55;
                    sumUnderHit += 55;
                    sumLeftLongHit += 15;
                    sumRightLongHit += 15;
                    sumUnderLongHit += 15;
                    sumLeftSungu += 48;
                    sumRightSungu += 48;
                    sumUnderSungu += 48;
                    sumSpeed += 30;
                    sumDefence += 30;
                }
                else if(hitterPlayers[i].grade == "스페셜")
                {
                    SumPlus(10);
                }
                else
                {
                    SumPlus(15);
                }

                if (hitterPlayers[i].grade != "라이징")
                {
                    int plusValue = int.Parse(hitterBoxList[i].levelInput.text);
                    sumLeftHit += plusValue;
                    sumRightHit += plusValue;
                    sumUnderHit += plusValue;
                    //임시 레벨적용
                    if (plusValue>=30)
                    {
                        sumAwakening += 1;
                        SumPlus(1);
                    }                    
                }
                    

                //추가능력치 계산
                SumPlus(int.Parse(hitterBoxList[i].addNumberInput.text));

            }


            // 결과
            Result();
        }
        
    }
    public void Result(string text)
    {
        resultText[4].text = text;
        resultPop.gameObject.SetActive(true);

    }
    public void Result()
    {
        resultText[0].text = "좌타격 : "+sumLeftHit +"\n(평균 : "+sumLeftHit/9+")";
        resultText[1].text = "우타격 : "+ sumRightHit + "\n(평균 : " + sumRightHit / 9 + ")"; 
        resultText[2].text = "언더타격 : "+ sumUnderHit + "\n(평균 : " + sumUnderHit / 9 + ")"; 
        resultText[3].text = "좌장타 : " + sumLeftLongHit + "\n(평균 : " + sumLeftLongHit / 9 + ")";
        resultText[4].text = "우장타 : " + sumRightLongHit + "\n(평균 : " + sumRightLongHit / 9 + ")"; 
        resultText[5].text = "언더장타 : " + sumUnderLongHit + "\n(평균 : " + sumUnderLongHit / 9 + ")";
        resultText[6].text = "좌선구 : " + sumLeftSungu + "\n(평균 : " + sumLeftSungu / 9 + ")"; 
        resultText[7].text = "우선구 : " + sumRightSungu + "\n(평균 : " + sumRightSungu / 9 + ")"; 
        resultText[8].text = "언더선구 : " + sumUnderSungu + "\n(평균 : " + sumUnderSungu / 9 + ")"; 
        resultText[9].text = "주루 : " + sumSpeed + "\n(평균 : " + sumSpeed / 9 + ")"; 
        resultText[10].text = "수비 : " + sumDefence + "\n(평균 : " + sumDefence / 9 + ")";
        resultText[11].text = "각성수 : " + sumAwakening;
        resultText[12].text = "좌타자수 : " + sumLeftHitterCnt;
        resultText[13].text = "양타자수 : " + sumDoubleHitterCnt;
        resultText[14].text = "우타자수 : " + sumRightHitterCnt;
        resultPop.gameObject.SetActive(true);

    }
    public void SumPlus(int plusValue)
    {
        sumLeftHit += plusValue;
        sumRightHit += plusValue;
        sumUnderHit += plusValue;
        sumLeftLongHit += plusValue;
        sumRightLongHit += plusValue;
        sumUnderLongHit += plusValue;
        sumLeftSungu += plusValue;
        sumRightSungu += plusValue;
        sumUnderSungu += plusValue;
        sumSpeed += plusValue;
        sumDefence += plusValue;
    }

    public void SetAllBtn(int index)
    {
        switch (index)
        {
            case 0:
                //팀
                for (int i = 0; i < hitterBoxList.Count; i++)
                {
                    hitterBoxList[i].teamInput.value = hitterBoxList[0].teamInput.value;
                }
                break;
            case 1:
                //등급
                for (int i = 0; i < hitterBoxList.Count; i++)
                {
                    hitterBoxList[i].gradeInput.value = hitterBoxList[0].gradeInput.value;
                }
                break;
            case 2:
                //연도
                for (int i = 0; i < hitterBoxList.Count; i++)
                {
                    hitterBoxList[i].yearInput.text = hitterBoxList[0].yearInput.text;
                }
                break;
            case 3:
                //추가오버롤
                for (int i = 0; i < hitterBoxList.Count; i++)
                {
                    hitterBoxList[i].addNumberInput.text = hitterBoxList[0].addNumberInput.text;
                }
                break;
            case 4:
                //각성여부
                for (int i = 0; i < hitterBoxList.Count; i++)
                {
                    hitterBoxList[i].levelInput.text = hitterBoxList[0].levelInput.text;
                }
                break;
        }
    }
}
