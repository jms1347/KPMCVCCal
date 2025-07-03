using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

// SkillType, NationType, SkillCategory, Character Ŭ������ ������Ʈ�� �°� ���ǵǾ� �־�� �մϴ�.
// (�� ���� ���� ������, ������Ʈ�� �̹� �ִٰ� �����մϴ�.)

public class ComboCalController : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>();
    public int maxComboLength = 6; // �޺��� �ִ� ���̸� �����մϴ�.

    // ComboState: ���� �޺� Ž���� ���¸� �����ϴ� ���� Ŭ����
    private class ComboState
    {
        public int CurrentComboCount = 0; // ���� �޺� ����
        public SkillType LastSkillOutput = SkillType.None; // ���� ��ų�� ��� Ÿ��
        public Character LastCharacterUsed = null; // ���������� ���� ĳ����
        public List<Character> UsedCharacters = new List<Character>(); // �޺� ��ο� ���� ĳ���� ��� (�ܼ� ��Ͽ�)
        // �� ĳ���Ͱ� �̹� �޺����� Chu ��ų�� ����ߴ��� ���θ� ����
        public Dictionary<Character, bool> _chuSkillUsedCharacters = new Dictionary<Character, bool>();
        // ��������� �޺� ���: (ĳ���� �̸�, ��ų ī�װ�, �Է� ��ų Ÿ��, ��� ��ų Ÿ��)
        public List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)> ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>();

        // ComboState�� �����ϴ� �޼��� (���ο� �޺� ��� Ž�� �� ���)
        public ComboState Clone()
        {
            return new ComboState
            {
                CurrentComboCount = CurrentComboCount,
                LastSkillOutput = LastSkillOutput,
                LastCharacterUsed = LastCharacterUsed,
                UsedCharacters = new List<Character>(UsedCharacters), // ����Ʈ�� ���Ӱ� ����
                                                                      // Dictionary ����: ���� Key-Value ���� ��� �����մϴ�.
                _chuSkillUsedCharacters = new Dictionary<Character, bool>(_chuSkillUsedCharacters),
                ComboPath = new List<(string, SkillCategory, SkillType, SkillType)>(ComboPath) // ��ε� ���Ӱ� ����
            };
        }
    }

    // ComboResultInfo: �ϼ��� �޺� ��ο� ���õ� ��Ÿ ������ �����ϴ� Ŭ����
    private class ComboResultInfo
    {
        public List<(string characterName, SkillCategory skillCat, SkillType inputSkill, SkillType outputSkill)> Path;
        public SkillType StartSkillType; // �� �޺��� ���� ��ų Ÿ�� (No, Bo, Chu �� �ϳ�)
        public bool IsDominantColorCombo; // �˴ٿ�/����/��� �� ���� ���� Ȯ���� ���� ��ų Ÿ�Կ� �ش��ϴ� �޺����� ����

        public ComboResultInfo(List<(string, SkillCategory, SkillType, SkillType)> path, SkillType startSkillType, bool isDominantColorCombo)
        {
            Path = path;
            StartSkillType = startSkillType;
            IsDominantColorCombo = isDominantColorCombo;
        }
    }

    private List<ComboResultInfo> _allComboResultInfos; // ��� �߰ߵ� �޺� ���� ����Ʈ
    private Dictionary<SkillType, int> _allPartySkillTypeCounts; // ��Ƽ�� ��ü�� No/Bo ��ų Ÿ�Ժ� ����
    private Dictionary<SkillType, int> _startSkillCounts; // �޺� ���� ��ų Ÿ�Ժ� ����

    private void Start()
    {
        TestComboCalculation();
    }

    /// <summary>
    /// ��Ƽ �������� ���� �� �ִ� ��� ������ �޺��� ����ϰ�,
    /// ����� ���ڿ��� �����Ͽ� ��ȯ�մϴ�. �ֿܼ��� ���� ������� �ʽ��ϴ�.
    /// </summary>
    /// <returns>�޺� ��� ����� ���Ե� ���˵� ���ڿ�</returns>
    public string CalculateAllCombosAndGetOutput()
    {
        _allComboResultInfos = new List<ComboResultInfo>();
        _allPartySkillTypeCounts = new Dictionary<SkillType, int>();
        _startSkillCounts = new Dictionary<SkillType, int>();

        // --- ��Ƽ������ No ��ų �� Bo ��ų �� Ư�� Ÿ���� �̸� ī�����ϴ� ���� ---
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

        // ��Ƽ�� ����� ��ȸ�ϸ� �� ĳ���͸� ���������� �޺� Ž���� �����մϴ�. (�ִ� 5��)
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4); // 5��° ĳ���ʹ� ���� ����� �����մϴ�.

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

            // Counterattack ��ų�� No ��ų�� ���� ����
            if (startChar.NoSkillType == SkillType.Counterattack)
            {
                canNoSkillStartCombo = true;
            }

            if (canNoSkillStartCombo)
            {
                SkillType effectiveStartSkillType = startChar.NoSkillType;

                // �޺� ���� Ȯ�� ����
                if (effectiveStartSkillType != SkillType.None) // None�� �ƴ� ��츸 ����
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

                // �ʱ� Chu ��ų ��� ���� ����: No ��ų�� ���������Ƿ� ���� Chu ��ų�� ��� �� ��.
                // ��, Counterattack�� Chu ��ų�̹Ƿ� ����ߴٰ� ���
                if (startChar.NoSkillType == SkillType.Counterattack)
                {
                    initialState._chuSkillUsedCharacters[startChar] = true;
                    initialState.LastSkillOutput = SkillType.None; // Counterattack�� Ư���� ��� ���� ���� �����ϵ��� None���� ����
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }
                else
                {
                    initialState._chuSkillUsedCharacters[startChar] = false;
                    initialState.LastSkillOutput = startChar.NoSkillType;
                    initialState.ComboPath.Add((startChar.Name, SkillCategory.No, SkillType.None, startChar.NoSkillType));
                }

                // ��� Ž�� ����, ���� �޺��� ���� ��ų Ÿ���� ��������� �����մϴ�.
                ExploreCombo(initialState, effectiveStartSkillType);
            }

            // 2. ��(Bo) ��ų�� �޺� ���� �õ� (���� ����� Bo ��ų�� ���� �Ұ�)
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
                initialState._chuSkillUsedCharacters[startChar] = false; // Bo ��ų�� ���������Ƿ� ���� Chu ��ų�� ��� �� ��
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Bo, SkillType.None, startChar.BoSkillType));

                // ��� Ž�� ����
                ExploreCombo(initialState, startChar.BoSkillType);
            }
        }

        // --- Ȯ�� ��� �� ���� ���� Ȯ���� ���� ��ų Ÿ�� ���� ---
        SkillType dominantStartSkillType = SkillType.None;
        float maxProbability = -1f;
        int totalStartSkills = _startSkillCounts.Values.Sum();

        if (totalStartSkills > 0)
        {
            // �˴ٿ� Ȯ�� ��� �� ��
            if (_startSkillCounts.ContainsKey(SkillType.Knockdown))
            {
                float prob = (float)_startSkillCounts[SkillType.Knockdown] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Knockdown; }
            }
            // ���� Ȯ�� ��� �� ��
            if (_startSkillCounts.ContainsKey(SkillType.Repel))
            {
                float prob = (float)_startSkillCounts[SkillType.Repel] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Repel; }
            }
            // ��� Ȯ�� ��� �� ��
            if (_startSkillCounts.ContainsKey(SkillType.Airborne))
            {
                float prob = (float)_startSkillCounts[SkillType.Airborne] / totalStartSkills;
                if (prob > maxProbability) { maxProbability = prob; dominantStartSkillType = SkillType.Airborne; }
            }
        }

        // ��� �޺� ����� IsDominantColorCombo �÷��� ����
        foreach (var comboInfo in _allComboResultInfos)
        {
            // �˴ٿ�, ����, ��� Ÿ�Ը� ���� ����Դϴ�.
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
            else // ���� ����� �ƴ� ��ų Ÿ���� ������ false�� ����
            {
                comboInfo.IsDominantColorCombo = false;
            }
        }

        // ���˵� ���ڿ� ����� ��ȯ�մϴ�.
        return GetFormattedComboOutput();
    }

    /// <summary>
    /// �޺��� ��������� Ž���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="currentState">���� �޺� Ž�� ����</param>
    /// <param name="initialStartSkillType">�� �޺��� ���� ��ų Ÿ�� (�ʱ� ȣ�� �� ���޵� ��)</param>
    private void ExploreCombo(ComboState currentState, SkillType initialStartSkillType)
    {
        // �޺� �ִ� ���̿� �����ϸ� ���� �޺� ��θ� �����ϰ� ��ȯ�մϴ�.
        if (currentState.CurrentComboCount >= maxComboLength)
        {
            // �޺� ������ ComboResultInfo ��ü�� ����� �����մϴ�.
            _allComboResultInfos.Add(new ComboResultInfo(currentState.ComboPath, initialStartSkillType, false));
            return;
        }

        bool canContinueCombo = false; // �޺��� �� �̾�� �� �ִ��� ����

        // ��Ƽ ����� ��ȸ�ϸ� ���� �޺� ���踦 �õ��մϴ�.
        foreach (Character nextChar in partyMembers)
        {
            // **�ٽ� ������:** ���� ĳ���Ͱ� �� �޺� ��ο��� �̹� Chu ��ų�� ����ߴ��� Ȯ��
            bool hasUsedChuSkill = currentState._chuSkillUsedCharacters.ContainsKey(nextChar) && currentState._chuSkillUsedCharacters[nextChar];

            // --- ��(Chu) ��ų ���� �õ� ---
            // ���� ĳ������ ChuConnectableSkillType�� ��ȿ�ϰ� Counterattack�� �ƴ� ���
            if (nextChar.ChuConnectableSkillType != SkillType.None &&
                nextChar.ChuConnectableSkillType != SkillType.Counterattack)
            {
                // ��ȹ �ǵ�: Chu ��ų ����� ĳ���Ͱ� '�̹� �޺� ��� ������ ���� Chu ��ų�� ������� �ʾ��� ���'���� ����
                if (hasUsedChuSkill)
                {
                    continue; // �̹� Chu ��ų�� ��������� �� ĳ������ Chu ��ų�� �� �̻� ��� �Ұ�
                }

                bool canConnect = false;

                // Chu ��ų ���� ���� Ȯ��
                if (nextChar.ChuConnectableSkillType == SkillType.All) // ��� ��ų Ÿ�԰� ���� ����
                {
                    canConnect = true;
                }
                // ���� ��ų ����� Airborne, Knockdown, Repel �� �ϳ��� �� �ش� Chu ��ų�� ��ġ�ϴ��� Ȯ��
                else if (currentState.LastSkillOutput == SkillType.Airborne ||
                            currentState.LastSkillOutput == SkillType.Knockdown ||
                            currentState.LastSkillOutput == SkillType.Repel)
                {
                    if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                    {
                        canConnect = true;
                    }
                }
                // �� ���� ���, ���� ��ų ��°� Chu ��ų ���� Ÿ���� ��Ȯ�� ��ġ�ϴ��� Ȯ��
                else if (nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
                {
                    canConnect = true;
                }

                // ���谡 �����ϴٸ� ���ο� �޺� ���¸� ����� ��� Ž���� �̾�ϴ�.
                if (canConnect)
                {
                    canContinueCombo = true;

                    ComboState nextState = currentState.Clone(); // ���� ���¸� ����
                    nextState.CurrentComboCount++; // �޺� ���� ����
                    nextState.LastSkillOutput = nextChar.Chu2OutputSkillType; // ���� ��ų ��� ����
                    nextState.LastCharacterUsed = nextChar; // ������ ��� ĳ���� ������Ʈ

                    // nextState.UsedCharacters�� ���� nextChar�� ������ �߰� (�ܼ��� ���� ���� ���)
                    if (!nextState.UsedCharacters.Contains(nextChar))
                    {
                        nextState.UsedCharacters.Add(nextChar);
                    }
                    // **�ٽ� ����: �� ĳ���Ͱ� Chu ��ų�� ��������� ��������� ���**
                    nextState._chuSkillUsedCharacters[nextChar] = true;

                    // �޺� ��ο� ���� ������ �߰��մϴ�.
                    nextState.ComboPath.Add((nextChar.Name, SkillCategory.Chu, nextChar.ChuConnectableSkillType, nextChar.Chu2OutputSkillType));

                    // ���� �޺� ������ Ž���ϱ� ���� ��� ȣ���մϴ�. ���� ��ų Ÿ���� ��� ���޵˴ϴ�.
                    ExploreCombo(nextState, initialStartSkillType);
                }
            }
            // --- ��(No) �Ǵ� ��(Bo) ��ų ���� �õ� ---
            // ���� ��ȹ�� �޺� �߰��� �ٸ� ĳ������ No/Bo ��ų�� ����� ����, Chu ��ų�� ���� �����մϴ�.
            // ���� �� �κ��� ������ �����Ǿ��� ���� �״�� ����Ӵϴ�.
        }

        // �� �̻� �޺��� �̾�� �� ���ٸ� ���� �޺� ��θ� ���� ��� ����Ʈ�� �߰��մϴ�.
        // �ִ� ���̿� �����Ͽ� ����Ǿ��ų�, �� �̻� ������ �� ���� ��� ��� ����
        if (!canContinueCombo)
        {
            _allComboResultInfos.Add(new ComboResultInfo(currentState.ComboPath, initialStartSkillType, false));
        }
    }

    /// <summary>
    /// ����� ��� �޺� ���, ��Ƽ�� ��ų ���, ���� ��ų Ȯ���� ���ڿ��� �����Ͽ� ��ȯ�մϴ�.
    /// HTML �÷� �±׸� ����Ͽ� ���� ���� Ȯ���� �޺� ������ ��ĥ�մϴ�.
    /// </summary>
    /// <returns>���˵� �޺� ��� ���ڿ�</returns>
    private string GetFormattedComboOutput()
    {
        StringBuilder outputBuilder = new StringBuilder(); // ȿ������ ���ڿ� ������ ���� StringBuilder ���

        // �޺� ���̿� ������� ��� �޺��� ǥ���մϴ�.
        var allCombos = _allComboResultInfos.OrderByDescending(c => c.Path.Count).ToList();

        outputBuilder.AppendLine("--- �߰ߵ� ��� �޺� ��� ---");
        if (allCombos.Any())
        {
            int comboIndex = 1;

            foreach (var comboInfo in allCombos)
            {
                string comboString = "";
                // �޺� ��θ� ���ڿ��� ����
                for (int i = 0; i < comboInfo.Path.Count; i++)
                {
                    var step = comboInfo.Path[i];
                    string outputSkillAbbr = GetSkillTypeAbbreviation(step.outputSkill); // ��� ��ų ���

                    comboString += $"{step.characterName}({step.skillCat}";

                    if (step.skillCat == SkillCategory.Chu)
                    {
                        string inputSkillAbbr = GetSkillTypeAbbreviation(step.inputSkill); // �Է� ��ų ���
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

                // ���� ���� Ȯ���� ���� ��ų Ÿ�Կ� �ش��ϴ� �޺� ���ο� ���� ����
                if (comboInfo.IsDominantColorCombo)
                {
                    string colorHex = SkillTypeColors.GetColorHex(comboInfo.StartSkillType);
                    // TextMeshPro�� HTML �÷� �±׸� ����Ͽ� ���� ����
                    outputBuilder.AppendLine($"<color={colorHex}>�޺� {comboIndex++} (����: {comboInfo.Path.Count}): {comboString}</color>");
                }
                else
                {
                    // �Ϲ� �޺� ����
                    outputBuilder.AppendLine($"�޺� {comboIndex++} (����: {comboInfo.Path.Count}): {comboString}");
                }
            }
            outputBuilder.AppendLine($"�� {allCombos.Count}���� �޺��� �߰ߵǾ����ϴ�.");

            int maxComboLengthFound = 0;
            if (allCombos.Any())
            {
                maxComboLengthFound = allCombos.First().Path.Count;
            }
            outputBuilder.AppendLine($"�ִ� �޺� ����: {maxComboLengthFound}");

            // �� �޺� ���̺� ������ ���� ��ų Ÿ�� ���� �߰�
            for (int i = 1; i <= maxComboLength; i++) // maxComboLength���� �ݺ�
            {
                int comboCount = allCombos.Count(c => c.Path.Count == i);

                // �ش� ������ �޺��� ���� ��쿡�� ���� ���
                if (comboCount > 0)
                {
                    // Ư�� ���� ��ų Ÿ�Ժ� �޺� ���� ����
                    int knockdownStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Knockdown);
                    int repelStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Repel);
                    int airborneStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Airborne);
                    int counterattackStarts = allCombos.Count(c => c.Path.Count == i && c.StartSkillType == SkillType.Counterattack);

                    // ���� ��� (0���� ������ ���� ����)
                    string knockdownRatio = comboCount > 0 ? $"K:{(float)knockdownStarts / comboCount * 100:F1}%" : "K:0.0%";
                    string repelRatio = comboCount > 0 ? $"R:{(float)repelStarts / comboCount * 100:F1}%" : "R:0.0%";
                    string airborneRatio = comboCount > 0 ? $"A:{(float)airborneStarts / comboCount * 100:F1}%" : "A:0.0%";
                    string counterattackRatio = comboCount > 0 ? $"��:{(float)counterattackStarts / comboCount * 100:F1}%" : "��:0.0%";

                    // ��� ���ڿ��� �߰� (���ڸ� ���)
                    outputBuilder.AppendLine($"{i}�޺� ����: {comboCount}�� ({knockdownRatio}, {repelRatio}, {airborneRatio}, {counterattackRatio})");
                }
                else
                {
                    // �޺��� ���� ���̴� �״�� ��� (Ȥ�� �ٸ� ������� ���� ����)
                    outputBuilder.AppendLine($"{i}�޺� ����: {comboCount}��");
                }
            }
        }
        else
        {
            outputBuilder.AppendLine("���� ������ �޺��� �����ϴ�.");
        }

        // --- ��Ƽ������ No �� Bo ��ų Ÿ�Ժ� �� ī��Ʈ ��� ---
        outputBuilder.AppendLine("\n--- ��Ƽ������ No �� Bo ��ų Ÿ�Ժ� �� ī��Ʈ (None ����) ---");
        if (_allPartySkillTypeCounts.Any())
        {
            foreach (var entry in _allPartySkillTypeCounts)
            {
                outputBuilder.AppendLine($"- {entry.Key}: {entry.Value}��");
            }
        }
        else
        {
            outputBuilder.AppendLine("��Ƽ�� �߿� ��ȿ�� No �Ǵ� Bo ��ų Ÿ���� �����ϴ�.");
        }

        // --- ���� �޺� ���� ��ų Ÿ�Ժ� ��� �� �ߵ� Ȯ�� ---
        outputBuilder.AppendLine("\n--- ���� �޺� ���� ��ų Ÿ�Ժ� ��� �� �ߵ� Ȯ�� ---");
        int totalActualStartSkills = _startSkillCounts.Values.Sum(); // ���� �޺��� ������ �� Ƚ��

        if (totalActualStartSkills > 0)
        {
            int knockdownAirborneRepelSum = 0; // �˴ٿ�, ���, ���� ���� ��ų�� ����

            foreach (var entry in _startSkillCounts)
            {
                float probability = (float)entry.Value / totalActualStartSkills * 100f;
                // ������ ����: �˴ٿ�(K), ���(A), ����(R), �߰�(��)
                string skillNameAbbr = GetSkillTypeAbbreviation(entry.Key);
                outputBuilder.AppendLine($"- {skillNameAbbr} ({entry.Key}): {entry.Value}�� (�ߵ� Ȯ��: {probability:F2}%)");

                if (entry.Key == SkillType.Knockdown ||
                    entry.Key == SkillType.Airborne ||
                    entry.Key == SkillType.Repel)
                {
                    knockdownAirborneRepelSum += entry.Value;
                }
            }

            // �� �˴ٿ� + ��� + ���� Ȯ�� �߰�
            if (knockdownAirborneRepelSum > 0)
            {
                float totalProbability = (float)knockdownAirborneRepelSum / totalActualStartSkills * 100f;
                outputBuilder.AppendLine($"\n- K+A+R ���� (���� ��ų): {knockdownAirborneRepelSum}�� (��ü ���� ��ų ��� Ȯ��: {totalProbability:F2}%)");
            }
            else
            {
                outputBuilder.AppendLine("K, A, R ���� ��ų�� �����ϴ�.");
            }
        }
        else
        {
            outputBuilder.AppendLine("������ ���� ������ �޺� ��ų�� �����ϴ�.");
        }

        return outputBuilder.ToString(); // �ϼ��� ���ڿ� ��ȯ
    }

    /// <summary>
    /// SkillType�� ���� ��� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    private string GetSkillTypeAbbreviation(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Knockdown: return "K";
            case SkillType.Airborne: return "A";
            case SkillType.Repel: return "R";
            case SkillType.Counterattack: return "��"; // �߰�
            default: return skillType.ToString(); // �� �� ��ų Ÿ���� ��ü �̸� ��ȯ
        }
    }


    // ����׿� ContextMenu: UIManager ���� �׽�Ʈ�ϰ� ���� �� ���
    [ContextMenu("Calculate All Combos (Debug)")]
    public void TestComboCalculation()
    {
        if (partyMembers.Count == 0)
        {
            Debug.LogWarning("��Ƽ ����� ���õ��� �ʾҽ��ϴ�. UIManager���� ĳ���͸� �����ϰų�, Debug �޴��� ���� partyMembers�� �������� ä���ּ���.");

            // ����׿����� �ӽ� ��Ƽ�� ���� ���� (�ʿ�� �ּ� �����Ͽ� ���)
            // Character char1 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
            // Character char2 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
            // Character char3 = GoogleSheetManager.instance.characterSo.GetCharacterByName("����");
            // Character char4 = GoogleSheetManager.instance.characterSo.GetCharacterByName("Ȳ��");
            // Character char5 = GoogleSheetManager.instance.characterSo.GetCharacterByName("ȭŸ");
            // partyMembers.Clear();
            // if (char1 != null) partyMembers.Add(char1);
            // if (char2 != null) partyMembers.Add(char2);
            // if (char3 != null) partyMembers.Add(char3);
            // if (char4 != null) partyMembers.Add(char4);
            // if (char5 != null) partyMembers.Add(char5);
            // Debug.Log($"����׿� ��Ƽ ��� �ε� �Ϸ�. ���� ��Ƽ�� ��: {partyMembers.Count}");
        }

        Debug.Log($"��Ƽ ��� �غ� �Ϸ�. ���� ��Ƽ�� ��: {partyMembers.Count}");

        // ����� ���ڿ��� �޾� �ֿܼ� ��� (����׿�)
        string output = CalculateAllCombosAndGetOutput();
        Debug.Log(output);
    }
}