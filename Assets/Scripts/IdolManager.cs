using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IdolManager : MonoBehaviour
{
    public IdolGroup groupData = new IdolGroup();
    public List<VenueBooking> activeBookings = new List<VenueBooking>();

    private FinancialManager financial;
    private MarketManager market;
    private StaffManager staffManager;
    private GameManager gameManager;

    public void Initialize(FinancialManager fm, MarketManager mm, StaffManager sm, GameManager gm)
    {
        financial = fm;
        market = mm;
        staffManager = sm;
        gameManager = gm;

        // データリセット
        groupData = new IdolGroup();
        activeBookings.Clear();
    }

    // --- 既存のアクション ---

    public void DoLesson(DailyReport report)
    {
        int cost = 50000;
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        float bonus = staffManager.GetStaffBonus(StaffType.Trainer);
        int growth = (int)(Random.Range(1, 4) * bonus);

        groupData.performance += growth;
        groupData.fatigue += 15;

        report.AddLog($"[レッスン] 実力+{growth} (疲労+15) / 費用 -{cost:N0}円");
    }

    public void DoPromotion(DailyReport report)
    {
        int cost = 30000;
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        float bonus = staffManager.GetStaffBonus(StaffType.Marketer);
        int fanIncrease = (int)(Random.Range(10, 20) * bonus);

        groupData.fans += fanIncrease;
        groupData.mental -= 5;
        groupData.fatigue += 5;

        report.AddLog($"[営業] ファン+{fanIncrease}人 (メンタル-5) / 費用 -{cost:N0}円");
    }

    public void DoRest(DailyReport report)
    {
        groupData.fatigue = 0;
        groupData.mental = Mathf.Min(100, groupData.mental + 20);
        int cost = 10000;
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        report.AddLog($"[休暇] 完全リフレッシュ (メンタル回復) / ケア費 -{cost:N0}円");
    }

    // --- 新規追加：コンセプト変更（ピボット） ---

    public void ChangeConcept(IdolGenre newGenre, DailyReport report)
    {
        // 変更コスト：高額（衣装や楽曲の作り直し）
        int cost = 3000000; // 300万円

        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>[変更不可]</color> 資金が足りません (300万円必要)");
            return;
        }

        if (groupData.genre == newGenre)
        {
            report.AddLog("既にそのジャンルです。");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        // ペナルティ：古参ファンが離れる
        int lostFans = (int)(groupData.fans * 0.2f); // 20%減少
        groupData.fans -= lostFans;

        // ペナルティ：実力が少し落ちる（慣れないジャンルのため）
        groupData.performance = (int)(groupData.performance * 0.9f);

        IdolGenre oldGenre = groupData.genre;
        groupData.genre = newGenre;

        report.AddLog($"<color=yellow>[路線変更]</color> {oldGenre} -> {newGenre} へ転向しました。");
        report.AddLog($"[影響] 衣装制作費 -{cost:N0}円 / ファン減少 -{lostFans}人");
    }

    // --- 予約・ライブシステム（既存） ---

    public List<Venue> GetVenueList()
    {
        return new List<Venue>() {
            new Venue{ venueName="ライブハウス", capacity=300, baseCost=150000, minFansReq=0 },
            new Venue{ venueName="Zepp級ホール", capacity=2500, baseCost=2000000, minFansReq=2000 },
            new Venue{ venueName="武道館", capacity=10000, baseCost=10000000, minFansReq=8000 },
            new Venue{ venueName="東京ドーム", capacity=55000, baseCost=50000000, minFansReq=40000 }
        };
    }

    public void BookVenue(int venueIndex, int monthsLater, DailyReport report)
    {
        Venue targetVenue = GetVenueList()[venueIndex];
        int targetDay = gameManager.currentDay + (monthsLater * 30);

        if (groupData.fans < targetVenue.minFansReq)
        {
            report.AddLog($"<color=red>[予約不可]</color> ファン不足 ({targetVenue.minFansReq}人必要)");
            return;
        }

        long deposit = (long)(targetVenue.baseCost * 0.3f);
        if (financial.currentCash < deposit)
        {
            report.AddLog($"<color=red>[予約不可]</color> 手付金不足 ({deposit:N0}円必要)");
            return;
        }

        financial.currentCash -= deposit;
        financial.dailyCashChange -= deposit;

        VenueBooking booking = new VenueBooking();
        booking.venue = targetVenue;
        booking.eventDay = targetDay;
        activeBookings.Add(booking);

        report.AddLog($"[予約] {targetVenue.venueName}を{monthsLater}ヶ月後に予約。手付金 -{deposit:N0}円");
    }

    public void CheckAndHoldLive(int today, DailyReport report)
    {
        var booking = activeBookings.FirstOrDefault(b => b.eventDay == today && !b.isCanceled);
        if (booking != null)
        {
            HoldLive(booking, report);
            activeBookings.Remove(booking);
        }
    }

    void HoldLive(VenueBooking booking, DailyReport report)
    {
        report.AddLog($"<color=cyan>★LIVE開催！ {booking.venue.venueName}★</color>");

        long remainingCost = (long)(booking.venue.baseCost * 0.7f);
        financial.currentCash -= remainingCost;
        financial.dailyCashChange -= remainingCost;
        report.AddLog($"[支出] 会場費残金: -{remainingCost:N0}円");

        float trendBonus = market.GetMarketMultiplier(groupData.genre);
        float perfRate = groupData.performance / 20.0f;
        if (groupData.fatigue > 80) perfRate *= 0.5f;

        int baseAudience = (int)(groupData.fans * perfRate * trendBonus);
        int actualAudience = Mathf.Min(baseAudience, booking.venue.capacity);

        int ticketPrice = 3000 + (booking.venue.capacity / 10);
        long totalSales = (long)actualAudience * ticketPrice + ((long)actualAudience * 2000);

        financial.RegisterTransaction($"ライブ売上({booking.venue.venueName})", totalSales, gameManager.currentDay, 60);

        report.AddLog($"[動員] {actualAudience}人 / キャパ{booking.venue.capacity}人");
        report.AddLog($"[売上予約] +{totalSales:N0}円 (60日後入金)");

        if (actualAudience >= booking.venue.capacity * 0.8f)
        {
            int newFans = (int)(actualAudience * 0.1f);
            groupData.fans += newFans;
            report.AddLog($"[成果] 大成功！ファン+{newFans}人");
        }
        else
        {
            groupData.mental -= 10;
            report.AddLog($"[成果] 空席多数...メンバーが落ち込んでいます。");
        }

        if (booking.venue.venueName == "東京ドーム" && actualAudience > 50000)
        {
            groupData.hasDoneDome = true;
            report.AddLog("<color=magenta>【伝説】東京ドーム満員達成！！</color>");
        }
    }

    public void DailyUpdate()
    {
        groupData.fatigue = Mathf.Max(0, groupData.fatigue - 2);
    }
}