// /Models/ShiftSchedule.cs
using System;

namespace RPACProductionPlanner.Models
{
    public class ShiftSchedule
    {
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
