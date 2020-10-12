using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CheckAllocation(int leavetypeid, string employeeId)
        {
            var period = DateTime.Now.Year;
            //return FindAll().Where(q => q.EmployeeId == employeeId && q.LeaveTypeId == leavetypeid && q.Period == period).Any();
            var items = await FindAll();
            return items.Where(q => q.EmployeeId == employeeId && q.LeaveTypeId == leavetypeid && q.Period == period).Any();
        }

        public async Task<bool> Create(LeaveAllocation entity)
        {
            await _db.LeaveAllocations.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            return await _db.LeaveAllocations.Include(q => q.LeaveType).ToListAsync();
        }

        public async Task<LeaveAllocation> FindById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationsByEmployee(string employeeId)
        {
            var period = DateTime.Now.Year;

            //return FindAll().Where(q => q.EmployeeId == employeeId && q.Period == period).ToList();
            var items = await FindAll();
            return items.Where(q => q.EmployeeId == employeeId && q.Period == period).ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationsByEmployeeAndType(string employeeId, int leaveTypeId)
        {
            var period = DateTime.Now.Year;

            //return FindAll().FirstOrDefault(q => q.EmployeeId == employeeId && q.Period == period && q.LeaveTypeId == leaveTypeId);
            var items = await FindAll();
            return items.FirstOrDefault(q => q.EmployeeId == employeeId && q.Period == period && q.LeaveTypeId == leaveTypeId); 
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveRequests.AnyAsync(q => q.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return await Save();
        }
    }
}
