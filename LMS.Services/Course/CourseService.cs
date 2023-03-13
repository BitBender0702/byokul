﻿using AutoMapper;
using LMS.Common.Enums;
using LMS.Common.ViewModels.Class;
using LMS.Common.ViewModels.Common;
using LMS.Common.ViewModels.Course;
using LMS.Common.ViewModels.Post;
using LMS.Common.ViewModels.School;
using LMS.Common.ViewModels.Student;
using LMS.Common.ViewModels.Teacher;
using LMS.Data.Entity;
using LMS.DataAccess.Repository;
using LMS.Services.Blob;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Services
{
    public class CourseService : ICourseService
    {
        private readonly IMapper _mapper;
        private IGenericRepository<Course> _courseRepository;
        private IGenericRepository<CourseLanguage> _courseLanguageRepository;
        private IGenericRepository<CourseDiscipline> _courseDisciplineRepository;
        private IGenericRepository<CourseStudent> _courseStudentRepository;
        private IGenericRepository<CourseTeacher> _courseTeacherRepository;
        private IGenericRepository<Post> _postRepository;
        private IGenericRepository<Class> _classRepository;
        private IGenericRepository<PostAttachment> _postAttachmentRepository;
        private IGenericRepository<PostTag> _postTagRepository;
        private IGenericRepository<CourseTag> _courseTagRepository;
        private IGenericRepository<CourseCertificate> _courseCertificateRepository;
        private readonly UserManager<User> _userManager;
        private readonly IBlobService _blobService;
        private readonly IClassService _classService;
        private readonly IUserService _userService;
        private IGenericRepository<CourseLike> _courseLikeRepository;
        private IGenericRepository<CourseViews> _courseViewsRepository;
        private IGenericRepository<School> _schoolRepository;
        private IGenericRepository<ClassCourseFilter> _classCourseFilterRepository;
        private IGenericRepository<UserClassCourseFilter> _userClassCourseFilterRepository;
        private IGenericRepository<UserSharedPost> _userSharedPostRepository;
        private IConfiguration _config;


        public CourseService(IMapper mapper, IGenericRepository<Course> courseRepository, IGenericRepository<CourseLanguage> courseLanguageRepository, IGenericRepository<CourseDiscipline> courseDisciplineRepository, IGenericRepository<CourseStudent> courseStudentRepository, IGenericRepository<CourseTeacher> courseTeacherRepository, IGenericRepository<Post> postRepository, IGenericRepository<Class> classRepository, IGenericRepository<PostAttachment> postAttachmentRepository, IGenericRepository<PostTag> postTagRepository, IGenericRepository<CourseCertificate> courseCertificateRepository, IGenericRepository<CourseTag> courseTagRepository, UserManager<User> userManager, IBlobService blobService, IClassService classService, IUserService userService, IGenericRepository<CourseLike> courseLikeRepository, IGenericRepository<CourseViews> courseViewsRepository, IGenericRepository<School> schoolRepository, IGenericRepository<ClassCourseFilter> classCourseFilterRepository, IGenericRepository<UserClassCourseFilter> userClassCourseFilterRepository, IGenericRepository<UserSharedPost> userSharedPostRepository, IConfiguration config)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _courseLanguageRepository = courseLanguageRepository;
            _courseDisciplineRepository = courseDisciplineRepository;
            _courseStudentRepository = courseStudentRepository;
            _courseTeacherRepository = courseTeacherRepository;
            _postRepository = postRepository;
            _classRepository = classRepository;
            _postAttachmentRepository = postAttachmentRepository;
            _postTagRepository = postTagRepository;
            _courseCertificateRepository = courseCertificateRepository;
            _courseTagRepository = courseTagRepository;
            _userManager = userManager;
            _blobService = blobService;
            _classService = classService;
            _userService = userService;
            _courseLikeRepository = courseLikeRepository;
            _courseViewsRepository = courseViewsRepository;
            _schoolRepository = schoolRepository;
            _classCourseFilterRepository = classCourseFilterRepository;
            _userClassCourseFilterRepository = userClassCourseFilterRepository;
            _userSharedPostRepository = userSharedPostRepository;
            _config = config;
        }

        public async Task<CourseViewModel> SaveNewCourse(CourseViewModel courseViewModel, string createdById)
        {
            var containerName = "coursethumbnail";
            var langList = JsonConvert.DeserializeObject<string[]>(courseViewModel.LanguageIds.First());
            courseViewModel.LanguageIds = langList;

            var teacherIdsList = JsonConvert.DeserializeObject<string[]>(courseViewModel.TeacherIds.First());
            courseViewModel.TeacherIds = teacherIdsList;

            var studentIds = JsonConvert.DeserializeObject<string[]>(courseViewModel.StudentIds.First());
            courseViewModel.StudentIds = studentIds;

            var disciplineIds = JsonConvert.DeserializeObject<string[]>(courseViewModel.DisciplineIds.First());
            courseViewModel.DisciplineIds = disciplineIds;

            courseViewModel.CourseUrl = JsonConvert.DeserializeObject<string>(courseViewModel.CourseUrl);

            courseViewModel.CourseTags = JsonConvert.DeserializeObject<string[]>(courseViewModel.CourseTags.First());

            if (courseViewModel.Thumbnail != null)
            {
                courseViewModel.ThumbnailUrl = await _blobService.UploadFileAsync(courseViewModel.Thumbnail, containerName);

                int index = courseViewModel.Thumbnail.ContentType.IndexOf('/');
                if (index > 0)
                {
                    if (courseViewModel.Thumbnail.ContentType.Substring(0, index) == "video")
                    {
                        courseViewModel.ThumbnailType = (int)FileTypeEnum.Video;
                    }
                    else
                    {
                        courseViewModel.ThumbnailType = (int)FileTypeEnum.Image;
                    }
                }
            }

            var course = new Course
            {
                CourseName = courseViewModel.CourseName,
                SchoolId = courseViewModel.SchoolId,
                ServiceTypeId = courseViewModel.ServiceTypeId,
                AccessibilityId = courseViewModel.AccessibilityId,
                Description = courseViewModel.Description,
                Price = courseViewModel.Price,
                CourseUrl = courseViewModel.CourseUrl,
                ThumbnailUrl = courseViewModel.ThumbnailUrl,
                ThumbnailType = courseViewModel.ThumbnailType,
                CreatedById = createdById,
                CreatedOn = DateTime.UtcNow
            };

            _courseRepository.Insert(course);
            _courseRepository.Save();
            courseViewModel.CourseId = course.CourseId;

            if (courseViewModel.LanguageIds.Any())
            {
                await SaveCourseLanguages(courseViewModel.LanguageIds, course.CourseId);
            }

            if (courseViewModel.DisciplineIds.Any())
            {
                await SaveCourseDisciplines(courseViewModel.DisciplineIds, course.CourseId);
            }

            if (courseViewModel.StudentIds.Any())
            {
                await SaveCourseStudents(courseViewModel.StudentIds, course.CourseId);
            }

            if (courseViewModel.TeacherIds.Any())
            {
                await SaveCourseTeachers(courseViewModel.TeacherIds, course.CourseId);
            }

            if (courseViewModel.CourseTags != null)
            {
                await SaveCourseTags(courseViewModel.CourseTags, courseViewModel.CourseId);
            }

            var school = await _schoolRepository.GetAll().Where(x => x.SchoolId == courseViewModel.SchoolId).FirstOrDefaultAsync();
            var schoolResult = _mapper.Map<SchoolViewModel>(school);
            courseViewModel.School = schoolResult;

            return courseViewModel;

        }

        public async Task SaveCourseLanguages(IEnumerable<string> languageIds, Guid courseId)
        {
            var courses = await GetAllCourses();
            var isCourseExist = courses.Where(x => x.CourseId == courseId).FirstOrDefault();
            if (isCourseExist == null)
            {
                await _classService.SaveClassLanguages(languageIds, courseId);
            }
            else
            {
                foreach (var languageId in languageIds)
                {

                    var courseLanguage = new CourseLanguage
                    {
                        CourseId = courseId,
                        LanguageId = new Guid(languageId)
                    };

                    _courseLanguageRepository.Insert(courseLanguage);
                    _courseLanguageRepository.Save();

                }
            }
        }

        async Task SaveCourseDisciplines(IEnumerable<string> disciplineIds, Guid courseId)
        {
            foreach (var disciplineId in disciplineIds)
            {
                var courseDiscipline = new CourseDiscipline
                {
                    CourseId = courseId,
                    DisciplineId = new Guid(disciplineId)
                };

                _courseDisciplineRepository.Insert(courseDiscipline);
                _courseDisciplineRepository.Save();

            }
        }

        async Task SaveCourseStudents(IEnumerable<string> studentIds, Guid courseId)
        {
            foreach (var studentId in studentIds)
            {
                var courseStudent = new CourseStudent
                {
                    CourseId = courseId,
                    StudentId = new Guid(studentId)
                };

                _courseStudentRepository.Insert(courseStudent);
                _courseStudentRepository.Save();
            }
        }

        public async Task SaveCourseTeachers(IEnumerable<string> teacherIds, Guid courseId)
        {
            var courses = await GetAllCourses();
            var isCourseExist = courses.Where(x => x.CourseId == courseId).FirstOrDefault();
            if (isCourseExist == null)
            {
                await _classService.SaveClassTeachers(teacherIds, courseId);
            }
            else
            {
                foreach (var teacherId in teacherIds)
                {
                    var courseTeacher = new CourseTeacher
                    {
                        CourseId = courseId,
                        TeacherId = new Guid(teacherId)
                    };
                    _courseTeacherRepository.Insert(courseTeacher);
                    _courseTeacherRepository.Save();
                }
            }
        }

        public async Task<CourseViewModel> UpdateCourse(CourseViewModel courseViewModel)
        {
            var containerName = "courselogo";
            if (courseViewModel.AvatarImage != null)
            {
                courseViewModel.Avatar = await _blobService.UploadFileAsync(courseViewModel.AvatarImage, containerName);
            }

            courseViewModel.LanguageIds = JsonConvert.DeserializeObject<string[]>(courseViewModel.LanguageIds.First());

            Course course = _courseRepository.GetById(courseViewModel.CourseId);
            course.Avatar = courseViewModel.Avatar;
            course.CourseName = courseViewModel.CourseName;
            course.ServiceTypeId = courseViewModel.ServiceTypeId;
            course.AccessibilityId = courseViewModel.AccessibilityId;
            course.Description = courseViewModel.Description;
            course.Price = courseViewModel.Price;

            _courseRepository.Update(course);
            _courseRepository.Save();

            if (courseViewModel.LanguageIds != null)
            {
                await UpdateCourseLanguages(courseViewModel.LanguageIds, courseViewModel.CourseId);
            }

            var school = await _schoolRepository.GetAll().Where(x => x.SchoolId == courseViewModel.SchoolId).FirstOrDefaultAsync();
            try
            {
                var schoolResult = _mapper.Map<SchoolViewModel>(school);
                courseViewModel.School = schoolResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return courseViewModel;

            //if (courseViewModel.DisciplineIds != null)
            //{
            //    await UpdateCourseDisciplines(courseViewModel.DisciplineIds, courseViewModel.CourseId);
            //}

            //if (courseViewModel.StudentIds != null)
            //{
            //    await UpdateCourseStudents(courseViewModel.StudentIds, courseViewModel.CourseId);
            //}

            //if (courseViewModel.TeacherIds != null)
            //{
            //    await UpdateCourseTeachers(courseViewModel.TeacherIds, courseViewModel.CourseId);
            //}

        }

        async Task UpdateCourseLanguages(IEnumerable<string> languageIds, Guid courseId)
        {
            var courseLanguages = _courseLanguageRepository.GetAll().Where(x => x.CourseId == courseId).ToList();

            if (courseLanguages.Any())
            {
                _courseLanguageRepository.DeleteAll(courseLanguages);
            }
            await SaveCourseLanguages(languageIds, courseId);
        }

        async Task UpdateCourseDisciplines(IEnumerable<string> disciplineIds, Guid courseId)
        {
            var courseDisciplines = _courseDisciplineRepository.GetAll().Where(x => x.CourseId == courseId).ToList();

            if (courseDisciplines.Any())
            {
                _courseDisciplineRepository.DeleteAll(courseDisciplines);
            }
            await SaveCourseDisciplines(disciplineIds, courseId);
        }

        async Task UpdateCourseStudents(IEnumerable<string> studentIds, Guid courseId)
        {
            var courseStudents = _courseStudentRepository.GetAll().Where(x => x.CourseId == courseId).ToList();

            if (courseStudents.Any())
            {
                _courseStudentRepository.DeleteAll(courseStudents);
            }
            await SaveCourseStudents(studentIds, courseId);
        }

        async Task UpdateCourseTeachers(IEnumerable<string> teacherIds, Guid courseId)
        {
            var courseTeachers = _courseTeacherRepository.GetAll().Where(x => x.CourseId == courseId).ToList();

            if (courseTeachers.Any())
            {
                _courseTeacherRepository.DeleteAll(courseTeachers);
            }
            await SaveCourseTeachers(teacherIds, courseId);
        }

        public async Task<CourseDetailsViewModel> GetCourseById(string courseName, string loginUserid)
        {
            CourseDetailsViewModel model = new CourseDetailsViewModel();

            if (courseName != null)
            {
                var course = await _courseRepository.GetAll()
                    .Include(x => x.ServiceType)
                    .Include(x => x.School)
                    .ThenInclude(x => x.Country)
                    .Include(x => x.School)
                    .ThenInclude(x => x.Specialization)
                    .Include(x => x.Accessibility)
                    .Include(x => x.CreatedBy)
                    .Where(x => x.CourseName.Replace(" ", "").ToLower() == courseName.Replace(" ", "").ToLower()).FirstOrDefaultAsync();

                if (course == null)
                {

                    var classDetails = await _classService.GetClassById(courseName, loginUserid);

                    var courses = new CourseDetailsViewModel();
                    courses.CourseId = classDetails.ClassId;
                    courses.CourseName = classDetails.ClassName;
                    courses.School = classDetails.School;
                    courses.ServiceType = classDetails.ServiceType;
                    courses.Accessibility = classDetails.Accessibility;
                    courses.School.Country = classDetails.School.Country;
                    courses.School.Specialization = classDetails.School.Specialization;
                    courses.CreatedBy = classDetails.CreatedBy;
                    courses.Avatar = classDetails.Avatar;
                    courses.CourseUrl = classDetails.ClassUrl;
                    courses.Languages = classDetails.Languages;
                    courses.Disciplines = classDetails.Disciplines;
                    courses.Students = classDetails.Students;
                    courses.Teachers = classDetails.Teachers;
                    courses.Posts = classDetails.Posts;
                    courses.Reels = classDetails.Reels;
                    courses.IsConvertable = true;


                    courses.CourseCertificates = _mapper.Map<IEnumerable<CourseCertificateViewModel>>(classDetails.ClassCertificates);

                    return courses;



                }
                model = _mapper.Map<CourseDetailsViewModel>(course);

                model.Languages = await GetLanguages(course.CourseId);
                model.Disciplines = await GetDisciplines(course.CourseId);
                model.Students = await GetStudents(course.CourseId);
                model.Teachers = await GetTeachers(course.CourseId);
                model.Posts = await GetPostsByCourseId(course.CourseId, loginUserid);
                model.Reels = await GetReelsByCourseId(course.CourseId, loginUserid);
                model.CourseCertificates = await GetCertificateByCourseId(course.CourseId);


                return model;
            }
            return null;
        }

        async Task<IEnumerable<LanguageViewModel>> GetLanguages(Guid courseId)
        {
            var courseLanguages = _courseLanguageRepository.GetAll()
                .Include(x => x.Language)
                .Where(x => x.CourseId == courseId).ToList();

            var languageViewModel = new List<LanguageViewModel>();
            foreach (var res in courseLanguages)
            {
                languageViewModel.Add(_mapper.Map<LanguageViewModel>(res.Language));
            }
            return languageViewModel;
        }

        async Task<IEnumerable<DisciplineViewModel>> GetDisciplines(Guid courseId)
        {
            var courseDisciplines = _courseDisciplineRepository.GetAll()
                .Include(x => x.Discipline)
                .Where(x => x.CourseId == courseId).ToList();

            var discipleneViewModel = new List<DisciplineViewModel>();
            foreach (var res in courseDisciplines)
            {
                discipleneViewModel.Add(_mapper.Map<DisciplineViewModel>(res.Discipline));
            }
            return discipleneViewModel;
        }

        public async Task<int> GetStudents(Guid courseId)
        {
            var courseStudents = _courseStudentRepository.GetAll()
                .Include(x => x.Student)
                .ThenInclude(x => x.CreatedBy)
                .Where(x => x.CourseId == courseId).ToList();

            return courseStudents.Count();
        }

        async Task<IEnumerable<TeacherViewModel>> GetTeachers(Guid courseId)
        {
            var courseTeachers = _courseTeacherRepository.GetAll()
                .Include(x => x.Teacher)
                .Where(x => x.CourseId == courseId).ToList();

            var teacherViewModel = new List<TeacherViewModel>();
            foreach (var res in courseTeachers)
            {
                teacherViewModel.Add(_mapper.Map<TeacherViewModel>(res.Teacher));
            }
            return teacherViewModel;
        }

        public async Task DeleteCourseById(Guid courseId, string deletedByid)
        {
            Course course = _courseRepository.GetById(courseId);
            course.IsDeleted = true;
            course.DeletedById = deletedByid;
            course.DeletedOn = DateTime.UtcNow;
            _courseRepository.Update(course);
            _courseRepository.Save();
        }

        public async Task<IEnumerable<CourseViewModel>> GetAllCourses()
        {
            IEnumerable<CourseViewModel> model = _courseRepository.GetAll().Where(x => !x.IsDeleted).Select(x => new CourseViewModel
            {
                CourseId = x.CourseId,
                CourseName = x.CourseName,
            });

            return model;
        }

        public async Task<IEnumerable<PostDetailsViewModel>> GetPostsByCourseId(Guid courseId, string loginUserId, int pageNumber = 1, int pageSize = 4)
        {
            var courseList = await _postRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.ParentId == courseId && x.PostType == (int)PostTypeEnum.Post).OrderByDescending(x => x.IsPinned).ToListAsync();
            var result = _mapper.Map<List<PostDetailsViewModel>>(courseList).Skip((pageNumber - 1) * pageSize).Take(pageSize); ;

            foreach (var post in result)
            {
                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                if (post.Likes.Any(x => x.UserId == loginUserId && x.PostId == post.Id))
                {
                    post.IsPostLikedByCurrentUser = true;
                }
                else
                {
                    post.IsPostLikedByCurrentUser = false;
                }
            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }

            return result;
        }

        public async Task<IEnumerable<PostDetailsViewModel>> GetReelsByCourseId(Guid courseId, string loginUserId, int pageNumber = 1, int pageSize = 8)
        {
            var courseList = await _postRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.ParentId == courseId && x.PostType == (int)PostTypeEnum.Reel).OrderByDescending(x => x.IsPinned).ToListAsync();
            var result = _mapper.Map<List<PostDetailsViewModel>>(courseList).Skip((pageNumber - 1) * pageSize).Take(pageSize); ;

            foreach (var post in result)
            {
                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                if (post.Likes.Any(x => x.UserId == loginUserId && x.PostId == post.Id))
                {
                    post.IsPostLikedByCurrentUser = true;
                }
                else
                {
                    post.IsPostLikedByCurrentUser = false;
                }
            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }

            return result;
        }

        async Task<IEnumerable<CourseCertificateViewModel>> GetCertificateByCourseId(Guid courseId)
        {
            var courseCertificate = _courseCertificateRepository.GetAll().Where(x => x.CourseId == courseId).ToList();
            var response = _mapper.Map<IEnumerable<CourseCertificateViewModel>>(courseCertificate);
            return response;
        }

        public async Task<CourseViewModel> GetBasicCourseInfo(Guid courseId)
        {
            var course = await _courseRepository.GetAll().Include(x => x.School).Where(x => x.CourseId == courseId).FirstOrDefaultAsync();

            if (course == null)
            {
                var classDetails = await _classRepository.GetAll().Include(x => x.School).Where(x => x.ClassId == courseId).FirstOrDefaultAsync();
                var courses = new CourseViewModel();
                courses.CourseId = classDetails.ClassId;
                courses.CourseName = classDetails.ClassName;
                courses.IsConvertable = true;
                courses.School = _mapper.Map<SchoolViewModel>(classDetails.School);
                return courses;
            }

            var response = _mapper.Map<CourseViewModel>(course);
            return response;

        }

        public async Task DeleteCourseLanguage(CourseLanguageViewModel model)
        {
            var courseLanguage = await _courseLanguageRepository.GetAll().Where(x => x.CourseId == model.CourseId && x.LanguageId == model.LanguageId).FirstOrDefaultAsync();

            if (courseLanguage == null)
            {
                await _classService.DeleteClassLanguage(_mapper.Map<ClassLanguageViewModel>(model));
            }
            else
            {
                _courseLanguageRepository.Delete(courseLanguage.Id);
                _courseLanguageRepository.Save();
            }
        }

        public async Task DeleteCourseTeacher(CourseTeacherViewModel model)
        {
            var courseTeacher = await _courseTeacherRepository.GetAll().Where(x => x.CourseId == model.CourseId && x.TeacherId == model.TeacherId).FirstOrDefaultAsync();

            if (courseTeacher == null)
            {
                await _classService.DeleteClassTeacher(_mapper.Map<ClassTeacherViewModel>(model));
            }
            else
            {
                _courseTeacherRepository.Delete(courseTeacher.Id);
                _courseTeacherRepository.Save();
            }

        }

        public async Task<IEnumerable<PostAttachmentViewModel>> GetAttachmentsByPostId(Guid postId)
        {
            var attacchmentList = await _postAttachmentRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.PostId == postId).ToListAsync();

            var result = _mapper.Map<List<PostAttachmentViewModel>>(attacchmentList);
            return result;
        }

        public async Task<IEnumerable<PostTagViewModel>> GetTagsByPostId(Guid postId)
        {
            var tagList = await _postTagRepository.GetAll().Where(x => x.PostId == postId).ToListAsync();

            var result = _mapper.Map<List<PostTagViewModel>>(tagList);
            return result;
        }
        public async Task SaveCourseCertificates(SaveCourseCertificateViewModel courseCertificates)
        {
            //string containerName = "coursecertificates";
            string containerName = this._config.GetValue<string>("Container:CourseContainer");

            var courses = await GetAllCourses();
            var isCourseExist = courses.Where(x => x.CourseId == courseCertificates.CourseId).FirstOrDefault();
            if (isCourseExist == null)
            {
                await _classService.SaveClassCertificates(_mapper.Map<SaveClassCertificateViewModel>(courseCertificates));
            }
            else
            {
                foreach (var certificate in courseCertificates.Certificates)
                {
                    string certificateUrl = await _blobService.UploadFileAsync(certificate, containerName);

                    string certificateName = certificate.FileName;

                    var classCertificate = new CourseCertificate
                    {
                        CertificateUrl = certificateUrl,
                        Name = certificateName,
                        CourseId = courseCertificates.CourseId
                    };
                    _courseCertificateRepository.Insert(classCertificate);
                    _courseCertificateRepository.Save();
                }
            }

        }

        public async Task DeleteCourseCertificate(CourseCertificateViewModel model)
        {
            var courseCertificate = await _courseCertificateRepository.GetAll().Where(x => x.CourseId == model.CourseId && x.CertificateId == model.CertificateId).FirstOrDefaultAsync();

            if (courseCertificate == null)
            {
                await _classService.DeleteClassCertificate(_mapper.Map<ClassCertificateViewModel>(model));
            }
            else
            {
                _courseCertificateRepository.Delete(courseCertificate.CertificateId);
                _courseCertificateRepository.Save();
            }

        }

        public async Task<bool> ConvertToClass(string courseName)
        {
            Class classes = await _classRepository.GetAll().Where(x => x.ClassName.Replace(" ", "").ToLower() == courseName).FirstOrDefaultAsync();
            if (classes != null)
            {
                classes.IsCourse = false;
                _classRepository.Update(classes);
                _classRepository.Save();
                return true;
            }
            return false;
        }

        public async Task<CourseViewModel> GetCourseByName(string courseName, string schoolName)
        {
            var course = await _courseRepository.GetAll().Include(x => x.School).Where(x => x.CourseName.Replace(" ", "").ToLower() == courseName.Replace(" ", "").ToLower() && x.School.SchoolName.Replace(" ", "").ToLower() == schoolName.Replace(" ", "").ToLower()).FirstOrDefaultAsync();
            if (course != null)
            {
                return _mapper.Map<CourseViewModel>(course);
            }
            return null;
        }

        public async Task<bool> IsCourseNameExist(string courseName)
        {
            var result = await _courseRepository.GetAll().Where(x => x.CourseName == courseName).FirstOrDefaultAsync();
            if (result != null)
            {
                return false;
            }
            return true;
        }

        async Task SaveCourseTags(IEnumerable<string> courseTags, Guid courseId)
        {
            foreach (var tagValue in courseTags)
            {
                var courseTag = new CourseTag
                {
                    CourseId = courseId,
                    CourseTagValue = tagValue
                };

                _courseTagRepository.Insert(courseTag);
                _courseTagRepository.Save();

            }
        }

        public async Task<List<CourseLikeViewModel>> GetLikesOnCourse(Guid courseId)
        {
            var likes = await _courseLikeRepository.GetAll().Where(x => x.CourseId == courseId).ToListAsync();
            return _mapper.Map<List<CourseLikeViewModel>>(likes);
        }
        public async Task<List<CourseViewsViewModel>> GetViewsOnCourse(Guid courseId)
        {
            var views = await _courseViewsRepository.GetAll().Where(x => x.CourseId == courseId).ToListAsync();
            return _mapper.Map<List<CourseViewsViewModel>>(views);

        }

        public async Task<List<CourseLikeViewModel>> LikeUnlikeCourse(LikeUnlikeClassCourse model)
        {

            var userLike = await _courseLikeRepository.GetAll().Where(x => x.UserId == model.UserId && x.CourseId == model.Id).FirstOrDefaultAsync();

            if (userLike != null)
            {
                _courseLikeRepository.Delete(userLike.Id);
                _courseLikeRepository.Save();
                var totalLikes = await _courseLikeRepository.GetAll().Where(x => x.CourseId == model.Id).ToListAsync();
                return _mapper.Map<List<CourseLikeViewModel>>(totalLikes);
            }

            else
            {
                var like = new CourseLike
                {
                    UserId = model.UserId,
                    CourseId = model.Id,
                    DateTime = DateTime.UtcNow
                };

                _courseLikeRepository.Insert(like);
                _courseLikeRepository.Save();
                var totalLikes = await _courseLikeRepository.GetAll().Where(x => x.CourseId == model.Id).ToListAsync();
                return _mapper.Map<List<CourseLikeViewModel>>(totalLikes);
            }
            return null;
        }

        public async Task<int> CourseView(CourseViewsViewModel model)
        {
            var isUserViewExist = await _courseViewsRepository.GetAll().Where(x => x.UserId == model.UserId && x.CourseId == model.CourseId).FirstOrDefaultAsync();
            if (isUserViewExist == null)
            {
                var view = new CourseViews
                {
                    UserId = model.UserId,
                    CourseId = model.CourseId,
                };

                _courseViewsRepository.Insert(view);
                _courseViewsRepository.Save();
                return _courseViewsRepository.GetAll().Where(x => x.CourseId == model.CourseId).Count();
            }
            return _courseViewsRepository.GetAll().Where(x => x.CourseId == model.CourseId).Count();
        }

        public async Task<List<ClassCourseFilterViewModel>> GetCourseFilters(string userId, Guid schoolId)
        {
            var classCourseFilters = await _classCourseFilterRepository.GetAll().OrderBy(x => x.DateTime).ToListAsync();

            var result = _mapper.Map<List<ClassCourseFilterViewModel>>(classCourseFilters);

            var userClassCourseFilters = await _userClassCourseFilterRepository.GetAll().Where(x => x.UserId == userId && x.SchoolId == schoolId).ToListAsync();


            foreach (var item in result)
            {
                var response = userClassCourseFilters.Where(x => x.ClassCourseFilterId == item.Id && x.UserId == userId && x.ClassCourseFilterType == ClassCourseFilterEnum.Course).FirstOrDefault();

                if (response != null)
                {
                    item.IsFilterActive = response.IsActive;
                }

            }

            return result;

        }

        public async Task SaveCourseFilters(List<UserClassCourseFilterViewModel> model, string userId)
        {
            var result = await _userClassCourseFilterRepository.GetAll().ToListAsync();
            foreach (var item in model)
            {
                var isUserClassCourseFilterExist = result.Where(x => x.UserId == userId && x.ClassCourseFilterId == item.Id && x.ClassCourseFilterType == ClassCourseFilterEnum.Course).FirstOrDefault();

                if (isUserClassCourseFilterExist != null)
                {
                    isUserClassCourseFilterExist.IsActive = item.IsActive;
                    _userClassCourseFilterRepository.Update(isUserClassCourseFilterExist);
                    _userClassCourseFilterRepository.Save();
                }
                else
                {
                    var userClassCourseFilter = new UserClassCourseFilter
                    {
                        UserId = userId,
                        ClassCourseFilterId = item.Id,
                        IsActive = item.IsActive,
                        SchoolId = item.SchoolId,
                        ClassCourseFilterType = ClassCourseFilterEnum.Course
                    };

                    _userClassCourseFilterRepository.Insert(userClassCourseFilter);
                    _userClassCourseFilterRepository.Save();
                }
            }

        }

        public async Task<CourseInfoForCertificateViewModel> GetCourseInfoForCertificate(Guid courseId)
        {
            CourseInfoForCertificateViewModel model = new CourseInfoForCertificateViewModel();

            var course = await _courseRepository.GetAll()
                .Include(x => x.School)
                .Include(x => x.CreatedBy)
                .Where(x => x.CourseId == courseId).FirstOrDefaultAsync();

            model = _mapper.Map<CourseInfoForCertificateViewModel>(course);

            model.Students = await GetCourseStudents(course.CourseId);
            return model;

        }

        public async Task<List<StudentViewModel>> GetCourseStudents(Guid courseId)
        {
            var courseStudents = _courseStudentRepository.GetAll()
                .Include(x => x.Student)
                .ThenInclude(x => x.CreatedBy)
                .Where(x => x.CourseId == courseId).ToList();

            var result = _mapper.Map<List<StudentViewModel>>(courseStudents.Select(x => x.Student));
            return result;
        }




    }
}
