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
        groupData = new IdolGroup();
        activeBookings.Clear();
    }

    // --- 既存アクション ---

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

    public void ChangeConcept(IdolGenre newGenre, DailyReport report)
    {
        int cost = 3000000;
        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>[変更不可]</color> 資金不足 (300万円必要)");
            return;
        }

        if (groupData.genre == newGenre) return;

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        int lostFans = (int)(groupData.fans * 0.2f);
        groupData.fans -= lostFans;
        groupData.performance = (int)(groupData.performance * 0.9f);

        IdolGenre oldGenre = groupData.genre;
        groupData.genre = newGenre;

        report.AddLog($"<color=yellow>[路線変更]</color> {oldGenre} -> {newGenre} へ転向。衣装費 -{cost:N0}円 / ファン減少 -{lostFans}人");
    }

    // --- ★新規追加：楽曲制作 ---

    public void ProduceSong(int budgetTier, DailyReport report)
    {
        // budgetTier: 0=低予算(100万), 1=高予算(500万)
        long cost = budgetTier == 0 ? 1000000 : 5000000;

        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>[制作不可]</color> 資金不足");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        // クオリティ計算: (実力 + 予算乱数) * トレンド補正
        int budgetBonus = budgetTier == 0 ? Random.Range(10, 30) : Random.Range(40, 80);
        float trendBonus = market.GetMarketMultiplier(groupData.genre);

        int quality = (int)((groupData.performance + budgetBonus) * trendBonus);

        // 楽曲生成
        Song newSong = new Song();
        newSong.title = $"Single #{groupData.discography.Count + 1}";
        newSong.genre = groupData.genre;
        newSong.quality = quality;
        newSong.releaseDay = gameManager.currentDay;
        newSong.totalSales = 0;
        newSong.peakRank = 100;

        groupData.discography.Add(newSong);

        report.AddLog($"<color=green>[新曲リリース]</color> 『{newSong.title}』 (Q:{quality}) 制作費 -{cost:N0}円");
    }

    // --- ★新規追加：週間売上処理 ---

    public void ProcessWeeklySales(DailyReport report)
    {
        if (groupData.discography.Count == 0) return;

        long totalRoyalties = 0;
        report.AddLog("=== 週間ランキング集計 ===");

        foreach (var song in groupData.discography)
        {
            // 売上計算ロジック
            // 基本売上 = クオリティ * ファン数 * 0.05
            // 補正 = トレンド * 発売からの鮮度(Momentum)
            float momentum = song.GetCurrentMomentum(gameManager.currentDay);
            float marketMul = market.GetMarketMultiplier(song.genre);

            long sales = (long)(song.quality * groupData.fans * 0.05f * momentum * marketMul);

            // 氷河期なら激減
            if (market.isIceAge) sales /= 2;

            song.totalSales += sales;

            // 順位判定（簡易ロジック：売上枚数で判定）
            int rank = 100;
            if (sales > 100000) rank = 1;
            else if (sales > 50000) rank = Random.Range(2, 10);
            else if (sales > 10000) rank = Random.Range(11, 50);
            else rank = Random.Range(51, 100);

            if (rank < song.peakRank) song.peakRank = rank;

            // 印税（売上の10%が入ると仮定）
            long royalty = (long)(sales * 100); // 1枚単価1000円の10%
            totalRoyalties += royalty;

            if (rank <= 50) // ランク圏外はログ省略
            {
                report.AddLog($"『{song.title}』 {rank}位 (売上{sales:N0}枚)");
            }
        }

        // 印税は即金で入る（と仮定して資金繰りを助ける）
        if (totalRoyalties > 0)
        {
            financial.currentCash += totalRoyalties;
            financial.dailyCashChange += totalRoyalties;
            report.AddLog($"<color=yellow>[印税収入]</color> +{totalRoyalties:N0}円");
        }
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
            report.AddLog($"<color=red>[予約不可]</color> ファン不足");
            return;
        }

        long deposit = (long)(targetVenue.baseCost * 0.3f);
        if (financial.currentCash < deposit)
        {
            report.AddLog($"<color=red>[予約不可]</color> 手付金不足");
            return;
        }

        financial.currentCash -= deposit;
        financial.dailyCashChange -= deposit;

        VenueBooking booking = new VenueBooking();
        booking.venue = targetVenue;
        booking.eventDay = targetDay;
        activeBookings.Add(booking);

        report.AddLog($"[予約] {targetVenue.venueName}を予約。手付金 -{deposit:N0}円");
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
            report.AddLog($"[成果] 空席多数...メンバー消沈。");
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