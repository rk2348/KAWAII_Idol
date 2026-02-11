using System.Collections.Generic;
using UnityEngine;
using System.Linq; // リスト操作用

public class FinancialManager : MonoBehaviour
{
    [Header("資産状況")]
    public long currentCash = 0; // 現在の現金（桁溢れ防止でlong）

    [Header("帳簿（未来の入出金リスト）")]
    public List<Transaction> pendingTransactions = new List<Transaction>();

    // 取引を予約する（例：delayDays=60なら、60日後にキャッシュが動く）
    public void RegisterTransaction(string desc, long amount, int currentDay, int delayDays)
    {
        Transaction t = new Transaction();
        t.description = desc;
        t.amount = amount;
        t.dueDay = currentDay + delayDays;
        t.isProcessed = false;

        pendingTransactions.Add(t);

        // ログ出力（コンソールで確認用）
        string type = amount >= 0 ? "入金予定" : "支払予定";
        Debug.Log($"【帳簿記入】{desc}: {amount.ToString("#,0")}円 ({delayDays}日後)");
    }

    // 毎日の処理：今日が期日の取引を決済する
    public void ProcessDailyTransactions(int today)
    {
        // 今日以前が期日で、まだ処理していないものを抽出
        List<Transaction> dueTransactions = pendingTransactions
            .Where(t => !t.isProcessed && t.dueDay <= today)
            .ToList();

        foreach (var t in dueTransactions)
        {
            currentCash += t.amount; // 実際に現金を増減
            t.isProcessed = true;
            Debug.Log($"<color=yellow>【決済実行】{t.description}: {t.amount.ToString("#,0")}円 / 残高: {currentCash.ToString("#,0")}円</color>");
        }

        // 処理済みをリストから削除（メモリ節約）
        pendingTransactions.RemoveAll(t => t.isProcessed);

        // 黒字倒産チェック
        if (currentCash < 0)
        {
            Debug.LogError("【GAMEOVER】資金ショート！倒産しました。");
            // ここにゲームオーバー処理を入れる
        }
    }

    // 借金の利子支払い（毎月呼ばれる想定）
    public void PayInterest(long debtAmount, float interestRate)
    {
        long interest = (long)(debtAmount * interestRate);
        currentCash -= interest;
        Debug.Log($"<color=red>【利子支払】借金の利子 {interest}円 を支払いました。</color>");
    }
}