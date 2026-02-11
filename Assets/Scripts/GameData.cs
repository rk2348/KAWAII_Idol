using System;
using System.Collections.Generic;

// --- 基本データ ---

[Serializable]
public class Transaction
{
    public string description;
    public long amount;
    public int dueDay;
    public bool isProcessed;
}

[Serializable]
public class IdolGroup
{
    public string groupName = "FRUITS ZIPPER";
    public int fans = 1000;
    public int performance = 10;
    public int mental = 100;
    public int fatigue = 0;
    public IdolGenre genre = IdolGenre.KAWAII;
    public bool hasDoneDome = false;
}

public enum IdolGenre { KAWAII, COOL, ROCK, TRADITIONAL }

// --- スタッフ・会場 ---

public enum StaffType { Trainer, Marketer, Manager }

[Serializable]
public class Staff
{
    public string name;
    public StaffType type;
    public int level;
    public long monthlySalary;

    public float GetEffectMultiplier()
    {
        return 1.0f + (level * 0.1f);
    }
}

[Serializable]
public class Venue
{
    public string venueName;
    public int capacity;
    public long baseCost;
    public int minFansReq;
}

[Serializable]
public class VenueBooking
{
    public Venue venue;
    public int eventDay;
    public bool isCanceled;
}

// --- 新規追加：ゲーム設定データ ---

// プロデューサーの出自（難易度）
public enum ProducerOrigin
{
    OldAgency,  // 老舗：借金1億、コネあり、利子あり
    Venture,    // ベンチャー：資金5000万、短期ノルマあり
    Indie       // 叩き上げ：資金500万、借金なし、ハードモード
}

// 1日の活動レポート（ログ用）
[Serializable]
public class DailyReport
{
    public int day;
    public List<string> logs = new List<string>();
    public long cashChange;

    public void AddLog(string text)
    {
        logs.Add(text);
    }
}