using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq; // Last()を使うために必要

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject mainPanel;
    public GameObject resultPanel;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;

    [Header("Setlist Panel")]
    public SetlistPanel setlistPanel;

    [Header("Song Name Input Panel")]
    public SongNameInputPanel songNamePanel;

    [Header("Main Screen Text")]
    public Text dateText;
    public Text cashText;
    public Text debtText;
    public Text statusText;
    public Text ledgerText;
    public Text trendText;
    public Text bookingText;
    public Text latestSongText; // ★追加：最新曲情報

    [Header("Main Screen Sliders")]
    public Slider mentalSlider;
    public Slider fatigueSlider;
    public Slider performanceSlider;

    [Header("Result Screen")]
    public Text resultLogText;

    private GameManager gameManager;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;

        if (songNamePanel != null) songNamePanel.Setup(gm);
    }

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

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"【Day {report.day} 結果報告】");
        sb.AppendLine($"収支: {report.cashChange:N0}円");
        sb.AppendLine("----------------");
        foreach (var log in report.logs)
        {
            sb.AppendLine(log);
        }

        if (gameManager.isGameOver) sb.AppendLine("\n<color=red>資金ショート！！ ゲームオーバー...</color>");
        if (gameManager.isGameClear) sb.AppendLine("\n<color=magenta>おめでとう！ 伝説のプロデューサー！</color>");

        resultLogText.text = sb.ToString();
    }

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

    void RefreshMainUI()
    {
        if (gameManager == null) return;

        dateText.text = $"Day: {gameManager.currentDay}";
        cashText.text = $"Cash: ?{gameManager.financial.currentCash:N0}";
        cashText.color = gameManager.financial.currentCash < 0 ? Color.red : Color.white;
        debtText.text = $"Debt: ?{gameManager.financial.currentDebt:N0}";

        var g = gameManager.idol.groupData;
        var m = gameManager.market;

        statusText.text = $"Fans: {g.fans:N0}\nGenre: {g.genre}";
        trendText.text = $"Trend: {m.currentTrend} {(m.isIceAge ? "<color=cyan>(ICE AGE)</color>" : "")}";

        if (mentalSlider != null) mentalSlider.value = g.mental / 100f;
        if (fatigueSlider != null) fatigueSlider.value = g.fatigue / 100f;
        if (performanceSlider != null) performanceSlider.value = g.performance / 100f;

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

        // ★追加：最新曲情報の表示
        if (latestSongText != null)
        {
            if (g.discography.Count > 0)
            {
                var song = g.discography.Last();
                latestSongText.text = $"最新曲: {song.title}\n最高位: {song.peakRank}位\n累積売上: {song.totalSales:N0}枚";
            }
            else
            {
                latestSongText.text = "最新曲: なし";
            }
        }
    }

    // ★追加：セットリスト画面を開く
    public void ShowSetlistScreen()
    {
        // IdolManagerを渡して初期化
        setlistPanel.Open(gameManager.idol);
    }

    // ★追加: 曲名入力画面を開く
    public void ShowSongProductionPanel(int budgetTier, int nextSongNum)
    {
        songNamePanel.Open(budgetTier, nextSongNum);
    }
}