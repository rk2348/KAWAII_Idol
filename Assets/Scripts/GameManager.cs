using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public FinancialManager financial;
    public MarketManager market;
    public IdolManager idol;
    public StaffManager staff;  // 追加
    public EventManager events; // 追加
    public UIManager uiManager;

    [Header("Game Status")]
    public int currentDay = 1;
    public bool isGameOver = false;

    void Start()
    {
        // 依存関係注入
        idol.Initialize(financial, market, staff, this);
        staff.Initialize(financial);
        events.Initialize(idol, financial);
        uiManager.Initialize(this);

        // 初期設定
        InitGame(50000000); // 5000万スタート
        market.UpdateTrendRandomly();
        UpdateUI();
    }

    void InitGame(long startCash)
    {
        financial.currentCash = startCash;
    }

    public void NextDay()
    {
        if (isGameOver) return;

        currentDay++;

        // 1. 金融処理
        financial.ProcessDailyTransactions(currentDay);

        // 2. 給料支払い（30日ごと）
        if (currentDay % 30 == 0)
        {
            staff.PayMonthlySalaries();
            // 利子払い等の固定費もここで
        }

        // 3. イベント発生チェック
        events.CheckDailyEvent();

        // 4. アイドル日次更新
        idol.DailyUpdate();

        // 5. ライブ当日チェック（予約システム）
        idol.CheckAndHoldLive(currentDay);

        // 6. 市場変動
        if (currentDay % 30 == 0) market.UpdateTrendRandomly();

        // 7. ゲームオーバー判定
        if (financial.currentCash < 0)
        {
            isGameOver = true;
            Debug.Log("【GAME OVER】破産しました...");
        }

        UpdateUI();
    }

    // ボタンアクション用
    public void OnClickLesson() { idol.DoLesson(); NextDay(); }
    public void OnClickPromo() { idol.DoPromotion(); NextDay(); }
    public void OnClickRest() { idol.DoRest(); NextDay(); }

    // 予約ボタン（UIから呼ぶ用：index 0=ライブハウス, 1=Zepp...）
    public void OnClickBookVenue(int index)
    {
        // 3ヶ月後（90日後）に予約を入れる仕様とする
        idol.BookVenue(index, 3);
        UpdateUI(); // 即金（手付金）が減るので更新
    }

    // スタッフ雇用ボタン（UIから呼ぶ用）
    public void OnClickHireTrainer()
    {
        staff.HireStaff(StaffType.Trainer, 1);
        UpdateUI();
    }

    public void UpdateUI()
    {
        uiManager.RefreshUI();
    }
}