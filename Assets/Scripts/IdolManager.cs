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

    // ★追加：クリエイターリスト
    private List<Creator> composers = new List<Creator>();
    private List<Creator> choreographers = new List<Creator>();
    private List<Creator> designers = new List<Creator>();

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

        // ★追加：クリエイターデータの初期化
        InitCreators();
    }

    // ★追加：クリエイター定義
    void InitCreators()
    {
        composers.Add(new Creator("新人ボカロP", CreatorType.Composer, 500000, 20, 30));
        composers.Add(new Creator("有名作曲家H", CreatorType.Composer, 2000000, 60, 10)); // クオリティ高いがSNSは普通

        choreographers.Add(new Creator("近所のダンサー", CreatorType.Choreographer, 200000, 10, 10));
        choreographers.Add(new Creator("カリスマ振付師M", CreatorType.Choreographer, 1000000, 40, 50)); // SNSバズり特化

        designers.Add(new Creator("既製服リメイク", CreatorType.CostumeDesigner, 50000, 5, 5));
        designers.Add(new Creator("有名ブランド特注", CreatorType.CostumeDesigner, 200000, 30, 20));
    }

    // ★追加：予算ランクに応じたクリエイター取得（UI簡略化のため）
    Creator GetCreator(List<Creator> list, int tier)
    {
        if (tier >= list.Count)
        {
            return list.Last();
        }
        return list[tier];
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
            CalculateGroupChemistry();
        }
    }

    private void CalculateGroupChemistry()
    {
        if (groupData.members.Count <= 1)
        {
            groupData.chemistry = 10;
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

    private int GetCompatibilityScore(IdolPersonality p1, IdolPersonality p2)
    {
        if (p1 == p2) return 5;

        if (CheckPair(p1, p2, IdolPersonality.Energetic, IdolPersonality.Cool)) return 10;
        if (CheckPair(p1, p2, IdolPersonality.Energetic, IdolPersonality.Serious)) return -10;
        if (CheckPair(p1, p2, IdolPersonality.Serious, IdolPersonality.Lazy)) return -20;
        if (CheckPair(p1, p2, IdolPersonality.Serious, IdolPersonality.Angel)) return 10;
        if (CheckPair(p1, p2, IdolPersonality.Cool, IdolPersonality.Angel)) return -5;
        if (CheckPair(p1, p2, IdolPersonality.Lazy, IdolPersonality.Energetic)) return 5;

        return 0;
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

            // 失踪時はライト層が真っ先に離れる
            int lostFans = (int)(groupData.fansLight * 0.3f);
            groupData.fansLight -= lostFans;

            report.AddLog($"<color=red>【失踪】メンバー音信不通。捜索費 -{searchCost:N0}円</color>");
            report.AddLog($"不信感により新規ファン離脱: -{lostFans}人");
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

        // 厄介ファントラブルのチェック
        CheckYakkaiTrouble(report);
    }

    private void CheckYakkaiTrouble(DailyReport report)
    {
        if (groupData.fansYakkai <= 0) return;

        // 厄介ファンの数に応じて発生確率上昇 (最大5%)
        float troubleChance = Mathf.Min(5.0f, groupData.fansYakkai * 0.05f);

        if (Random.Range(0f, 100f) < troubleChance)
        {
            int type = Random.Range(0, 3);
            switch (type)
            {
                case 0: // つきまとい
                    report.AddLog("<color=red>【厄介】一部ファンのつきまとい被害が発生。</color>");
                    report.AddLog("恐怖でメンバーのメンタル -15");
                    groupData.mental -= 15;
                    break;
                case 1: // イベント妨害
                    long securityCost = 500000;
                    financial.currentCash -= securityCost;
                    financial.dailyCashChange -= securityCost;
                    report.AddLog($"<color=red>【厄介】イベント妨害予告。警備強化費 -{securityCost:N0}円</color>");
                    break;
                case 2: // デマ拡散
                    int lostLight = (int)(groupData.fansLight * 0.1f);
                    groupData.fansLight -= lostLight;
                    report.AddLog($"<color=red>【厄介】根拠のないデマが拡散され、新規ファンが幻滅... ファン-{lostLight}人</color>");
                    break;
            }
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

        float chemistryMultiplier = 1.0f + (groupData.chemistry * 0.01f);
        chemistryMultiplier = Mathf.Max(0.5f, chemistryMultiplier);

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

        // 広告で増えるのは主にライト層
        groupData.fansLight += fanIncrease;

        groupData.mental -= 5;
        groupData.fatigue += 5;

        report.AddLog($"[広告] 新規ファン(Light)+{fanIncrease}人 / 広告費 -{cost:N0}円");
    }

    public void DoSNSPromotion(DailyReport report)
    {
        if (groupData.discography.Count == 0)
        {
            report.AddLog("<color=red>[投稿不可]</color> 動画に使用する楽曲がありません。");
            return;
        }

        Song targetSong = groupData.discography.Last(); // 最新曲を使用

        // 撮影・編集経費
        int cost = 30000;
        if (financial.currentCash < cost)
        {
            report.AddLog("<color=red>資金不足で動画制作ができません。</color>");
            return;
        }
        financial.currentCash -= cost;
        financial.dailyCashChange -= cost;

        // 動画担当メンバーをランダムに1人選出
        IdolMember actor = groupData.members[Random.Range(0, groupData.members.Count)];

        // 1. 基本魅力値: 楽曲のSNS適性
        float baseAppeal = targetSong.snsAppeal;

        // 2. 性格ボーナス
        float personalityMultiplier = 1.0f;
        string personalityLog = "";

        switch (actor.personality)
        {
            case IdolPersonality.Energetic: // 元気: TikTokなどのノリに強い
                personalityMultiplier = 1.5f;
                personalityLog = "(元気◎)";
                break;
            case IdolPersonality.Angel: // 天使: 愛嬌で伸びる
                personalityMultiplier = 1.3f;
                personalityLog = "(天使○)";
                break;
            case IdolPersonality.Serious: // 真面目: 硬くなりがち
                personalityMultiplier = 0.8f;
                personalityLog = "(真面目△)";
                break;
            case IdolPersonality.Cool: // クール: 普通
                personalityMultiplier = 1.0f;
                break;
            case IdolPersonality.Lazy: // 怠惰: ギャンブル要素
                // 30%で「脱力ダンス」が大バズり、70%でやる気なし
                if (Random.Range(0, 100) < 30)
                {
                    personalityMultiplier = 2.5f;
                    personalityLog = "<color=magenta>(脱力バズ！)</color>";
                }
                else
                {
                    personalityMultiplier = 0.4f;
                    personalityLog = "(サボり...)";
                }
                break;
        }

        // 3. トレンドボーナス
        float trendMultiplier = market.GetMarketMultiplier(targetSong.genre);
        string trendLog = trendMultiplier > 1.0f ? "<color=orange>(流行?)</color>" : "";

        // 4. 計算
        float totalScore = baseAppeal * personalityMultiplier * trendMultiplier * Random.Range(0.8f, 1.2f);

        // 結果判定
        int newFans = 0;
        string resultMsg = "";

        if (totalScore > 120)
        {
            newFans = Random.Range(500, 1000);
            resultMsg = "<color=magenta>【大バズり】</color> おすすめに掲載！ファン急増！";
            // バズると一定数厄介も混ざる
            int newYakkai = Random.Range(1, 5);
            groupData.fansYakkai += newYakkai;
            resultMsg += $" (厄介+{newYakkai}発生)";
        }
        else if (totalScore > 60)
        {
            newFans = Random.Range(50, 150);
            resultMsg = "【好評】 まあまあの再生数。";
        }
        else
        {
            newFans = Random.Range(5, 20);
            resultMsg = "【不発】 あまり伸びませんでした...";
        }

        // スタッフ(マーケター)の効果も少し乗せる
        float marketerBonus = staffManager.GetStaffBonus(StaffType.Marketer);
        newFans = (int)(newFans * marketerBonus);

        // 基本的にSNSで増えるのはライト層
        groupData.fansLight += newFans;
        groupData.fatigue += 10; // 動画撮影疲れ

        report.AddLog($"[SNS] 『{targetSong.title}』で踊ってみた投稿 (担当:{actor.firstName})");
        report.AddLog($"{resultMsg} 新規+{newFans}人 {personalityLog}{trendLog}");
    }

    public void DoRest(DailyReport report)
    {
        int costPerMember = 5000;
        long totalCost = costPerMember * groupData.memberCount;

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;

        float managerBonus = staffManager.GetStaffBonus(StaffType.Manager);
        int baseRecovery = 30;
        int actualRecovery = (int)(baseRecovery * managerBonus);

        groupData.fatigue = 0;
        groupData.mental = Mathf.Min(100, groupData.mental + actualRecovery);

        string managerLog = managerBonus > 1.0f ? $"<color=green>(Mg効果+{actualRecovery - baseRecovery})</color>" : "";
        report.AddLog($"[休暇] 全員リフレッシュ ({actualRecovery}回復) {managerLog} / ケア費 -{totalCost:N0}円");
    }

    public void DoChekiEvent(DailyReport report)
    {
        int setupCost = 100000; // 簡易な会場設営や警備費
        if (financial.currentCash < setupCost)
        {
            report.AddLog("<color=red>資金不足で特典会の会場が手配できません。</color>");
            return;
        }

        financial.currentCash -= setupCost;
        financial.dailyCashChange -= setupCost;

        float totalSatisfaction = 0f;
        int totalMentalDamage = 0;
        int totalFatigueDamage = 0;
        bool hasSaltResponse = false;

        // メンバーそれぞれの性格に基づく影響を加算
        foreach (var member in groupData.members)
        {
            switch (member.personality)
            {
                case IdolPersonality.Energetic: // 元気：満足度少しUP、メンタル消費普通、疲労普通
                    totalSatisfaction += 1.1f;
                    totalMentalDamage += 10;
                    totalFatigueDamage += 15;
                    break;
                case IdolPersonality.Serious:   // 真面目：満足度標準、メンタル消費やや大、疲労少なめ
                    totalSatisfaction += 1.0f;
                    totalMentalDamage += 15;
                    totalFatigueDamage += 10;
                    break;
                case IdolPersonality.Cool:      // クール：満足度やや下がるが、メンタルも疲労も削られにくい
                    totalSatisfaction += 0.8f;
                    totalMentalDamage += 5;
                    totalFatigueDamage += 5;
                    break;
                case IdolPersonality.Lazy:      // 怠惰：満足度低い。20%の確率で塩対応による炎上発生
                    totalSatisfaction += 0.7f;
                    totalMentalDamage += 10;
                    totalFatigueDamage += 10;
                    if (Random.Range(0, 100) < 20)
                    {
                        hasSaltResponse = true;
                    }
                    break;
                case IdolPersonality.Angel:     // 天使：満足度大幅UP（神対応）だが、メンタル激減り
                    totalSatisfaction += 1.3f;
                    totalMentalDamage += 25;
                    totalFatigueDamage += 20;
                    break;
            }
        }

        // グループ全体の平均値をとる
        float avgSatisfaction = totalSatisfaction / groupData.memberCount;
        int avgMentalDmg = totalMentalDamage / groupData.memberCount;
        int avgFatigueDmg = totalFatigueDamage / groupData.memberCount;

        // 特典会参加人数はコア層が中心
        int participants = (int)(groupData.fansCore * 0.3f) + (int)(groupData.fansLight * 0.05f) + (int)(groupData.fansYakkai * 0.1f);
        if (participants < 10) participants = 10; // 最低保証

        // チェキ単価2000円 × 参加者 × 満足度による売上補正
        long sales = (long)(participants * 2000 * avgSatisfaction);

        financial.currentCash += sales;
        financial.dailyCashChange += sales;

        groupData.mental -= avgMentalDmg;
        groupData.fatigue += avgFatigueDmg;

        report.AddLog($"[特典会] {participants}人参加 (売上+{sales:N0}円)");

        // イベント結果の判定
        if (hasSaltResponse)
        {
            int lostFans = (int)(groupData.fans * 0.05f); // 5%のファン離れ
            // 塩対応で離れるのはコア層（幻滅）とライト層
            int lostCore = (int)(lostFans * 0.5f);
            int lostLight = lostFans - lostCore;
            if (lostCore > groupData.fansCore) lostCore = groupData.fansCore;

            groupData.fansCore -= lostCore;
            groupData.fansLight -= lostLight;

            // 残りの一部が厄介化する
            int toYakkai = (int)(lostCore * 0.2f);
            groupData.fansYakkai += toYakkai;

            report.AddLog($"<color=red>【塩対応】コアファンが幻滅... (離脱{lostCore}人 / 厄介化{toYakkai}人)</color>");
        }
        else if (avgSatisfaction >= 1.15f)
        {
            int newFans = (int)(participants * 0.2f); // 神対応が評判を呼び新規獲得
            groupData.fansLight += newFans;

            // 既存ライト層がコア層へ昇格
            int promotedCount = (int)(participants * 0.1f);
            if (promotedCount > groupData.fansLight) promotedCount = groupData.fansLight;

            groupData.fansLight -= promotedCount;
            groupData.fansCore += promotedCount;

            report.AddLog($"<color=green>【神対応】手厚い対応が話題になり、新規ファン獲得！ +{newFans}人</color>");
            report.AddLog($"さらに {promotedCount}人が太客(Core)に昇格しました！");
        }
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

        // MV効果は主にライト層増加
        groupData.fansLight += fanBoost;

        report.AddLog($"<color=cyan>[MV公開]</color> 『{latestSong.title}』MV完成！ ファン急増(Light) +{fanBoost}人 / 制作費 -{cost:N0}円");
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

        // 路線変更はコア層（古参）にダメージが大きい
        int lostCore = (int)(lostFans * 0.7f);
        int lostLight = lostFans - lostCore;
        if (lostCore > groupData.fansCore)
        {
            lostCore = groupData.fansCore;
            lostLight = lostFans - lostCore;
        }

        groupData.fansCore -= lostCore;
        groupData.fansLight -= lostLight;

        groupData.performance = (int)(groupData.performance * 0.9f);

        IdolGenre oldGenre = groupData.genre;
        groupData.genre = newGenre;

        report.AddLog($"<color=yellow>[路線変更]</color> {oldGenre} -> {newGenre} / 新衣装費({groupData.memberCount}人分) -{totalCost:N0}円");
        report.AddLog($"ファン離脱 (Core:-{lostCore} / Light:-{lostLight})");
    }

    // ★変更：ProduceSongにクリエイター選択ロジックを統合
    public void ProduceSong(int budgetTier, DailyReport report, string songTitle)
    {
        // クリエイターを予算Tierに応じて選択
        Creator composer = GetCreator(composers, budgetTier);
        Creator choreographer = GetCreator(choreographers, budgetTier);
        Creator designer = GetCreator(designers, budgetTier);

        long songCost = composer.cost;
        long costumeUnitCost = designer.cost;
        long choreoCost = choreographer.cost;
        long totalCostumeCost = costumeUnitCost * groupData.memberCount;
        long totalCost = songCost + totalCostumeCost + choreoCost;

        if (financial.currentCash < totalCost)
        {
            report.AddLog($"<color=red>[制作不可]</color> 資金不足 ({totalCost:N0}円必要)");
            return;
        }

        financial.currentCash -= totalCost;
        financial.dailyCashChange -= totalCost;

        // 品質計算：クリエイターの能力値を反映
        int budgetBonus = composer.qualityBonus + choreographer.qualityBonus + designer.qualityBonus;
        float trendBonus = market.GetMarketMultiplier(groupData.genre);
        int quality = (int)((groupData.performance + budgetBonus) * trendBonus);

        // SNS適性計算：クリエイターのSNS適性を反映
        int baseAppeal = composer.snsBonus + choreographer.snsBonus + designer.snsBonus + Random.Range(10, 30);
        if (groupData.genre == IdolGenre.KAWAII) baseAppeal += 10;
        if (groupData.genre == IdolGenre.TRADITIONAL) baseAppeal -= 10;
        int finalSnsAppeal = Mathf.Clamp(baseAppeal, 1, 100);

        Song newSong = new Song();
        newSong.title = songTitle;
        newSong.genre = groupData.genre;
        newSong.quality = quality;
        newSong.releaseDay = gameManager.currentDay;
        newSong.totalSales = 0;
        newSong.peakRank = 100;
        newSong.hasMV = false;
        newSong.snsAppeal = finalSnsAppeal;

        groupData.discography.Add(newSong);

        // 3時点のログを維持し、指名クリエイター情報を追加
        report.AddLog($"<color=green>[新曲リリース]</color> 『{newSong.title}』(Q:{quality} / <color=magenta>SNS適性:{finalSnsAppeal}</color>)");
        report.AddLog($"[内訳] 楽曲:-{songCost:N0} 衣装({groupData.memberCount}人):-{totalCostumeCost:N0} 振付:-{choreoCost:N0}");
        report.AddLog($"[指名] 作:{composer.name} / 振:{choreographer.name} / 衣:{designer.name}");
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

        if (booking.setlist.Count > 0)
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
        else
        {
            totalSetlistPower = groupData.performance * 0.1f;
        }

        float perfRate = (groupData.performance + (totalSetlistPower / 5.0f)) / 20.0f;
        if (groupData.fatigue > 70) perfRate *= 0.6f;
        if (groupData.mental < 40) perfRate *= 0.8f;

        float trendBonus = market.GetMarketMultiplier(groupData.genre);

        // 属性別集客計算
        // Coreは来やすく(80%)、Lightは少し来にくい(30%)
        int audienceCore = (int)(groupData.fansCore * 0.8f * perfRate * trendBonus);
        int audienceLight = (int)(groupData.fansLight * 0.3f * perfRate * trendBonus);
        int audienceYakkai = (int)(groupData.fansYakkai * 0.5f);

        int totalDemand = audienceCore + audienceLight + audienceYakkai;
        int actualAudience = Mathf.Min(totalDemand, booking.venue.capacity);

        int ticketPrice = 3000 + (booking.venue.capacity / 10);
        long ticketSales = (long)actualAudience * ticketPrice;

        long goodsSales = 0;
        if (groupData.goodsStock > 0)
        {
            // 物販はCore層の比率が高いほど伸びる
            int potentialBuyers = (int)(audienceCore * 0.8f) + (int)(audienceLight * 0.1f);
            int soldCount = Mathf.Min(potentialBuyers, groupData.goodsStock);
            groupData.goodsStock -= soldCount;
            goodsSales = soldCount * 2000;
            report.AddLog($"[物販] {soldCount}個販売 (+{goodsSales:N0}円) Core率高めで売上増！");
        }
        else
        {
            report.AddLog("<color=orange>[物販] グッズ在庫切れで機会損失...</color>");
        }

        long totalRevenue = ticketSales + goodsSales;
        financial.RegisterTransaction($"ライブ収益", totalRevenue, gameManager.currentDay, 60);

        report.AddLog($"[動員] {actualAudience}人 (Core:{audienceCore} Light:{audienceLight})");

        if (actualAudience >= booking.venue.capacity * 0.8f)
        {
            // ライブ成功で獲得するのはCoreファン
            int newFans = (int)(actualAudience * 0.1f);
            groupData.fansCore += newFans;

            // さらにライト層がコア層に昇格
            int promoted = (int)(audienceLight * 0.2f);
            if (promoted > groupData.fansLight) promoted = groupData.fansLight;
            groupData.fansLight -= promoted;
            groupData.fansCore += promoted;

            groupData.mental += 10;
            report.AddLog($"<color=green>[大成功]</color> {promoted}人がCoreファンに定着！");
        }
        else
        {
            groupData.mental -= 20;
            report.AddLog("[失敗] 空席目立つ...");
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