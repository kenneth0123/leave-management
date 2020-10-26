using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;
        private readonly ILeaveAllocationRepository _leaveAllocRepo;

        public LeaveRequestController(
            ILeaveRequestRepository leaveRequestRepo,
            ILeaveTypeRepository leaveTypeRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILeaveAllocationRepository leaveAllocRepo,
            UserManager<Employee> userManager
        )
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveAllocRepo = leaveAllocRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequestController
        public ActionResult Index()
        {
            //var leaveRequests = _leaveRequestRepo.FindAll();
            var leaveRequests = _unitOfWork.LeaveRequests.FindAll(
                includes: new List<string> { "RequestingEmployee", "LeaveType" });
            var leaveRequestsModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequestsModel.Count,
                ApprovedRequests = leaveRequestsModel.Where(q => q.Approved == true).Count(),
                PendingRequests = leaveRequestsModel.Where(q => q.Approved == null).Count(),
                RejectedRequests = leaveRequestsModel.Where(q => q.Approved == false).Count(),
                LeaveRequests = leaveRequestsModel
            };
            
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {

            //var leaveRequest = _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id, includes: new List<string> { "ApprovedBy", "RequestingEMployee", "LeaveType" });
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);

            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                //var leaveRequest = await _leaveRequestRepo.FindById(id);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
                var requestingEmployeeId = leaveRequest.RequestingEmployeeId;
                var requestLeaveTypeId = leaveRequest.LeaveTypeId;
                int daysRequested = (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                //var updateLeaveRequestIsSuccess = _leaveRequestRepo.Update(leaveRequest);
                _unitOfWork.LeaveRequests.Update(leaveRequest);

                //var leaveAllocation = await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(requestingEmployeeId, requestLeaveTypeId);
                var period = DateTime.Now.Year;
                var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == requestingEmployeeId && q.Period == period && q.LeaveTypeId == requestLeaveTypeId);
                leaveAllocation.NumberOfDays = leaveAllocation.NumberOfDays - daysRequested;

                //var updateLeaveAllocationIsSuccess = _leaveAllocRepo.Update(leaveAllocation);
                _unitOfWork.LeaveAllocations.Update(leaveAllocation);

                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
            
        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                //var leaveRequest = await _leaveRequestRepo.FindById(id);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                // var isSuccess = _leaveRequestRepo.Update(leaveRequest);
                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> EmpSummary()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeid = employee.Id;

            //var leaveAllocations = await _leaveAllocRepo.GetLeaveAllocationsByEmployee(employeeid);
            var leaveAllocations = await _unitOfWork.LeaveAllocations.FindAll(q => q.EmployeeId == employeeid, includes: new List<string> { "LeaveType" });
            var leaveAllocVM = _mapper.Map<List<LeaveAllocationVM>>(leaveAllocations);

            //var leaveRequests = await _leaveRequestRepo.GetLeaveRequestsByEmployee(employeeid);
            var leaveRequests = await _unitOfWork.LeaveRequests.FindAll(q => q.RequestingEmployeeId == employeeid);
            var leaveRequestVM = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);

            var model = new EmployeeLeaveRequestVM
            {
                LeaveAllocations = leaveAllocVM,
                LeaveRequests = leaveRequestVM
            };
            
            return View(model);
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            //var leaveTypes = await _leaveTypeRepo.FindAll();
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem {
                Text = q.Name,
                Value = q.Id.ToString()
            });

            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };
            
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);

                //var leaveTypes = await _leaveTypeRepo.FindAll();
                var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return View(model);
                }

                var employee = _userManager.GetUserAsync(User).Result;
                var period = DateTime.Now.Year;
                //var allocations = await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                var allocations = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == employee.Id && q.Period == period && q.LeaveTypeId == model.LeaveTypeId);

                int daysRequested = (int)(endDate.Date - startDate.Date).TotalDays;

                if (daysRequested > allocations.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days for this request");
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    LeaveTypeId = model.LeaveTypeId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    RequestComments = model.RequestComments,
                    Cancelled = false
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                //var isSuccess = await _leaveRequestRepo.Create(leaveRequest);
                await _unitOfWork.LeaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index), "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View();
            }
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            //var leaveRequest = await _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            //var isSuccess = await _leaveRequestRepo.Delete(leaveRequest);
            _unitOfWork.LeaveRequests.Delete(leaveRequest);

            await _unitOfWork.Save();
            
            return RedirectToAction(nameof(Index), "Home");
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
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

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
