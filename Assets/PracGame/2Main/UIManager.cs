using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button, Dropdown (for generic UI classes if needed)
using TMPro; // TextMeshPro 관련 클래스 (TMP_Dropdown, TextMeshProUGUI)
using System.Linq;

// SkillTypeColors.cs (프로젝트 내 아무 폴더에 생성)
public static class SkillTypeColors // 이 클래스는 이 파일에 포함되지 않고 별도의 파일에 있을 것으로 가정합니다.
{
    public static string GetColorHex(SkillType skillType) // SkillType은 프로젝트에 정의된 enum으로 가정합니다.
    {
        switch (skillType)
        {
            case SkillType.Knockdown:
                return "#0000FF"; // 파란색 (Blue)
            case SkillType.Repel:
                return "#FF0000"; // 빨간색 (Red)
            case SkillType.Airborne:
                return "#00FF00"; // 초록색 (Green)
            default:
                return "#FFFFFF"; // 기본값 흰색
        }
    }
}

public class UIManager : MonoBehaviour
{
    public TMP_Dropdown[] characterDropdowns; // TextMeshPro 드롭다운 배열
    public Button calculateButton;           // 콤보 계산 버튼
    public ComboCalController comboCalController; // 콤보 계산 컨트롤러 참조

    public TextMeshProUGUI guideText;       // 콤보 계산 결과를 출력할 TextMeshProUGUI
    public RectTransform guideTextContent;  // 스크롤뷰의 Content RectTransform

    private List<Character> allCharacters; // 모든 캐릭터 데이터

    void Start()
    {
        // ComboCalController 참조 확인 및 할당 (FindObjectOfType 추가)
        if (comboCalController == null)
        {
            comboCalController = FindObjectOfType<ComboCalController>();
        }

        if (comboCalController == null)
        {
            Debug.LogError("ComboCalController를 찾을 수 없습니다. 씬에 배치되었는지 확인하세요.");
            return;
        }

        // guideText가 할당되었는지 확인
        if (guideText == null)
        {
            Debug.LogError("GuideText(TextMeshProUGUI)가 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
            return;
        }

        // guideTextContent가 할당되었는지 확인
        if (guideTextContent == null)
        {
            Debug.LogError("GuideText Content (RectTransform)가 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
            return;
        }

        LoadAllCharacters();         // 모든 캐릭터 데이터 로드
        InitializeDropdowns();       // 드롭다운 초기화
        calculateButton.onClick.AddListener(OnCalculateButtonClick); // 버튼 클릭 이벤트 연결

        // 초기 화면 텍스트 설정
        guideText.text = "캐릭터를 선택하고 '콤보 계산' 버튼을 눌러주세요.";
        AdjustContentHeight(); // 초기 텍스트에 맞춰 Content 높이 조절
    }

    private void LoadAllCharacters()
    {
        if (GoogleSheetManager.instance != null && GoogleSheetManager.instance.characterSo != null)
        {
            // Character 데이터 리스트 이름을 characterDataList로 변경된 것을 반영
            allCharacters = GoogleSheetManager.instance.characterSo.characterDataList;
            if (allCharacters == null || allCharacters.Count == 0)
            {
                Debug.LogWarning("GoogleSheetManager에서 불러온 캐릭터 데이터가 없거나 비어 있습니다. GoogleSheetManager 설정을 확인하세요.");
            }
        }
        else
        {
            Debug.LogError("GoogleSheetManager 인스턴스 또는 characterSo를 찾을 수 없습니다. GoogleSheetManager가 초기화되었는지 확인하세요.");
            allCharacters = new List<Character>();
        }
    }

    private void InitializeDropdowns()
    {
        List<string> characterNames = new List<string> { "선택 안 함" };
        if (allCharacters != null)
        {
            characterNames.AddRange(allCharacters.Select(c => c.Name).ToList());
        }

        foreach (TMP_Dropdown dropdown in characterDropdowns)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(characterNames);
            dropdown.value = 0;
            dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdown); });
        }
    }

    private void OnDropdownValueChanged(TMP_Dropdown changedDropdown)
    {
        // 드롭다운 값이 변경될 때마다 특별한 액션 없음 (버튼 클릭 시 일괄 처리)
    }

    private void OnCalculateButtonClick()
    {
        List<Character> selectedPartyMembers = new List<Character>();
        HashSet<Character> uniqueSelectedCharacters = new HashSet<Character>(); // 중복 선택 방지

        foreach (TMP_Dropdown dropdown in characterDropdowns)
        {
            string selectedName = dropdown.options[dropdown.value].text; // 선택된 옵션의 텍스트 가져오기

            if (selectedName == "선택 안 함")
            {
                continue; // "선택 안 함"은 파티에 포함하지 않음
            }

            Character selectedChar = allCharacters.FirstOrDefault(c => c.Name == selectedName);

            if (selectedChar != null)
            {
                if (uniqueSelectedCharacters.Add(selectedChar)) // 중복되지 않은 경우에만 추가
                {
                    selectedPartyMembers.Add(selectedChar);
                }
                else
                {
                    Debug.LogWarning($"캐릭터 '{selectedChar.Name}'이(가) 중복 선택되었습니다. 하나만 파티에 추가됩니다.");
                }
            }
        }

        if (comboCalController != null)
        {
            comboCalController.partyMembers.Clear(); // 기존 파티원 초기화
            comboCalController.partyMembers.AddRange(selectedPartyMembers); // 새 파티원 설정
            Debug.Log($"선택된 파티 멤버 수: {comboCalController.partyMembers.Count}");

            string resultOutput = comboCalController.CalculateAllCombosAndGetOutput(); // 콤보 계산 및 결과 문자열 받기
            guideText.text = resultOutput; // TextMeshProUGUI에 결과 출력
            AdjustContentHeight(); // 텍스트 내용 변경 후 Content 높이 조절
        }
    }

    /// <summary>
    /// guideText의 내용 길이에 맞춰 guideTextContent의 높이를 조절합니다.
    /// </summary>
    private void AdjustContentHeight()
    {
        // 텍스트의 "선호하는" 높이를 가져옵니다. 이는 텍스트 내용 전체를 표시하는 데 필요한 최소 높이입니다.
        // guideText의 RectTransform의 너비에 따라 자동으로 줄바꿈된 높이가 계산됩니다.
        float preferredTextHeight = guideText.preferredHeight + 300;

        // guideTextContent의 현재 크기를 가져옵니다.
        Vector2 currentSizeDelta = guideTextContent.sizeDelta;

        // guideTextContent의 원래 기준 높이를 설정합니다.
        // 스크롤 뷰의 Content는 보통 상단에 앵커되어 있고, Bottom 값을 늘려 높이를 조절합니다.
        // 여기서는 guideText 자체가 Content의 자식이며, guideText의 높이만큼 Content의 높이를 직접 늘리는 방식으로 진행합니다.
        // 만약 Content가 여러 자식을 가지고 있고 guideText가 그 중 하나라면,
        // Content의 자식 레이아웃 그룹 (Vertical Layout Group 등)을 사용하는 것이 더 효율적입니다.
        // 여기서는 guideText가 Content의 유일한 또는 주된 내용이라고 가정하고 Content의 sizeDelta.y를 조절합니다.

        // 새로운 높이는 최소한 현재 guideTextContent의 높이 이상이어야 하며, 텍스트의 선호 높이만큼 늘어납니다.
        // 즉, TextMeshProUGUI의 preferredHeight를 Content의 height로 직접 설정합니다.
        // 이때 Content의 pivot.y와 anchorMax.y를 1 (상단)으로 설정하고 offsetMin.y를 조절하는 방식이 일반적입니다.

        // guideTextContent의 피봇과 앵커가 상단에 고정되어 있다고 가정합니다 (pivot.y = 1, anchorMin.y = 1, anchorMax.y = 1).
        // 이런 경우 sizeDelta.y를 조절하면 하단이 늘어나게 됩니다.

        // 현재 TextMeshProUGUI의 RectTransform 높이를 Content의 높이로 직접 설정합니다.
        // guideText의 RectTransform이 guideTextContent의 자식이라고 가정합니다.
        guideText.rectTransform.sizeDelta = new Vector2(guideText.rectTransform.sizeDelta.x, preferredTextHeight);

        // Content의 높이를 guideText의 높이에 맞춥니다.
        // 이때 Content의 offsetMax.y와 offsetMin.y를 조절하여 top이 0에 고정되게 하고 bottom을 늘립니다.
        guideTextContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredTextHeight);

        // 만약 guideTextContent가 Vertical Layout Group을 사용하고 있다면,
        // LayoutRebuilder.ForceRebuildLayoutImmediate(guideTextContent);
        // 또는 Canvas.ForceUpdateCanvases();
        // 등을 호출하여 즉시 레이아웃을 갱신해야 할 수도 있습니다.
    }
}