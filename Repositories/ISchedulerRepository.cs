// /Repositories/ISchedulerRepository.cs
using System;
using System.Collections.Generic;

namespace RPACProductionPlanner.Repositories
{
    public interface ISchedulerRepository
    {
        System.Collections.Generic.IEnumerable<Models.ProductionOrder> GetMachineSchedule(DateTime start, DateTime end, string department = null);
        System.Collections.Generic.IEnumerable<Models.MachineResource> GetAllMachines();
        System.Collections.Generic.IEnumerable<dynamic> GetUPSData();
        System.Collections.Generic.IEnumerable<string> GetDepartments();
    }
}
