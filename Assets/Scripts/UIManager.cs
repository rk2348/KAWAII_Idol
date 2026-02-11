using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject mainPanel;
    public GameObject resultPanel;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;

    [Header("Main Screen Objects")]
    public Text dateText;
    public Text cashText;
    public Text debtText; // 借金表示用
    public Text statusText;
    public Text ledgerText;
    public Text trendText;
    public Text bookingText;

    [Header("Result Screen Objects")]
    public Text resultLogText;

    private GameManager gameManager;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
    }

    // --- 画面切り替え ---

    public void ShowStartScreen()
    {
        startPanel.SetActive(true);
        mainPanel.SetActive(false);
        resultPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(false);
    }

    public void ShowMainScreen()
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(true);
        resultPanel.SetActive(false);
        RefreshMainUI();
    }

    public void ShowResultScreen(DailyReport report)
    {
        mainPanel.SetActive(false);
        resultPanel.SetActive(true);

        // ログの生成
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"【Day {report.day} の結果】");
        sb.AppendLine($"収支変動: {report.cashChange:N0}円");
        sb.AppendLine("----------------");
        foreach (var log in report.logs)
        {
            sb.AppendLine(log);
        }

        // ゲームオーバー/クリアなら追記
        if (gameManager.isGameOver) sb.AppendLine("\n<color=red>資金ショート！！ ゲームオーバー...</color>");
        if (gameManager.isGameClear) sb.AppendLine("\n<color=magenta>おめでとう！ 伝説のプロデューサー！</color>");

        resultLogText.text = sb.ToString();

        // 終了フラグが立っていたら、閉じるボタンで専用画面へ
    }

    // 結果画面の「閉じる（次の日へ）」ボタンから呼ぶ
    public void CloseResultScreen()
    {
        if (gameManager.isGameOver)
        {
            resultPanel.SetActive(false);
            gameOverPanel.SetActive(true);
        }
        else if (gameManager.isGameClear)
        {
            resultPanel.SetActive(false);
            gameClearPanel.SetActive(true);
        }
        else
        {
            ShowMainScreen();
        }
    }

    // --- 表示更新 ---

    void RefreshMainUI()
    {
        if (gameManager == null) return;

        dateText.text = $"Day: {gameManager.currentDay}";

        cashText.text = $"Cash: ?{gameManager.financial.currentCash:N0}";
        cashText.color = gameManager.financial.currentCash < 0 ? Color.red : Color.white;

        debtText.text = $"Debt: ?{gameManager.financial.currentDebt:N0}";

        var g = gameManager.idol.groupData;
        statusText.text = $"Fans: {g.fans:N0}\nPerf: {g.performance}\nMental: {g.mental}\nFatigue: {g.fatigue}";

        var m = gameManager.market;
        trendText.text = $"Trend: {m.currentTrend} {(m.isIceAge ? "<color=cyan>(ICE AGE)</color>" : "")}";

        // 帳簿リスト
        string ledgerStr = "【入出金予定】\n";
        int count = 0;
        foreach (var t in gameManager.financial.pendingTransactions)
        {
            if (count >= 5) break;
            int daysLeft = t.dueDay - gameManager.currentDay;
            string color = t.amount >= 0 ? "green" : "red";
            ledgerStr += $"<color={color}>{t.description}: {t.amount:N0} ({daysLeft}日後)</color>\n";
            count++;
        }
        ledgerText.text = ledgerStr;

        // 予約リスト
        string bookingStr = "【ライブ予約】\n";
        if (gameManager.idol.activeBookings.Count == 0) bookingStr += "なし";
        else
        {
            foreach (var b in gameManager.idol.activeBookings)
            {
                int daysLeft = b.eventDay - gameManager.currentDay;
                bookingStr += $"{b.venue.venueName} (あと{daysLeft}日)\n";
            }
        }
        bookingText.text = bookingStr;
    }
}