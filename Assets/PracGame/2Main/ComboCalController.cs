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
    None,       // �⺻��, ��ų ����
    Airborne,   // ���
    Knockdown,  // �˴ٿ�
    Repel       // ����
}
public enum SkillCategory
{
    No,         // �� (�г� ��ų)
    Bo,         // �� (��Ÿ ���� ��ų)
    Chu,        // �� (�ٸ� ��� ����� �̾�޴� ��ų)
    Chu2        // ��2 (�� ���� ����� �Ѱ��ִ� ��ų)
}

public class Character
{
    public string Name;
    public NationType nation;
    // �� ��ų ����
    public SkillType NoSkillType;

    // �� ��ų ����
    public SkillType BoSkillType;

    // �� ��ų ���� (�ٸ� ����� ����⸦ �̾�޾� �ߵ��ϴ� ��ų)
    // �� �� ��ų�� �ߵ��ϱ� ���� ���� ��ų Ÿ��
    public SkillType ChuConnectableSkillType;

    // ��2 ��ų ���� (�� �Ŀ� ��2�� �Ѱ��ִ� ����� ��ų)
    // '��' ��ų�� �ߵ��� �� �� '��2' ��ų�� � ��ų Ÿ���� ����ϴ���
    // (���� ChuOutputSkillType�� ���� + ���� ������ ��ų Ÿ�Ա���)
    public SkillType Chu2OutputSkillType;

    // ĳ���� ������ (���� ���������� �ʱ�ȭ�� �����մϴ�)
    public Character(string pName)
    {
        Name = pName;
    }

    // ����: ĳ���� ���� ��� (������)
    public void PrintCharacterInfo()
    {
        Debug.Log($"ĳ���� �̸�: {Name}");
        Debug.Log($"�� ��ų: {NoSkillType}");
        Debug.Log($"�� ��ų: {BoSkillType}");
        Debug.Log($"�� ��ų ���� ����: {ChuConnectableSkillType}");
        Debug.Log($"��2 ��ų ���: {Chu2OutputSkillType}");
    }
}

public class ComboCalController : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>();

    // �޺� ����� ���� ���� Ŭ����
    private class ComboState
    {
        public int CurrentComboCount = 0;
        public SkillType LastSkillOutput = SkillType.None;
        public Character LastCharacterUsed = null;
        public List<Character> UsedCharacters = new List<Character>();
        // �޺� ��θ� ����� ����Ʈ: (ĳ���� �̸�, ��ų ī�װ�, ��ų ��� Ÿ��)
        public List<(string characterName, SkillCategory skillCat, SkillType skillOutput)> ComboPath = new List<(string, SkillCategory, SkillType)>();

        public ComboState Clone()
        {
            return new ComboState
            {
                CurrentComboCount = CurrentComboCount,
                LastSkillOutput = LastSkillOutput,
                LastCharacterUsed = LastCharacterUsed,
                UsedCharacters = new List<Character>(UsedCharacters),
                ComboPath = new List<(string, SkillCategory, SkillType)>(ComboPath) // ��ε� ����
            };
        }
    }

    // ��� ��ȿ�� �޺� ��θ� ������ ����Ʈ
    private List<List<(string characterName, SkillCategory skillCat, SkillType skillOutput)>> _allComboPaths;

    private void Start()
    {
        TestComboCalculation(); // Start �� �ڵ����� �޺� ��� �� ���
    }

    /// <summary>
    /// ��Ƽ �������� ���� �� �ִ� ��� ������ �޺��� ����ϰ� �ֿܼ� ����մϴ�.
    /// </summary>
    public void CalculateAllCombosAndPrint()
    {
        _allComboPaths = new List<List<(string, SkillCategory, SkillType)>>(); // ��� �޺� ��� �ʱ�ȭ

        // ��Ƽ�� ����� ��ȸ (�ִ� 5����� ���)
        for (int i = 0; i < partyMembers.Count && i < 5; i++)
        {
            Character startChar = partyMembers[i];
            bool isSupportCharacter = (i == 4);

            // 1. �� ��ų�� �����ϴ� ���
            if (startChar.NoSkillType != SkillType.None)
            {
                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastSkillOutput = startChar.NoSkillType;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.ComboPath.Add((startChar.Name, SkillCategory.No, startChar.NoSkillType)); // ��� �߰�

                ExploreCombo(initialState); // ��� Ž�� ����
            }

            // 2. �� ��ų�� �����ϴ� ��� (���� ����� �� ��ų �޺����� ���Ե��� ����)
            if (startChar.BoSkillType != SkillType.None && !isSupportCharacter)
            {
                ComboState initialState = new ComboState();
                initialState.CurrentComboCount = 1;
                initialState.LastSkillOutput = startChar.BoSkillType;
                initialState.LastCharacterUsed = startChar;
                initialState.UsedCharacters.Add(startChar);
                initialState.ComboPath.Add((startChar.Name, SkillCategory.Bo, startChar.BoSkillType)); // ��� �߰�

                ExploreCombo(initialState); // ��� Ž�� ����
            }
        }

        PrintAllCombos(); // ��� �޺� ���
    }

    /// <summary>
    /// ��������� �޺��� Ž���ϸ� ��� ������ �޺� ��θ� �����մϴ�.
    /// </summary>
    /// <param name="currentState">���� �޺� ����</param>
    private void ExploreCombo(ComboState currentState)
    {
        // ���� ���°� ��ȿ�� �޺���� (�� �̻� ������ ��ų�� ���ų�, ��� ����� ���� Ž���� ������ �ܰ�)
        // ���� �޺� ��θ� _allComboPaths�� �߰��մϴ�.
        // **����: ������ �޺��� �ߺ����� �߰��ϴ� ���� �����ϱ� ����, �ʿ�� HashSet<string> ���� ����Ͽ�
        // �޺� ��θ� ���ڿ��� ����ȭ�Ͽ� �ߺ� üũ�� �� �ֽ��ϴ�. ���⼭�� �ܼ� �߰��մϴ�.**
        _allComboPaths.Add(currentState.ComboPath);


        foreach (Character nextChar in partyMembers)
        {
            // �̹� ���� ����� �ٽ� ������� ���� (�� �޺����� �ߺ� ��� �Ұ�)
            if (currentState.UsedCharacters.Contains(nextChar))
            {
                continue;
            }

            // --- �� ��ų ���� �õ� ---
            // ���� ����� �� ��ų�� ���� LastSkillOutput�� ���� �������� Ȯ��
            // �� ��ų ���� ���� Ÿ���� None�� �ƴϸ鼭 ���� ��ų ��°� ��ġ�ؾ� ��
            if (nextChar.ChuConnectableSkillType != SkillType.None &&
                nextChar.ChuConnectableSkillType == currentState.LastSkillOutput)
            {
                // �˴ٿ� ���� Ư�� ��Ģ:
                // ���� ��ų�� �˴ٿ��̶��, ���� �� ��ų�� ���� ���� Ÿ�Ե� �˴ٿ��̾�� ��
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
                    nextState.LastSkillOutput = nextChar.Chu2OutputSkillType; // '��' ��ų �ߵ� ��, ����� Chu2OutputSkillType�� ��
                    nextState.LastCharacterUsed = nextChar;
                    nextState.UsedCharacters.Add(nextChar);
                    // �޺� ��ο� �߰� �� ��ų ī�װ��� Chu, ��� Ÿ���� Chu2OutputSkillType���� ���
                    nextState.ComboPath.Add((nextChar.Name, SkillCategory.Chu, nextChar.Chu2OutputSkillType));

                    ExploreCombo(nextState); // ��� ȣ��
                }
            }
        }
    }

    /// <summary>
    /// ����� ��� �޺� ��θ� �ֿܼ� �� �ٷ� ����մϴ�.
    /// </summary>
    private void PrintAllCombos()
    {
        Debug.Log("--- ��� ������ �޺� ��� ---");
        if (_allComboPaths.Any())
        {
            // �޺� ���̿� ���� �����Ͽ� ���� ���� ���
            // `.Distinct(new ComboPathComparer())`�� ����ϸ� ������ ������ ����� �ߺ��� ������ �� �ֽ��ϴ�.
            // ���⼭�� ����� ���븸���� ���ϴ� ���� �ƴ϶�, ������ �ٸ� ��ü�� �ߺ� ������ �� �ʿ��մϴ�.
            // �ܼ� ��� Ž�� ������δ� ������ '���' ��ΰ� ������ ������ �� �ֽ��ϴ�.
            // ������ ����� ��� 'Ž����' ��θ� �����ִ� �� ������ �Ӵϴ�.
            var sortedCombos = _allComboPaths.OrderByDescending(c => c.Count).ToList();

            int comboIndex = 1;
            foreach (var combo in sortedCombos)
            {
                // �� �޺� ��θ� ���ڿ��� ����
                string comboString = "";
                for (int i = 0; i < combo.Count; i++)
                {
                    var step = combo[i];
                    // "ĳ���͸�(��ų����:���Ÿ��)" �������� ǥ��
                    comboString += $"{step.characterName}({step.skillCat}:{step.skillOutput})";
                    if (i < combo.Count - 1)
                    {
                        comboString += " -> ";
                    }
                }
                Debug.Log($"�޺� {comboIndex++} (����: {combo.Count}): {comboString}");
            }
            Debug.Log($"�� {sortedCombos.Count}���� �޺��� �߰ߵǾ����ϴ�.");

            // �ִ� �޺� ���� ã��
            int maxComboLength = 0;
            if (sortedCombos.Any())
            {
                maxComboLength = sortedCombos.First().Count; // ���������� ù ��°�� ���� ��
            }
            Debug.Log($"�ִ� �޺� ����: {maxComboLength}");
        }
        else
        {
            Debug.Log("���� ������ �޺��� �����ϴ�.");
        }
    }

    // ����Ƽ �����Ϳ��� �׽�Ʈ�� ���� �޼���
    [ContextMenu("Calculate All Combos")]
    public void TestComboCalculation()
    {
        partyMembers.Clear();

        // �ֽ� ĳ���� ������ ����
        // ���� (Green)
        Character char1 = new Character("����");
        char1.nation = NationType.Green;
        char1.NoSkillType = SkillType.Knockdown;
        char1.BoSkillType = SkillType.Airborne;
        char1.ChuConnectableSkillType = SkillType.Airborne; // ����� ���� ����
        char1.Chu2OutputSkillType = SkillType.Knockdown; // �� ��ų �ߵ� �� �˴ٿ� ���
        partyMembers.Add(char1);

        // ���� (Red)
        Character char2 = new Character("����");
        char2.nation = NationType.Red;
        char2.NoSkillType = SkillType.Airborne;
        char2.BoSkillType = SkillType.Knockdown;
        char2.ChuConnectableSkillType = SkillType.Airborne; // ����� ���� ����
        char2.Chu2OutputSkillType = SkillType.Airborne; // �� ��ų �ߵ� �� ��� ���
        partyMembers.Add(char2);

        // ��� (Blue)
        Character char3 = new Character("���");
        char3.nation = NationType.Blue;
        char3.NoSkillType = SkillType.Airborne;
        char3.BoSkillType = SkillType.Knockdown;
        char3.ChuConnectableSkillType = SkillType.Knockdown; // �˴ٿ ���� ����
        char3.Chu2OutputSkillType = SkillType.Repel; // �� ��ų �ߵ� �� ���� ���
        partyMembers.Add(char3);

        // ������ (Green)
        Character char4 = new Character("������");
        char4.nation = NationType.Green;
        char4.NoSkillType = SkillType.Airborne;
        char4.BoSkillType = SkillType.Airborne;
        char4.ChuConnectableSkillType = SkillType.Knockdown; // �˴ٿ ���� ����
        char4.Chu2OutputSkillType = SkillType.Airborne; // �� ��ų �ߵ� �� ��� ���
        partyMembers.Add(char4);

        // ���� (Purple) - 5��° ĳ���� (�ε��� 4)�̹Ƿ� ���� ����� ����
        Character supportChar = new Character("����");
        supportChar.nation = NationType.Purple;
        supportChar.NoSkillType = SkillType.Knockdown;
        supportChar.BoSkillType = SkillType.Knockdown; // �� ��ų�� ������ �޺��� ��� �ȵ� (�������� ó��)
        supportChar.ChuConnectableSkillType = SkillType.Repel; // ���� ���� ����
        supportChar.Chu2OutputSkillType = SkillType.Airborne; // �� ��ų �ߵ� �� ��� ���
        partyMembers.Add(supportChar);

        CalculateAllCombosAndPrint(); // ��� �޺��� ����ϰ� ���
    }
}