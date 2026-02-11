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

    // --- ★追加：状態異常のチェック（毎日実行） ---
    public void CheckConditionEvents(DailyReport report)
    {
        // 1. 入院チェック (疲労100超え)
        if (groupData.fatigue >= 100 && groupData.hospitalDaysLeft <= 0)
        {
            groupData.hospitalDaysLeft = 3; // 3日間入院
            long hospitalCost = 1000000;
            financial.currentCash -= hospitalCost;
            financial.dailyCashChange -= hospitalCost;
            groupData.fatigue = 50; // 少し回復するが万全ではない
            report.AddLog($"<color=red>【緊急搬送】メンバーが過労で倒れました！！</color>");
            report.AddLog($"[ペナルティ] 3日間行動不能 / 入院費 -{hospitalCost:N0}円");
            return;
        }

        // 2. 失踪チェック (メンタル0以下)
        if (groupData.mental <= 0 && groupData.runawayDaysLeft <= 0)
        {
            groupData.runawayDaysLeft = 5; // 5日間失踪
            long searchCost = 2000000; // 捜索費
            financial.currentCash -= searchCost;
            financial.dailyCashChange -= searchCost;
            groupData.mental = 30; // 説得して戻る
            int lostFans = (int)(groupData.fans * 0.3f); // ファン3割減
            groupData.fans -= lostFans;

            report.AddLog($"<color=red>【失踪】メンバーと連絡が取れません！！</color>");
            report.AddLog($"[ペナルティ] 5日間行動不能 / ファン減少 -{lostFans}人 / 捜索費 -{searchCost:N0}円");
            return;
        }

        // 3. 入院・失踪中の処理
        if (groupData.hospitalDaysLeft > 0)
        {
            groupData.hospitalDaysLeft--;
            report.AddLog($"<color=grey>[入院中] 復帰まであと {groupData.hospitalDaysLeft + 1}日...</color>");
        }
        
        if (groupData.runawayDaysLeft > 0)
        {
            groupData.runawayDaysLeft--;
            report.AddLog($"<color=grey>[捜索中] 発見まであと {groupData.runawayDaysLeft + 1}日...</color>");
        }
    }

    // --- アクション ---

    public void DoLesson(DailyReport report)
    {
        // 疲労による事故判定
        if (groupData.fatigue > 70)
        {
            // 30%の確率で怪我
            if (Random.Range(0, 100) < 30)
            {
                int injuryCost = 200000;
                financial.currentCash -= injuryCost;
                financial.dailyCashChange -= injuryCost;
                groupData.fatigue += 10;
                groupData.mental -= 10;
                report.AddLog($"<color=red>[事故]</color> 疲労で怪我をしました！ 治療費 -{injuryCost:N0}円");
                return;
            }
        }

        int cost = 50000;
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        float bonus = staffManager.GetStaffBonus(StaffType.Trainer);
        
        // 疲労が高いと効率ダウン
        float fatiguePenalty = (groupData.fatigue > 50) ? 0.5f : 1.0f;

        int growth = (int)(Random.Range(1, 4) * bonus * fatiguePenalty);
        groupData.performance += growth;
        groupData.fatigue += 15;
        
        report.AddLog($"[レッスン] 実力+{growth} (疲労+15) / 費用 -{cost:N0}円");
        if(fatiguePenalty < 1.0f) report.AddLog("<color=orange>※疲労で効率が落ちています</color>");
    }

    public void DoPromotion(DailyReport report)
    {
        // メンタルによる塩対応判定
        if (groupData.mental < 30)
        {
            if (Random.Range(0, 100) < 40)
            {
                int decrease = (int)(groupData.fans * 0.05f);
                groupData.fans -= decrease;
                groupData.fatigue += 5;
                groupData.mental -= 5;
                report.AddLog($"<color=red>[炎上]</color> メンタル不安定で塩対応をしてしまいました... ファン -{decrease}人");
                return;
            }
        }

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
        groupData.mental = Mathf.Min(100, groupData.mental + 30); // 回復量UP
        int cost = 10000;
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        report.AddLog($"[休暇] 完全リフレッシュ (メンタル+30 / 疲労0) / ケア費 -{cost:N0}円");
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

    public void ProduceSong(int budgetTier, DailyReport report)
    {
        long cost = budgetTier == 0 ? 1000000 : 5000000;
        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>[制作不可]</color> 資金不足");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        int budgetBonus = budgetTier == 0 ? Random.Range(10, 30) : Random.Range(40, 80);
        float trendBonus = market.GetMarketMultiplier(groupData.genre);
        int quality = (int)((groupData.performance + budgetBonus) * trendBonus);
        
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

    public void ProcessWeeklySales(DailyReport report)
    {
        if (groupData.discography.Count == 0) return;
        long totalRoyalties = 0;
        report.AddLog("=== 週間ランキング集計 ===");

        foreach (var song in groupData.discography)
        {
            float momentum = song.GetCurrentMomentum(gameManager.currentDay);
            float marketMul = market.GetMarketMultiplier(song.genre);
            long sales = (long)(song.quality * groupData.fans * 0.05f * momentum * marketMul);
            if (market.isIceAge) sales /= 2;

            song.totalSales += sales;
            
            int rank = 100;
            if (sales > 100000) rank = 1;
            else if (sales > 50000) rank = Random.Range(2, 10);
            else if (sales > 10000) rank = Random.Range(11, 50);
            else rank = Random.Range(51, 100);

            if (rank < song.peakRank) song.peakRank = rank;

            long royalty = (long)(sales * 100); 
            totalRoyalties += royalty;

            if (rank <= 50) report.AddLog($"『{song.title}』 {rank}位 (売上{sales:N0}枚)");
        }

        if (totalRoyalties > 0)
        {
            financial.currentCash += totalRoyalties;
            financial.dailyCashChange += totalRoyalties;
            report.AddLog($"<color=yellow>[印税収入]</color> +{totalRoyalties:N0}円");
        }
    }

    // --- 予約・ライブシステム ---

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
            // 入院/失踪中はライブ中止（違約金100%）
            if (!groupData.IsAvailable())
            {
                report.AddLog($"<color=red>【公演中止】メンバー不在のためライブが中止になりました！</color>");
                report.AddLog($"[損害] 違約金（会場費全額） -{booking.venue.baseCost:N0}円");
                financial.currentCash -= booking.venue.baseCost;
                financial.dailyCashChange -= booking.venue.baseCost;
                activeBookings.Remove(booking);
                return;
            }

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
        
        // 疲労によるパフォーマンス低下
        if (groupData.fatigue > 70) perfRate *= 0.6f;
        if (groupData.mental < 40) perfRate *= 0.8f;

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
            groupData.mental += 10; // 成功で回復
            report.AddLog($"[成果] 大成功！ファン+{newFans}人");
        }
        else
        {
            groupData.mental -= 20; // 失敗で激減
            report.AddLog($"[成果] 空席多数...メンバー消沈。");
        }
        
        groupData.fatigue += 30; // ライブは疲れる

        if (booking.venue.venueName == "東京ドーム" && actualAudience > 50000)
        {
            groupData.hasDoneDome = true;
            report.AddLog("<color=magenta>【伝説】東京ドーム満員達成！！</color>");
        }
    }

    public void DailyUpdate()
    {
        // 入院・失踪中でなければ少し回復
        if (groupData.IsAvailable())
        {
            groupData.fatigue = Mathf.Max(0, groupData.fatigue - 2);
        }
    }
}