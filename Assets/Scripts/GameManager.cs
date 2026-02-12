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

    // ★変更：ここからセットアップ画面へ遷移
    public void StartGame(int originIndex)
    {
        origin = (ProducerOrigin)originIndex;
        // セットアップ画面を表示して、名前と人数を決めさせる
        uiManager.ShowSetupScreen();
    }

    // ★追加：セットアップ画面で決定ボタンが押されたらここに来る
    public void OnSetupConfirmed(string groupName, int memberCount)
    {
        idol.SetGroupInfo(groupName, memberCount);
        StartGameLogic();
    }

    // 実際のゲーム開始処理（旧StartGameの後半部分）
    private void StartGameLogic()
    {
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

        // 初期費用（オーディション・宣材・ロゴ + 人数分の支度金）
        long setupCost = 2000000 + (idol.groupData.memberCount * 100000);
        financial.currentCash -= setupCost;

        Debug.Log($"初期費用（オーディション・人数分初期費）として {setupCost:N0}円 支払いました。");

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
                case "ProduceGoods": idol.ProduceGoods(report); break;
                case "MakeMV": idol.MakeMV(report); break;
                case "Next": report.AddLog("何もしなかった。"); break;
            }
        }
        else
        {
            report.AddLog("<color=grey>【行動不能】メンバー不在のため何もできません...</color>");
        }

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
            financial.PayMonthlyCosts(report);

            // ★追加：メンバー生活費（人数分）
            long livingCost = idol.CalcMonthlyMemberCost();
            financial.currentCash -= livingCost;
            financial.dailyCashChange -= livingCost;
            report.AddLog($"[固定費] メンバー生活費・寮費: -{livingCost:N0}円");

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
    public void OnClickProduceSongLow() { int nextNum = idol.groupData.discography.Count + 1; uiManager.ShowSongProductionPanel(0, nextNum); }
    public void OnClickProduceSongHigh() { int nextNum = idol.groupData.discography.Count + 1; uiManager.ShowSongProductionPanel(1, nextNum); }
    public void OnClickSetlist() { uiManager.ShowSetlistScreen(); }
    public void OnClickProduceGoods() { ExecuteAction("ProduceGoods"); }
    public void OnClickMakeMV() { ExecuteAction("MakeMV"); }
}