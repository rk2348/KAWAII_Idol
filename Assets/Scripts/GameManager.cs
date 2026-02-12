using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public FinancialManager financial;
    public MarketManager market;
    public IdolManager idol;
    public StaffManager staff;
    public EventManager events;
    public UIManager uiManager;

    [Header("Game Status")]
    public int currentDay = 1;
    public bool isGameOver = false;
    public bool isGameClear = false;
    public ProducerOrigin origin;

    private string pendingSongTitle = "";

    void Start()
    {
        idol.Initialize(financial, market, staff, this);
        staff.Initialize(financial);
        events.Initialize(idol, financial);
        uiManager.Initialize(this);
        uiManager.ShowStartScreen();
    }

    public void StartGame(int originIndex)
    {
        origin = (ProducerOrigin)originIndex;
        long startCash = 0;
        long startDebt = 0;
        float interest = 0;

        switch (origin)
        {
            case ProducerOrigin.OldAgency:
                startCash = 100000000; startDebt = 100000000; interest = 0.02f; break;
            case ProducerOrigin.Venture:
                startCash = 50000000; startDebt = 0; interest = 0; break;
            case ProducerOrigin.Indie:
                startCash = 5000000; startDebt = 0; interest = 0; break;
        }

        currentDay = 1;
        isGameOver = false;
        isGameClear = false;

        financial.Initialize(startCash, startDebt, interest);
        market.UpdateTrendRandomly(new DailyReport());

        // ★追加：グループ立ち上げ初期費用（オーディション・宣材・ロゴ）
        long setupCost = 2000000; // 200万円
        financial.currentCash -= setupCost;

        // ログはUI表示前に処理されるためConsoleに出すか、初日のレポートに含める工夫が必要
        // ここでは初日の所持金に反映済みとする
        Debug.Log($"初期費用（オーディション・宣材等）として {setupCost:N0}円 支払いました。");

        uiManager.ShowMainScreen();
    }

    public void OnSongNameConfirmed(string title, int budgetTier)
    {
        pendingSongTitle = title;
        ExecuteAction("ProduceSong", budgetTier);
    }

    private void ExecuteAction(string actionType, int param = 0)
    {
        if (isGameOver || isGameClear) return;

        DailyReport report = new DailyReport();
        report.day = currentDay;

        idol.CheckConditionEvents(report);

        bool canAct = idol.groupData.IsAvailable();

        if (canAct)
        {
            switch (actionType)
            {
                case "Lesson": idol.DoLesson(report); break;
                case "Promo": idol.DoPromotion(report); break;
                case "Rest": idol.DoRest(report); break;
                case "BookVenue": idol.BookVenue(param, 3, report); break;
                case "Hire": staff.HireStaff((StaffType)param, 1, report); break;
                case "ChangeConcept": idol.ChangeConcept((IdolGenre)param, report); break;
                case "ProduceSong": idol.ProduceSong(param, report, pendingSongTitle); break;

                // ★追加アクション
                case "ProduceGoods": idol.ProduceGoods(report); break;
                case "MakeMV": idol.MakeMV(report); break;

                case "Next": report.AddLog("何もしなかった。"); break;
            }
        }
        else
        {
            report.AddLog("<color=grey>【行動不能】メンバー不在のため何もできません...</color>");
        }

        // セットリスト安全装置
        var todayLive = idol.activeBookings.FirstOrDefault(b => b.eventDay == currentDay && !b.isCanceled);
        if (todayLive != null && todayLive.setlist.Count == 0)
        {
            idol.AutoGenerateSetlist(todayLive);
        }

        currentDay++;

        financial.ProcessDailyTransactions(currentDay, report);

        if (currentDay % 30 == 0)
        {
            staff.PayMonthlySalaries(report);
            // ★変更: 名前をより一般的なCostsに変更
            financial.PayMonthlyCosts(report);
            market.UpdateTrendRandomly(report);
        }

        if (currentDay % 7 == 0)
        {
            idol.ProcessWeeklySales(report);
        }

        events.CheckDailyEvent(report);
        idol.CheckAndHoldLive(currentDay, report);
        idol.DailyUpdate();

        report.cashChange = financial.dailyCashChange;
        CheckGameEnd();
        uiManager.ShowResultScreen(report);
    }

    void CheckGameEnd()
    {
        if (financial.currentCash < 0)
        {
            isGameOver = true;
            Debug.Log("GAME OVER");
        }
        if (idol.groupData.hasDoneDome && financial.currentDebt == 0 && financial.currentCash >= 300000000)
        {
            isGameClear = true;
            Debug.Log("GAME CLEAR");
        }
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Button Handlers ---
    public void OnClickLesson() { ExecuteAction("Lesson"); }
    public void OnClickPromo() { ExecuteAction("Promo"); }
    public void OnClickRest() { ExecuteAction("Rest"); }
    public void OnClickNext() { ExecuteAction("Next"); }
    public void OnClickBookZepp() { ExecuteAction("BookVenue", 1); }
    public void OnClickBookDome() { ExecuteAction("BookVenue", 3); }
    public void OnClickHireTrainer() { ExecuteAction("Hire", 0); }
    public void OnClickHireMarketer() { ExecuteAction("Hire", 1); }
    public void OnClickChangeGenreToKawaii() { ExecuteAction("ChangeConcept", 0); }
    public void OnClickChangeGenreToCool() { ExecuteAction("ChangeConcept", 1); }
    public void OnClickChangeGenreToRock() { ExecuteAction("ChangeConcept", 2); }

    public void OnClickProduceSongLow()
    {
        int nextNum = idol.groupData.discography.Count + 1;
        uiManager.ShowSongProductionPanel(0, nextNum);
    }

    public void OnClickProduceSongHigh()
    {
        int nextNum = idol.groupData.discography.Count + 1;
        uiManager.ShowSongProductionPanel(1, nextNum);
    }

    public void OnClickSetlist()
    {
        uiManager.ShowSetlistScreen();
    }

    // ★追加：新機能用ボタンハンドラ
    public void OnClickProduceGoods() { ExecuteAction("ProduceGoods"); }
    public void OnClickMakeMV() { ExecuteAction("MakeMV"); }
}