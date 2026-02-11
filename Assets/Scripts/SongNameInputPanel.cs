using UnityEngine;
using UnityEngine.UI;

public class SongNameInputPanel : MonoBehaviour
{
    [Header("UI References")]
    public InputField nameInputField;
    public Text titleText; // 「低予算制作」or「豪華制作」
    public Text costText;  // 「費用: 1,000,000円」

    private GameManager gameManager;
    private int currentBudgetTier;

    public void Setup(GameManager gm)
    {
        gameManager = gm;
        this.gameObject.SetActive(false);
    }

    public void Open(int budgetTier, int nextSongNumber)
    {
        currentBudgetTier = budgetTier;
        this.gameObject.SetActive(true);

        // デフォルト名をセット（例: Single #5）
        nameInputField.text = $"Single #{nextSongNumber}";

        if (budgetTier == 0)
        {
            titleText.text = "新曲制作 (低予算)";
            costText.text = "制作費: 1,000,000円";
        }
        else
        {
            titleText.text = "新曲制作 (豪華)";
            costText.text = "制作費: 5,000,000円";
        }
    }

    // 決定ボタン
    public void OnConfirm()
    {
        string title = nameInputField.text;
        if (string.IsNullOrEmpty(title)) title = "無題の楽曲"; // 空欄防止

        // ゲームマネージャーに入力内容を渡して実行させる
        gameManager.OnSongNameConfirmed(title, currentBudgetTier);

        Close();
    }

    // キャンセルボタン
    public void OnCancel()
    {
        Close();
    }

    private void Close()
    {
        this.gameObject.SetActive(false);
    }
}