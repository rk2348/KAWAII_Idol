using System;
using System.Collections.Generic;
using UnityEngine;

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
    public int mental = 100; // 0で失踪
    public int fatigue = 0;  // 100で入院
    public IdolGenre genre = IdolGenre.KAWAII;
    public bool hasDoneDome = false;

    // ★追加：状態異常管理
    public int hospitalDaysLeft = 0; // 入院残り日数
    public int runawayDaysLeft = 0;  // 失踪残り日数

    public List<Song> discography = new List<Song>();

    // 行動可能か？
    public bool IsAvailable()
    {
        return hospitalDaysLeft <= 0 && runawayDaysLeft <= 0;
    }
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

// --- ゲーム設定データ ---

public enum ProducerOrigin
{
    OldAgency,
    Venture,
    Indie
}

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

[Serializable]
public class Song
{
    public string title;
    public IdolGenre genre;
    public int quality;
    public int releaseDay;
    public long totalSales;
    public int peakRank;

    public float GetCurrentMomentum(int currentDay)
    {
        int weeksOld = (currentDay - releaseDay) / 7;
        return Mathf.Max(0.1f, 1.0f - (weeksOld * 0.15f));
    }
}