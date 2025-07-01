using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NationType
{
    Green = 0,
    Blue = 1,
    Red = 2,
    Black = 3,
    Purple = 4
}

public enum SkillType
{
    None,       // 기본값, 스킬 없음
    Airborne,   // 에어본
    Knockdown,  // 넉다운
    Repel       // 격퇴
}
public enum SkillCategory
{
    No,         // 노 (분노 스킬)
    Bo,         // 보 (평타 연계 스킬)
    Chu,        // 추 (다른 장수 연계기 이어받는 스킬)
    Chu2        // 추2 (추 이후 연계기 넘겨주는 스킬)
}

public class Character
{
    public string Name;
    public NationType nation;
    // 노 스킬 정보
    public SkillType NoSkillType;

    // 보 스킬 정보
    public SkillType BoSkillType;

    // 추 스킬 정보 (다른 장수의 연계기를 이어받아 발동하는 스킬)
    // 이 추 스킬이 발동하기 위한 선행 스킬 타입
    public SkillType ChuConnectableSkillType;

    // 추2 스킬 정보 (추 후에 추2로 넘겨주는 연계기 스킬)
    // '추' 스킬이 발동한 후 이 '추2' 스킬이 어떤 스킬 타입을 출력하는지
    // (기존 ChuOutputSkillType의 역할 + 연계 가능한 스킬 타입까지)
    public SkillType Chu2OutputSkillType;

    // 캐릭터 생성자 (선택 사항이지만 초기화에 용이합니다)
    public Character(string pName)
    {
        Name = pName;
    }

    // 예시: 캐릭터 정보 출력 (디버깅용)
    public void PrintCharacterInfo()
    {
        Debug.Log($"캐릭터 이름: {Name}");
        Debug.Log($"노 스킬: {NoSkillType}");
        Debug.Log($"보 스킬: {BoSkillType}");
        Debug.Log($"추 스킬 연계 가능: {ChuConnectableSkillType}");
        Debug.Log($"추2 스킬 출력: {Chu2OutputSkillType}");
    }
}

public class ComboCalController : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>();

    // 콤보 계산을 위한 내부 클래스
    private class ComboState
    {
        public int CurrentComboCount = 0;
        public SkillType LastSkillOutput = SkillType.None;
        public Character LastCharacterUsed = null;
        public List<Character> UsedCharacters = new List<Character>();
        // 콤보 경로를 기록할 리스트: (캐릭터 이름, 스킬 카테고리, 스킬 출력 타입)
        public List<(string characterName, SkillCategory skillCat, SkillType skillOutput)> ComboPath = new List<(string, SkillCategory, SkillType)>();

        public ComboState Clone()
        {
            return new ComboState
            {
                CurrentComboCount = CurrentComboCount,
                LastSkillOutput = LastSkillOutput,
                LastCharacterUsed = LastCharacterUsed,
                UsedCharacters = new List<Character>(UsedCharacters),
                ComboPath = new List<(string, SkillCategory, SkillType)>(ComboPath) // 경로도 복사
            };
        }
    }

    // 모든 유효한 콤보 경로를 저장할 리스트
    private List<List<(string characterName, SkillCategory skillCat, SkillType skillOutput)>> _allComboPaths;

    private void Start()
    {
        TestComboCalculation(); // Start 시 자동으로 콤보 계산 및 출력
    }

    /// <summary>
    /// 파티 조합으로 만들 수 있는 모든 가능한 콤보를 계산하고 콘솔에 출력합니다.
    /// </summary>
    public void CalculateAllCombosAndPrint()
    {
        _allComboPaths = new List<List<(string, SkillCategory, SkillType)>>(); // 모든 콤보 경로 초기화

        // 파티원 목록을 순회 (최대 5명까지 고려)
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4);

            // 1. 노 스킬로 시작하는 경우
            if (startChar.NoSkillType != SkillType.None)
            {
                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastSkillOutput = startChar.NoSkillType;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.ComboPath.Add((startChar.Name, SkillCategory.No, startChar.NoSkillType)); // 경로 추가

                ExploreCombo(initialState); // 재귀 탐색 시작
            }

            // 2. 보 스킬로 시작하는 경우 (지원 장수는 보 스킬 콤보수에 포함되지 않음)
            if (startChar.BoSkillType != SkillType.None && !isSupportCharacter)
            {
                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastSkillOutput = startChar.BoSkillType;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Bo, startChar.BoSkillType)); // 경로 추가

                ExploreCombo(initialState); // 재귀 탐색 시작
            }
        }

        PrintAllCombos(); // 모든 콤보 출력
    }

    /// <summary>
    /// 재귀적으로 콤보를 탐색하며 모든 가능한 콤보 경로를 저장합니다.
    /// </summary>
    /// <param name="currentState">현재 콤보 상태</param>
    private void ExploreCombo(ComboState currentState)
    {
        // 현재 상태가 유효한 콤보라면 (더 이상 연계할 스킬이 없거나, 모든 경우의 수를 탐색한 마지막 단계)
        // 현재 콤보 경로를 _allComboPaths에 추가합니다.
        // **주의: 동일한 콤보를 중복으로 추가하는 것을 방지하기 위해, 필요시 HashSet<string> 등을 사용하여
        // 콤보 경로를 문자열로 직렬화하여 중복 체크할 수 있습니다. 여기서는 단순 추가합니다.**
        _allComboPaths.Add(currentState.ComboPath);


        foreach (Character nextChar in partyMembers)
        {
            // 이미 사용된 장수는 다시 사용하지 않음 (한 콤보에서 중복 사용 불가)
            if (currentState.UsedCharacters.Contains(nextChar))
            {
                continue;
            }

            // --- 추 스킬 연계 시도 ---
            // 다음 장수의 추 스킬이 현재 LastSkillOutput에 연계 가능한지 확인
            // 추 스킬 연계 가능 타입이 None이 아니면서 현재 스킬 출력과 일치해야 함
            if (nextChar.ChuConnectableSkillType != SkillType.None &&
                nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
            {
                // 넉다운 연계 특별 규칙:
                // 이전 스킬이 넉다운이라면, 다음 추 스킬의 연계 가능 타입도 넉다운이어야 함
                bool isKnockdownRuleMet = true;
                if (currentState.LastSkillOutput == SkillType.Knockdown)
                {
                    if (nextChar.ChuConnectableSkillType != SkillType.Knockdown)
                    {
                        isKnockdownRuleMet = false;
                    }
                }

                if (isKnockdownRuleMet)
                {
                    ComboState nextState = currentState.Clone();
                    nextState.CurrentComboCount++;
                    nextState.LastSkillOutput = nextChar.Chu2OutputSkillType; // '추' 스킬 발동 후, 결과는 Chu2OutputSkillType이 됨
                    nextState.LastCharacterUsed = nextChar;
                    nextState.UsedCharacters.Add(nextChar);
                    // 콤보 경로에 추가 시 스킬 카테고리는 Chu, 출력 타입은 Chu2OutputSkillType으로 기록
                    nextState.ComboPath.Add((nextChar.Name, SkillCategory.Chu, nextChar.Chu2OutputSkillType));

                    ExploreCombo(nextState); // 재귀 호출
                }
            }
        }
    }

    /// <summary>
    /// 저장된 모든 콤보 경로를 콘솔에 한 줄로 출력합니다.
    /// </summary>
    private void PrintAllCombos()
    {
        Debug.Log("--- 모든 가능한 콤보 경로 ---");
        if (_allComboPaths.Any())
        {
            // 콤보 길이에 따라 정렬하여 보기 좋게 출력
            // `.Distinct(new ComboPathComparer())`를 사용하면 완전히 동일한 경로의 중복을 제거할 수 있습니다.
            // 여기서는 경로의 내용만으로 비교하는 것이 아니라, 참조가 다른 객체도 중복 제거할 때 필요합니다.
            // 단순 재귀 탐색 방식으로는 동일한 '결과' 경로가 여러번 생성될 수 있습니다.
            // 하지만 현재는 모든 '탐색된' 경로를 보여주는 데 중점을 둡니다.
            var sortedCombos = _allComboPaths.OrderByDescending(c => c.Count).ToList();

            int comboIndex = 1;
            foreach (var combo in sortedCombos)
            {
                // 각 콤보 경로를 문자열로 조합
                string comboString = "";
                for (int i = 0; i < combo.Count; i++)
                {
                    var step = combo[i];
                    // "캐릭터명(스킬종류:출력타입)" 형식으로 표시
                    comboString += $"{step.characterName}({step.skillCat}:{step.skillOutput})";
                    if (i < combo.Count - 1)
                    {
                        comboString += " -> ";
                    }
                }
                Debug.Log($"콤보 {comboIndex++} (길이: {combo.Count}): {comboString}");
            }
            Debug.Log($"총 {sortedCombos.Count}가지 콤보가 발견되었습니다.");

            // 최대 콤보 길이 찾기
            int maxComboLength = 0;
            if (sortedCombos.Any())
            {
                maxComboLength = sortedCombos.First().Count; // 정렬했으니 첫 번째가 가장 김
            }
            Debug.Log($"최대 콤보 길이: {maxComboLength}");
        }
        else
        {
            Debug.Log("생성 가능한 콤보가 없습니다.");
        }
    }

    // 유니티 에디터에서 테스트를 위한 메서드
    [ContextMenu("Calculate All Combos")]
    public void TestComboCalculation()
    {
        partyMembers.Clear();

        // 주신 캐릭터 데이터 설정
        // 조운 (Green)
        Character char1 = new Character("조운");
        char1.nation = NationType.Green;
        char1.NoSkillType = SkillType.Knockdown;
        char1.BoSkillType = SkillType.Airborne;
        char1.ChuConnectableSkillType = SkillType.Airborne; // 에어본에 연계 가능
        char1.Chu2OutputSkillType = SkillType.Knockdown; // 추 스킬 발동 후 넉다운 출력
        partyMembers.Add(char1);

        // 관우 (Red)
        Character char2 = new Character("관우");
        char2.nation = NationType.Red;
        char2.NoSkillType = SkillType.Airborne;
        char2.BoSkillType = SkillType.Knockdown;
        char2.ChuConnectableSkillType = SkillType.Airborne; // 에어본에 연계 가능
        char2.Chu2OutputSkillType = SkillType.Airborne; // 추 스킬 발동 후 에어본 출력
        partyMembers.Add(char2);

        // 장비 (Blue)
        Character char3 = new Character("장비");
        char3.nation = NationType.Blue;
        char3.NoSkillType = SkillType.Airborne;
        char3.BoSkillType = SkillType.Knockdown;
        char3.ChuConnectableSkillType = SkillType.Knockdown; // 넉다운에 연계 가능
        char3.Chu2OutputSkillType = SkillType.Repel; // 추 스킬 발동 후 격퇴 출력
        partyMembers.Add(char3);

        // 제갈량 (Green)
        Character char4 = new Character("제갈량");
        char4.nation = NationType.Green;
        char4.NoSkillType = SkillType.Airborne;
        char4.BoSkillType = SkillType.Airborne;
        char4.ChuConnectableSkillType = SkillType.Knockdown; // 넉다운에 연계 가능
        char4.Chu2OutputSkillType = SkillType.Airborne; // 추 스킬 발동 후 에어본 출력
        partyMembers.Add(char4);

        // 유비 (Purple) - 5번째 캐릭터 (인덱스 4)이므로 지원 장수로 간주
        Character supportChar = new Character("유비");
        supportChar.nation = NationType.Purple;
        supportChar.NoSkillType = SkillType.Knockdown;
        supportChar.BoSkillType = SkillType.Knockdown; // 보 스킬이 있지만 콤보에 계산 안됨 (로직에서 처리)
        supportChar.ChuConnectableSkillType = SkillType.Repel; // 격퇴에 연계 가능
        supportChar.Chu2OutputSkillType = SkillType.Airborne; // 추 스킬 발동 후 에어본 출력
        partyMembers.Add(supportChar);

        CalculateAllCombosAndPrint(); // 모든 콤보를 계산하고 출력
    }
}