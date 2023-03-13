﻿using LMS.Common.ViewModels.Common;
using LMS.Common.ViewModels.Course;
using LMS.Data.Entity;
using LMS.Services;
using LMS.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.App.Controllers
{
    [Authorize]
    [Route("course")]
    public class CourseController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly ICourseService _courseService;
        private readonly ICommonService _commonService;

        public CourseController(UserManager<User> userManager, ICourseService courseService, ICommonService commonService)
        {
            _userManager = userManager;
            _courseService = courseService;
            _commonService = commonService;
        }

        [Route("saveNewCourse")]
        [HttpPost]
        public async Task<IActionResult> SaveNewCourse(CourseViewModel courseViewModel)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var courseId = await _courseService.SaveNewCourse(courseViewModel, userId);
            return Ok(courseId);
        }

        [Route("updateCourse")]
        [HttpPost]
        public async Task<IActionResult> UpdateCourse(CourseViewModel courseViewModel)
        {
            var response = await _courseService.UpdateCourse(courseViewModel);
            return Ok(response);
        }

        [Route("deleteCourseById")]
        [HttpPost]
        public async Task<IActionResult> DeleteCourseById(Guid courseId)
        {
            var userId = await GetUserIdAsync(this._userManager);
            await _courseService.DeleteCourseById(courseId, userId);
            return Ok();
        }

        [Route("getCourseById")]
        [HttpGet]
        public async Task<IActionResult> GetCourseById(string courseName)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _courseService.GetCourseById(courseName, userId);
            return Ok(response);
        }

        [Route("getAllCourses")]
        [HttpPost]
        public async Task<IActionResult> GetAllCourses()
        {
            var response = await _courseService.GetAllCourses();
            return Ok(response);
        }

        [Route("languageList")]
        [HttpGet]
        public async Task<IActionResult> LanguageList()
        {
            return Ok(await _commonService.LanguageList());
        }

        [Route("getDisciplines")]
        [HttpGet]
        public async Task<IActionResult> GetDisciplines()
        {
            var response = await _commonService.GetDisciplines();
            return Ok(response);
        }

        [Route("getBasicCourseInfo")]
        [HttpGet]
        public async Task<IActionResult> GetBasicCourseInfo(Guid courseId)
        {
            var response = await _courseService.GetBasicCourseInfo(courseId);
            return Ok(response);
        }


        [Route("saveCourseLanguages")]  
        [HttpPost]
        public async Task<IActionResult> SaveCourseLanguages([FromBody] SaveCourseLanguageViewModel model)
        {
            await _courseService.SaveCourseLanguages(model.LanguageIds, new Guid(model.CourseId));
            return Ok();
        }

        [Route("deleteCourseLanguage")]
        [HttpPost]
        public async Task<IActionResult> DeleteCourseLanguage([FromBody] CourseLanguageViewModel model)
        {
            await _courseService.DeleteCourseLanguage(model);
            return Ok();
        }

        [Route("saveCourseTeachers")]
        [HttpPost]
        public async Task<IActionResult> SaveCourseTeachers([FromBody] SaveCourseTeacherViewModel model)
        {
            await _courseService.SaveCourseTeachers(model.TeacherIds,new Guid(model.CourseId));
            return Ok();
        }

        [Route("deleteCourseTeacher")]
        [HttpPost]
        public async Task<IActionResult> DeleteCourseTeacher([FromBody] CourseTeacherViewModel model)
        {
            await _courseService.DeleteCourseTeacher(model);
            return Ok();
        }

        [Route("saveCourseCertificates")]
        [HttpPost]
        public async Task<IActionResult> SaveCourseCertificates(SaveCourseCertificateViewModel model)
        {
            await _courseService.SaveCourseCertificates(model);
            return Ok();
        }

        [Route("deleteCourseCertificate")]
        [HttpPost]
        public async Task<IActionResult> DeleteCourseCertificate([FromBody] CourseCertificateViewModel model)
        {
            await _courseService.DeleteCourseCertificate(model);
            return Ok();
        }

        [Route("convertToClass")]
        [HttpPost]
        public async Task<IActionResult> ConvertToClass(string courseName)
        {
            await _courseService.ConvertToClass(courseName);
            return Ok();
        }

        [Route("getCourseByName")]
        [HttpGet]
        public async Task<IActionResult> GetCourseByName(string courseName, string schoolName)
        {
            var response = await _courseService.GetCourseByName(courseName, schoolName);
            return Ok(response);
        }

        [Route("isCourseNameExist")]
        [HttpGet]
        public async Task<IActionResult> IsCourseNameExist(string courseName)
        {
            return Ok(await _courseService.IsCourseNameExist(courseName));
        }

        [Route("courseView")]
        [HttpPost]
        public async Task<IActionResult> CourseView([FromBody] CourseViewsViewModel model)
        {
            var userId = await GetUserIdAsync(this._userManager);
            model.UserId = userId;
            var response = await _courseService.CourseView(model);
            return Ok(response);
        }

        [Route("getPostsByCourse")]
        [HttpGet]
        public async Task<IActionResult> GetPostsByCourse(Guid courseId, int pageNumber, int pageSize = 4)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _courseService.GetPostsByCourseId(courseId, userId, pageNumber, pageSize);
            return Ok(response);
        }

        [Route("getReelsByCourse")]
        [HttpGet]
        public async Task<IActionResult> getReelsByCourse(Guid courseId, int pageNumber, int pageSize = 4)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _courseService.GetReelsByCourseId(courseId, userId, pageNumber, pageSize);
            return Ok(response);
        }

        [Route("getCourseFilters")]
        [HttpGet]
        public async Task<IActionResult> GetCourseFilters(string userId, Guid schoolId)
        {
            var response = await _courseService.GetCourseFilters(userId, schoolId);
            return Ok(response);
        }

        [Route("saveCourseFilters")]
        [HttpPost]
        public async Task<IActionResult> SaveCourseFilters([FromBody] List<UserClassCourseFilterViewModel> model)
        {
            var userId = await GetUserIdAsync(this._userManager);
            await _courseService.SaveCourseFilters(model, userId);
            return Ok();
        }

        [Route("getCourseInfoForCertificate")]
        [HttpGet]
        public async Task<IActionResult> GetCourseInfoForCertificate(Guid courseId)
        {
            var response = await _courseService.GetCourseInfoForCertificate(courseId);
            return Ok(response);
        }

    }
}
