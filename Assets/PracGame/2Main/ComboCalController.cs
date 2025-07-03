using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

// SkillType, NationType, SkillCategory, Character 클래스는 프로젝트에 맞게 정의되어 있어야 합니다.
// (이 파일 내에 없지만, 프로젝트에 이미 있다고 가정합니다.)

public class ComboCalController : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>();
    public int maxComboLength = 6; // 콤보의 최대 길이를 제한합니다.

    // ComboState: 현재 콤보 탐색의 상태를 저장하는 내부 클래스
    private class ComboState
    {
        public int CurrentComboCount = 0; // 현재 콤보 길이
        public SkillType LastSkillOutput = SkillType.None; // 이전 스킬의 출력 타입
        public Character LastCharacterUsed = null; // 마지막으로 사용된 캐릭터
        public List<Character> UsedCharacters = new List<Character>(); // 콤보 경로에 사용된 캐릭터 목록 (단순 기록용)
        // 각 캐릭터가 이번 콤보에서 Chu 스킬을 사용했는지 여부를 추적
        public Dictionary<Character, bool> _chuSkillUsedCharacters = new Dictionary<Character, bool>();
        // 현재까지의 콤보 경로: (캐릭터 이름, 스킬 카테고리, 입력 스킬 타입, 출력 스킬 타입)
        public List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)> ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>();

        // ComboState를 복제하는 메서드 (새로운 콤보 경로 탐색 시 사용)
        public ComboState Clone()
        {
            return new ComboState
            {
                CurrentComboCount = CurrentComboCount,
                LastSkillOutput = LastSkillOutput,
                LastCharacterUsed = LastCharacterUsed,
                UsedCharacters = new List<Character>(UsedCharacters), // 리스트는 새롭게 복사
                                                                      // Dictionary 복제: 기존 Key-Value 쌍을 모두 복사합니다.
                _chuSkillUsedCharacters = new Dictionary<Character, bool>(_chuSkillUsedCharacters),
                ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>(ComboPath) // 경로도 새롭게 복사
            };
        }
    }

    // ComboResultInfo: 완성된 콤보 경로와 관련된 메타 정보를 저장하는 클래스
    private class ComboResultInfo
    {
        public List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)> Path;
        public SkillType StartSkillType; // 이 콤보의 시작 스킬 타입 (No, Bo, Chu 중 하나)
        public bool IsDominantColorCombo; // 넉다운/격퇴/에어본 중 가장 높은 확률의 시작 스킬 타입에 해당하는 콤보인지 여부

        public ComboResultInfo(List<(string, SkillCategory, SkillType, SkillType)> path, SkillType startSkillType, bool isDominantColorCombo)
        {
            Path = path;
            StartSkillType = startSkillType;
            IsDominantColorCombo = isDominantColorCombo;
        }
    }

    private List<ComboResultInfo> _allComboResultInfos; // 모든 발견된 콤보 정보 리스트
    private Dictionary<SkillType, int> _allPartySkillTypeCounts; // 파티원 전체의 No/Bo 스킬 타입별 개수
    private Dictionary<SkillType, int> _startSkillCounts; // 콤보 시작 스킬 타입별 개수

    private void Start()
    {
        TestComboCalculation();
    }

    /// <summary>
    /// 파티 조합으로 만들 수 있는 모든 가능한 콤보를 계산하고,
    /// 결과를 문자열로 포맷하여 반환합니다. 콘솔에는 직접 출력하지 않습니다.
    /// </summary>
    /// <returns>콤보 계산 결과가 포함된 포맷된 문자열</returns>
    public string CalculateAllCombosAndGetOutput()
    {
        _allComboResultInfos = new List<ComboResultInfo>();
        _allPartySkillTypeCounts = new Dictionary<SkillType, int>();
        _startSkillCounts = new Dictionary<SkillType, int>();

        // --- 파티원들의 No 스킬 및 Bo 스킬 중 특정 타입을 미리 카운팅하는 로직 ---
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

        // 파티원 목록을 순회하며 각 캐릭터를 시작점으로 콤보 탐색을 시작합니다. (최대 5명)
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4); // 5번째 캐릭터는 지원 장수로 가정합니다.

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

            // Counterattack 스킬은 No 스킬로 시작 가능
            if (startChar.NoSkillType == SkillType.Counterattack)
            {
                canNoSkillStartCombo = true;
            }

            if (canNoSkillStartCombo)
            {
                SkillType effectiveStartSkillType = startChar.NoSkillType;

                // 콤보 시작 확률 집계
                if (effectiveStartSkillType != SkillType.None) // None이 아닌 경우만 집계
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

                // 초기 Chu 스킬 사용 여부 설정: No 스킬로 시작했으므로 아직 Chu 스킬은 사용 안 함.
                // 단, Counterattack은 Chu 스킬이므로 사용했다고 기록
                if (startChar.NoSkillType == SkillType.Counterattack)
                {
                    initialState._chuSkillUsedCharacters[startChar] = true;
                    initialState.LastSkillOutput = SkillType.None; // Counterattack은 특별한 출력 없이 연계 가능하도록 None으로 설정
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }
                else
                {
                    initialState._chuSkillUsedCharacters[startChar] = false;
                    initialState.LastSkillOutput = startChar.NoSkillType;
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }

                // 재귀 탐색 시작, 현재 콤보의 시작 스킬 타입을 명시적으로 전달합니다.
                ExploreCombo(initialState, effectiveStartSkillType);
            }

            // 2. 보(Bo) 스킬로 콤보 시작 시도 (지원 장수는 Bo 스킬로 시작 불가)
            if (startChar.BoSkillType != SkillType.None && !isSupportCharacter)
            {
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
                initialState._chuSkillUsedCharacters[startChar] = false; // Bo 스킬로 시작했으므로 아직 Chu 스킬은 사용 안 함
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Bo, SkillType.None, startChar.BoSkillType));

                // 재귀 탐색 시작
                ExploreCombo(initialState, startChar.BoSkillType);
            }
        }

        // --- 확률 계산 후 가장 높은 확률의 시작 스킬 타입 결정 ---
        SkillType dominantStartSkillType = SkillType.None;
        float maxProbability = -1f;
        int totalStartSkills = _startSkillCounts.Values.Sum();

        if (totalStartSkills > 0)
        {
            // 넉다운 확률 계산 및 비교
            if (_startSkillCounts.ContainsKey(SkillType.Knockdown))
            {
                float prob = (float)_startSkillCounts[SkillType.Knockdown] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Knockdown; }
            }
            // 격퇴 확률 계산 및 비교
            if (_startSkillCounts.ContainsKey(SkillType.Repel))
            {
                float prob = (float)_startSkillCounts[SkillType.Repel] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Repel; }
            }
            // 에어본 확률 계산 및 비교
            if (_startSkillCounts.ContainsKey(SkillType.Airborne))
            {
                float prob = (float)_startSkillCounts[SkillType.Airborne] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Airborne; }
            }
        }

        // 모든 콤보 결과에 IsDominantColorCombo 플래그 설정
        foreach (var comboInfo in _allComboResultInfos)
        {
            // 넉다운, 격퇴, 에어본 타입만 색상 대상입니다.
            if (comboInfo.StartSkillType == SkillType.Knockdown ||
                comboInfo.StartSkillType == SkillType.Repel ||
                comboInfo.StartSkillType == SkillType.Airborne)
            {
                if (comboInfo.StartSkillType == dominantStartSkillType)
                {
                    comboInfo.IsDominantColorCombo = true;
                }
                else
                {
                    comboInfo.IsDominantColorCombo = false;
                }
            }
            else // 색상 대상이 아닌 스킬 타입은 무조건 false로 설정
            {
                comboInfo.IsDominantColorCombo = false;
            }
        }

        // 포맷된 문자열 결과를 반환합니다.
        return GetFormattedComboOutput();
    }

    /// <summary>
    /// 콤보를 재귀적으로 탐색하는 메서드입니다.
    /// </summary>
    /// <param name="currentState">현재 콤보 탐색 상태</param>
    /// <param name="initialStartSkillType">이 콤보의 시작 스킬 타입 (초기 호출 시 전달된 값)</param>
    private void ExploreCombo(ComboState currentState, SkillType initialStartSkillType)
    {
        // 콤보 최대 길이에 도달하면 현재 콤보 경로를 저장하고 반환합니다.
        if (currentState.CurrentComboCount >= maxComboLength)
        {
            // 콤보 정보를 ComboResultInfo 객체로 만들어 저장합니다.
            _allComboResultInfos.Add(new ComboResultInfo(currentState.ComboPath, initialStartSkillType, false));
            return;
        }

        bool canContinueCombo = false; // 콤보를 더 이어나갈 수 있는지 여부

        // 파티 멤버를 순회하며 다음 콤보 연계를 시도합니다.
        foreach (Character nextChar in partyMembers)
        {
            // **핵심 변경점:** 다음 캐릭터가 이 콤보 경로에서 이미 Chu 스킬을 사용했는지 확인
            bool hasUsedChuSkill = currentState._chuSkillUsedCharacters.ContainsKey(nextChar) && currentState._chuSkillUsedCharacters[nextChar];

            // --- 추(Chu) 스킬 연계 시도 ---
            // 다음 캐릭터의 ChuConnectableSkillType이 유효하고 Counterattack이 아닌 경우
            if (nextChar.ChuConnectableSkillType != SkillType.None &&
                nextChar.ChuConnectableSkillType != SkillType.Counterattack)
            {
                // 기획 의도: Chu 스킬 연계는 캐릭터가 '이번 콤보 경로 내에서 아직 Chu 스킬을 사용하지 않았을 경우'에만 가능
                if (hasUsedChuSkill)
                {
                    continue; // 이미 Chu 스킬을 사용했으면 이 캐릭터의 Chu 스킬은 더 이상 사용 불가
                }

                bool canConnect = false;

                // Chu 스킬 연계 조건 확인
                if (nextChar.ChuConnectableSkillType == SkillType.All) // 모든 스킬 타입과 연계 가능
                {
                    canConnect = true;
                }
                // 이전 스킬 출력이 Airborne, Knockdown, Repel 중 하나일 때 해당 Chu 스킬과 일치하는지 확인
                else if (currentState.LastSkillOutput == SkillType.Airborne ||
                            currentState.LastSkillOutput == SkillType.Knockdown ||
                            currentState.LastSkillOutput == SkillType.Repel)
                {
                    if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                    {
                        canConnect = true;
                    }
                }
                // 그 외의 경우, 이전 스킬 출력과 Chu 스킬 연계 타입이 정확히 일치하는지 확인
                else if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                {
                    canConnect = true;
                }

                // 연계가 가능하다면 새로운 콤보 상태를 만들고 재귀 탐색을 이어갑니다.
                if (canConnect)
                {
                    canContinueCombo = true;

                    ComboState nextState = currentState.Clone(); // 현재 상태를 복제
                    nextState.CurrentComboCount++; // 콤보 길이 증가
                    nextState.LastSkillOutput = nextChar.Chu2OutputSkillType; // 다음 스킬 출력 설정
                    nextState.LastCharacterUsed = nextChar; // 마지막 사용 캐릭터 업데이트

                    // nextState.UsedCharacters에 현재 nextChar가 없으면 추가 (단순히 등장 여부 기록)
                    if (!nextState.UsedCharacters.Contains(nextChar))
                    {
                        nextState.UsedCharacters.Add(nextChar);
                    }
                    // **핵심 변경: 이 캐릭터가 Chu 스킬을 사용했음을 명시적으로 기록**
                    nextState._chuSkillUsedCharacters[nextChar] = true;

                    // 콤보 경로에 현재 스텝을 추가합니다.
                    nextState.ComboPath.Add((nextChar.Name, SkillCategory.Chu, nextChar.ChuConnectableSkillType, nextChar.Chu2OutputSkillType));

                    // 다음 콤보 스텝을 탐색하기 위해 재귀 호출합니다. 시작 스킬 타입은 계속 전달됩니다.
                    ExploreCombo(nextState, initialStartSkillType);
                }
            }
            // --- 노(No) 또는 보(Bo) 스킬 연계 시도 ---
            // 현재 기획상 콤보 중간에 다른 캐릭터의 No/Bo 스킬로 연계는 없고, Chu 스킬만 연계 가능합니다.
            // 따라서 이 부분은 이전에 삭제되었던 로직 그대로 비워둡니다.
        }

        // 더 이상 콤보를 이어나갈 수 없다면 현재 콤보 경로를 최종 결과 리스트에 추가합니다.
        // 최대 길이에 도달하여 종료되었거나, 더 이상 연결할 수 없는 경우 모두 저장
        if (!canContinueCombo)
        {
            _allComboResultInfos.Add(new ComboResultInfo(currentState.ComboPath, initialStartSkillType, false));
        }
    }

    /// <summary>
    /// 저장된 모든 콤보 경로, 파티원 스킬 통계, 시작 스킬 확률을 문자열로 구성하여 반환합니다.
    /// HTML 컬러 태그를 사용하여 가장 높은 확률의 콤보 라인을 색칠합니다.
    /// </summary>
    /// <returns>포맷된 콤보 결과 문자열</returns>
    private string GetFormattedComboOutput()
    {
        StringBuilder outputBuilder = new StringBuilder(); // 효율적인 문자열 생성을 위해 StringBuilder 사용

        // 콤보 길이에 관계없이 모든 콤보를 표시합니다.
        var allCombos = _allComboResultInfos.OrderByDescending(c => c.Path.Count).ToList();

        outputBuilder.AppendLine("--- 발견된 모든 콤보 경로 ---");
        if (allCombos.Any())
        {
            int comboIndex = 1;

            foreach (var comboInfo in allCombos)
            {
                string comboString = "";
                // 콤보 경로를 문자열로 구성
                for (int i = 0; i < comboInfo.Path.Count; i++)
                {
                    var step = comboInfo.Path[i];
                    string outputSkillAbbr = GetSkillTypeAbbreviation(step.outputSkill); // 출력 스킬 약어

                    comboString += $"{step.characterName}({step.skillCat}";

                    if (step.skillCat == SkillCategory.Chu)
                    {
                        string inputSkillAbbr = GetSkillTypeAbbreviation(step.inputSkill); // 입력 스킬 약어
                        comboString += $":{inputSkillAbbr}->{outputSkillAbbr})";
                    }
                    else
                    {
                        comboString += $":{outputSkillAbbr})";
                    }

                    if (i < comboInfo.Path.Count - 1)
                    {
                        comboString += " -> ";
                    }
                }

                // 가장 높은 확률의 시작 스킬 타입에 해당하는 콤보 라인에 색상 적용
                if (comboInfo.IsDominantColorCombo)
                {
                    string colorHex = SkillTypeColors.GetColorHex(comboInfo.StartSkillType);
                    // TextMeshPro용 HTML 컬러 태그를 사용하여 색상 적용
                    outputBuilder.AppendLine($"<color={colorHex}>콤보 {comboIndex++} (길이: {comboInfo.Path.Count}): {comboString}</color>");
                }
                else
                {
                    // 일반 콤보 라인
                    outputBuilder.AppendLine($"콤보 {comboIndex++} (길이: {comboInfo.Path.Count}): {comboString}");
                }
            }
            outputBuilder.AppendLine($"총 {allCombos.Count}가지 콤보가 발견되었습니다.");

            int maxComboLengthFound = 0;
            if (allCombos.Any())
            {
                maxComboLengthFound = allCombos.First().Path.Count;
            }
            outputBuilder.AppendLine($"최대 콤보 길이: {maxComboLengthFound}");

            // 각 콤보 길이별 개수와 시작 스킬 타입 비율 추가
            for (int i = 1; i <= maxComboLength; i++) // maxComboLength까지 반복
            {
                int comboCount = allCombos.Count(c => c.Path.Count == i);

                // 해당 길이의 콤보가 있을 경우에만 비율 계산
                if (comboCount > 0)
                {
                    // 특정 시작 스킬 타입별 콤보 개수 집계
                    int knockdownStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Knockdown);
                    int repelStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Repel);
                    int airborneStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Airborne);
                    int counterattackStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Counterattack);

                    // 비율 계산 (0으로 나누는 것을 방지)
                    string knockdownRatio = comboCount > 0 ? $"K:{(float)knockdownStarts / comboCount * 100:F1}%" : "K:0.0%";
                    string repelRatio = comboCount > 0 ? $"R:{(float)repelStarts / comboCount * 100:F1}%" : "R:0.0%";
                    string airborneRatio = comboCount > 0 ? $"A:{(float)airborneStarts / comboCount * 100:F1}%" : "A:0.0%";
                    string counterattackRatio = comboCount > 0 ? $"추:{(float)counterattackStarts / comboCount * 100:F1}%" : "추:0.0%";

                    // 결과 문자열에 추가 (약자만 사용)
                    outputBuilder.AppendLine($"{i}콤보 개수: {comboCount}개 ({knockdownRatio}, {repelRatio}, {airborneRatio}, {counterattackRatio})");
                }
                else
                {
                    // 콤보가 없는 길이는 그대로 출력 (혹은 다른 방식으로 변경 가능)
                    outputBuilder.AppendLine($"{i}콤보 개수: {comboCount}개");
                }
            }
        }
        else
        {
            outputBuilder.AppendLine("생성 가능한 콤보가 없습니다.");
        }

        // --- 파티원들의 No 및 Bo 스킬 타입별 총 카운트 통계 ---
        outputBuilder.AppendLine("\n--- 파티원들의 No 및 Bo 스킬 타입별 총 카운트 (None 제외) ---");
        if (_allPartySkillTypeCounts.Any())
        {
            foreach (var entry in _allPartySkillTypeCounts)
            {
                outputBuilder.AppendLine($"- {entry.Key}: {entry.Value}개");
            }
        }
        else
        {
            outputBuilder.AppendLine("파티원 중에 유효한 No 또는 Bo 스킬 타입이 없습니다.");
        }

        // --- 실제 콤보 시작 스킬 타입별 통계 및 발동 확률 ---
        outputBuilder.AppendLine("\n--- 실제 콤보 시작 스킬 타입별 통계 및 발동 확률 ---");
        int totalActualStartSkills = _startSkillCounts.Values.Sum(); // 실제 콤보를 시작한 총 횟수

        if (totalActualStartSkills > 0)
        {
            int knockdownAirborneRepelSum = 0; // 넉다운, 에어본, 격퇴 시작 스킬의 총합

            foreach (var entry in _startSkillCounts)
            {
                float probability = (float)entry.Value / totalActualStartSkills * 100f;
                // 가독성 개선: 넉다운(K), 에어본(A), 격퇴(R), 추격(추)
                string skillNameAbbr = GetSkillTypeAbbreviation(entry.Key);
                outputBuilder.AppendLine($"- {skillNameAbbr} ({entry.Key}): {entry.Value}개 (발동 확률: {probability:F2}%)");

                if (entry.Key == SkillType.Knockdown ||
                    entry.Key == SkillType.Airborne ||
                    entry.Key == SkillType.Repel)
                {
                    knockdownAirborneRepelSum += entry.Value;
                }
            }

            // 총 넉다운 + 에어본 + 격퇴 확률 추가
            if (knockdownAirborneRepelSum > 0)
            {
                float totalProbability = (float)knockdownAirborneRepelSum / totalActualStartSkills * 100f;
                outputBuilder.AppendLine($"\n- K+A+R 총합 (시작 스킬): {knockdownAirborneRepelSum}개 (전체 시작 스킬 대비 확률: {totalProbability:F2}%)");
            }
            else
            {
                outputBuilder.AppendLine("K, A, R 시작 스킬이 없습니다.");
            }
        }
        else
        {
            outputBuilder.AppendLine("실제로 시작 가능한 콤보 스킬이 없습니다.");
        }

        return outputBuilder.ToString(); // 완성된 문자열 반환
    }

    /// <summary>
    /// SkillType에 대한 약어 문자열을 반환합니다.
    /// </summary>
    private string GetSkillTypeAbbreviation(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Knockdown: return "K";
            case SkillType.Airborne: return "A";
            case SkillType.Repel: return "R";
            case SkillType.Counterattack: return "추"; // 추격
            default: return skillType.ToString(); // 그 외 스킬 타입은 전체 이름 반환
        }
    }


    // 디버그용 ContextMenu: UIManager 없이 테스트하고 싶을 때 사용
    [ContextMenu("Calculate All Combos (Debug)")]
    public void TestComboCalculation()
    {
        if (partyMembers.Count == 0)
        {
            Debug.LogWarning("파티 멤버가 선택되지 않았습니다. UIManager에서 캐릭터를 선택하거나, Debug 메뉴를 위해 partyMembers를 수동으로 채워주세요.");

            // 디버그용으로 임시 파티원 설정 예시 (필요시 주석 해제하여 사용)
            // Character char1 = GoogleSheetManager.instance.characterSo.GetCharacterByName("조운");
            // Character char2 = GoogleSheetManager.instance.characterSo.GetCharacterByName("관우");
            // Character char3 = GoogleSheetManager.instance.characterSo.GetCharacterByName("마초");
            // Character char4 = GoogleSheetManager.instance.characterSo.GetCharacterByName("황충");
            // Character char5 = GoogleSheetManager.instance.characterSo.GetCharacterByName("화타");
            // partyMembers.Clear();
            // if (char1 != null) partyMembers.Add(char1);
            // if (char2 != null) partyMembers.Add(char2);
            // if (char3 != null) partyMembers.Add(char3);
            // if (char4 != null) partyMembers.Add(char4);
            // if (char5 != null) partyMembers.Add(char5);
            // Debug.Log($"디버그용 파티 멤버 로드 완료. 현재 파티원 수: {partyMembers.Count}");
        }

        Debug.Log($"파티 멤버 준비 완료. 현재 파티원 수: {partyMembers.Count}");

        // 결과를 문자열로 받아 콘솔에 출력 (디버그용)
        string output = CalculateAllCombosAndGetOutput();
        Debug.Log(output);
    }
}