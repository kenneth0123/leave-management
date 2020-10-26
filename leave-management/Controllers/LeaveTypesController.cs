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
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LeaveTypesController(ILeaveTypeRepository repo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        // GET: LeaveTypesController
        public async Task<ActionResult> Index()
        {
            //var leaveTypesTemp = await _repo.FindAll();
            var leaveTypesTemp = await _unitOfWork.LeaveTypes.FindAll();
            var leaveTypes = leaveTypesTemp.ToList();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes);
            return View(model);
        }

        // GET: LeaveTypesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            //var isExist = await _repo.isExists(id);
            var isExist = await _unitOfWork.LeaveTypes.isExists(q => q.Id == id);
            if (!isExist)
            {
                return NotFound();
            }

            //var leaveType = _repo.FindById(id);
            var leaveType = _unitOfWork.LeaveTypes.Find(q => q.Id == id);
            var model = _mapper.Map<LeaveTypeVM>(leaveType);

            return View(model);
        }

        // GET: LeaveTypesController/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: LeaveTypesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeVM model)
        {
            try
            {
                // TODO: Add logic here
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //Map the model after LeaveType and save to a variable
                var leaveType = _mapper.Map<LeaveType>(model);

                //Assign value to Date Created
                leaveType.DateCreated = DateTime.Now;

                //var isSuccess = await _repo.Create(leaveType);
                await _unitOfWork.LeaveTypes.Create(leaveType);
                await _unitOfWork.Save();
                
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            //var isExist = await _repo.isExists(id);
            var isExist = await _unitOfWork.LeaveTypes.isExists(q => q.Id == id);
            if (!isExist)
            {
                return NotFound();
            }

            //var leaveType = _repo.FindById(id);
            var leaveType = _unitOfWork.LeaveTypes.Find(q => q.Id == id);
            var model = _mapper.Map<LeaveTypeVM>(leaveType);

            return View(model);
        }

        // POST: LeaveTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, LeaveTypeVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //Map the model after LeaveType and save to a variable
                var leaveType = _mapper.Map<LeaveType>(model);

                //Call the repository operation to update
                //var isSuccess = await _repo.Update(leaveType);

                //if (!isSuccess)
                //{
                //    ModelState.AddModelError("", "Something went wrong...");
                //    return View(model);
                //}
                _unitOfWork.LeaveTypes.Update(leaveType);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            //var leaveType = await _repo.FindById(id);
            var leaveType = await _unitOfWork.LeaveTypes.Find(expression: q=> q.Id ==id);

            if (leaveType == null)
            {
                return NotFound();
            }

            _unitOfWork.LeaveTypes.Delete(leaveType);
            await _unitOfWork.Save();
            //var isSuccess = await _repo.Delete(leaveType);

            //if (!isSuccess)
            //{
            //    return BadRequest();
            //}

            return RedirectToAction(nameof(Index));
        }

        // POST: LeaveTypesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveType model)
        {
            try
            {
                try
                {
                    var leaveType = await _unitOfWork.LeaveTypes.Find(expression: q => q.Id == id);

                    if (leaveType == null)
                    {
                        return NotFound();
                    }

                    _unitOfWork.LeaveTypes.Delete(leaveType);
                    await _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Something went wrong...");
                    return View(model);
                }
            }
            catch
            {
                return View(model);
            }
        }
    }
}
