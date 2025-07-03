using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button, Dropdown (for generic UI classes if needed)
using TMPro; // TextMeshPro ���� Ŭ���� (TMP_Dropdown, TextMeshProUGUI)
using System.Linq;

// SkillTypeColors.cs (������Ʈ �� �ƹ� ������ ����)
public static class SkillTypeColors // �� Ŭ������ �� ���Ͽ� ���Ե��� �ʰ� ������ ���Ͽ� ���� ������ �����մϴ�.
{
    public static string GetColorHex(SkillType skillType) // SkillType�� ������Ʈ�� ���ǵ� enum���� �����մϴ�.
    {
        switch (skillType)
        {
            case SkillType.Knockdown:
                return "#0000FF"; // �Ķ��� (Blue)
            case SkillType.Repel:
                return "#FF0000"; // ������ (Red)
            case SkillType.Airborne:
                return "#00FF00"; // �ʷϻ� (Green)
            default:
                return "#FFFFFF"; // �⺻�� ���
        }
    }
}

public class UIManager : MonoBehaviour
{
    public TMP_Dropdown[] characterDropdowns; // TextMeshPro ��Ӵٿ� �迭
    public Button calculateButton;           // �޺� ��� ��ư
    public ComboCalController comboCalController; // �޺� ��� ��Ʈ�ѷ� ����

    public TextMeshProUGUI guideText;       // �޺� ��� ����� ����� TextMeshProUGUI
    public RectTransform guideTextContent;  // ��ũ�Ѻ��� Content RectTransform

    private List<Character> allCharacters; // ��� ĳ���� ������

    void Start()
    {
        // ComboCalController ���� Ȯ�� �� �Ҵ� (FindObjectOfType �߰�)
        if (comboCalController == null)
        {
            comboCalController = FindObjectOfType<ComboCalController>();
        }

        if (comboCalController == null)
        {
            Debug.LogError("ComboCalController�� ã�� �� �����ϴ�. ���� ��ġ�Ǿ����� Ȯ���ϼ���.");
            return;
        }

        // guideText�� �Ҵ�Ǿ����� Ȯ��
        if (guideText == null)
        {
            Debug.LogError("GuideText(TextMeshProUGUI)�� �Ҵ���� �ʾҽ��ϴ�. �ν����Ϳ��� �Ҵ����ּ���.");
            return;
        }

        // guideTextContent�� �Ҵ�Ǿ����� Ȯ��
        if (guideTextContent == null)
        {
            Debug.LogError("GuideText Content (RectTransform)�� �Ҵ���� �ʾҽ��ϴ�. �ν����Ϳ��� �Ҵ����ּ���.");
            return;
        }

        LoadAllCharacters();         // ��� ĳ���� ������ �ε�
        InitializeDropdowns();       // ��Ӵٿ� �ʱ�ȭ
        calculateButton.onClick.AddListener(OnCalculateButtonClick); // ��ư Ŭ�� �̺�Ʈ ����

        // �ʱ� ȭ�� �ؽ�Ʈ ����
        guideText.text = "ĳ���͸� �����ϰ� '�޺� ���' ��ư�� �����ּ���.";
        AdjustContentHeight(); // �ʱ� �ؽ�Ʈ�� ���� Content ���� ����
    }

    private void LoadAllCharacters()
    {
        if (GoogleSheetManager.instance != null && GoogleSheetManager.instance.characterSo != null)
        {
            // Character ������ ����Ʈ �̸��� characterDataList�� ����� ���� �ݿ�
            allCharacters = GoogleSheetManager.instance.characterSo.characterDataList;
            if (allCharacters == null || allCharacters.Count == 0)
            {
                Debug.LogWarning("GoogleSheetManager���� �ҷ��� ĳ���� �����Ͱ� ���ų� ��� �ֽ��ϴ�. GoogleSheetManager ������ Ȯ���ϼ���.");
            }
        }
        else
        {
            Debug.LogError("GoogleSheetManager �ν��Ͻ� �Ǵ� characterSo�� ã�� �� �����ϴ�. GoogleSheetManager�� �ʱ�ȭ�Ǿ����� Ȯ���ϼ���.");
            allCharacters = new List<Character>();
        }
    }

    private void InitializeDropdowns()
    {
        List<string> characterNames = new List<string> { "���� �� ��" };
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
        // ��Ӵٿ� ���� ����� ������ Ư���� �׼� ���� (��ư Ŭ�� �� �ϰ� ó��)
    }

    private void OnCalculateButtonClick()
    {
        List<Character> selectedPartyMembers = new List<Character>();
        HashSet<Character> uniqueSelectedCharacters = new HashSet<Character>(); // �ߺ� ���� ����

        foreach (TMP_Dropdown dropdown in characterDropdowns)
        {
            string selectedName = dropdown.options[dropdown.value].text; // ���õ� �ɼ��� �ؽ�Ʈ ��������

            if (selectedName == "���� �� ��")
            {
                continue; // "���� �� ��"�� ��Ƽ�� �������� ����
            }

            Character selectedChar = allCharacters.FirstOrDefault(c => c.Name == selectedName);

            if (selectedChar != null)
            {
                if (uniqueSelectedCharacters.Add(selectedChar)) // �ߺ����� ���� ��쿡�� �߰�
                {
                    selectedPartyMembers.Add(selectedChar);
                }
                else
                {
                    Debug.LogWarning($"ĳ���� '{selectedChar.Name}'��(��) �ߺ� ���õǾ����ϴ�. �ϳ��� ��Ƽ�� �߰��˴ϴ�.");
                }
            }
        }

        if (comboCalController != null)
        {
            comboCalController.partyMembers.Clear(); // ���� ��Ƽ�� �ʱ�ȭ
            comboCalController.partyMembers.AddRange(selectedPartyMembers); // �� ��Ƽ�� ����
            Debug.Log($"���õ� ��Ƽ ��� ��: {comboCalController.partyMembers.Count}");

            string resultOutput = comboCalController.CalculateAllCombosAndGetOutput(); // �޺� ��� �� ��� ���ڿ� �ޱ�
            guideText.text = resultOutput; // TextMeshProUGUI�� ��� ���
            AdjustContentHeight(); // �ؽ�Ʈ ���� ���� �� Content ���� ����
        }
    }

    /// <summary>
    /// guideText�� ���� ���̿� ���� guideTextContent�� ���̸� �����մϴ�.
    /// </summary>
    private void AdjustContentHeight()
    {
        // �ؽ�Ʈ�� "��ȣ�ϴ�" ���̸� �����ɴϴ�. �̴� �ؽ�Ʈ ���� ��ü�� ǥ���ϴ� �� �ʿ��� �ּ� �����Դϴ�.
        // guideText�� RectTransform�� �ʺ� ���� �ڵ����� �ٹٲ޵� ���̰� ���˴ϴ�.
        float preferredTextHeight = guideText.preferredHeight + 300;

        // guideTextContent�� ���� ũ�⸦ �����ɴϴ�.
        Vector2 currentSizeDelta = guideTextContent.sizeDelta;

        // guideTextContent�� ���� ���� ���̸� �����մϴ�.
        // ��ũ�� ���� Content�� ���� ��ܿ� ��Ŀ�Ǿ� �ְ�, Bottom ���� �÷� ���̸� �����մϴ�.
        // ���⼭�� guideText ��ü�� Content�� �ڽ��̸�, guideText�� ���̸�ŭ Content�� ���̸� ���� �ø��� ������� �����մϴ�.
        // ���� Content�� ���� �ڽ��� ������ �ְ� guideText�� �� �� �ϳ����,
        // Content�� �ڽ� ���̾ƿ� �׷� (Vertical Layout Group ��)�� ����ϴ� ���� �� ȿ�����Դϴ�.
        // ���⼭�� guideText�� Content�� ������ �Ǵ� �ֵ� �����̶�� �����ϰ� Content�� sizeDelta.y�� �����մϴ�.

        // ���ο� ���̴� �ּ��� ���� guideTextContent�� ���� �̻��̾�� �ϸ�, �ؽ�Ʈ�� ��ȣ ���̸�ŭ �þ�ϴ�.
        // ��, TextMeshProUGUI�� preferredHeight�� Content�� height�� ���� �����մϴ�.
        // �̶� Content�� pivot.y�� anchorMax.y�� 1 (���)���� �����ϰ� offsetMin.y�� �����ϴ� ����� �Ϲ����Դϴ�.

        // guideTextContent�� �Ǻ��� ��Ŀ�� ��ܿ� �����Ǿ� �ִٰ� �����մϴ� (pivot.y = 1, anchorMin.y = 1, anchorMax.y = 1).
        // �̷� ��� sizeDelta.y�� �����ϸ� �ϴ��� �þ�� �˴ϴ�.

        // ���� TextMeshProUGUI�� RectTransform ���̸� Content�� ���̷� ���� �����մϴ�.
        // guideText�� RectTransform�� guideTextContent�� �ڽ��̶�� �����մϴ�.
        guideText.rectTransform.sizeDelta = new Vector2(guideText.rectTransform.sizeDelta.x, preferredTextHeight);

        // Content�� ���̸� guideText�� ���̿� ����ϴ�.
        // �̶� Content�� offsetMax.y�� offsetMin.y�� �����Ͽ� top�� 0�� �����ǰ� �ϰ� bottom�� �ø��ϴ�.
        guideTextContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredTextHeight);

        // ���� guideTextContent�� Vertical Layout Group�� ����ϰ� �ִٸ�,
        // LayoutRebuilder.ForceRebuildLayoutImmediate(guideTextContent);
        // �Ǵ� Canvas.ForceUpdateCanvases();
        // ���� ȣ���Ͽ� ��� ���̾ƿ��� �����ؾ� �� ���� �ֽ��ϴ�.
    }
}