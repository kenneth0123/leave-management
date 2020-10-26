using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Data.Migrations;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]

    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository _leaverepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(ILeaveTypeRepository leaverepo, ILeaveAllocationRepository leaveallocationrepo, IUnitOfWork unitOfWork, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaverepo = leaverepo;
            _leaveallocationrepo = leaveallocationrepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocationController
        public async Task<ActionResult> Index(int num, int leaveId)
        {
            //var leavetypestemp = await _leaverepo.FindAll();
            var leavetypestemp = await _unitOfWork.LeaveTypes.FindAll();
            var leavetypes = leavetypestemp.ToList();
            var mappedLeaveTypes = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leavetypes);

            var model = new CreateLeaveAllocationVM
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = num,
                LeaveTypeClicked = leaveId,
            };
            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            //var leaveType = await _leaverepo.FindById(id);
            var leaveType = await _unitOfWork.LeaveTypes.Find(q => q.Id == id);

            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;

            int numOfEmployeesUpdated = 0;

            var period = DateTime.Now.Year;

            foreach (var emp in employees)
            {
                //var checkAlloc = await _leaveallocationrepo.CheckAllocation(id, emp.Id);
                var checkAlloc = await _unitOfWork.LeaveAllocations.isExists(q => q.EmployeeId == emp.Id && q.LeaveTypeId == id && q.Period == period);
                if (checkAlloc)
                    continue;
                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.Days,
                    Period = DateTime.Now.Year
                };
                var leaveallocation = _mapper.Map<LeaveAllocation>(allocation);
                //await _leaveallocationrepo.Create(leaveallocation);
                await _unitOfWork.LeaveAllocations.Create(leaveallocation);
                await _unitOfWork.Save();
                numOfEmployeesUpdated++;
                
            }
            return RedirectToAction(nameof(Index), new {num = numOfEmployeesUpdated, leaveId = id });
        }

        public ActionResult ListEmployees()
        {
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            var model = _mapper.Map<List<EmployeeVM>>(employees);

            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _mapper.Map<EmployeeVM>(_userManager.FindByIdAsync(id).Result);
            var period = DateTime.Now.Year;
            var records = await _unitOfWork.LeaveAllocations.FindAll(
                expression: q => q.EmployeeId == id && q.Period == period,
                includes: new List<string> { "LeaveType" }
                );
            var allocations = _mapper.Map<List<LeaveAllocationVM>>(records);
            var model = new ViewAllocationsVM
            {
                Employee = employee,
                LeaveAllocations = allocations
            };
            
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public ActionResult Edit(int id)
        {            
            return View();
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
