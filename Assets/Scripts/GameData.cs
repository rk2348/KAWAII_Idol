using System;
using System.Collections.Generic;
using UnityEngine;

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

    // ★追加：リリース済み楽曲リスト
    public List<Song> discography = new List<Song>();
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

// --- ★新規追加：楽曲データ ---
[Serializable]
public class Song
{
    public string title;
    public IdolGenre genre;
    public int quality;      // 楽曲の完成度 (1-100)
    public int releaseDay;   // リリース日
    public long totalSales;  // 総売上枚数
    public int peakRank;     // 最高順位

    // 現在の勢い（週が経つごとに減衰）
    public float GetCurrentMomentum(int currentDay)
    {
        int weeksOld = (currentDay - releaseDay) / 7;
        // 1週目は1.0, 以降は指数関数的に減衰 (最低0.1)
        return Mathf.Max(0.1f, 1.0f - (weeksOld * 0.15f));
    }
}