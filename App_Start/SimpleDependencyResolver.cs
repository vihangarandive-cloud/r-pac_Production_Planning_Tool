using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RPACProductionPlanner.Repositories;
using RPACProductionPlanner.Services;
using RPACProductionPlanner.Controllers;

namespace RPACProductionPlanner.App_Start
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();

        public SimpleDependencyResolver()
        {
            // Use Lazy initialization to avoid creating all repositories/services on every startup/request
            var inventoryRepo = new Lazy<IInventoryRepository>(() => new InventoryRepository());
            var orderRepo = new Lazy<IProductionOrderRepository>(() => new ProductionOrderRepository());
            var reportRepo = new Lazy<IReportRepository>(() => new ReportRepository());
            var auditRepo = new Lazy<IAuditRepository>(() => new AuditRepository());
            var notificationRepo = new Lazy<INotificationRepository>(() => new NotificationRepository());
            var userRepo = new Lazy<IUserRepository>(() => new UserRepository());
            var schedulerRepo = new Lazy<ISchedulerRepository>(() => new SchedulerRepository());

            _registrations[typeof(IInventoryRepository)] = () => inventoryRepo.Value;
            _registrations[typeof(IProductionOrderRepository)] = () => orderRepo.Value;
            _registrations[typeof(IReportRepository)] = () => reportRepo.Value;
            _registrations[typeof(IAuditRepository)] = () => auditRepo.Value;
            _registrations[typeof(INotificationRepository)] = () => notificationRepo.Value;
            _registrations[typeof(IUserRepository)] = () => userRepo.Value;
            _registrations[typeof(ISchedulerRepository)] = () => schedulerRepo.Value;

            // 2. Services (Lazy)
            var schedulerService = new Lazy<SchedulerService>(() => new SchedulerService(schedulerRepo.Value));
            var productionService = new Lazy<ProductionService>(() => new ProductionService(orderRepo.Value, inventoryRepo.Value));
            var notificationService = new Lazy<NotificationService>(() => new NotificationService(inventoryRepo.Value));

            _registrations[typeof(SchedulerService)] = () => schedulerService.Value;
            _registrations[typeof(ProductionService)] = () => productionService.Value;
            _registrations[typeof(NotificationService)] = () => notificationService.Value;

            // 3. Controllers (Lazy)
            _registrations[typeof(DashboardController)] = () => new DashboardController(orderRepo.Value, schedulerService.Value, notificationService.Value);
            _registrations[typeof(ProductionOrderController)] = () => new ProductionOrderController(orderRepo.Value, productionService.Value, schedulerService.Value);
            _registrations[typeof(ReportsController)] = () => new ReportsController(reportRepo.Value, orderRepo.Value, inventoryRepo.Value);
            _registrations[typeof(AccountController)] = () => new AccountController(userRepo.Value);
            _registrations[typeof(SchedulerController)] = () => new SchedulerController(schedulerService.Value, orderRepo.Value);
            _registrations[typeof(MasterDataController)] = () => new MasterDataController(schedulerRepo.Value, inventoryRepo.Value);
            _registrations[typeof(AdminController)] = () => new AdminController(userRepo.Value);
        }

        public object GetService(Type serviceType)
        {
            if (_registrations.ContainsKey(serviceType))
            {
                return _registrations[serviceType]();
            }

            try
            {
                return Activator.CreateInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }
}
