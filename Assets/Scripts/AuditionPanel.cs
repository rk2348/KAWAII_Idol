using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class AuditionPanel : MonoBehaviour
{
    [Header("UI References")]
    public Text instructionText;       // 「あと○人選んでください」
    public Transform candidatesRoot;   // ScrollViewのContent
    public GameObject candidateButtonPrefab; // 候補者ボタンのプレハブ
    public Button confirmButton;

    private GameManager gameManager;
    private IdolManager idolManager;
    private int targetMemberCount;
    private List<IdolMember> generatedCandidates = new List<IdolMember>();
    private List<IdolMember> selectedMembers = new List<IdolMember>();

    public void Setup(GameManager gm, IdolManager im)
    {
        gameManager = gm;
        idolManager = im;
        this.gameObject.SetActive(false);
    }

    public void Open(int count)
    {
        targetMemberCount = count;
        selectedMembers.Clear();
        generatedCandidates.Clear();

        // 候補者を生成（必要人数の3倍など）
        int candidateCount = Mathf.Max(count * 3, 10);
        generatedCandidates = idolManager.GenerateCandidates(candidateCount);

        this.gameObject.SetActive(true);
        RefreshList();
    }

    void RefreshList()
    {
        // 既存のボタン削除
        foreach (Transform child in candidatesRoot) Destroy(child.gameObject);

        // ボタン生成
        foreach (var candidate in generatedCandidates)
        {
            GameObject obj = Instantiate(candidateButtonPrefab, candidatesRoot);
            Button btn = obj.GetComponent<Button>();
            Text txt = obj.GetComponentInChildren<Text>();

            bool isSelected = selectedMembers.Contains(candidate);

            // ★追加：性格の表示を追加
            string checkMark = isSelected ? "<color=yellow>★採用</color>" : "　";
            txt.text = $"{checkMark} <b>{candidate.GetFullName()}</b> ({candidate.age}歳)\n" +
                       $"性:{candidate.GetPersonalityName()} / Vo:{candidate.vocal} Da:{candidate.dance} Vi:{candidate.visual}";

            // 色変更
            ColorBlock cb = btn.colors;
            cb.normalColor = isSelected ? new Color(1f, 0.9f, 0.7f) : Color.white;
            btn.colors = cb;

            btn.onClick.AddListener(() => OnCandidateClicked(candidate));
        }

        UpdateInfo();
    }

    void OnCandidateClicked(IdolMember member)
    {
        if (selectedMembers.Contains(member))
        {
            selectedMembers.Remove(member);
        }
        else
        {
            // 上限チェック
            if (selectedMembers.Count < targetMemberCount)
            {
                selectedMembers.Add(member);
            }
        }
        RefreshList();
    }

    void UpdateInfo()
    {
        int current = selectedMembers.Count;
        instructionText.text = $"メンバーを選抜してください ({current}/{targetMemberCount}人)";

        // 規定人数選んだときだけ決定ボタンを押せる
        confirmButton.interactable = (current == targetMemberCount);
    }

    public void OnConfirmClick()
    {
        // GameManagerへ選択メンバーを渡す
        gameManager.OnAuditionFinished(selectedMembers);
        this.gameObject.SetActive(false);
    }
}