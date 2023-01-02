﻿using AutoMapper;
using LMS.Common.Enums;
using LMS.Common.ViewModels.Account;
using LMS.Common.ViewModels.Class;
using LMS.Common.ViewModels.Common;
using LMS.Common.ViewModels.Course;
using LMS.Common.ViewModels.Post;
using LMS.Common.ViewModels.School;
using LMS.Common.ViewModels.Student;
using LMS.Common.ViewModels.User;
using LMS.Data.Entity;
using LMS.DataAccess.Repository;
using LMS.Services.Blob;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public class UserService : IUserService
    {
        public string containerName = "userlogo";
        private readonly IMapper _mapper;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<UserFollower> _userFollowerRepository;
        private IGenericRepository<UserLanguage> _userLanguageRepository;
        private IGenericRepository<City> _cityRepository;
        private IGenericRepository<Country> _countryRepository;
        private IGenericRepository<SchoolFollower> _schoolFollowerRepository;
        private IGenericRepository<PostAttachment> _postAttachmentRepository;
        private IGenericRepository<ClassStudent> _classStudentRepository;
        private IGenericRepository<CourseStudent> _courseStudentRepository;
        private IGenericRepository<ClassTeacher> _classTeacherRepository;
        private IGenericRepository<CourseTeacher> _courseTeacherRepository;
        private IGenericRepository<SchoolTeacher> _schoolTeacherRepository;
        private IGenericRepository<Student> _studentRepository;
        private IGenericRepository<Teacher> _teacherRepository;
        private IGenericRepository<School> _schoolRepository;
        private IGenericRepository<Class> _classRepository;
        private IGenericRepository<Post> _postRepository;
        private IGenericRepository<PostTag> _postTagRepository;
        private readonly UserManager<User> _userManager;
        private readonly IBlobService _blobService;
        public UserService(IMapper mapper, IGenericRepository<User> userRepository, IGenericRepository<UserFollower> userFollowerRepository, IGenericRepository<UserLanguage> userLanguageRepository, IGenericRepository<City> cityRepository, IGenericRepository<Country> countryRepository, IGenericRepository<SchoolFollower> schoolFollowerRepository, IGenericRepository<PostAttachment> postAttachmentRepository, IGenericRepository<ClassStudent> classStudentRepository, IGenericRepository<CourseStudent> courseStudentRepository, IGenericRepository<ClassTeacher> classTeacherRepository, IGenericRepository<CourseTeacher> courseTeacherRepository,
          IGenericRepository<SchoolTeacher> schoolteacherRepository, IGenericRepository<Student> studentRepository, IGenericRepository<Teacher> teacherRepository, IGenericRepository<School> schoolRepository, IGenericRepository<Class> classRepository, IGenericRepository<Post> postRepository, IGenericRepository<PostTag> postTagRepository, UserManager<User> userManager, IBlobService blobService)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _userFollowerRepository = userFollowerRepository;
            _userLanguageRepository = userLanguageRepository;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _schoolFollowerRepository = schoolFollowerRepository;
            _postAttachmentRepository = postAttachmentRepository;
            _classStudentRepository = classStudentRepository;
            _courseStudentRepository = courseStudentRepository;
            _classTeacherRepository = classTeacherRepository;
            _courseTeacherRepository = courseTeacherRepository;
            _schoolTeacherRepository = schoolteacherRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _schoolRepository = schoolRepository;
            _classRepository = classRepository;
            _postRepository = postRepository;
            _postTagRepository = postTagRepository;
            _userManager = userManager;
            _blobService = blobService;
        }
        public async Task<UserDetailsViewModel> GetUserById(string userId)
        {
            var user = await _userRepository.GetAll().Include(x => x.City).Where(x => x.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserDetailsViewModel>(user);
            result.Followers = await GetFollowers(userId);
            result.Languages = await GetLanguages(userId);
            result.Followings = await GetFollowings(userId);
            result.Posts = await GetPostsByUserId(userId);
            var classStudents = await GetClassStudents(userId);
            var courseStudents = await GetCourseStudents(userId);

            result.SchoolsAsStudent = classStudents.Union(courseStudents).DistinctBy(x => x.SchoolId).ToList();

            var schoolTeachers = await GetSchoolTeachers(userId);
            var classTeachers = await GetClassTeachers(userId);
            var courseTeachers = await GetCourseTeachers(userId);

            var classCourseTeachers = classTeachers.Union(courseTeachers).DistinctBy(x => x.SchoolId).ToList();

            result.SchoolsAsTeacher = classCourseTeachers.Union(schoolTeachers).DistinctBy(x => x.SchoolId).ToList();

            return result;

        }

        public async Task<UserUpdateViewModel> GetUserEditDetails(string userId)
        {
            var user = await _userRepository.GetAll().Where(x => x.Id == userId)
                .FirstOrDefaultAsync();

            var result = _mapper.Map<UserUpdateViewModel>(user);
            return result;
        }

        public async Task<bool> FollowUnFollowUser(FollowUnFollowViewModel model, string followerId)
        {
            var userFollowers = new List<UserFollower>();
            userFollowers = await _userFollowerRepository.GetAll().Where(x => x.UserId == model.Id && x.FollowerId == followerId).ToListAsync();

            if (userFollowers.Any(x => x.UserId == model.Id && x.FollowerId == followerId))
            {
                _userFollowerRepository.DeleteAll(userFollowers);
                _userFollowerRepository.Save();
            }

            else
            {
                var userFollower = new UserFollower
                {
                    UserId = model.Id,
                    FollowerId = followerId,
                    IsBan = false
                };

                _userFollowerRepository.Insert(userFollower);
                _userFollowerRepository.Save();
                return true;
            }
            return false;

            //if (!model.IsFollowed)
            //{
            //    var userFollower = await _userFollowerRepository.GetAll().Where(x => x.UserId == model.Id && x.FollowerId == followerId).FirstOrDefaultAsync();

            //    if (userFollower != null)
            //    {
            //        _userFollowerRepository.Delete(userFollower.Id);
            //        _userFollowerRepository.Save();
            //        return false;
            //    }
            //}

            //else
            //{
            //    var userFollower = new UserFollower
            //    {
            //        UserId = model.Id,
            //        FollowerId = followerId,
            //        IsBan = false
            //    };

            //    _userFollowerRepository.Insert(userFollower);
            //    _userFollowerRepository.Save();
            //    return true;
            //}
            //return false;
        }

        public async Task<IEnumerable<UserFollowerViewModel>> GetFollowers(string userId)
        {
            var followerList = await _userFollowerRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();
            return _mapper.Map<IEnumerable<UserFollowerViewModel>>(followerList);
        }

        public async Task<int> GetFollowings(string userId)
        {
            var followerList = await _userFollowerRepository.GetAll().Where(x => x.FollowerId == userId).ToListAsync();
            return followerList.Count();
        }

        public async Task<IEnumerable<SchoolViewModel>> GetClassStudents(string userId)
        {
            var studentList = await _studentRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();

            var classStudentsList = await _classStudentRepository.GetAll().Include(x => x.Class).ThenInclude(x => x.School).Distinct().ToListAsync();

            var requiredClassList = classStudentsList.Where(x => studentList.Any(y => y.StudentId == x.StudentId)).DistinctBy(x => x.ClassId);

            var schools = new List<School>();
            foreach (var item in requiredClassList)
            {
                schools.Add(item.Class.School);
            }

            var requiredSchools = _mapper.Map<List<SchoolViewModel>>(schools);

            //var classStudents = new List<Student>();
            //foreach (var classStudent in requiredClassList)
            //{
            //    classStudents.Add(classStudent.Student);
            //}

            //var result = _mapper.Map<List<StudentViewModel>>(classStudents);
            return requiredSchools;

        }

        public async Task<IEnumerable<SchoolViewModel>> GetCourseStudents(string userId)
        {
            var studentList = await _studentRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();

            var courseStudentsList = await _courseStudentRepository.GetAll().Include(x => x.Course).ThenInclude(x => x.School).Distinct().ToListAsync();

            var requiredCourseList = courseStudentsList.Where(x => studentList.Any(y => y.StudentId == x.StudentId)).DistinctBy(x => x.CourseId);

            var schools = new List<School>();
            foreach (var item in requiredCourseList)
            {
                schools.Add(item.Course.School);
            }

            var requiredSchools = _mapper.Map<List<SchoolViewModel>>(schools);
            return requiredSchools;

        }

        public async Task<IEnumerable<SchoolViewModel>> GetSchoolTeachers(string userId)
        {
            var teacherList = await _teacherRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();

            var schoolTeachersList = await _schoolTeacherRepository.GetAll().Include(x => x.School).Distinct().ToListAsync();

            var requiredSchoolsList = schoolTeachersList.Where(x => teacherList.Any(y => y.TeacherId == x.TeacherId)).DistinctBy(x => x.SchoolId);

            var schools = new List<School>();
            foreach (var item in requiredSchoolsList)
            {
                schools.Add(item.School);
            }

            var requiredSchools = _mapper.Map<List<SchoolViewModel>>(schools);

            return requiredSchools;

        }

        public async Task<IEnumerable<SchoolViewModel>> GetClassTeachers(string userId)
        {
            var teachertList = await _teacherRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();

            var classTeachersList = await _classTeacherRepository.GetAll().Include(x => x.Class).ThenInclude(x => x.School).Distinct().ToListAsync();

            var requiredClassList = classTeachersList.Where(x => teachertList.Any(y => y.TeacherId == x.TeacherId)).DistinctBy(x => x.ClassId);

            var schools = new List<School>();
            foreach (var item in requiredClassList)
            {
                schools.Add(item.Class.School);
            }

            var requiredSchools = _mapper.Map<List<SchoolViewModel>>(schools);
            return requiredSchools;

        }

        public async Task<IEnumerable<SchoolViewModel>> GetCourseTeachers(string userId)
        {
            var teachertList = await _teacherRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();

            var courseTeachersList = await _courseTeacherRepository.GetAll().Include(x => x.Course).ThenInclude(x => x.School).Distinct().ToListAsync();

            var requiredCourseList = courseTeachersList.Where(x => teachertList.Any(y => y.TeacherId == x.TeacherId)).DistinctBy(x => x.CourseId);

            var schools = new List<School>();
            foreach (var item in requiredCourseList)
            {
                schools.Add(item.Course.School);
            }

            var requiredSchools = _mapper.Map<List<SchoolViewModel>>(schools);
            return requiredSchools;

        }

        public async Task SaveUserLanguages(UserLanguageViewModel userLanguageViewModel)
        {
            foreach (var languageId in userLanguageViewModel.LanguageIds)
            {
                var userLanguage = new UserLanguage
                {
                    UserId = userLanguageViewModel.UserId,
                    LanguageId = new Guid(languageId)
                };

                _userLanguageRepository.Insert(userLanguage);
                _userLanguageRepository.Save();
            }
        }

        public async Task DeleteUserLanguage(UserLanguageDeleteViewModel model)
        {

            var userLanguage = await _userLanguageRepository.GetAll().Where(x => x.UserId == model.UserId && x.LanguageId == new Guid(model.LanguageId)).FirstOrDefaultAsync();

            _userLanguageRepository.Delete(userLanguage.Id);
            _userLanguageRepository.Save();
        }

        async Task<IEnumerable<LanguageViewModel>> GetLanguages(string userId)
        {
            var userLanguages = _userLanguageRepository.GetAll()
                .Include(x => x.Language)
                .Where(x => x.UserId == userId).ToList();

            var languageViewModel = new List<LanguageViewModel>();
            foreach (var userLanguage in userLanguages)
            {
                languageViewModel.Add(_mapper.Map<LanguageViewModel>(userLanguage.Language));
            }
            return languageViewModel;
        }

        public async Task<string> UpdateUser(UserUpdateViewModel userUpdateViewModel)
        {

            if (userUpdateViewModel.AvatarImage != null)
            {
                userUpdateViewModel.Avatar = await _blobService.UploadFileAsync(userUpdateViewModel.AvatarImage, containerName);
            }

            User user = _userRepository.GetById(userUpdateViewModel.Id);
            user.Avatar = userUpdateViewModel.Avatar;
            user.FirstName = userUpdateViewModel.FirstName;
            user.LastName = userUpdateViewModel.LastName;
            user.DOB = userUpdateViewModel.DOB;
            user.Gender = userUpdateViewModel.Gender;
            user.Description = userUpdateViewModel.Description;
            user.ContactEmail = userUpdateViewModel.ContactEmail;

            _userRepository.Update(user);
            _userRepository.Save();
            return user.Id;

        }

        public async Task<IEnumerable<CountryViewModel>> CountryList()
        {
            var countryList = await _countryRepository.GetAll().ToListAsync();
            var result = _mapper.Map<IEnumerable<CountryViewModel>>(countryList);
            return result;
        }

        public async Task<IEnumerable<CityViewModel>> CityList(Guid countryId)
        {
            var cityList = await _cityRepository.GetAll().Where(x => x.CountryId == countryId).ToListAsync();
            var result = _mapper.Map<IEnumerable<CityViewModel>>(cityList);
            return result;
        }

        public async Task<IEnumerable<PostAttachmentViewModel>> GetMyFeed(string userId)
        {
            var myFeeds = new List<PostAttachmentViewModel>();

            // feeds from schools user follow
            var schoolFollowers = await _schoolFollowerRepository.GetAll()
                .Include(x => x.User)
                .Include(x => x.School)
                .Where(x => x.UserId == userId).ToListAsync();


            foreach (var schoolFollower in schoolFollowers)
            {
                var postAttachments = await _postAttachmentRepository.GetAll().Include(x => x.Post).Where(x => x.Post.ParentId == schoolFollower.SchoolId).ToListAsync();

                myFeeds.AddRange(_mapper.Map<List<PostAttachmentViewModel>>(postAttachments));

            }

            // posts from people user follow
            var userFollowers = await _userFollowerRepository.GetAll().Where(x => x.FollowerId == userId).ToListAsync();

            foreach (var userFollower in userFollowers)
            {
                var postAttachments = await _postAttachmentRepository.GetAll().Include(x => x.Post).Where(x => x.Post.ParentId == new Guid(userFollower.UserId)).ToListAsync();

                myFeeds.AddRange(_mapper.Map<List<PostAttachmentViewModel>>(postAttachments));
            }

            // posts from classes user is a student for
            var classStudents = await _classStudentRepository.GetAll()
                .Include(x => x.Student)
                .Include(x => x.Class)
                .Where(x => x.Student.UserId == userId).ToListAsync();

            foreach (var classStudent in classStudents)
            {
                var postAttachments = await _postAttachmentRepository.GetAll().Include(x => x.Post).Where(x => x.Post.ParentId == classStudent.ClassId).ToListAsync();

                myFeeds.AddRange(_mapper.Map<List<PostAttachmentViewModel>>(postAttachments));
            }



            // posts from courses user is a student for
            var courseStudents = await _courseStudentRepository.GetAll()
                .Include(x => x.Student)
                .Include(x => x.Course)
                .Where(x => x.Student.UserId == userId).ToListAsync();

            foreach (var courseStudent in courseStudents)
            {
                var postAttachments = await _postAttachmentRepository.GetAll().Include(x => x.Post).Where(x => x.Post.ParentId == courseStudent.CourseId).ToListAsync();

                myFeeds.AddRange(_mapper.Map<List<PostAttachmentViewModel>>(postAttachments));
            }

            return myFeeds;
        }

        public async Task<IEnumerable<PostAttachmentViewModel>> GetUserProfileFeed(string userId)
        {
            var userProfileFeeds = new List<PostAttachmentViewModel>();

            var postAttachments = await _postAttachmentRepository.GetAll().Include(x => x.Post).Where(x => x.Post.CreatedById == userId).ToListAsync();

            userProfileFeeds.AddRange(_mapper.Map<List<PostAttachmentViewModel>>(postAttachments));
            return userProfileFeeds;

        }

        public async Task<UserDetailsViewModel> GetBasicUserInfo(string userId)
        {
            var user = await _userRepository.GetAll().Where(x => x.Id == userId).FirstOrDefaultAsync();

            var response = _mapper.Map<UserDetailsViewModel>(user);
            return response;

        }

        public async Task<IEnumerable<PostDetailsViewModel>> GetPostsByUserId(string userId)
        {
            var courseList = await _postRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.ParentId == new Guid (userId)).OrderByDescending(x => x.IsPinned).ToListAsync();

            var result = _mapper.Map<List<PostDetailsViewModel>>(courseList);

            foreach (var post in result)
            {
                var attachment = await GetAttachmentsByPostId(post.Id);
                post.PostAttachments = attachment;
            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }
            return result;
            //var posts = await _postAttachmentRepository.GetAll()
            //    .Include(x => x.Post)
            //    .Where(x => x.CreatedById == userId).OrderByDescending(x => x.Post.IsPinned).ToListAsync();

            //var response = _mapper.Map<List<PostAttachmentViewModel>>(posts);

            //foreach (var item in response)
            //{
            //    if (item.Post.PostAuthorType == (int)PostAuthorTypeEnum.School)
            //    {
            //        var school = _schoolRepository.GetById(item.Post.ParentId);
            //        item.School = _mapper.Map<SchoolViewModel>(school);
            //    }

            //    if (item.Post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
            //    {
            //        var classes = _classRepository.GetById(item.Post.ParentId);
            //        item.Class = _mapper.Map<ClassViewModel>(classes);
            //    }

            //    if (item.Post.PostAuthorType == (int)PostAuthorTypeEnum.User)
            //    {
            //        try
            //        {
            //            var user = _userRepository.GetById(item.Post.ParentId.ToString());
            //            item.User = _mapper.Map<UserDetailsViewModel>(user);
            //        }
            //        catch (Exception ex)
            //        {
            //            throw ex;
            //        }
            //    }
            //}


            //return response;
        }

        public async Task<List<UserFollowerViewModel>> GetUserFollowers(string userId)
        {
            var followerList = await _userFollowerRepository.GetAll().Include(x => x.Follower).Where(x => x.UserId == userId && !x.IsBan).ToListAsync();

            var response = _mapper.Map<List<UserFollowerViewModel>>(followerList);
            return response;

        }

        public async Task<bool> BanFollower(string followerId)
       {
            var follower = await _userFollowerRepository.GetAll().Where(x => x.FollowerId == followerId).FirstOrDefaultAsync();

            if (follower != null)
            {
                follower.IsBan = true;
                _userFollowerRepository.Update(follower);
                _userFollowerRepository.Save();
                return true;
            }

            return false;

            

        }

        public async Task<IEnumerable<PostAttachmentViewModel>> GetAttachmentsByPostId(Guid postId)
        {
            var attacchmentList = await _postAttachmentRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.PostId == postId).OrderByDescending(x => x.IsPinned).ToListAsync();

            var result = _mapper.Map<List<PostAttachmentViewModel>>(attacchmentList);
            return result;
        }

        public async Task<IEnumerable<PostTagViewModel>> GetTagsByPostId(Guid postId)
        {
            var tagList = await _postTagRepository.GetAll().Where(x => x.PostId == postId).ToListAsync();

            var result = _mapper.Map<List<PostTagViewModel>>(tagList);
            return result;
        }



    }
}
