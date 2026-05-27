// /Repositories/IProductionOrderRepository.cs
using System;
using System.Collections.Generic;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public interface IProductionOrderRepository
    {
        IEnumerable<ProductionOrder> GetAll(string status = null, DateTime? start = null, DateTime? end = null, string department = null);
        System.Threading.Tasks.Task<IEnumerable<ProductionOrder>> GetAllAsync(string status = null, DateTime? start = null, DateTime? end = null, string department = null);
        ProductionOrder GetById(int id);
        ProductionOrder GetByOrderCode(string orderCode);
        int Create(ProductionOrder order);
        bool Update(ProductionOrder order);
        bool UpdatePlanning(string orderCode, int machineId, DateTime plannedStart, DateTime? plannedEnd = null, string priority = "N/A");
        System.Threading.Tasks.Task<bool> UpdatePlanningAsync(string orderCode, int machineId, DateTime plannedStart, DateTime? plannedEnd = null, string priority = "N/A");
        bool UpdateNotes(string orderCode, string notes);
        bool Unschedule(string orderCode);
        bool Delete(int id);
        KPISummary GetDashboardKPIs();
        System.Threading.Tasks.Task<KPISummary> GetDashboardKPIsAsync();
    }
}
