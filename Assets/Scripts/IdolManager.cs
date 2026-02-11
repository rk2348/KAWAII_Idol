using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IdolManager : MonoBehaviour
{
    public IdolGroup groupData = new IdolGroup();

    // 予約リスト
    public List<VenueBooking> activeBookings = new List<VenueBooking>();

    private FinancialManager financial;
    private MarketManager market;
    private StaffManager staffManager; // 追加
    private GameManager gameManager;

    public void Initialize(FinancialManager fm, MarketManager mm, StaffManager sm, GameManager gm)
    {
        financial = fm;
        market = mm;
        staffManager = sm;
        gameManager = gm;
    }

    // 行動：レッスン（スタッフ補正あり）
    public void DoLesson()
    {
        int cost = 50000;
        financial.currentCash -= cost;

        // トレーナーの効果を適用
        float bonus = staffManager.GetStaffBonus(StaffType.Trainer);

        int growth = (int)(Random.Range(1, 4) * bonus);
        groupData.performance += growth;
        groupData.fatigue += 15;

        Debug.Log($"レッスン(効果{bonus}倍)：実力+{growth} / 疲労+15");
    }

    // 行動：営業（スタッフ補正あり）
    public void DoPromotion()
    {
        int cost = 30000;
        financial.currentCash -= cost;

        // マーケターの効果を適用
        float bonus = staffManager.GetStaffBonus(StaffType.Marketer);

        int fanIncrease = (int)(Random.Range(10, 20) * bonus);
        groupData.fans += fanIncrease;
        groupData.mental -= 5;
        groupData.fatigue += 5;

        Debug.Log($"営業(効果{bonus}倍)：ファン+{fanIncrease} / メンタル-5");
    }

    // 行動：休息（メンタル・疲労回復）
    public void DoRest()
    {
        groupData.fatigue = 0;
        groupData.mental = Mathf.Min(100, groupData.mental + 20);
        financial.currentCash -= 10000; // ケア費用
        Debug.Log("完全休養：リフレッシュしました。");
    }

    // --- 新：会場予約システム ---

    // 会場リスト（本来は外部データから読む）
    public List<Venue> GetVenueList()
    {
        return new List<Venue>() {
            new Venue{ venueName="ライブハウス", capacity=300, baseCost=150000, minFansReq=0 },
            new Venue{ venueName="Zepp級ホール", capacity=2500, baseCost=2000000, minFansReq=2000 },
            new Venue{ venueName="武道館", capacity=10000, baseCost=10000000, minFansReq=8000 },
            new Venue{ venueName="東京ドーム", capacity=55000, baseCost=50000000, minFansReq=40000 }
        };
    }

    // 予約実行（手付金を払って予約リストに追加）
    public void BookVenue(int venueIndex, int monthsLater)
    {
        Venue targetVenue = GetVenueList()[venueIndex];
        int targetDay = gameManager.currentDay + (monthsLater * 30);

        // ファン数足切りチェック
        if (groupData.fans < targetVenue.minFansReq)
        {
            Debug.LogError($"ファン不足で予約できません。（必要数: {targetVenue.minFansReq}）");
            return;
        }

        // 手付金（30%）
        long deposit = (long)(targetVenue.baseCost * 0.3f);
        if (financial.currentCash < deposit)
        {
            Debug.LogError("手付金が足りません！");
            return;
        }

        financial.currentCash -= deposit;

        // 予約作成
        VenueBooking booking = new VenueBooking();
        booking.venue = targetVenue;
        booking.eventDay = targetDay;
        booking.isCanceled = false;

        activeBookings.Add(booking);

        Debug.Log($"【予約完了】{targetVenue.venueName} を {monthsLater}ヶ月後(Day {targetDay}) に予約しました。手付金: -{deposit:N0}円");
    }

    // 当日のライブ開催チェック（GameManagerから毎朝呼ばれる）
    public void CheckAndHoldLive(int today)
    {
        // 今日の予約があるか？
        var booking = activeBookings.FirstOrDefault(b => b.eventDay == today && !b.isCanceled);

        if (booking != null)
        {
            HoldLive(booking);
            activeBookings.Remove(booking); // 完了したので削除
        }
    }

    // ライブ本番処理
    void HoldLive(VenueBooking booking)
    {
        Debug.Log($"<color=cyan>★本日開催！ {booking.venue.venueName} ライブ！★</color>");

        // 1. 残金支払い（70%）
        long remainingCost = (long)(booking.venue.baseCost * 0.7f);
        financial.currentCash -= remainingCost;
        Debug.Log($"会場費残金支払: -{remainingCost:N0}円");

        // 2. 集客と売上計算
        float trendBonus = market.GetMarketMultiplier(groupData.genre);

        // パフォーマンスが低いと事故る
        float perfRate = groupData.performance / 20.0f; // 例: Perf20で1.0倍

        // 疲労度によるデバフ
        if (groupData.fatigue > 80) perfRate *= 0.5f;

        int baseAudience = (int)(groupData.fans * perfRate * trendBonus);
        int actualAudience = Mathf.Min(baseAudience, booking.venue.capacity);

        // チケット単価（キャパに応じて変動させる簡易ロジック）
        int ticketPrice = 3000 + (booking.venue.capacity / 10);

        long totalSales = (long)actualAudience * ticketPrice + ((long)actualAudience * 2000); // 物販2000円/人

        // 3. 入金予約（60日サイト）
        financial.RegisterTransaction($"ライブ売上({booking.venue.venueName})", totalSales, gameManager.currentDay, 60);

        // 4. ファン増減
        if (actualAudience >= booking.venue.capacity * 0.8f)
        {
            int newFans = (int)(actualAudience * 0.1f);
            groupData.fans += newFans;
            Debug.Log($"ライブ大成功！ファンが {newFans}人 増えた！");
        }
        else
        {
            Debug.LogWarning("空席が目立ちました...ファンが増えません。");
            groupData.mental -= 10;
        }

        // ドーム判定
        if (booking.venue.venueName == "東京ドーム" && actualAudience > 50000)
        {
            groupData.hasDoneDome = true;
        }
    }

    public void DailyUpdate()
    {
        // 疲労は自然には回復しない（休息コマンドが必要）
    }
}