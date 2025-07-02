using System.Collections;
using System.Collections.Generic;
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
    public Character()
    {
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


[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObject/CharacterSO")]
public class CharacterSo : ScriptableObject
{
    public List<Character> characterDataList = new List<Character>();

}