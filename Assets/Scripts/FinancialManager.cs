using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FinancialManager : MonoBehaviour
{
    [Header("‘Yó‹µ")]
    public long currentCash = 0;
    public long currentDebt = 0; // Ø‹à‘Šz
    public float interestRate = 0.0f; // Œ—˜i—á: 0.02 = 2%j

    [Header("’ •ë")]
    public List<Transaction> pendingTransactions = new List<Transaction>();

    // 1“ú‚Ìûx•Ï“®‹L˜^—p
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

        Debug.Log($"y’ •ëz{desc}: {amount:N0}‰~ ({delayDays}“úŒã)");
    }

    // –ˆ“ú‚ÌŒˆÏˆ—
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

            string type = t.amount >= 0 ? "“ü‹à" : "x•¥‚¢";
            report.AddLog($"[{type}] {t.description}: {t.amount:N0}‰~");
        }

        pendingTransactions.RemoveAll(t => t.isProcessed);
    }

    // Œ––‚Ì—˜qx•¥‚¢
    public void PayMonthlyInterest(DailyReport report)
    {
        if (currentDebt > 0 && interestRate > 0)
        {
            long interest = (long)(currentDebt * interestRate);
            currentCash -= interest;
            dailyCashChange -= interest;
            report.AddLog($"<color=red>[—˜q] Ø‹à•ÔÏ—˜q: -{interest:N0}‰~</color>");
        }
    }
}