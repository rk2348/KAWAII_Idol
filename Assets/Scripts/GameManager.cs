using UnityEngine;

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

        // スタート画面を表示
        uiManager.ShowStartScreen();
    }

    // スタート画面で「出自」を選んだら呼ばれる
    public void StartGame(int originIndex)
    {
        origin = (ProducerOrigin)originIndex;
        long startCash = 0;
        long startDebt = 0;
        float interest = 0;

        switch (origin)
        {
            case ProducerOrigin.OldAgency:
                startCash = 100000000; // 1億
                startDebt = 100000000; // 借金1億
                interest = 0.02f;      // 月利2%
                break;
            case ProducerOrigin.Venture:
                startCash = 50000000;  // 5000万
                startDebt = 0;
                interest = 0;
                break;
            case ProducerOrigin.Indie:
                startCash = 5000000;   // 500万
                startDebt = 0;
                interest = 0;
                break;
        }

        currentDay = 1;
        isGameOver = false;
        isGameClear = false;

        financial.Initialize(startCash, startDebt, interest);
        market.UpdateTrendRandomly(new DailyReport()); // 初回のトレンドログは捨てる

        // メイン画面へ
        uiManager.ShowMainScreen();
    }

    // プレイヤーが行動ボタンを押した時の処理
    public void ExecuteAction(string actionType, int param = 0)
    {
        if (isGameOver || isGameClear) return;

        // 今日のレポート作成開始
        DailyReport report = new DailyReport();
        report.day = currentDay;

        // 1. プレイヤーの行動実行
        switch (actionType)
        {
            case "Lesson": idol.DoLesson(report); break;
            case "Promo": idol.DoPromotion(report); break;
            case "Rest": idol.DoRest(report); break;
            case "BookVenue": idol.BookVenue(param, 3, report); break; // 3ヶ月後予約
            case "Hire": staff.HireStaff((StaffType)param, 1, report); break;
            case "Next": report.AddLog("何もしなかった。"); break;
        }

        // 2. 自動処理（金融・イベント・ライブ）
        currentDay++;

        financial.ProcessDailyTransactions(currentDay, report); // 予約決済

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

        // 3. 勝敗判定
        CheckGameEnd();

        // 4. 結果画面表示
        uiManager.ShowResultScreen(report);
    }

    void CheckGameEnd()
    {
        // 倒産判定
        if (financial.currentCash < 0)
        {
            isGameOver = true;
            Debug.Log("GAME OVER: 破産");
        }

        // クリア判定（ドーム成功 かつ 借金完済 かつ 現金3億）
        if (idol.groupData.hasDoneDome && financial.currentDebt == 0 && financial.currentCash >= 300000000)
        {
            isGameClear = true;
            Debug.Log("GAME CLEAR: 伝説達成");
        }
    }
}