using UnityEngine;
using UnityEngine.SceneManagement;

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

        uiManager.ShowMainScreen();
    }

    private void ExecuteAction(string actionType, int param = 0)
    {
        if (isGameOver || isGameClear) return;

        DailyReport report = new DailyReport();
        report.day = currentDay;

        // ★追加：状態異常（入院/失踪）チェック
        idol.CheckConditionEvents(report); // まず今日の状態を確認（入院発生など）

        bool canAct = idol.groupData.IsAvailable();

        if (canAct)
        {
            // 正常時：アクション実行
            switch (actionType)
            {
                case "Lesson": idol.DoLesson(report); break;
                case "Promo": idol.DoPromotion(report); break;
                case "Rest": idol.DoRest(report); break;
                case "BookVenue": idol.BookVenue(param, 3, report); break;
                case "Hire": staff.HireStaff((StaffType)param, 1, report); break;
                case "ChangeConcept": idol.ChangeConcept((IdolGenre)param, report); break;
                case "ProduceSong": idol.ProduceSong(param, report); break;
                case "Next": report.AddLog("何もしなかった。"); break;
            }
        }
        else
        {
            // 異常時：アクション強制キャンセル
            report.AddLog("<color=grey>【行動不能】メンバー不在のため何もできません...</color>");
        }

        currentDay++;

        financial.ProcessDailyTransactions(currentDay, report);

        if (currentDay % 30 == 0)
        {
            staff.PayMonthlySalaries(report);
            financial.PayMonthlyInterest(report);
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
    public void OnClickProduceSongLow() { ExecuteAction("ProduceSong", 0); }
    public void OnClickProduceSongHigh() { ExecuteAction("ProduceSong", 1); }
}