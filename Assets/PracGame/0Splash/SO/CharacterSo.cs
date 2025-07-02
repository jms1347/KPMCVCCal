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
    public Character()
    {
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


[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObject/CharacterSO")]
public class CharacterSo : ScriptableObject
{
    public List<Character> characterDataList = new List<Character>();

}