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

// ★追加：性格の定義
public enum IdolPersonality
{
    Energetic, // 元気：ムードメーカー
    Serious,   // 真面目：練習熱心だが融通が効かない
    Cool,      // クール：冷静だが付き合いが悪い
    Lazy,      // 怠惰：才能はあるがサボり魔
    Angel      // 天使：誰とでも仲良くできるがストレスを溜めやすい
}

[Serializable]
public class IdolMember
{
    public string lastName;
    public string firstName;
    public int birthMonth;
    public int birthDay;
    public int age;

    // 能力値
    public int visual;
    public int vocal;
    public int dance;

    // ★追加：性格
    public IdolPersonality personality;

    public string GetFullName()
    {
        return $"{lastName} {firstName}";
    }

    public string GetBirthdayString()
    {
        return $"{birthMonth}月{birthDay}日";
    }

    // ★追加：性格名の日本語取得
    public string GetPersonalityName()
    {
        switch (personality)
        {
            case IdolPersonality.Energetic: return "元気";
            case IdolPersonality.Serious: return "真面目";
            case IdolPersonality.Cool: return "クール";
            case IdolPersonality.Lazy: return "怠惰";
            case IdolPersonality.Angel: return "天使";
            default: return "";
        }
    }
}

[Serializable]
public class IdolGroup
{
    public string groupName = "Default Group";
    public List<IdolMember> members = new List<IdolMember>();

    public int memberCount
    {
        get { return members.Count; }
    }

    public int fans = 1000;
    public int performance = 10;
    public int mental = 100;
    public int fatigue = 0;
    public IdolGenre genre = IdolGenre.KAWAII;
    public bool hasDoneDome = false;

    // ★追加：グループの人間関係（ケミストリー）値 (-100 ? 100)
    public int chemistry = 0;

    public int goodsStock = 0;

    public int hospitalDaysLeft = 0;
    public int runawayDaysLeft = 0;

    public List<Song> discography = new List<Song>();

    public bool IsAvailable()
    {
        return hospitalDaysLeft <= 0 && runawayDaysLeft <= 0 && members.Count > 0;
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
    public int maxSongs;
}

[Serializable]
public class VenueBooking
{
    public Venue venue;
    public int eventDay;
    public bool isCanceled;
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
    public bool hasMV = false;

    public float GetCurrentMomentum(int currentDay)
    {
        int weeksOld = (currentDay - releaseDay) / 7;
        float mvBonus = hasMV ? 1.2f : 1.0f;
        return Mathf.Max(0.1f, (1.0f - (weeksOld * 0.15f)) * mvBonus);
    }
}