﻿using LMS.Common.ViewModels.Teacher;
using LMS.Data.Entity;
using LMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.App.Controllers
{
    [Route("teachers")]
    public class TeachersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly ITeacherService _teacherService;
        private readonly IPermissionService _permissionService;


        public TeachersController(UserManager<User> userManager, ITeacherService teacherService, IPermissionService permissionService)
        {
            _userManager = userManager;
            _teacherService = teacherService;
            _permissionService = permissionService;
        }

        [Route("updateTeacher")]
        [HttpPost]
        public async Task<IActionResult> UpdateTeacher([FromBody] TeacherViewModel teacherViewModel)
        {
            await _teacherService.UpdateTeacher(teacherViewModel);
            return Ok("success");
        }

        [Route("getTeacherById")]
        [HttpPost]
        public async Task<IActionResult> GetTeacherById(Guid teacherId)
        {
            var response = await _teacherService.GetTeacherById(teacherId);
            return Ok(response);
        }

        [Route("deleteTeacherById")]
        [HttpPost]
        public async Task<IActionResult> DeleteTeacherById(Guid teacherId)
        {
            var userId = await GetUserIdAsync(this._userManager);
            await _teacherService.DeleteTeacherById(teacherId, userId);
            return Ok("success");
        }

        [Route("getAllTeachers")]
        [HttpGet]
        public async Task<IActionResult> GetAllTeachers()
        {
            var response = await _teacherService.GetAllTeachers();
            return Ok(response);
        }

        [Route("getAllPermissions")]
        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var response = await _permissionService.GetAllPermissions();
            return Ok(response);
        }

        [Route("addTeacher")]
        [HttpPost]
        public async Task<IActionResult> AddTeacherPermissions([FromBody] AddTeacherViewModel model)
        {
            await _teacherService.AddTeacher(model);
            return Ok();
        }

        [Route("getClassTeachers")]
        [HttpGet]
        public async Task<IActionResult> GetClassTeachers(Guid classId)
        {
            var response = await _teacherService.GetClassTeachers(classId);
            return Ok(response);
        }

        [Route("getCourseTeachers")]
        [HttpGet]
        public async Task<IActionResult> GetCourseTeachers(Guid courseId)
        {
            var response = await _teacherService.GetCourseTeachers(courseId);
            return Ok(response);
        }
    }
}
