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
    public int mental = 100;
    public int fatigue = 0;
    public IdolGenre genre = IdolGenre.KAWAII;
    public bool hasDoneDome = false;

    public int hospitalDaysLeft = 0;
    public int runawayDaysLeft = 0;

    public List<Song> discography = new List<Song>();

    public bool IsAvailable()
    {
        return hospitalDaysLeft <= 0 && runawayDaysLeft <= 0;
    }
}

public enum IdolGenre { KAWAII, COOL, ROCK, TRADITIONAL }

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
    public int maxSongs; // ★追加：この会場で披露できる曲数
}

[Serializable]
public class VenueBooking
{
    public Venue venue;
    public int eventDay;
    public bool isCanceled;

    // ★追加：セットリスト（曲のリスト）
    public List<Song> setlist = new List<Song>();
}

public enum ProducerOrigin { OldAgency, Venture, Indie }

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