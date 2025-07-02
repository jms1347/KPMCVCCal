using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// SkillType, NationType, SkillCategory, Character Ŭ������ ������Ʈ�� �°� ���ǵǾ� �־�� �մϴ�.

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
    // �޺� ���� ��ų Ÿ�Ժ� ������ ������ Dictionary
    private Dictionary<SkillType, int> _startSkillCounts;

    private void Start()
    {
        TestComboCalculation();
    }

    /// <summary>
    /// ��Ƽ �������� ���� �� �ִ� ��� ������ �޺��� ����ϰ� �ֿܼ� ����մϴ�.
    /// ����, ��Ƽ������ ���� Ư�� No ��ų �� Bo ��ų�� ������ ������ �����ϰ�,
    /// �޺� ���� ��ų�� ��� �� Ȯ���� ����մϴ�.
    /// </summary>
    public void CalculateAllCombosAndPrint()
    {
        _allComboPaths = new List<List<(string, SkillCategory, SkillType, SkillType)>>();
        _allPartySkillTypeCounts = new Dictionary<SkillType, int>();
        _startSkillCounts = new Dictionary<SkillType, int>(); // �޺� ���� ��ų ī��Ʈ Dictionary �ʱ�ȭ

        // --- ��Ƽ������ No ��ų �� Bo ��ų �� Ư�� Ÿ���� �̸� ī�����ϴ� ���� (���� ����) ---
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

        // ��Ƽ�� ����� ��ȸ�ϸ� �� ĳ���͸� ���������� �޺� Ž��
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4);

            // 1. ��(No) ��ų�� �޺� ���� �õ�
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
                // No ��ų�� �޺� ���� ��ų�� ���� ��� ī��Ʈ
                SkillType effectiveStartSkillType = startChar.NoSkillType;
                // Counterattack�� None���� ����Ͽ� �ߵ� Ȯ�� ��꿡�� ������ �� �ֵ���
                if (effectiveStartSkillType == SkillType.Counterattack)
                {
                    // No ��ų Counterattack�� �޺� ������ ���������� Ȯ�� ���迡���� ���� (��ȹ �ǵ��� ���� ���� ����)
                    // ������ Ȯ�� ��꿡 �������� �ʰ� �Ѿ�ڽ��ϴ�.
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

            // 2. ��(Bo) ��ų�� �޺� ���� �õ� (���� ����� Bo ��ų�� ���� �Ұ�)
            if (startChar.BoSkillType != SkillType.None && !isSupportCharacter)
            {
                // Bo ��ų�� �޺� ���� ��ų�� ���� ��� ī��Ʈ
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

            // 3. ��(Chu) ��ų�� Counterattack�� �� �޺� ���� ��ų�� ����
            if (startChar.ChuConnectableSkillType == SkillType.Counterattack)
            {
                // Chu Counterattack�� �޺� ���� ��ų�� ���� ��� ī��Ʈ
                // (�Ϲ����� �˴ٿ�, ���, ����ʹ� �ٸ��� Counterattack�� �� ��ü�� ���� ��ų�̹Ƿ�,
                // ���⼭�� outputSkill�� Chu2OutputSkillType�� �ƴ�, Counterattack ��ü�� ī��Ʈ)
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
    /// ����� ��� �޺� ��θ� �ֿܼ� �� �ٷ� ����ϰ�, ��Ƽ������ ���� No ��ų �� Bo ��ų ��踦 ǥ���մϴ�.
    /// ����, �޺� ���� ��ų�� ��� �� Ȯ���� ����Ͽ� ����մϴ�.
    /// </summary>
    private void PrintAllCombos()
    {
        Debug.Log("--- ��� ������ �޺� ��� ---");
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
                Debug.Log($"�޺� {comboIndex++} (����: {combo.Count}): {comboString}");
            }
            Debug.Log($"�� {validCombos.Count}���� �޺��� �߰ߵǾ����ϴ�.");

            int maxComboLength = 0;
            if (validCombos.Any())
            {
                maxComboLength = validCombos.First().Count;
            }
            Debug.Log($"�ִ� �޺� ����: {maxComboLength}");
        }
        else
        {
            Debug.Log("���� ������ �޺��� �����ϴ�.");
        }

        // --- �޺� ������ �����ϰ� ��Ƽ������ ��ü No/Bo ��ų Ÿ�Ժ� ī��Ʈ ��� ---
        Debug.Log("--- ��Ƽ������ No �� Bo ��ų Ÿ�Ժ� �� ī��Ʈ (None ����) ---");
        if (_allPartySkillTypeCounts.Any())
        {
            foreach (var entry in _allPartySkillTypeCounts)
            {
                Debug.Log($"- {entry.Key}: {entry.Value}��");
            }
        }
        else
        {
            Debug.Log("��Ƽ�� �߿� ��ȿ�� No �Ǵ� Bo ��ų Ÿ���� �����ϴ�.");
        }

        // --- �޺� ���� ��ų ��� �� Ȯ�� ��� ---
        Debug.Log("--- �޺� ���� ��ų Ÿ�Ժ� ��� �� �ߵ� Ȯ�� ---");
        int totalStartSkills = _startSkillCounts.Values.Sum(); // ��� ���� ��ų�� ����

        if (totalStartSkills > 0)
        {
            // �˴ٿ�, ���, ������ �հ踦 ���� ���� �ʱ�ȭ
            int knockdownAirborneRepelSum = 0;

            foreach (var entry in _startSkillCounts)
            {
                float probability = (float)entry.Value / totalStartSkills * 100f;
                Debug.Log($"- {entry.Key}: {entry.Value}�� (Ȯ��: {probability:F2}%)");

                // �˴ٿ�, ���, ������ �հ� ���
                if (entry.Key == SkillType.Knockdown ||
                    entry.Key == SkillType.Airborne ||
                    entry.Key == SkillType.Repel)
                {
                    knockdownAirborneRepelSum += entry.Value;
                }
            }

            // �˴ٿ� + ��� + ������ ���� �� Ȯ�� ���
            if (knockdownAirborneRepelSum > 0)
            {
                float totalProbability = (float)knockdownAirborneRepelSum / totalStartSkills * 100f;
                Debug.Log($"\n- �˴ٿ� + ��� + ���� ����: {knockdownAirborneRepelSum}�� (��ü ���� ��ų ��� Ȯ��: {totalProbability:F2}%)");
            }
            else
            {
                Debug.Log("�˴ٿ�, ���, ���� ���� ��ų�� �����ϴ�.");
            }
        }
        else
        {
            Debug.Log("���� ������ �޺� ��ų�� �����ϴ�.");
        }
        // ----------------------------------------------------
    }

    // �� TestComboCalculation �Լ��� ����� ��û�� ���� �� �̻� �������� �ʽ��ϴ�.
    [ContextMenu("Calculate All Combos")]
    public void TestComboCalculation()
    {
        partyMembers.Clear();

        // GoogleSheetManager�� ���� ĳ���� �����͸� �ҷ��ɴϴ�.
        Character char1 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
        Character char2 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
        Character char3 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
        Character char4 = GoogleSheetManager.instance.characterSo.GetCharacterByName("Ȳ��");
        Character char5 = GoogleSheetManager.instance.characterSo.GetCharacterByName("ȭŸ");

        if (char1 != null) partyMembers.Add(char1);
        if (char2 != null) partyMembers.Add(char2);
        if (char3 != null) partyMembers.Add(char3);
        if (char4 != null) partyMembers.Add(char4);
        if (char5 != null) partyMembers.Add(char5);

        Debug.Log($"��Ƽ ��� �ε� �Ϸ�. ���� ��Ƽ�� ��: {partyMembers.Count}");

        CalculateAllCombosAndPrint();
    }
}