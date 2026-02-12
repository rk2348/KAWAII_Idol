using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FinancialManager : MonoBehaviour
{
    [Header("資産状況")]
    public long currentCash = 0;
    public long currentDebt = 0; // 借金総額
    public float interestRate = 0.0f; // 月利（例: 0.02 = 2%）

    [Header("帳簿")]
    public List<Transaction> pendingTransactions = new List<Transaction>();

    // 1日の収支変動記録用
    public long dailyCashChange = 0;

    public void Initialize(long startCash, long startDebt, float rate)
    {
        currentCash = startCash;
        currentDebt = startDebt;
        interestRate = rate;
        pendingTransactions.Clear();
    }

    public void RegisterTransaction(string desc, long amount, int currentDay, int delayDays)
    {
        Transaction t = new Transaction();
        t.description = desc;
        t.amount = amount;
        t.dueDay = currentDay + delayDays;
        t.isProcessed = false;
        pendingTransactions.Add(t);

        Debug.Log($"【帳簿】{desc}: {amount:N0}円 ({delayDays}日後)");
    }

    // 毎日の決済処理
    public void ProcessDailyTransactions(int today, DailyReport report)
    {
        dailyCashChange = 0;

        List<Transaction> dueTransactions = pendingTransactions
            .Where(t => !t.isProcessed && t.dueDay <= today)
            .ToList();

        foreach (var t in dueTransactions)
        {
            currentCash += t.amount;
            dailyCashChange += t.amount;
            t.isProcessed = true;

            string type = t.amount >= 0 ? "入金" : "支払い";
            report.AddLog($"[{type}] {t.description}: {t.amount:N0}円");
        }

        pendingTransactions.RemoveAll(t => t.isProcessed);
    }

    // 月末の支払い（利子・運営費）
    public void PayMonthlyCosts(DailyReport report)
    {
        // 借金利子
        if (currentDebt > 0 && interestRate > 0)
        {
            long interest = (long)(currentDebt * interestRate);
            currentCash -= interest;
            dailyCashChange -= interest;
            report.AddLog($"<color=red>[利子] 借金返済利子: -{interest:N0}円</color>");
        }

        // ★追加：運営維持費（公式サイト、サーバー、FC運営など）
        long operationCost = 50000; // 基本5万円
        currentCash -= operationCost;
        dailyCashChange -= operationCost;
        report.AddLog($"[固定費] 公式サイト・サーバー維持費: -{operationCost:N0}円");
    }
}