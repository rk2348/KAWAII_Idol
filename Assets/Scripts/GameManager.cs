using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理用

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
        // 初期化順序
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
                startCash = 100000000;
                startDebt = 100000000;
                interest = 0.02f;
                break;
            case ProducerOrigin.Venture:
                startCash = 50000000;
                startDebt = 0;
                interest = 0;
                break;
            case ProducerOrigin.Indie:
                startCash = 5000000;
                startDebt = 0;
                interest = 0;
                break;
        }

        currentDay = 1;
        isGameOver = false;
        isGameClear = false;

        financial.Initialize(startCash, startDebt, interest);
        market.UpdateTrendRandomly(new DailyReport());

        uiManager.ShowMainScreen();
    }

    // --- メイン処理 ---
    private void ExecuteAction(string actionType, int param = 0)
    {
        if (isGameOver || isGameClear) return;

        DailyReport report = new DailyReport();
        report.day = currentDay;

        switch (actionType)
        {
            case "Lesson": idol.DoLesson(report); break;
            case "Promo": idol.DoPromotion(report); break;
            case "Rest": idol.DoRest(report); break;
            case "BookVenue": idol.BookVenue(param, 3, report); break;
            case "Hire": staff.HireStaff((StaffType)param, 1, report); break;
            case "ChangeConcept": idol.ChangeConcept((IdolGenre)param, report); break; // 追加
            case "Next": report.AddLog("何もしなかった。"); break;
        }

        currentDay++;

        financial.ProcessDailyTransactions(currentDay, report);

        if (currentDay % 30 == 0)
        {
            staff.PayMonthlySalaries(report);
            financial.PayMonthlyInterest(report);
            market.UpdateTrendRandomly(report);
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
            Debug.Log("GAME OVER: 破産");
        }

        if (idol.groupData.hasDoneDome && financial.currentDebt == 0 && financial.currentCash >= 300000000)
        {
            isGameClear = true;
            Debug.Log("GAME CLEAR: 伝説達成");
        }
    }

    // --- リトライ機能 ---
    public void RetryGame()
    {
        // 現在のシーンを再読み込み
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Unity UI Button用ラッパー関数群 ---

    public void OnClickLesson() { ExecuteAction("Lesson"); }
    public void OnClickPromo() { ExecuteAction("Promo"); }
    public void OnClickRest() { ExecuteAction("Rest"); }
    public void OnClickNext() { ExecuteAction("Next"); }

    // 予約系
    public void OnClickBookZepp() { ExecuteAction("BookVenue", 1); }
    public void OnClickBookDome() { ExecuteAction("BookVenue", 3); }

    // 雇用系
    public void OnClickHireTrainer() { ExecuteAction("Hire", 0); }
    public void OnClickHireMarketer() { ExecuteAction("Hire", 1); }

    // コンセプト変更系 (0=KAWAII, 1=COOL, 2=ROCK, 3=TRADITIONAL)
    public void OnClickChangeGenreToKawaii() { ExecuteAction("ChangeConcept", 0); }
    public void OnClickChangeGenreToCool() { ExecuteAction("ChangeConcept", 1); }
    public void OnClickChangeGenreToRock() { ExecuteAction("ChangeConcept", 2); }
}