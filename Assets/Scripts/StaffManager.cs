using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StaffManager : MonoBehaviour
{
    public List<Staff> hiredStaffs = new List<Staff>();
    private FinancialManager financial;

    public void Initialize(FinancialManager fm)
    {
        financial = fm;
        hiredStaffs.Clear();
    }

    public void HireStaff(StaffType type, int level, DailyReport report)
    {
        Staff newStaff = new Staff();
        newStaff.type = type;
        newStaff.level = level;
        newStaff.name = $"{type} Lv.{level}";
        newStaff.monthlySalary = 200000 * level;

        long contractFee = newStaff.monthlySalary * 3;

        if (financial.currentCash < contractFee)
        {
            report.AddLog("éëã‡ïsë´Ç≈ÉXÉ^ÉbÉtÇåŸÇ¶Ç‹ÇπÇÒÇ≈ÇµÇΩÅB");
            return;
        }

        financial.currentCash -= contractFee;
        financial.dailyCashChange -= contractFee;
        hiredStaffs.Add(newStaff);

        report.AddLog($"ÅyçÃópÅz{newStaff.name} ÇåŸóp (å_ñÒã‡ -{contractFee:N0}â~)");
    }

    public void PayMonthlySalaries(DailyReport report)
    {
        long totalSalary = hiredStaffs.Sum(s => s.monthlySalary);
        if (totalSalary > 0)
        {
            financial.currentCash -= totalSalary;
            financial.dailyCashChange -= totalSalary;
            report.AddLog($"<color=red>[êlåèîÔ] ÉXÉ^ÉbÉtããó^ëçäz: -{totalSalary:N0}â~</color>");
        }
    }

    public float GetStaffBonus(StaffType type)
    {
        var staff = hiredStaffs.Where(s => s.type == type).OrderByDescending(s => s.level).FirstOrDefault();
        return staff != null ? staff.GetEffectMultiplier() : 1.0f;
    }
}