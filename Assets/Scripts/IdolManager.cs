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

    private readonly string[] lastNames = { "佐藤", "鈴木", "高橋", "田中", "渡辺", "伊藤", "山本", "中村", "小林", "加藤", "星野", "天海", "如月", "渋谷", "本田" };
    private readonly string[] firstNames = { "愛", "未来", "さくら", "美咲", "花音", "七海", "遥", "彩花", "結衣", "莉子", "美月", "凛", "陽菜", "美優", "桃子" };

    public void Initialize(FinancialManager fm, MarketManager mm, StaffManager sm, GameManager gm)
    {
        financial = fm;
        market = mm;
        staffManager = sm;
        gameManager = gm;
        groupData = new IdolGroup();
        groupData.members.Clear();
        activeBookings.Clear();
    }

    public void SetGroupName(string name)
    {
        groupData.groupName = name;
    }

    public void SetGroupMembers(List<IdolMember> members)
    {
        groupData.members = members;

        if (members.Count > 0)
        {
            groupData.performance = (int)members.Average(m => (m.vocal + m.dance + m.visual) / 3);
            // ★追加：メンバー決定時に相性（ケミストリー）を計算
            CalculateGroupChemistry();
        }
    }

    // ★追加：ケミストリー計算
    // 全メンバーの組み合わせの相性値を合計し、平均を取る
    private void CalculateGroupChemistry()
    {
        if (groupData.members.Count <= 1)
        {
            groupData.chemistry = 10; // 1人なら平和
            return;
        }

        int totalScore = 0;
        int pairCount = 0;

        for (int i = 0; i < groupData.members.Count; i++)
        {
            for (int j = i + 1; j < groupData.members.Count; j++)
            {
                totalScore += GetCompatibilityScore(groupData.members[i].personality, groupData.members[j].personality);
                pairCount++;
            }
        }

        groupData.chemistry = totalScore / pairCount;
        Debug.Log($"Group Chemistry: {groupData.chemistry}");
    }

    // ★追加：性格相性マトリクス
    // 正の値なら相性が良い、負なら悪い
    private int GetCompatibilityScore(IdolPersonality p1, IdolPersonality p2)
    {
        // 同じ性格同士は基本わかりあえる(+5)
        if (p1 == p2) return 5;

        // 特定の組み合わせ
        // 元気(Energetic) <-> クール(Cool): 凸凹コンビで良い (+10)
        // 元気(Energetic) <-> 真面目(Serious): うるさいと思われる (-10)
        // 真面目(Serious) <-> 怠惰(Lazy): 許せない (-20)
        // 真面目(Serious) <-> 天使(Angel): 癒やされる (+10)
        // クール(Cool) <-> 天使(Angel): 調子が狂う (-5)
        // 怠惰(Lazy) <-> 元気(Energetic): 引っ張ってもらえる (+5)

        if (CheckPair(p1, p2, IdolPersonality.Energetic, IdolPersonality.Cool)) return 10;
        if (CheckPair(p1, p2, IdolPersonality.Energetic, IdolPersonality.Serious)) return -10;
        if (CheckPair(p1, p2, IdolPersonality.Serious, IdolPersonality.Lazy)) return -20;
        if (CheckPair(p1, p2, IdolPersonality.Serious, IdolPersonality.Angel)) return 10;
        if (CheckPair(p1, p2, IdolPersonality.Cool, IdolPersonality.Angel)) return -5;
        if (CheckPair(p1, p2, IdolPersonality.Lazy, IdolPersonality.Energetic)) return 5;

        return 0; // その他の組み合わせは普通
    }

    private bool CheckPair(IdolPersonality p1, IdolPersonality p2, IdolPersonality targetA, IdolPersonality targetB)
    {
        return (p1 == targetA && p2 == targetB) || (p1 == targetB && p2 == targetA);
    }

    public List<IdolMember> GenerateCandidates(int count)
    {
        List<IdolMember> candidates = new List<IdolMember>();
        for (int i = 0; i < count; i++)
        {
            IdolMember m = new IdolMember();
            m.lastName = lastNames[Random.Range(0, lastNames.Length)];
            m.firstName = firstNames[Random.Range(0, firstNames.Length)];
            m.birthMonth = Random.Range(1, 13);
            m.birthDay = Random.Range(1, 29);
            m.age = Random.Range(15, 23);

            m.vocal = Random.Range(1, 20);
            m.dance = Random.Range(1, 20);
            m.visual = Random.Range(1, 20);

            // ★追加：性格をランダム設定
            m.personality = (IdolPersonality)Random.Range(0, System.Enum.GetValues(typeof(IdolPersonality)).Length);

            candidates.Add(m);
        }
        return candidates;
    }

    public void CheckConditionEvents(DailyReport report)
    {
        if (groupData.fatigue >= 100 && groupData.hospitalDaysLeft <= 0)
        {
            groupData.hospitalDaysLeft = 3;
            long hospitalCost = 1000000;
            financial.currentCash -= hospitalCost;
            financial.dailyCashChange -= hospitalCost;
            groupData.fatigue = 50;
            report.AddLog($"<color=red>【緊急搬送】過労ダウン！ 入院費 -{hospitalCost:N0}円</color>");
            return;
        }

        if (groupData.mental <= 0 && groupData.runawayDaysLeft <= 0)
        {
            groupData.runawayDaysLeft = 5;
            long searchCost = 2000000;
            financial.currentCash -= searchCost;
            financial.dailyCashChange -= searchCost;
            groupData.mental = 30;
            int lostFans = (int)(groupData.fans * 0.3f);
            groupData.fans -= lostFans;
            report.AddLog($"<color=red>【失踪】メンバー音信不通。捜索費 -{searchCost:N0}円</color>");
            return;
        }

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

    public void DoLesson(DailyReport report)
    {
        int cost = 50000;
        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>資金不足でレッスンスタジオを借りられません。</color>");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        float bonus = staffManager.GetStaffBonus(StaffType.Trainer);
        float fatiguePenalty = (groupData.fatigue > 50) ? 0.5f : 1.0f;

        // ★追加：ケミストリーボーナス
        // ケミストリーが高いほど、相乗効果で成長しやすい
        // 例: ケミストリー10なら1.1倍、-20なら0.8倍
        float chemistryMultiplier = 1.0f + (groupData.chemistry * 0.01f);
        chemistryMultiplier = Mathf.Max(0.5f, chemistryMultiplier); // 下限あり

        int baseGrowth = Random.Range(1, 4);
        int growth = (int)(baseGrowth * bonus * fatiguePenalty * chemistryMultiplier);

        groupData.performance += growth;
        groupData.fatigue += 15;

        string chemLog = "";
        if (groupData.chemistry > 5) chemLog = "<color=orange>(相性良)</color>";
        if (groupData.chemistry < -5) chemLog = "<color=blue>(不仲...)</color>";

        report.AddLog($"[レッスン] 実力+{growth} {chemLog} / 費用 -{cost:N0}円");
    }

    public void DoPromotion(DailyReport report)
    {
        int cost = 100000;
        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>資金不足で広告が出せません。</color>");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        float bonus = staffManager.GetStaffBonus(StaffType.Marketer);
        float memberBonus = 1.0f + (groupData.memberCount * 0.05f);

        int fanIncrease = (int)(Random.Range(15, 30) * bonus * memberBonus);
        groupData.fans += fanIncrease;
        groupData.mental -= 5;
        groupData.fatigue += 5;

        report.AddLog($"[広告] ファン+{fanIncrease}人 / SNS・Web広告費 -{cost:N0}円");
    }

    public void DoRest(DailyReport report)
    {
        int costPerMember = 5000;
        long totalCost = costPerMember * groupData.memberCount;

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;

        groupData.fatigue = 0;
        groupData.mental = Mathf.Min(100, groupData.mental + 30);
        report.AddLog($"[休暇] 全員リフレッシュ / ケア費({groupData.memberCount}人分) -{totalCost:N0}円");
    }

    public void ProduceGoods(DailyReport report)
    {
        int unitCost = 500;
        int amount = 1000;
        long totalCost = unitCost * amount;

        if (financial.currentCash < totalCost)
        {
            report.AddLog("<color=red>[発注不可]</color> グッズ制作費が足りません。");
            return;
        }

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;
        groupData.goodsStock += amount;

        report.AddLog($"[グッズ] タオル・Tシャツ制作 (在庫+{amount}) / 制作費 -{totalCost:N0}円");
    }

    public void MakeMV(DailyReport report)
    {
        if (groupData.discography.Count == 0)
        {
            report.AddLog("MVを作る曲がありません！");
            return;
        }

        Song latestSong = groupData.discography.Last();
        if (latestSong.hasMV)
        {
            report.AddLog("最新曲のMVは既に制作済みです。");
            return;
        }

        long cost = 3000000 + (groupData.memberCount * 100000);

        if (financial.currentCash < cost)
        {
            report.AddLog($"<color=red>[制作不可]</color> MV制作費不足 ({cost:N0}円必要)");
            return;
        }

        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        latestSong.hasMV = true;
        int fanBoost = (int)(groupData.fans * 0.1f) + 500;
        groupData.fans += fanBoost;

        report.AddLog($"<color=cyan>[MV公開]</color> 『{latestSong.title}』MV完成！ ファン急増 +{fanBoost}人 / 制作費 -{cost:N0}円");
    }

    public void ChangeConcept(IdolGenre newGenre, DailyReport report)
    {
        long costPerMember = 300000;
        long totalCost = costPerMember * groupData.memberCount;

        if (financial.currentCash < totalCost)
        {
            report.AddLog($"<color=red>[変更不可]</color> 資金不足 (衣装費 {totalCost:N0}円必要)");
            return;
        }

        if (groupData.genre == newGenre) return;

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;

        int lostFans = (int)(groupData.fans * 0.2f);
        groupData.fans -= lostFans;
        groupData.performance = (int)(groupData.performance * 0.9f);

        IdolGenre oldGenre = groupData.genre;
        groupData.genre = newGenre;

        report.AddLog($"<color=yellow>[路線変更]</color> {oldGenre} -> {newGenre} / 新衣装費({groupData.memberCount}人分) -{totalCost:N0}円");
    }

    public void ProduceSong(int budgetTier, DailyReport report, string songTitle)
    {
        long songCost, costumeUnitCost, choreoCost;

        if (budgetTier == 0)
        {
            songCost = 500000;
            costumeUnitCost = 50000;
            choreoCost = 200000;
        }
        else
        {
            songCost = 2000000;
            costumeUnitCost = 200000;
            choreoCost = 1000000;
        }

        long totalCostumeCost = costumeUnitCost * groupData.memberCount;
        long totalCost = songCost + totalCostumeCost + choreoCost;

        if (financial.currentCash < totalCost)
        {
            report.AddLog($"<color=red>[制作不可]</color> 資金不足 ({totalCost:N0}円必要)");
            return;
        }

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;

        int budgetBonus = budgetTier == 0 ? Random.Range(10, 30) : Random.Range(40, 80);
        float trendBonus = market.GetMarketMultiplier(groupData.genre);
        int quality = (int)((groupData.performance + budgetBonus) * trendBonus);

        Song newSong = new Song();
        newSong.title = songTitle;
        newSong.genre = groupData.genre;
        newSong.quality = quality;
        newSong.releaseDay = gameManager.currentDay;
        newSong.totalSales = 0;
        newSong.peakRank = 100;
        newSong.hasMV = false;

        groupData.discography.Add(newSong);
        report.AddLog($"<color=green>[新曲リリース]</color> 『{newSong.title}』(Q:{quality})");
        report.AddLog($"[内訳] 楽曲:-{songCost:N0} 衣装({groupData.memberCount}人):-{totalCostumeCost:N0} 振付:-{choreoCost:N0}");
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

    public List<Venue> GetVenueList()
    {
        return new List<Venue>() {
            new Venue{ venueName="ライブハウス", capacity=300, baseCost=150000, minFansReq=0, maxSongs=3 },
            new Venue{ venueName="Zepp級ホール", capacity=2500, baseCost=2000000, minFansReq=1000, maxSongs=4 },
            new Venue{ venueName="武道館", capacity=10000, baseCost=10000000, minFansReq=8000, maxSongs=5 },
            new Venue{ venueName="東京ドーム", capacity=55000, baseCost=50000000, minFansReq=40000, maxSongs=6 }
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
            if (!groupData.IsAvailable())
            {
                report.AddLog($"<color=red>【公演中止】メンバー不在！</color>");
                report.AddLog($"[損害] 違約金 -{booking.venue.baseCost:N0}円");
                financial.currentCash -= booking.venue.baseCost;
                financial.dailyCashChange -= booking.venue.baseCost;
                activeBookings.Remove(booking);
                return;
            }

            HoldLive(booking, report);
            activeBookings.Remove(booking);
        }
    }

    public void AutoGenerateSetlist(VenueBooking booking)
    {
        if (groupData.discography.Count == 0) return;
        var bestSongs = groupData.discography.OrderByDescending(s => s.quality).Take(booking.venue.maxSongs).ToList();
        booking.setlist = bestSongs;
    }

    public void RegisterSetlist(VenueBooking booking, List<Song> selectedSongs)
    {
        booking.setlist.Clear();
        foreach (var song in selectedSongs)
        {
            if (booking.setlist.Count >= booking.venue.maxSongs) break;
            booking.setlist.Add(song);
        }
    }

    void HoldLive(VenueBooking booking, DailyReport report)
    {
        report.AddLog($"<color=cyan>★LIVE開催！ {booking.venue.venueName}★</color>");

        if (booking.setlist == null || booking.setlist.Count == 0) AutoGenerateSetlist(booking);

        long remainingCost = (long)(booking.venue.baseCost * 0.7f);
        financial.currentCash -= remainingCost;
        financial.dailyCashChange -= remainingCost;

        long staffCost = booking.venue.capacity * 200;

        long travelCostPerMember = 10000;
        long baseTravelCost = 30000;
        if (booking.venue.capacity > 1000) { travelCostPerMember = 50000; baseTravelCost = 200000; }

        long totalTravelCost = baseTravelCost + (travelCostPerMember * groupData.memberCount);

        financial.currentCash -= (staffCost + totalTravelCost);
        financial.dailyCashChange -= (staffCost + totalTravelCost);

        report.AddLog($"[支出] 会場費残金:-{remainingCost:N0}");
        report.AddLog($"[経費] スタッフ費:-{staffCost:N0} 交通宿泊費({groupData.memberCount}人分):-{totalTravelCost:N0}");

        float totalSetlistPower = 0;
        int songCount = 0;

        if (booking.setlist.Count == 0) totalSetlistPower = groupData.performance * 0.1f;
        else
        {
            foreach (var song in booking.setlist)
            {
                songCount++;
                float songScore = song.quality;
                if (song.genre == groupData.genre) songScore *= 1.2f;
                if ((gameManager.currentDay - song.releaseDay) <= 30) songScore *= 1.5f;
                totalSetlistPower += songScore;
            }
        }

        float basePerf = groupData.performance;
        if (songCount > 0) basePerf += (totalSetlistPower / 5.0f);
        float perfRate = basePerf / 20.0f;

        if (groupData.fatigue > 70) perfRate *= 0.6f;
        if (groupData.mental < 40) perfRate *= 0.8f;

        float trendBonus = market.GetMarketMultiplier(groupData.genre);
        int baseAudience = (int)(groupData.fans * perfRate * trendBonus);
        int actualAudience = Mathf.Min(baseAudience, booking.venue.capacity);

        int ticketPrice = 3000 + (booking.venue.capacity / 10);
        long ticketSales = (long)actualAudience * ticketPrice;

        long goodsSales = 0;
        if (groupData.goodsStock > 0)
        {
            int buyers = (int)(actualAudience * 0.3f);
            int soldCount = Mathf.Min(buyers, groupData.goodsStock);
            groupData.goodsStock -= soldCount;
            goodsSales = soldCount * 2000;
            report.AddLog($"[物販] グッズ売上 +{goodsSales:N0}円 ({soldCount}個販売)");
        }
        else
        {
            report.AddLog("<color=orange>[物販] グッズ在庫切れで機会損失...</color>");
        }

        long totalRevenue = ticketSales + goodsSales;
        financial.RegisterTransaction($"ライブ収益({booking.venue.venueName})", totalRevenue, gameManager.currentDay, 60);

        report.AddLog($"[動員] {actualAudience}人 / キャパ{booking.venue.capacity}");
        report.AddLog($"[売上予定] +{totalRevenue:N0}円 (60日後)");

        if (actualAudience >= booking.venue.capacity * 0.8f)
        {
            int newFans = (int)(actualAudience * 0.1f);
            groupData.fans += newFans;
            groupData.mental += 10;
            report.AddLog($"[成果] 大成功！ファン+{newFans}人");
        }
        else
        {
            groupData.mental -= 20;
            report.AddLog($"[成果] 空席多数...メンバー消沈。");
        }

        groupData.fatigue += 20 + (songCount * 5);

        if (booking.venue.venueName == "東京ドーム" && actualAudience > 50000)
        {
            groupData.hasDoneDome = true;
            report.AddLog("<color=magenta>【伝説】東京ドーム満員達成！！</color>");
        }
    }

    public long CalcMonthlyMemberCost()
    {
        return 150000 * groupData.memberCount;
    }

    public void DailyUpdate()
    {
        if (groupData.IsAvailable())
        {
            // ★追加：ケミストリーによる日々の変動
            // 仲が良いと毎日メンタル微回復、悪いと微減
            if (groupData.chemistry > 0)
            {
                groupData.mental = Mathf.Min(100, groupData.mental + 1);
            }
            else if (groupData.chemistry < -10)
            {
                groupData.mental = Mathf.Max(0, groupData.mental - 1);
            }

            groupData.fatigue = Mathf.Max(0, groupData.fatigue - 2);
        }
    }
}