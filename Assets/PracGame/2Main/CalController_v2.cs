using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Linq;

public class CalController_v2 : MonoBehaviour
{
    public HitterPlayerSo hitterPlayerDB;
    public PitcherPlayerSo pitcherPlayerDB;
    public MyClanDataSo clanPlayerDB;

    [Header("인풋ID데이터")]
    public TMP_InputField redTeamIDInput;
    public TMP_InputField blueTeamIDInput;
    public TMP_InputField[] pitcherCodeInput = new TMP_InputField[2];
    [System.Serializable]
    public class Team
    {
        public string playerID;
        public List<HitterPlayerDB> hitterPlayers = new List<HitterPlayerDB>();
        public List<PitcherPlayerDB> pitcherPlayers = new List<PitcherPlayerDB>();
        public List<TextMeshProUGUI> dataTexts = new List<TextMeshProUGUI>();

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
        public int sumAwakeningHitter = 0;
        public int sumLeftHitterCnt = 0;
        public int sumRightHitterCnt = 0;
        public int sumDoubleHitterCnt = 0;
        public int catcherDefense = 0;

        public float sumLeftChangeball = 0;
        public float sumRightChangeball = 0;
        public float sumLeftNinth = 0;
        public float sumRightNinth = 0;
        public float sumLeftPowerball = 0;
        public float sumRightPowerball = 0;
        public float sumMental = 0;
        public float sumHealth = 0;
        public int sumAwakeningPitcher = 0;
        //public int sumLeftPitcherCnt = 0;
        //public int sumRightPitcherCnt = 0;
        //public int sumUnderPitcherCnt = 0;
        public void SumPitcherPlus(int plusValue)
        {
            sumLeftChangeball += plusValue;
            sumRightChangeball += plusValue;
            sumLeftNinth += plusValue;
            sumRightNinth += plusValue;
            sumLeftPowerball += plusValue;
            sumRightPowerball += plusValue;
            sumMental += plusValue;
            sumHealth += plusValue;
        }
        public void SumHitterPlus(int plusValue)
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

        public void ErrorMessageOutPut(string message)
        {
            dataTexts[12].text = message;
        }
        public void SettingText()
        {
            //타자합
            dataTexts[0].text = sumLeftHit.ToString() + "(평균 : " + sumLeftHit / 9 + ")"; 
            dataTexts[1].text = sumRightHit.ToString() + "(평균 : " + sumRightHit / 9 + ")"; 
            dataTexts[2].text = sumUnderHit.ToString() + "(평균 : " + sumUnderHit / 9 + ")"; 
            dataTexts[3].text = sumLeftLongHit.ToString() + "(평균 : " + sumLeftLongHit / 9 + ")"; 
            dataTexts[4].text = sumRightLongHit.ToString() + "(평균 : " + sumRightLongHit / 9 + ")"; 
            dataTexts[5].text = sumUnderLongHit.ToString() + "(평균 : " + sumUnderLongHit / 9 + ")";
            dataTexts[6].text = sumLeftSungu.ToString() + "(평균 : " + sumLeftSungu / 9 + ")";
            dataTexts[7].text = sumRightSungu.ToString() + "(평균 : " + sumRightSungu / 9 + ")";
            dataTexts[8].text = sumUnderSungu.ToString() + "(평균 : " + sumUnderSungu / 9 + ")";
            //dataTexts[9].text = sumSpeed.ToString() + "(평균 : " + sumSpeed / 9 + ")";
            //dataTexts[10].text = sumDefence.ToString() + "(평균 : " + sumDefence / 9 + ")";
            dataTexts[11].text = sumAwakeningHitter.ToString();
            dataTexts[12].text = sumLeftHitterCnt.ToString();
            dataTexts[13].text = sumRightHitterCnt.ToString();
            dataTexts[14].text = sumDoubleHitterCnt.ToString();
            //dataTexts[15].text = catcherDefense.ToString();

            //투수합
            dataTexts[18].text = (sumLeftChangeball).ToString();
            dataTexts[19].text = (sumRightChangeball).ToString();
            dataTexts[20].text = (sumLeftNinth ).ToString();
            dataTexts[21].text = (sumRightNinth ).ToString();
            dataTexts[22].text = (sumLeftPowerball ).ToString();
            dataTexts[23].text = (sumRightPowerball ).ToString();
            //dataTexts[24].text = (sumMental / 100).ToString();
            //dataTexts[25].text = (sumHealth / 100).ToString();
            //dataTexts[26].text = sumAwakeningPitcher.ToString();
        }

        public void Init()
        {
            playerID = "";
            hitterPlayers.Clear();
            pitcherPlayers.Clear();

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
                 sumAwakeningHitter = 0;
                 sumLeftHitterCnt = 0;
               sumRightHitterCnt = 0;
               sumDoubleHitterCnt = 0;
            catcherDefense = 0;

            sumLeftChangeball = 0;
               sumRightChangeball = 0;
                sumLeftNinth = 0;
                sumRightNinth = 0;
                sumLeftPowerball = 0;
                 sumRightPowerball = 0;
                 sumMental = 0;
                sumHealth = 0;
                 sumAwakeningPitcher = 0;
               //sumLeftPitcherCnt = 0;
               //  sumRightPitcherCnt = 0;
               //sumUnderPitcherCnt = 0;
        }
    }

    public Team[] teams = new Team[2];

    public void CalBtn()
    {
        SettingData(redTeamIDInput.text, 0);
        SettingData(blueTeamIDInput.text, 1);
    }
    public void SettingData(string pId, int pIndex)
    {
        //초기화
        teams[pIndex].Init();

        List<PlayerData> tempData = new List<PlayerData>();
        tempData = clanPlayerDB.GetGroupIDData(pId);
        bool isError = false;

        //선수 검색 및 세팅
        for (int i = 0; i < tempData.Count; i++)
        {
            if (i < 9)
            {
                //타자데이터
                HitterPlayerDB tempHitter = hitterPlayerDB.FindHitterPlayer(tempData[i].name,
                                                tempData[i].teamName,
                                                tempData[i].grade, tempData[i].year);

                if(tempHitter == null)
                {
                    isError = true;
                    teams[pIndex].ErrorMessageOutPut((i + 1) + "번 타자 입력 오류");
                    break;
                }
                else
                {
                    tempHitter.level = tempData[i].level;
                    tempHitter.addpoint = tempData[i].addPoint;

                    teams[pIndex].hitterPlayers.Add(tempHitter);
                }
            }
            else
            {
                //투수데이터
                PitcherPlayerDB tempPitcher = pitcherPlayerDB.FindPitcherPlayer(tempData[i].name,
                                                tempData[i].teamName,
                                                tempData[i].grade, tempData[i].year);

                if (tempPitcher == null)
                {
                    isError = true;
                    teams[pIndex].ErrorMessageOutPut((i + 1) + "번 투수 입력 오류");
                    break;
                }
                else
                {
                    tempPitcher.level = tempData[i].level;
                    tempPitcher.addpoint = tempData[i].addPoint;
                    tempPitcher.positionCode = tempData[i].positionCode;
                    teams[pIndex].pitcherPlayers.Add(tempPitcher);
                }
            }
        }

        //에러없으면 합산진행
        if (!isError)
        {
            //선수 계산(타자)
            for (int i = 0; i < teams[pIndex].hitterPlayers.Count; i++)
            {
                teams[pIndex].sumLeftHit += teams[pIndex].hitterPlayers[i].hit_left;
                teams[pIndex].sumRightHit += teams[pIndex].hitterPlayers[i].hit_right;
                teams[pIndex].sumUnderHit += teams[pIndex].hitterPlayers[i].hit_under;
                teams[pIndex].sumLeftLongHit += teams[pIndex].hitterPlayers[i].longHit_left;
                teams[pIndex].sumRightLongHit += teams[pIndex].hitterPlayers[i].longHit_right;
                teams[pIndex].sumUnderLongHit += teams[pIndex].hitterPlayers[i].longHit_under;
                teams[pIndex].sumLeftSungu += teams[pIndex].hitterPlayers[i].sungu_left;
                teams[pIndex].sumRightSungu += teams[pIndex].hitterPlayers[i].sungu_right;
                teams[pIndex].sumUnderSungu += teams[pIndex].hitterPlayers[i].sungu_under;
                teams[pIndex].sumSpeed += teams[pIndex].hitterPlayers[i].speed;
                teams[pIndex].sumDefence += teams[pIndex].hitterPlayers[i].defence;

                //포수 수비 세팅
                //if (teams[pIndex].hitterPlayers[i].position == HitterPlayerDB.Position.posu)
                //    teams[pIndex].catcherDefense = teams[pIndex].hitterPlayers[i].defence;

                //포지션 별 추가 능력치 계싼
                if (i < 2 && (teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.top || teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    teams[pIndex].SumHitterPlus(1);
                }
                else if ((i > 1 && i < 5) && (teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.cleanup || teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    teams[pIndex].SumHitterPlus(1);
                }
                else if ((i > 4 && i < 9) && (teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.down || teams[pIndex].hitterPlayers[i].preferredBattingOrder == HitterPlayerDB.PreferredBattingOrder.balance))
                {
                    teams[pIndex].SumHitterPlus(1);
                }

                string[] split_temp_text = teams[pIndex].hitterPlayers[i].preferredBattingOrder_Detail.Split('/');

                for (int x = 0; x < split_temp_text.Length; x++)
                {
                    string temp = split_temp_text[x].Trim();

                    if (temp == i.ToString())
                    {
                        teams[pIndex].SumHitterPlus(2);
                        break;
                    }
                }

                //좌우타 수 계산
                if (teams[pIndex].hitterPlayers[i].hand == HitterPlayerDB.Hand.leftHand)
                {
                    teams[pIndex].sumLeftHitterCnt += 1;

                }
                else if (teams[pIndex].hitterPlayers[i].hand == HitterPlayerDB.Hand.rightHand)
                {
                    teams[pIndex].sumRightHitterCnt += 1;
                }
                else
                {
                    teams[pIndex].sumDoubleHitterCnt += 1;
                }

                //등급 더하기
                if (teams[pIndex].hitterPlayers[i].grade == "라이징")
                {
                    teams[pIndex].sumLeftHit += 55;
                    teams[pIndex].sumRightHit += 55;
                    teams[pIndex].sumUnderHit += 55;
                    teams[pIndex].sumLeftLongHit += 15;
                    teams[pIndex].sumRightLongHit += 15;
                    teams[pIndex].sumUnderLongHit += 15;
                    teams[pIndex].sumLeftSungu += 48;
                    teams[pIndex].sumRightSungu += 48;
                    teams[pIndex].sumUnderSungu += 48;
                    teams[pIndex].sumSpeed += 30;
                    teams[pIndex].sumDefence += 30;
                }
                else if (teams[pIndex].hitterPlayers[i].grade == "스페셜")
                {
                    teams[pIndex].SumHitterPlus(10);
                }
                else
                {
                    teams[pIndex].SumHitterPlus(15);
                }

                if (teams[pIndex].hitterPlayers[i].grade != "라이징")
                {
                    int plusValue = teams[pIndex].hitterPlayers[i].level;
                    teams[pIndex].sumLeftHit += plusValue;
                    teams[pIndex].sumRightHit += plusValue;
                    teams[pIndex].sumUnderHit += plusValue;
                    if (teams[pIndex].hitterPlayers[i].grade == "몬스터")
                    {
                        //임시 레벨적용
                        if (plusValue >= 30)
                        {
                            teams[pIndex].sumAwakeningHitter += 1;
                            teams[pIndex].SumHitterPlus(1);
                        }
                    }else if (teams[pIndex].hitterPlayers[i].grade == "에이스")
                    {
                        if (plusValue >= 25)
                        {
                            teams[pIndex].sumAwakeningHitter += 1;
                            teams[pIndex].SumHitterPlus(1);
                        }
                    }
                    else if (teams[pIndex].hitterPlayers[i].grade == "스페셜")
                    {
                        if (plusValue >= 15)
                        {
                            //teams[pIndex].sumAwakeningHitter += 1;
                            teams[pIndex].SumHitterPlus(1);
                        }
                    }
                }
                //추가능력치 계산
                teams[pIndex].SumHitterPlus(teams[pIndex].hitterPlayers[i].addpoint);
            }


            ////////////////////////////////////////////투수
            //투수 계산
            PitcherPlayerDB tempPitchers = teams[pIndex].pitcherPlayers.Where((temp) => temp.positionCode.ToString() == pitcherCodeInput[pIndex].text).FirstOrDefault();

            if(tempPitchers != null)
            {
                teams[pIndex].sumLeftChangeball += tempPitchers.changeball_left;
                teams[pIndex].sumRightChangeball += tempPitchers.changeball_right;
                teams[pIndex].sumLeftNinth += tempPitchers.ninth_left;
                teams[pIndex].sumRightNinth += tempPitchers.ninth_right;
                teams[pIndex].sumLeftPowerball += tempPitchers.powerball_left;
                teams[pIndex].sumRightPowerball += tempPitchers.powerball_right;
                teams[pIndex].sumMental += tempPitchers.mental;
                teams[pIndex].sumHealth += tempPitchers.health;

                //등급 더하기
                if (tempPitchers.grade == "라이징")
                {
                    teams[pIndex].sumLeftChangeball += 50;
                    teams[pIndex].sumRightChangeball += 50;
                    teams[pIndex].sumLeftNinth += 50;
                    teams[pIndex].sumRightNinth += 50;
                    teams[pIndex].sumLeftPowerball += 30;
                    teams[pIndex].sumRightPowerball += 30;
                    teams[pIndex].sumMental += 15;
                    teams[pIndex].sumHealth += 30;
                }
                else if (tempPitchers.grade == "스페셜")
                {
                    teams[pIndex].SumPitcherPlus(10);
                }
                else
                {
                    teams[pIndex].SumPitcherPlus(15);
                }

                if (tempPitchers.grade != "라이징")
                {
                    int plusValue = tempPitchers.level;
                    teams[pIndex].sumLeftChangeball += plusValue;
                    teams[pIndex].sumRightChangeball += plusValue;
                    if (tempPitchers.grade == "몬스터")
                    {
                        //임시 레벨적용
                        if (plusValue >= 30)
                        {
                            teams[pIndex].sumAwakeningPitcher += 1;
                            teams[pIndex].SumPitcherPlus(1);
                        }
                    }
                    else if (tempPitchers.grade == "에이스")
                    {
                        if (plusValue >= 25)
                        {
                            teams[pIndex].sumAwakeningPitcher += 1;
                            teams[pIndex].SumPitcherPlus(1);
                        }
                    }
                    else if (tempPitchers.grade == "스페셜")
                    {
                        if (plusValue >= 15)
                        {
                            //teams[pIndex].sumAwakeningHitter += 1;
                            teams[pIndex].SumPitcherPlus(1);
                        }
                    }
                }
                //추가능력치 계산
                teams[pIndex].SumHitterPlus(tempPitchers.addpoint);
                teams[pIndex].SumPitcherPlus(tempPitchers.addpoint);
            }

            // 결과 출력
            teams[pIndex].SettingText();
        }
    }


}
