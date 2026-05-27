using System;
using System.Collections.Generic;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;
using RPACProductionPlanner.Repositories;

namespace RPACProductionPlanner.Services
{
    public class SchedulerService
    {
        private readonly ISchedulerRepository _schedulerRepo;

        public SchedulerService(ISchedulerRepository schedulerRepo)
        {
            _schedulerRepo = schedulerRepo;
        }

        public IEnumerable<ProductionOrder> GetScheduleData(DateTime start, DateTime end, string department = null)
        {
            return _schedulerRepo.GetMachineSchedule(start, end, department);
        }

        public IEnumerable<MachineResource> GetMachines()
        {
            return _schedulerRepo.GetAllMachines();
        }

        public IEnumerable<string> GetDepartments()
        {
            return _schedulerRepo.GetDepartments();
        }

        public IEnumerable<dynamic> GetUPSStatus()
        {
            return _schedulerRepo.GetUPSData();
        }

        public bool CheckConflicts(int machineId, DateTime start, DateTime end, int? excludeOrderId = null)
        {
            return false;
        }
    }
}
