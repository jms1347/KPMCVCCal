using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// SkillType, NationType, SkillCategory, Character 클래스는 프로젝트에 맞게 정의되어 있어야 합니다.

public class ComboCalController : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>();

    private class ComboState
    {
        public int CurrentComboCount = 0;
        public SkillType LastSkillOutput = SkillType.None;
        public Character LastCharacterUsed = null;
        public List<Character> UsedCharacters = new List<Character>();
        public List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)> ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>();

        public ComboState Clone()
        {
            return new ComboState
            {
                CurrentComboCount = CurrentComboCount,
                LastSkillOutput = LastSkillOutput,
                LastCharacterUsed = LastCharacterUsed,
                UsedCharacters = new List<Character>(UsedCharacters),
                ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>(ComboPath)
            };
        }
    }

    private List<List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)>> _allComboPaths;
    private Dictionary<SkillType, int> _allPartySkillTypeCounts;
    // 콤보 시작 스킬 타입별 개수를 저장할 Dictionary
    private Dictionary<SkillType, int> _startSkillCounts;

    private void Start()
    {
        TestComboCalculation();
    }

    /// <summary>
    /// 파티 조합으로 만들 수 있는 모든 가능한 콤보를 계산하고 콘솔에 출력합니다.
    /// 또한, 파티원들이 가진 특정 No 스킬 및 Bo 스킬의 종류별 개수를 집계하고,
    /// 콤보 시작 스킬의 통계 및 확률을 계산합니다.
    /// </summary>
    public void CalculateAllCombosAndPrint()
    {
        _allComboPaths = new List<List<(string, SkillCategory, SkillType, SkillType)>>();
        _allPartySkillTypeCounts = new Dictionary<SkillType, int>();
        _startSkillCounts = new Dictionary<SkillType, int>(); // 콤보 시작 스킬 카운트 Dictionary 초기화

        // --- 파티원들의 No 스킬 및 Bo 스킬 중 특정 타입을 미리 카운팅하는 로직 (기존 유지) ---
        foreach (Character member in partyMembers)
        {
            if (member.NoSkillType != SkillType.None)
            {
                if (_allPartySkillTypeCounts.ContainsKey(member.NoSkillType))
                {
                    _allPartySkillTypeCounts[member.NoSkillType]++;
                }
                else
                {
                    _allPartySkillTypeCounts.Add(member.NoSkillType, 1);
                }
            }

            if (member.BoSkillType != SkillType.None)
            {
                if (_allPartySkillTypeCounts.ContainsKey(member.BoSkillType))
                {
                    _allPartySkillTypeCounts[member.BoSkillType]++;
                }
                else
                {
                    _allPartySkillTypeCounts.Add(member.BoSkillType, 1);
                }
            }
        }
        // ----------------------------------------------------------------------

        // 파티원 목록을 순회하며 각 캐릭터를 시작점으로 콤보 탐색
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4);

            // 1. 노(No) 스킬로 콤보 시작 시도
            bool canNoSkillStartCombo =
                startChar.NoSkillType != SkillType.None &&
                startChar.NoSkillType != SkillType.Love &&
                startChar.NoSkillType != SkillType.Flame &&
                startChar.NoSkillType != SkillType.Fire &&
                startChar.NoSkillType != SkillType.CC &&
                startChar.NoSkillType != SkillType.Summon &&
                startChar.NoSkillType != SkillType.Enhance &&
                startChar.NoSkillType != SkillType.Heal &&
                startChar.NoSkillType != SkillType.Ugil &&
                startChar.NoSkillType != SkillType.Invincible;

            if (startChar.NoSkillType == SkillType.Counterattack)
            {
                canNoSkillStartCombo = true;
            }

            if (canNoSkillStartCombo)
            {
                // No 스킬이 콤보 시작 스킬로 사용될 경우 카운트
                SkillType effectiveStartSkillType = startChar.NoSkillType;
                // Counterattack은 None으로 취급하여 발동 확률 계산에서 제외할 수 있도록
                if (effectiveStartSkillType == SkillType.Counterattack)
                {
                    // No 스킬 Counterattack은 콤보 시작은 가능하지만 확률 집계에서는 제외 (기획 의도에 따라 변경 가능)
                    // 지금은 확률 계산에 포함하지 않고 넘어가겠습니다.
                }
                else if (effectiveStartSkillType != SkillType.None)
                {
                    if (_startSkillCounts.ContainsKey(effectiveStartSkillType))
                    {
                        _startSkillCounts[effectiveStartSkillType]++;
                    }
                    else
                    {
                        _startSkillCounts.Add(effectiveStartSkillType, 1);
                    }
                }

                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);

                if (startChar.NoSkillType == SkillType.Counterattack)
                {
                    initialState.LastSkillOutput = SkillType.None;
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }
                else
                {
                    initialState.LastSkillOutput = startChar.NoSkillType;
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }
                ExploreCombo(initialState);
            }

            // 2. 보(Bo) 스킬로 콤보 시작 시도 (지원 장수는 Bo 스킬로 시작 불가)
            if (startChar.BoSkillType != SkillType.None && !isSupportCharacter)
            {
                // Bo 스킬이 콤보 시작 스킬로 사용될 경우 카운트
                if (_startSkillCounts.ContainsKey(startChar.BoSkillType))
                {
                    _startSkillCounts[startChar.BoSkillType]++;
                }
                else
                {
                    _startSkillCounts.Add(startChar.BoSkillType, 1);
                }

                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastSkillOutput = startChar.BoSkillType;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Bo, SkillType.None, startChar.BoSkillType));

                ExploreCombo(initialState);
            }

            // 3. 추(Chu) 스킬이 Counterattack일 때 콤보 시작 스킬로 간주
            if (startChar.ChuConnectableSkillType == SkillType.Counterattack)
            {
                // Chu Counterattack이 콤보 시작 스킬로 사용될 경우 카운트
                // (일반적인 넉다운, 에어본, 격퇴와는 다르게 Counterattack은 그 자체로 시작 스킬이므로,
                // 여기서는 outputSkill인 Chu2OutputSkillType이 아닌, Counterattack 자체를 카운트)
                if (_startSkillCounts.ContainsKey(SkillType.Counterattack))
                {
                    _startSkillCounts[SkillType.Counterattack]++;
                }
                else
                {
                    _startSkillCounts.Add(SkillType.Counterattack, 1);
                }

                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.LastSkillOutput = startChar.Chu2OutputSkillType;
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Chu, SkillType.Counterattack, startChar.Chu2OutputSkillType));

                ExploreCombo(initialState);
            }
        }

        PrintAllCombos();
    }

    private void ExploreCombo(ComboState currentState)
    {
        bool canContinueCombo = false;

        foreach (Character nextChar in partyMembers)
        {
            if (currentState.UsedCharacters.Contains(nextChar))
            {
                continue;
            }

            if (nextChar.ChuConnectableSkillType != SkillType.None &&
                nextChar.ChuConnectableSkillType != SkillType.Counterattack)
            {
                bool canConnect = false;

                if (nextChar.ChuConnectableSkillType == SkillType.All)
                {
                    canConnect = true;
                }
                else if (currentState.LastSkillOutput == SkillType.Airborne ||
                         currentState.LastSkillOutput == SkillType.Knockdown ||
                         currentState.LastSkillOutput == SkillType.Repel)
                {
                    if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                    {
                        canConnect = true;
                    }
                }
                else if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                {
                    canConnect = true;
                }

                if (canConnect)
                {
                    canContinueCombo = true;

                    ComboState nextState = currentState.Clone();
                    nextState.CurrentComboCount++;
                    nextState.LastSkillOutput = nextChar.Chu2OutputSkillType;
                    nextState.LastCharacterUsed = nextChar;
                    nextState.UsedCharacters.Add(nextState.LastCharacterUsed);
                    nextState.ComboPath.Add((nextChar.Name, SkillCategory.Chu, nextChar.ChuConnectableSkillType, nextChar.Chu2OutputSkillType));

                    ExploreCombo(nextState);
                }
            }
        }

        if (!canContinueCombo)
        {
            _allComboPaths.Add(currentState.ComboPath);
        }
    }

    /// <summary>
    /// 저장된 모든 콤보 경로를 콘솔에 한 줄로 출력하고, 파티원들이 가진 No 스킬 및 Bo 스킬 통계를 표시합니다.
    /// 또한, 콤보 시작 스킬의 통계 및 확률을 계산하여 출력합니다.
    /// </summary>
    private void PrintAllCombos()
    {
        Debug.Log("--- 모든 가능한 콤보 경로 ---");
        if (_allComboPaths.Any())
        {
            var validCombos = _allComboPaths.Where(c => c.Count > 0).OrderByDescending(c => c.Count).ToList();
            int comboIndex = 1;

            foreach (var combo in validCombos)
            {
                string comboString = "";
                for (int i = 0; i < combo.Count; i++)
                {
                    var step = combo[i];
                    comboString += $"{step.characterName}({step.skillCat}";

                    if (step.skillCat == SkillCategory.Chu)
                    {
                        comboString += $":{step.inputSkill}->{step.outputSkill})";
                    }
                    else
                    {
                        comboString += $":{step.outputSkill})";
                    }

                    if (i < combo.Count - 1)
                    {
                        comboString += " -> ";
                    }
                }
                Debug.Log($"콤보 {comboIndex++} (길이: {combo.Count}): {comboString}");
            }
            Debug.Log($"총 {validCombos.Count}가지 콤보가 발견되었습니다.");

            int maxComboLength = 0;
            if (validCombos.Any())
            {
                maxComboLength = validCombos.First().Count;
            }
            Debug.Log($"최대 콤보 길이: {maxComboLength}");
        }
        else
        {
            Debug.Log("생성 가능한 콤보가 없습니다.");
        }

        // --- 콤보 생성과 무관하게 파티원들의 전체 No/Bo 스킬 타입별 카운트 출력 ---
        Debug.Log("--- 파티원들의 No 및 Bo 스킬 타입별 총 카운트 (None 제외) ---");
        if (_allPartySkillTypeCounts.Any())
        {
            foreach (var entry in _allPartySkillTypeCounts)
            {
                Debug.Log($"- {entry.Key}: {entry.Value}개");
            }
        }
        else
        {
            Debug.Log("파티원 중에 유효한 No 또는 Bo 스킬 타입이 없습니다.");
        }

        // --- 콤보 시작 스킬 통계 및 확률 계산 ---
        Debug.Log("--- 콤보 시작 스킬 타입별 통계 및 발동 확률 ---");
        int totalStartSkills = _startSkillCounts.Values.Sum(); // 모든 시작 스킬의 총합

        if (totalStartSkills > 0)
        {
            // 넉다운, 에어본, 격퇴의 합계를 위한 변수 초기화
            int knockdownAirborneRepelSum = 0;

            foreach (var entry in _startSkillCounts)
            {
                float probability = (float)entry.Value / totalStartSkills * 100f;
                Debug.Log($"- {entry.Key}: {entry.Value}개 (확률: {probability:F2}%)");

                // 넉다운, 에어본, 격퇴의 합계 계산
                if (entry.Key == SkillType.Knockdown ||
                    entry.Key == SkillType.Airborne ||
                    entry.Key == SkillType.Repel)
                {
                    knockdownAirborneRepelSum += entry.Value;
                }
            }

            // 넉다운 + 에어본 + 격퇴의 총합 및 확률 출력
            if (knockdownAirborneRepelSum > 0)
            {
                float totalProbability = (float)knockdownAirborneRepelSum / totalStartSkills * 100f;
                Debug.Log($"\n- 넉다운 + 에어본 + 격퇴 총합: {knockdownAirborneRepelSum}개 (전체 시작 스킬 대비 확률: {totalProbability:F2}%)");
            }
            else
            {
                Debug.Log("넉다운, 에어본, 격퇴 시작 스킬이 없습니다.");
            }
        }
        else
        {
            Debug.Log("시작 가능한 콤보 스킬이 없습니다.");
        }
        // ----------------------------------------------------
    }

    // 이 TestComboCalculation 함수는 사용자 요청에 따라 더 이상 수정하지 않습니다.
    [ContextMenu("Calculate All Combos")]
    public void TestComboCalculation()
    {
        partyMembers.Clear();

        // GoogleSheetManager를 통해 캐릭터 데이터를 불러옵니다.
        Character char1 = GoogleSheetManager.instance.characterSo.GetCharacterByName("조운");
        Character char2 = GoogleSheetManager.instance.characterSo.GetCharacterByName("관우");
        Character char3 = GoogleSheetManager.instance.characterSo.GetCharacterByName("마초");
        Character char4 = GoogleSheetManager.instance.characterSo.GetCharacterByName("황충");
        Character char5 = GoogleSheetManager.instance.characterSo.GetCharacterByName("화타");

        if (char1 != null) partyMembers.Add(char1);
        if (char2 != null) partyMembers.Add(char2);
        if (char3 != null) partyMembers.Add(char3);
        if (char4 != null) partyMembers.Add(char4);
        if (char5 != null) partyMembers.Add(char5);

        Debug.Log($"파티 멤버 로드 완료. 현재 파티원 수: {partyMembers.Count}");

        CalculateAllCombosAndPrint();
    }
}