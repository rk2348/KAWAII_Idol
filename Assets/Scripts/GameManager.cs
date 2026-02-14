using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

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
        uiManager.ShowSetupScreen();
    }

    // セットアップ画面で人数が決まったらオーディション画面へ
    public void OnSetupConfirmed(string groupName, int memberCount)
    {
        idol.SetGroupName(groupName);
        uiManager.ShowAuditionScreen(memberCount);
    }

    // オーディションでメンバーが確定したらゲーム開始
    public void OnAuditionFinished(List<IdolMember> selectedMembers)
    {
        idol.SetGroupMembers(selectedMembers);
        StartGameLogic();
    }

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
        // オーディション開催費として少し上乗せ
        long setupCost = 2500000 + (idol.groupData.memberCount * 100000);
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
                case "Cheki": idol.DoChekiEvent(report); break; // 特典会
                case "SNSPromo": idol.DoSNSPromotion(report); break; // ★追加：SNS投稿
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

    // --- UI Button Handlers ---
    public void OnClickLesson() { ExecuteAction("Lesson"); }
    public void OnClickPromo() { ExecuteAction("Promo"); }
    public void OnClickRest() { ExecuteAction("Rest"); }
    public void OnClickNext() { ExecuteAction("Next"); }

    // 会場予約 (0:LiveHouse, 1:Zepp, 2:Budokan, 3:TokyoDome)
    public void OnClickBookLiveHouse() { ExecuteAction("BookVenue", 0); }
    public void OnClickBookZepp() { ExecuteAction("BookVenue", 1); }
    public void OnClickBookBudokan() { ExecuteAction("BookVenue", 2); }
    public void OnClickBookDome() { ExecuteAction("BookVenue", 3); }

    // スタッフ雇用 (0:Trainer, 1:Marketer, 2:Manager)
    public void OnClickHireTrainer() { ExecuteAction("Hire", 0); }
    public void OnClickHireMarketer() { ExecuteAction("Hire", 1); }
    public void OnClickHireManager() { ExecuteAction("Hire", 2); }

    // コンセプト変更 (0:Kawaii, 1:Cool, 2:Rock, 3:Traditional)
    public void OnClickChangeGenreToKawaii() { ExecuteAction("ChangeConcept", 0); }
    public void OnClickChangeGenreToCool() { ExecuteAction("ChangeConcept", 1); }
    public void OnClickChangeGenreToRock() { ExecuteAction("ChangeConcept", 2); }
    public void OnClickChangeGenreToTraditional() { ExecuteAction("ChangeConcept", 3); }

    // 楽曲制作
    public void OnClickProduceSongLow() { int nextNum = idol.groupData.discography.Count + 1; uiManager.ShowSongProductionPanel(0, nextNum); }
    public void OnClickProduceSongHigh() { int nextNum = idol.groupData.discography.Count + 1; uiManager.ShowSongProductionPanel(1, nextNum); }

    // その他
    public void OnClickSetlist() { uiManager.ShowSetlistScreen(); }
    public void OnClickProduceGoods() { ExecuteAction("ProduceGoods"); }
    public void OnClickMakeMV() { ExecuteAction("MakeMV"); }

    // 特典会
    public void OnClickCheki() { ExecuteAction("Cheki"); }

    // ★追加: SNSプロモーション
    public void OnClickSNSPromo() { ExecuteAction("SNSPromo"); }
}