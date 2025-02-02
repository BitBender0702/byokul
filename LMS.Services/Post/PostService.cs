﻿using LMS.Common.ViewModels.Post;
using LMS.Data.Entity;
using AutoMapper;
using LMS.DataAccess.Repository;
using Microsoft.AspNetCore.Http;
using LMS.Services.Blob;
using LMS.Common.Enums;
using LMS.Services.BigBlueButton;
using LMS.Common.ViewModels.BigBlueButton;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using LMS.Common.ViewModels.School;
using LMS.Common.ViewModels.Class;
using LMS.Common.ViewModels.User;
using Newtonsoft.Json;
using LMS.Common.ViewModels.Course;
using LMS.Services.Chat;
using LMS.Common.ViewModels.FileStorage;
using LMS.Common.ViewModels.Notification;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using System.Collections;
using LMS.DataAccess.GenericRepository;
using System.Text.RegularExpressions;
using LMS.Common.ViewModels.Common;
using Microsoft.AspNetCore.SignalR;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
//using LMS.Data.Entity.Chat;
using LMS.Data.Migrations;
using System.Threading;
using System.Web;

namespace LMS.Services
{
    public class PostService : IPostService
    {
        private readonly IMapper _mapper;
        private IGenericRepository<Post> _postRepository;
        private IGenericRepository<PostAttachment> _postAttachmentRepository;
        private IGenericRepository<PostTag> _postTagRepository;
        private IGenericRepository<School> _schoolRepository;
        private IGenericRepository<Class> _classRepository;
        private IGenericRepository<Course> _courseRepository;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<Like> _likeRepository;
        private IGenericRepository<View> _viewRepository;
        private IGenericRepository<Comment> _commentRepository;
        private IGenericRepository<CommentLike> _commentLikeRepository;
        private IGenericRepository<UserSharedPost> _userSharedPostRepository;
        private IGenericRepository<Notification> _notificationRepository;
        private IGenericRepository<SavedPost> _savedPostRepository;
        private readonly IBlobService _blobService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly INotificationService _notificationService;
        private readonly IBigBlueButtonService _bigBlueButtonService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<ChatHubs> _hubContext;
        private IConfiguration _config;
        private readonly ChatHubs _chatHubs;


        public PostService(IMapper mapper, IGenericRepository<Post> postRepository, IGenericRepository<PostAttachment> postAttachmentRepository, IGenericRepository<PostTag> postTagRepository, IGenericRepository<School> schoolRepository, IGenericRepository<Class> classRepository, IGenericRepository<Course> courseRepository, IGenericRepository<User> userRepository, IGenericRepository<Like> likeRpository, IGenericRepository<View> viewRepository, IGenericRepository<Comment> commentRepository, IGenericRepository<CommentLike> commentLikeRepository, IGenericRepository<SavedPost> savedPostRepository, IBlobService blobService, IUserService userService, IChatService chatService, IBigBlueButtonService bigBlueButtonService, IConfiguration config, IGenericRepository<UserSharedPost> userSharedPostRepository, INotificationService notificationService, IGenericRepository<Notification> notificationRepository, IHubContext<ChatHubs> hubContext, IWebHostEnvironment webHostEnvironment, ChatHubs chatHubs)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _postAttachmentRepository = postAttachmentRepository;
            _postTagRepository = postTagRepository;
            _schoolRepository = schoolRepository;
            _classRepository = classRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _likeRepository = likeRpository;
            _viewRepository = viewRepository;
            _commentRepository = commentRepository;
            _commentLikeRepository = commentLikeRepository;
            _savedPostRepository = savedPostRepository;
            _blobService = blobService;
            _userService = userService;
            _chatService = chatService;
            _bigBlueButtonService = bigBlueButtonService;
            _config = config;
            _userSharedPostRepository = userSharedPostRepository;
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
            _chatHubs = chatHubs;
        }
        public async Task<PostViewModel> SavePost(PostViewModel postViewModel, string createdById)
        {
            bool scheduleLiveStream = false;
            bool isPostSchedule = false;
            //postViewModel.BlobUrls[0].BlobName = postViewModel.BlobUrls[0].BlobName + "_index0";
            try
            {
                var post = new Post
                {
                    Title = postViewModel.Title,
                    Status = postViewModel.Status,
                    OwnerId = postViewModel.OwnerId,
                    AuthorId = postViewModel.AuthorId,
                    DateTime = postViewModel.DateTime,
                    PostType = postViewModel.PostType,
                    Description = postViewModel.Description,
                    PostAuthorType = postViewModel.PostAuthorType,
                    ParentId = postViewModel.ParentId,
                    CoverLetter = postViewModel.CoverLetter,
                    IsPostSchedule = false,
                    IsCommentsDisabled = false,
                    SharedProfileUrl = postViewModel.SharedProfileUrl,
                    CommentsPerMinute = postViewModel.CommentsPerMinute == 0 ? null : postViewModel.CommentsPerMinute,
                    Attachments = postViewModel.BlobUrls.Select(x => new PostAttachment
                    {
                        Id = x.Id,
                        CreatedById = createdById,
                        CreatedOn = DateTime.UtcNow,
                        FileName = x.BlobName,
                        FileType = Convert.ToInt32(x.FileType),
                        FileUrl = x.BlobUrl,
                        IsCompressed = false,
                        FileThumbnail = x.FileThumbnail
                    }).ToList(),
                    Tags = postViewModel.PostTags.Select(x => new PostTag
                    {
                        PostTagValue = x
                    }).ToList(),
                    CreatedById = createdById,
                    CreatedOn = DateTime.UtcNow
                };

                if (postViewModel.PostType == (int)PostTypeEnum.Stream && postViewModel.DateTime.HasValue)
                {
                    post.StreamUrl = postViewModel.BlobUrls[0].BlobUrl;
                    post.IsPostSchedule = true;
                    scheduleLiveStream = true;
                }
                else if (postViewModel.PostType == (int)PostTypeEnum.Stream && postViewModel.BlobUrls.Any(x => x.FileType == FileTypeEnum.Video))
                {
                    post.StreamUrl = postViewModel.BlobUrls.Where(x => x.FileType == FileTypeEnum.Video).Select(x => x.BlobUrl).First();
                    post.IsLive = false;
                    post.IsLiveStreamEnded = false;
                }
                if (postViewModel.PostType == (int)PostTypeEnum.Stream && postViewModel.BlobUrls.All(x => x.FileType != FileTypeEnum.Video))
                {
                    var user = _userRepository.GetById(createdById);
                    var model = new NewMeetingViewModel();
                    model.meetingName = postViewModel.Title;
                    model.IsMicrophoneOpen = (bool)postViewModel.IsMicroPhoneOpen;
                    model.ModeratorName = user.FirstName;
                    model.PostId = post.Id;
                    var response = await _bigBlueButtonService.Create(model);
                    //response.StreamUrl = HttpUtility.UrlEncode(response.StreamUrl);

                    //response.StreamUrl = Uri.EscapeDataString(response.StreamUrl);
                    postViewModel.StreamUrl = response.StreamUrl;
                    post.StreamUrl = response.StreamUrl;
                    post.ExternalMeetingId = new Guid(response.MeetingID);
                    post.InternalMeetingId = response.InternalMeetingID;
                    post.IsLive = true;
                }

                if ((postViewModel.PostType == (int)PostTypeEnum.Post || postViewModel.PostType == (int)PostTypeEnum.Reel) && postViewModel.DateTime != null)
                {
                    post.IsPostSchedule = true;
                    isPostSchedule = true;
                }

                try
                {
                    _postRepository.Insert(post);
                    _postRepository.Save();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                postViewModel.Id = post.Id;

                if (scheduleLiveStream)
                    await ScheduleLiveStream(postViewModel, createdById, new DateTimeOffset(postViewModel.DateTime.Value));
                if (isPostSchedule)
                    BackgroundJob.Schedule(() => SaveSchedulePostInfo(postViewModel, createdById), new DateTimeOffset(postViewModel.DateTime.Value));


                if (postViewModel.PostType == (int)PostTypeEnum.Reel)
                {
                    postViewModel.ReelId = await _postAttachmentRepository.GetAll().Where(x => x.PostId == post.Id).Select(x => x.Id).FirstAsync();
                }

                postViewModel.DateTime = post.DateTime;
                return postViewModel;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<VideoUploadResponseViewModel>> SaveBlobUrls(List<BlobUrlsViewModel> bloburls, IEnumerable<IFormFile> uploadVideosThumbnail, Guid postId, string createdById, bool isJobRunning)
        {
            var response = new List<VideoUploadResponseViewModel>();
            var uploadBlobUrls = new List<string>();
            string containerName = this._config.GetValue<string>("Container:PostContainer");
            string videoThumbnailName = "";
            foreach (var video in bloburls)
            {
                int lastDotIndex = video.BlobName.LastIndexOf(".");
                if (lastDotIndex != -1)
                {
                    videoThumbnailName = video.BlobName.Substring(0, lastDotIndex + 1);
                }

                var isVideoThumbnailExist = bloburls.Where(x => x.BlobName.Substring(0, x.BlobName.LastIndexOf(".") + 1) == videoThumbnailName && x.FileType == FileTypeEnum.Thumbnail).FirstOrDefault();

                var postAttachment = new PostAttachmentViewModel();
                if (isVideoThumbnailExist != null)
                {
                    postAttachment.FileThumbnail = isVideoThumbnailExist.BlobUrl;
                    response.Add(new VideoUploadResponseViewModel
                    {
                        FileName = isVideoThumbnailExist.BlobName,
                        ThumbnailUrl = postAttachment.FileThumbnail,
                        IsVideo = false
                    });

                }

                //postAttachment.FileUrl = await _blobService.UploadFileAsync(video, containerName, true);

                var postAttach = new PostAttachment
                {
                    Id = video.Id,
                    PostId = postId,
                    FileName = video.BlobName,
                    FileUrl = video.BlobUrl,
                    FileThumbnail = postAttachment.FileThumbnail,
                    FileType = (int)video.FileType,
                    CreatedById = createdById,
                    CreatedOn = DateTime.UtcNow,
                    IsCompressed = false
                };

                _postAttachmentRepository.Insert(postAttach);
                _postAttachmentRepository.Save();
                //uploadBlobUrls.Add(postAttach.FileUrl);
                response.Add(new VideoUploadResponseViewModel
                {
                    FileName = video.BlobName,
                    VideoUrl = postAttach.FileUrl,
                    IsVideo = true
                });

            }

            return response;
        }

        public async Task SaveCompressedVideos2(VideoUploadResponseViewModel model, string createdById, Guid PostId)
        {
            var attachment = await _postAttachmentRepository.GetAll().Where(x => x.PostId == PostId).FirstAsync();
            attachment.IsCompressed = true;
            _postAttachmentRepository.Update(attachment);
            _postAttachmentRepository.Save();
        }

        public async Task SaveCompressedVideos(List<VideoUploadResponseViewModel> model, string createdById, Guid postId)
        {
            byte[] byteArray;
            List<IFormFile> uploadVideos = new List<IFormFile>();
            List<IFormFile> uploadThumbnails = new List<IFormFile>();

            foreach (var item in model)
            {
                if (item.IsVideo)
                {
                    using (var httpClient = new HttpClient())
                    {
                        byteArray = await httpClient.GetByteArrayAsync(item.VideoUrl);
                    }

                    var memoryStream = new MemoryStream(byteArray);

                    var formFile = new FormFile(memoryStream, 0, memoryStream.Length, null, item.FileName)
                    {
                        Headers = new HeaderDictionary(),
                    };
                    formFile.ContentType = item.FileType;
                    uploadVideos.Add(formFile);
                }

                else
                {
                    using (var httpClient = new HttpClient())
                    {
                        byteArray = await httpClient.GetByteArrayAsync(item.ThumbnailUrl);
                    }

                    var memoryStream = new MemoryStream(byteArray);

                    var formFile = new FormFile(memoryStream, 0, memoryStream.Length, null, item.FileName)
                    {
                        Headers = new HeaderDictionary(),
                    };
                    formFile.ContentType = item.FileType;
                    uploadThumbnails.Add(formFile);
                }
            }

            var blobUrls = await SaveUploadVideos(uploadVideos, uploadThumbnails, postId, createdById, true);



            //List<IFormFile> uploadVideos = new List<IFormFile>();
            //List<IFormFile> uploadThumbnails = new List<IFormFile>();
            //foreach (var item in videoByteArrayList)
            //{
            //    var stream = new MemoryStream(item.Bytes);
            //    var file = new FormFile(stream, 0, item.Bytes.Length, item.FileName, item.FileName)
            //    {
            //        Headers = new HeaderDictionary()
            //    };
            //    file.ContentType = item.ContentType;
            //    uploadVideos.Add(file);
            //}

            //foreach (var item in thumbnailByteArrayList)
            //{
            //    var stream = new MemoryStream(item.Bytes);
            //    var file = new FormFile(stream, 0, item.Bytes.Length, item.FileName, item.FileName)
            //    {
            //        Headers = new HeaderDictionary()
            //    };
            //    file.ContentType = item.ContentType;
            //    uploadThumbnails.Add(file);
            //}

            //var blobUrls = await SaveUploadVideos(uploadVideos, uploadThumbnails, postId, createdById, true);
        }

        //public async Task SaveCompressedVideos(List<CompressVideoViewModel> videoByteArrayList, List<CompressVideoViewModel> thumbnailByteArrayList, string createdById, Guid postId)
        //{




        //    List<IFormFile> uploadVideos = new List<IFormFile>();
        //    List<IFormFile> uploadThumbnails = new List<IFormFile>();
        //    foreach (var item in videoByteArrayList)
        //    {
        //        var stream = new MemoryStream(item.Bytes);
        //        var file = new FormFile(stream, 0, item.Bytes.Length, item.FileName, item.FileName)
        //        {
        //            Headers = new HeaderDictionary()
        //        };
        //        file.ContentType = item.ContentType;
        //        uploadVideos.Add(file);
        //    }

        //    foreach (var item in thumbnailByteArrayList)
        //    {
        //        var stream = new MemoryStream(item.Bytes);
        //        var file = new FormFile(stream, 0, item.Bytes.Length, item.FileName, item.FileName)
        //        {
        //            Headers = new HeaderDictionary()
        //        };
        //        file.ContentType = item.ContentType;
        //        uploadThumbnails.Add(file);
        //    }

        //    var blobUrls = await SaveUploadVideos(uploadVideos, uploadThumbnails, postId, createdById, true);
        //}

        public async Task<PostViewModel> UpdatePost(PostViewModel model, string createdById)
        {
            var post = await _postRepository.GetAll().FirstAsync(x => x.Id == model.Id);
            post.Title = model.Title;
            post.Description = model.Description;
            post.UpdatedOn = DateTime.UtcNow;
            post.SharedProfileUrl = model.SharedProfileUrl;
            await SavePostTags(model.PostTags, post.Id);
            _postRepository.Update(post);
            _postRepository.Save();
            var attachments = await _postAttachmentRepository.GetAll().Where(x => x.PostId == model.Id).ToListAsync();

            foreach (var item in attachments)
            {
                if (!model.BlobUrls.Any(x => x.BlobUrl == item.FileUrl))
                {
                    var attachment = await _postAttachmentRepository.GetAll().Where(x => x.FileUrl == item.FileUrl).FirstAsync();
                    _postAttachmentRepository.Delete(attachment.Id);
                    //_postAttachmentRepository.Save();
                }
            }

            foreach (var item in model.BlobUrls)
            {
                if (!attachments.Any(x => x.FileUrl == item.BlobUrl))
                {
                    var attachment = new PostAttachment
                    {
                        Id = item.Id,
                        CreatedById = createdById,
                        CreatedOn = DateTime.UtcNow,
                        FileName = item.BlobName,
                        FileType = Convert.ToInt32(item.FileType),
                        FileUrl = item.BlobUrl,
                        IsCompressed = false,
                        PostId = post.Id,
                        FileThumbnail = item.FileThumbnail
                    };
                    _postAttachmentRepository.Insert(attachment);
                    //_postAttachmentRepository.Save();
                }
            }
            try
            {
                _postAttachmentRepository.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //_postRepository.Update(post);
            //_postRepository.Save();
            model.UpdatedOn = post.UpdatedOn;
            return model;
        }

        //public async Task SavePostsInCache(PostViewModel postViewModel)
        //{
        //    var model = new PostDetailsViewModel();

        //}

        public async Task SaveSchedulePostInfo(PostViewModel postViewModel, string userId)
        {
            string reelId = "";
            var post = _postRepository.GetById(postViewModel.Id);
            post.IsPostSchedule = false;
            _postRepository.Update(post);
            _postRepository.Save();

            if (postViewModel.PostType == (int)PostTypeEnum.Reel)
            {
                var postAttachment = await _postAttachmentRepository.GetAll().Where(x => x.PostId == post.Id).FirstOrDefaultAsync();
                if (postAttachment != null)
                {
                    reelId = postAttachment.Id.ToString();
                }

            }

            //this._notificationService.initializeNotificationViewModel(this.loginUserId, NotificationType.PostUploaded, notificationContent, this.loginUserId, response.id, response.postType, null, null).subscribe((response) => {
            //});

            var from = postViewModel.PostAuthorType == 1 ? "school" : postViewModel.PostAuthorType == 2 ? "class" : postViewModel.PostAuthorType == 4 ? "user" : null;
            var chatType = from == "user" ? 1 : from == "school" ? 3 : from == "class" ? 4 : 0;

            // here we aill add signal to send notification
            var user = _userRepository.GetById(userId);
            var model = new NotificationViewModel();
            {
                model.Id = Guid.NewGuid();
                model.UserId = userId;
                model.ActionDoneBy = userId;
                model.Avatar = user.Avatar;
                model.IsRead = false;
                model.NotificationContent = user.FirstName + " " + user.LastName + " " + Constants.PostReadyToViewMessage;
                model.NotificationType = NotificationTypeEnum.PostUploaded;
                model.PostId = postViewModel.Id;
                model.PostType = postViewModel.PostType == (int)PostTypeEnum.Post ? (int)PostTypeEnum.Post : (int)PostTypeEnum.Reel;
                model.ChatType = (Data.Entity.Chat.ChatType)chatType;
                model.ReelId = reelId == "" ? null : reelId;
            }

            var responseNotification = await _notificationService.AddNotification(model);
            var userConnectionId = _chatHubs.GetUserConnectionId(userId);
            await _hubContext.Clients.Client(userConnectionId).SendAsync("ReceiveNotification", responseNotification);
        }

        public async Task ScheduleLiveStream(PostViewModel postViewModel, string userId, DateTimeOffset scheduledTime)
        {
            postViewModel.DateTime = null;
            var scheduleJobId = BackgroundJob.Schedule(() => SaveLiveStreamInfo(postViewModel.Id, userId, postViewModel.PostAuthorType), scheduledTime);
        }

        public async Task SaveLiveStreamInfo(Guid id, string userId, int postAuthorType)
        {
            var post = _postRepository.GetById(id);
            //var postAttachment = await _postAttachmentRepository.GetAll().Where(x => x.PostId == postViewModel.Id).FirstAsync();
            //if (postViewModel.UploadVideos != null)
            //{
            //post.StreamUrl = postAttachment.FileUrl;
            post.IsLive = false;
            post.IsLiveStreamEnded = false;
            post.IsPostSchedule = false;
            _postRepository.Update(post);
            _postRepository.Save();

            var from = postAuthorType == 1 ? "school" : postAuthorType == 2 ? "class" : postAuthorType == 4 ? "user" : null;
            var chatType = from == "user" ? 1 : from == "school" ? 3 : from == "class" ? 4 : 0;

            // here we aill add signal to send notification
            var user = _userRepository.GetById(userId);
            var model = new NotificationViewModel();
            {
                model.Id = Guid.NewGuid();
                model.UserId = userId;
                model.ActionDoneBy = userId;
                model.Avatar = user.Avatar;
                model.IsRead = false;
                model.NotificationContent = user.FirstName + " " + user.LastName + " " + Constants.VideoReadyToStream;
                model.NotificationType = NotificationTypeEnum.PostUploaded;
                model.PostId = id;
                model.PostType = (int)PostTypeEnum.Stream;
                model.ChatType = (Data.Entity.Chat.ChatType)chatType;
            }

            var responseNotification = await _notificationService.AddNotification(model);
            var userConnectionId = _chatHubs.GetUserConnectionId(userId);
            await _hubContext.Clients.Client(userConnectionId).SendAsync("ReceiveNotification", responseNotification);
            //await _chatHubs.SendNotification(model);
            //}
            //else
            //{
            //    var user = _userRepository.GetById(userId);
            //    var model = new NewMeetingViewModel();
            //    model.meetingName = postViewModel.Title;
            //    model.IsMicrophoneOpen = (bool)postViewModel.IsMicroPhoneOpen;
            //    model.ModeratorName = user.FirstName;
            //    model.PostId = post.Id;
            //    var url = await _bigBlueButtonService.Create(model);
            //    postViewModel.StreamUrl = url;
            //    post.StreamUrl = url;
            //    post.IsLive = true;
            //    _postRepository.Update(post);
            //    _postRepository.Save();
            //}
        }


        async Task SendNotifications(string postTitle, Guid postId, string actionDoneBy)
        {
            var notificationViewModel = new NotificationViewModel();
            var user = _userRepository.GetById(actionDoneBy);
            notificationViewModel.ActionDoneBy = actionDoneBy;
            notificationViewModel.PostId = postId;
            notificationViewModel.Avatar = user.Avatar;
            notificationViewModel.NotificationType = NotificationTypeEnum.LectureStart;
            notificationViewModel.MeetingId = postTitle + "meetings";
            notificationViewModel.NotificationContent = $"{user.FirstName + ' '} {user.LastName} start a live {postTitle}";
            await _notificationService.AddNotification(notificationViewModel);
        }

        async Task SaveUploadImages(IEnumerable<IFormFile> uploadImages, Guid postId, string createdById)
        {
            string containerName = "posts";
            foreach (var image in uploadImages)
            {
                var postAttachment = new PostAttachmentViewModel();
                postAttachment.FileUrl = await _blobService.UploadFileAsync(image, containerName, false);

                var postAttach = new PostAttachment
                {
                    PostId = postId,
                    FileName = image.FileName,
                    FileUrl = postAttachment.FileUrl,
                    FileType = (int)FileTypeEnum.Image,
                    CreatedById = createdById,
                    CreatedOn = DateTime.UtcNow
                };

                _postAttachmentRepository.Insert(postAttach);
                _postAttachmentRepository.Save();
            }
        }

        public async Task<List<VideoUploadResponseViewModel>> SaveUploadVideos(IEnumerable<IFormFile> uploadVideos, IEnumerable<IFormFile> uploadVideosThumbnail, Guid postId, string createdById, bool isJobRunning)
        {
            var response = new List<VideoUploadResponseViewModel>();
            var uploadBlobUrls = new List<string>();
            string containerName = "posts";
            string videoThumbnailName = "";
            foreach (var video in uploadVideos)
            {
                int lastDotIndex = video.FileName.LastIndexOf(".");
                if (lastDotIndex != -1)
                {
                    videoThumbnailName = video.FileName.Substring(0, lastDotIndex + 1);
                }

                var isVideoThumbnailExist = uploadVideosThumbnail.Where(x => x.FileName.Substring(0, x.FileName.LastIndexOf(".") + 1) == videoThumbnailName).FirstOrDefault();

                var postAttachment = new PostAttachmentViewModel();
                if (isVideoThumbnailExist != null)
                {
                    postAttachment.FileThumbnail = await _blobService.UploadFileAsync(isVideoThumbnailExist, containerName, false);
                    response.Add(new VideoUploadResponseViewModel
                    {
                        FileName = isVideoThumbnailExist.FileName,
                        FileType = isVideoThumbnailExist.ContentType,
                        ThumbnailUrl = postAttachment.FileThumbnail,
                        IsVideo = false
                    });

                }
                if (!isJobRunning)
                {
                    postAttachment.FileUrl = await _blobService.UploadFileAsync(video, containerName, true);

                    var postAttach = new PostAttachment
                    {
                        PostId = postId,
                        FileName = video.FileName,
                        FileUrl = postAttachment.FileUrl,
                        FileThumbnail = postAttachment.FileThumbnail,
                        FileType = (int)FileTypeEnum.Video,
                        CreatedById = createdById,
                        CreatedOn = DateTime.UtcNow,
                        IsCompressed = false
                    };

                    _postAttachmentRepository.Insert(postAttach);
                    _postAttachmentRepository.Save();
                    //uploadBlobUrls.Add(postAttach.FileUrl);
                    response.Add(new VideoUploadResponseViewModel
                    {
                        FileName = video.FileName,
                        FileType = video.ContentType,
                        VideoUrl = postAttach.FileUrl,
                        IsVideo = true
                    });

                }

                else
                {
                    var fileUrl = await _blobService.CompressAndUploadFileAsync(video, containerName, true);
                    var attachments = await _postAttachmentRepository.GetAll().Where(x => x.PostId == postId).ToListAsync();

                    _postAttachmentRepository.DeleteAll(attachments);
                    _postAttachmentRepository.Save();

                    var postAttach = new PostAttachment
                    {
                        PostId = postId,
                        FileName = video.FileName,
                        FileUrl = fileUrl,
                        FileThumbnail = postAttachment.FileThumbnail,
                        FileType = (int)FileTypeEnum.Video,
                        CreatedById = createdById,
                        CreatedOn = DateTime.UtcNow,
                        IsCompressed = true
                    };

                    _postAttachmentRepository.Insert(postAttach);
                    _postAttachmentRepository.Save();

                }
            }

            return response;

        }

        async Task SaveUploadAttachments(IEnumerable<IFormFile> uploadAttachments, Guid postId, string createdById)
        {
            string containerName = "posts";
            foreach (var attachment in uploadAttachments)
            {
                var postAttachment = new PostAttachmentViewModel();
                postAttachment.FileUrl = await _blobService.UploadFileAsync(attachment, containerName, false);

                var postAttach = new PostAttachment
                {
                    PostId = postId,
                    FileName = attachment.FileName,
                    FileUrl = postAttachment.FileUrl,
                    FileType = (int)FileTypeEnum.Attachment,
                    CreatedById = createdById,
                    CreatedOn = DateTime.UtcNow
                };

                _postAttachmentRepository.Insert(postAttach);
                _postAttachmentRepository.Save();
            }
        }

        async Task SaveFileStorageAttachments(IEnumerable<FileAttachmentViewModel> attachments, Guid postId, string createdById)
        {
            foreach (var attachment in attachments)
            {
                var postAttach = new PostAttachment
                {
                    PostId = postId,
                    FileName = attachment.FileName,
                    FileUrl = attachment.FileUrl,
                    FileType = (int)FileTypeEnum.Attachment,
                    CreatedById = createdById,
                    CreatedOn = DateTime.UtcNow,
                };

                _postAttachmentRepository.Insert(postAttach);
                _postAttachmentRepository.Save();
            }
        }


        async Task SavePostTags(IEnumerable<string> postTags, Guid postId)
        {
            var postTagList = await _postTagRepository.GetAll().Where(x => x.PostId == postId).ToListAsync();
            if (postTagList.Count() != 0)
            {
                _postTagRepository.DeleteAll(postTagList);
                _postTagRepository.Save();
            }
            foreach (var tagValue in postTags)
            {
                var postTag = new PostTag
                {
                    PostId = postId,
                    PostTagValue = tagValue
                };

                _postTagRepository.Insert(postTag);
                _postTagRepository.Save();

            }
        }

        public async Task<PostAttachmentViewModel> GetReelById(Guid id, string userId)
        {
            var postAttachment = await _postAttachmentRepository.GetAll()
                .Include(x => x.Post)
                .ThenInclude(x => x.CreatedBy)
                .Include(x => x.CreatedBy)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            var result = _mapper.Map<PostAttachmentViewModel>(postAttachment);

            int lastSlashIndex = result.FileUrl.LastIndexOf('/');
            var fileName = result.FileUrl.Substring(lastSlashIndex + 1);

            result.ByteArray = await _blobService.GetFileContentAsync(this._config.GetValue<string>("Container:PostContainer"), fileName);

            result.Post.Likes = await _userService.GetLikesOnPost(postAttachment.Post.Id);
            result.Post.Views = await _userService.GetViewsOnPost(postAttachment.Post.Id);
            result.Post.Comments = await _chatService.GetComments(id, userId, 1);
            result.Post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == postAttachment.Post.Id).CountAsync();
            result.Post.SavedPostsCount = await _savedPostRepository.GetAll().Where(x => x.PostId == postAttachment.Post.Id).CountAsync();
            result.Post.IsPostSavedByCurrentUser = await _savedPostRepository.GetAll().AnyAsync(x => x.PostId == postAttachment.Post.Id && x.UserId == userId);

            if (result.Post.Likes.Any(x => x.UserId == userId && x.PostId == postAttachment.Post.Id))
            {
                result.Post.IsPostLikedByCurrentUser = true;
            }
            else
            {
                result.Post.IsPostLikedByCurrentUser = false;
            }
            var post = await GetPostById(result.Post.Id, userId);
            result.Post.ParentName = post.ParentName;
            result.Post.ParentImageUrl = post.ParentImageUrl;
            result.Post.IsParentVerified = post.IsParentVerified;

            //result.Post.ParentImageUrl = 
            //if (result.Post.PostAuthorType == (int)PostAuthorTypeEnum.User)
            //{
            //    var user = _userRepository.GetById(result.Post.ParentId);
            //    result.Post = _mapper.Map<PostDetailsViewModel>(user);
            //}

            if (result.Post.PostAuthorType == (int)PostAuthorTypeEnum.School)
            {
                var school = _schoolRepository.GetById(result.Post.ParentId);
                result.School = _mapper.Map<SchoolViewModel>(school);
            }

            if (result.Post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
            {
                var classes = _classRepository.GetById(result.Post.ParentId);
                result.Class = _mapper.Map<ClassViewModel>(classes);
            }
            //if (result.Post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
            //{
            //    var course = _courseRepository.GetById(result.Post.ParentId);
            //    result.Course = _mapper.Map<CourseViewModel>(course);
            //}

            //if (result.Post.PostAuthorType == (int)PostAuthorTypeEnum.User)
            //{
            //    var user = _userRepository.GetById(result.Post.ParentId);
            //    result.User = _mapper.Map<UserDetailsViewModel>(user);
            //}

            return result;
        }

        public async Task<PostDetailsViewModel> GetPostById(Guid id, string userId)
        {
            var postResult = await _postRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.Id == id).FirstOrDefaultAsync();

            postResult.CreatedOn = postResult.CreatedOn.AddHours(5);
            postResult.CreatedOn = postResult.CreatedOn.AddMinutes(30);
            //postResult.CreatedOn = postResult.CreatedOn.ToLocalTime();

            var post = _mapper.Map<PostDetailsViewModel>(postResult);
            if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
            {
                var classes = await _classRepository.GetAll().Include(x => x.ServiceType).Include(x => x.Accessibility).Where(x => x.ClassId == post.ParentId).FirstOrDefaultAsync();
                if (classes.ServiceType.Type == "Paid" || classes.Accessibility.Name == "Private")
                {
                    post.IsClassPrivateOrPaid = true;
                }

                post.ParentName = classes.ClassName;
                post.ParentImageUrl = classes.Avatar;
            }

            if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
            {
                var course = await _courseRepository.GetAll().Include(x => x.ServiceType).Include(x => x.Accessibility).Where(x => x.CourseId == post.ParentId).FirstOrDefaultAsync();
                post.ParentName = course.CourseName;
                post.ParentImageUrl = course.Avatar;
            }

            if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
            {
                var user = _userRepository.GetById(post.ParentId.ToString());
                post.ParentName = user.FirstName + " " + user.LastName;
                post.ParentImageUrl = user.Avatar;
                post.IsParentVerified = user.IsVarified;
            }

            if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
            {
                var school = _schoolRepository.GetById(post.ParentId);
                post.ParentName = school.SchoolName;
                post.ParentImageUrl = school.Avatar;
                post.IsParentVerified = school.IsVarified;
            }

            post.PostAttachments = await GetAttachmentsByPostId(post.Id);
            foreach (var item in post.PostAttachments)
            {
                int lastSlashIndex = item.FileUrl.LastIndexOf('/');
                var fileName = item.FileUrl.Substring(lastSlashIndex + 1);

                //item.ByteArray = await _blobService.GetFileContentAsync(this._config.GetValue<string>("Container:PostContainer"), fileName);

            }
            post.Likes = await _userService.GetLikesOnPost(post.Id);
            post.Views = await _userService.GetViewsOnPost(post.Id);
            post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
            post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
            post.SavedPostsCount = await _savedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
            post.IsPostSavedByCurrentUser = await _savedPostRepository.GetAll().AnyAsync(x => x.PostId == post.Id && x.UserId == userId);
            if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
            {
                post.IsPostLikedByCurrentUser = true;
            }
            else
            {
                post.IsPostLikedByCurrentUser = false;
            }

            var tags = await GetTagsByPostId(post.Id);
            post.PostTags = tags;

            post.IsLiveStreamEnded = postResult.IsLiveStreamEnded;

            return post;
        }

        public async Task<IEnumerable<PostAttachmentViewModel>> GetAttachmentsByPostId(Guid postId)
        {
            var attachmentList = await _postAttachmentRepository.GetAll().Include(x => x.CreatedBy).Where(x => x.PostId == postId).OrderByDescending(x => x.IsPinned).ToListAsync();
            var sortedAttachmentList = attachmentList
    .OrderBy(x =>
    {
        var match = Regex.Match(x.FileName, @"_index(\d+)$");
        return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
    }).ToList();
            foreach (var isCompressed in sortedAttachmentList)
            {
                if (!string.IsNullOrEmpty(isCompressed.CompressedFileUrl))
                {
                    isCompressed.FileUrl = isCompressed.CompressedFileUrl;
                }
                //isCompressed.FileThumbnail = $"https://byokulstorage.blob.core.windows.net/userpostscompressed/thumbnails/{isCompressed.Id}.png";
            }

            var result = _mapper.Map<List<PostAttachmentViewModel>>(sortedAttachmentList);
            return result;
        }

        public async Task<IEnumerable<PostTagViewModel>> GetTagsByPostId(Guid postId)
        {
            var tagList = await _postTagRepository.GetAll().Where(x => x.PostId == postId).ToListAsync();

            var result = _mapper.Map<List<PostTagViewModel>>(tagList);
            return result;
        }

        public async Task<string> PinUnpinPost(Guid postId, bool isPinned)
        {
            var post = await _postRepository.GetAll().Where(x => x.Id == postId).FirstOrDefaultAsync();

            if (post != null)
            {
                post.IsPinned = isPinned;
                _postRepository.Update(post);
                _postRepository.Save();
                if (isPinned)
                {
                    return Constants.PostPinnedSuccessfully;
                }
                return Constants.PostUnPinnedSuccessfully;
            }

            return Constants.PostIdInvalid;


        }

        public async Task<List<LikeViewModel>> LikeUnlikePost(LikeUnlikeViewModel model)
        {
            var isPostExist = _postRepository.GetById(model.PostId);
            if (isPostExist == null)
            {
                var isPostAttachmentExist = _postAttachmentRepository.GetById(model.PostId);
                if (isPostAttachmentExist == null)
                    return null;
            }

            var userLike = await _likeRepository.GetAll().Where(x => x.UserId == model.UserId && x.PostId == model.PostId).FirstOrDefaultAsync();

            if (userLike != null)
            {
                _likeRepository.Delete(userLike.Id);
                _likeRepository.Save();
                var totalLikes = await _likeRepository.GetAll().Where(x => x.PostId == model.PostId).ToListAsync();
                return _mapper.Map<List<LikeViewModel>>(totalLikes);
            }

            else
            {
                var like = new Like
                {
                    UserId = model.UserId,
                    PostId = model.PostId,
                    DateTime = DateTime.UtcNow,
                    //CommentId = model.CommentId == Guid.Empty ? null : model.CommentId,

                };

                _likeRepository.Insert(like);
                _likeRepository.Save();
                var totalLikes = await _likeRepository.GetAll().Where(x => x.PostId == model.PostId).ToListAsync();
                return _mapper.Map<List<LikeViewModel>>(totalLikes);
            }
            return null;
        }

        public async Task<int> PostView(PostViewsViewModel model)
        {
            var isUserViewExist = await _viewRepository.GetAll().Where(x => x.UserId == model.UserId && x.PostId == model.PostId).FirstOrDefaultAsync();
            if (isUserViewExist == null)
            {
                var view = new View
                {
                    UserId = model.UserId,
                    PostId = model.PostId,
                };

                _viewRepository.Insert(view);
                _viewRepository.Save();
                return _viewRepository.GetAll().Where(x => x.PostId == model.PostId).Count();
            }
            return _viewRepository.GetAll().Where(x => x.PostId == model.PostId).Count();
        }

        public async Task<bool> LikeUnlikeComment(Guid commentId, bool isLike, string userId)
        {

            var commentLike = await _commentLikeRepository.GetAll().Where(x => x.CommentId == commentId && x.UserId == userId).FirstOrDefaultAsync();

            if (commentLike != null)
            {
                _commentLikeRepository.Delete(commentLike.Id);
                _commentLikeRepository.Save();

                Comment comment = _commentRepository.GetById(commentId);
                comment.LikeCount = comment.LikeCount - 1;
                _commentRepository.Update(comment);
                _commentRepository.Save();
                return true;

                //var totalLikes = await _likeRepository.GetAll().Where(x => x.PostId == model.PostId).ToListAsync();
                //return _mapper.Map<List<LikeViewModel>>(totalLikes);
            }

            else
            {
                var commentLikes = new CommentLike
                {
                    UserId = userId,
                    CommentId = commentId
                };

                _commentLikeRepository.Insert(commentLikes);
                _commentLikeRepository.Save();

                Comment comment = _commentRepository.GetById(commentId);
                comment.LikeCount = comment.LikeCount + 1;
                _commentRepository.Update(comment);
                _commentRepository.Save();
                return true;
                //var totalLikes = await _likeRepository.GetAll().Where(x => x.PostId == model.PostId).ToListAsync();
                //return _mapper.Map<List<LikeViewModel>>(totalLikes);
            }
            return false;
        }


        public async Task EnableDisableComments(Guid postId, bool isHideComments)
        {
            var post = _postRepository.GetById(postId);

            post.IsCommentsDisabled = isHideComments;
            _postRepository.Update(post);
            _postRepository.Save();

        }

        public async Task<bool> SaveUserSharedPost(string userId, Guid postId)
        {
            try
            {
                var userSharedPost = new UserSharedPost
                {
                    UserId = userId,
                    PostId = postId,
                    CreatedOn = DateTime.UtcNow
                };

                _userSharedPostRepository.Insert(userSharedPost);
                _userSharedPostRepository.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool?> SavePostByUser(string userId, Guid postId)
        {

            var isSavedPostExist = await _savedPostRepository.GetAll().Where(x => x.UserId == userId && x.PostId == postId).FirstOrDefaultAsync();

            if (isSavedPostExist != null)
            {
                _savedPostRepository.Delete(isSavedPostExist.Id);
                _savedPostRepository.Save();
                return false;
            }

            else
            {
                var savedPost = new SavedPost
                {
                    UserId = userId,
                    PostId = postId,
                    CreatedOn = DateTime.UtcNow,
                    IsPinned = false
                };

                _savedPostRepository.Insert(savedPost);
                _savedPostRepository.Save();
                return true;
            }
        }

        public async Task<List<PostDetailsViewModel>> GetSavedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            int pageSize = 0;
            if (type == PostTypeEnum.Post)
            {
                pageSize = 9;

            }
            else
            {
                pageSize = 8;
            }
            var savedPostList = await _savedPostRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)type).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).Select(x => x.Post).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = _mapper.Map<List<PostDetailsViewModel>>(savedPostList).ToList();
            var savedPosts = await _savedPostRepository.GetAll().ToListAsync();
            var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();

            foreach (var post in result)
            {
                post.IsSavedPostPinned = savedPosts.Any(x => x.PostId == post.Id && x.IsPinned);
                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.IsPostSavedByCurrentUser = savedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.SavedPostsCount = savedPosts.Where(x => x.PostId == post.Id).Count();
                post.IsPostSharedByCurrentUser = sharedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.PostSharedCount = sharedPosts.Where(x => x.PostId == post.Id).Count();

                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
                {
                    post.IsPostLikedByCurrentUser = true;
                }
                else
                {
                    post.IsPostLikedByCurrentUser = false;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.School;
                    post.SchoolName = "";
                    post.ParentId = school.SchoolId;
                    post.IsParentVerified = school.IsVarified;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = await _classRepository.GetAll().Where(x => x.ClassId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Class;
                    post.SchoolName = classes.School.SchoolName;
                    post.ParentId = classes.ClassId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = await _courseRepository.GetAll().Where(x => x.CourseId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Course;
                    post.SchoolName = course.School.SchoolName;
                    post.ParentId = course.CourseId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.User;
                    post.SchoolName = "";
                    post.ParentId = new Guid(user.Id);
                    post.IsParentVerified = user.IsVarified;
                }

            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }

            return result;
        }


        public async Task<List<PostDetailsViewModel>> GetSavedSliderReels(string userId, Guid lastPostId, ScrollTypesEnum scrollType)
        {
            var requiredResults = new List<Post>();
            var savedReelList = await _savedPostRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)PostTypeEnum.Reel).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).Select(x => x.Post).ToListAsync();

            //var savedPosts = await _savedPostRepository.GetAll().ToListAsync();
            //var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();







            if (scrollType == ScrollTypesEnum.None)
            {

                var attachment = _postAttachmentRepository.GetById(lastPostId);
                int index = savedReelList.FindIndex(x => x.Id == attachment.PostId);
                int startIndex = Math.Max(0, index - 3);
                int totalItems = 7;
                requiredResults = savedReelList.GetRange(startIndex, Math.Min(totalItems, savedReelList.Count - startIndex));


            }
            if (scrollType == ScrollTypesEnum.Down)
            {
                requiredResults = savedReelList.SkipWhile(x => x.Id != lastPostId).Skip(1).Take(3).ToList();

            }
            if (scrollType == ScrollTypesEnum.Up)
            {
                requiredResults = savedReelList.TakeWhile(x => x.Id != lastPostId).Reverse().Take(3).Reverse().ToList();

            }
            var result = _mapper.Map<List<PostDetailsViewModel>>(requiredResults);

            foreach (var post in result)
            {
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.IsParentVerified = school.IsVarified;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = _classRepository.GetById(post.ParentId);
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = _courseRepository.GetById(post.ParentId);
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.IsParentVerified = user.IsVarified;
                }

                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                post.SavedPostsCount = await _savedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
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


        public async Task<List<PostDetailsViewModel>> GetSharedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            int pageSize = 0;
            if (type == PostTypeEnum.Post)
            {
                pageSize = 9;

            }
            else
            {
                pageSize = 8;
            }
            var sharedPostList = await _userSharedPostRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)type).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();


            //var result = _mapper.Map<List<PostDetailsViewModel>>(sharedPostList.Select(x => x.Post)).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).ToList();
            var result = _mapper.Map<List<PostDetailsViewModel>>(sharedPostList.Select(x => x.Post)).ToList();
            var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();
            var savedPosts = await _savedPostRepository.GetAll().ToListAsync();





            foreach (var post in result)
            {

                //post.IsSharedPostPinned = sharedPostList.Any()
                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                post.IsPostSharedByCurrentUser = sharedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.PostSharedCount = sharedPosts.Where(x => x.PostId == post.Id).Count();
                post.IsPostSavedByCurrentUser = savedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.SavedPostsCount = savedPosts.Where(x => x.PostId == post.Id).Count();
                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
                {
                    post.IsPostLikedByCurrentUser = true;
                }
                else
                {
                    post.IsPostLikedByCurrentUser = false;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.School;
                    post.SchoolName = "";
                    post.ParentId = school.SchoolId;
                    post.IsParentVerified = school.IsVarified;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = await _classRepository.GetAll().Where(x => x.ClassId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Class;
                    post.SchoolName = classes.School.SchoolName;
                    post.ParentId = classes.ClassId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = await _courseRepository.GetAll().Where(x => x.CourseId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Course;
                    post.SchoolName = course.School.SchoolName;
                    post.ParentId = course.CourseId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.User;
                    post.SchoolName = "";
                    post.ParentId = new Guid(user.Id);
                    post.IsParentVerified = user.IsVarified;
                }

            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }

            return result;
        }


        public async Task<List<PostDetailsViewModel>> GetSharedSliderReels(string userId, Guid lastPostId, ScrollTypesEnum scrollType)
        {
            var requiredResults = new List<Post>();
            var sharedReelList = await _userSharedPostRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)PostTypeEnum.Reel).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).Select(x => x.Post).ToListAsync();

            //var savedPosts = await _savedPostRepository.GetAll().ToListAsync();
            //var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();







            if (scrollType == ScrollTypesEnum.None)
            {

                var attachment = _postAttachmentRepository.GetById(lastPostId);
                int index = sharedReelList.FindIndex(x => x.Id == attachment.PostId);
                int startIndex = Math.Max(0, index - 3);
                int totalItems = 7;
                requiredResults = sharedReelList.GetRange(startIndex, Math.Min(totalItems, sharedReelList.Count - startIndex));


            }
            if (scrollType == ScrollTypesEnum.Down)
            {
                requiredResults = sharedReelList.SkipWhile(x => x.Id != lastPostId).Skip(1).Take(3).ToList();

            }
            if (scrollType == ScrollTypesEnum.Up)
            {
                requiredResults = sharedReelList.TakeWhile(x => x.Id != lastPostId).Reverse().Take(3).Reverse().ToList();

            }
            var result = _mapper.Map<List<PostDetailsViewModel>>(requiredResults);

            foreach (var post in result)
            {
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.IsParentVerified = school.IsVarified;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = _classRepository.GetById(post.ParentId);
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = _courseRepository.GetById(post.ParentId);
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.IsParentVerified = user.IsVarified;
                }

                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                post.SavedPostsCount = await _savedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
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

        public async Task<List<PostDetailsViewModel>> GetLikedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            int pageSize = 0;
            if (type == PostTypeEnum.Post)
            {
                pageSize = 9;

            }
            else
            {
                pageSize = 8;
            }
            var likedPostList = await _likeRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)type).OrderByDescending(x => x.DateTime).Select(x => x.Post).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = _mapper.Map<List<PostDetailsViewModel>>(likedPostList).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOn).ToList();
            var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();
            var savedPosts = await _savedPostRepository.GetAll().ToListAsync();
            var likedPosts = await _likeRepository.GetAll().ToListAsync();

            foreach (var post in result)
            {
                post.IsLikedPostPinned = likedPosts.Any(x => x.PostId == post.Id && x.IsPinned);
                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                post.IsPostSharedByCurrentUser = sharedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.IsPostSavedByCurrentUser = savedPosts.Any(x => x.PostId == post.Id && x.UserId == userId);
                post.SavedPostsCount = savedPosts.Where(x => x.PostId == post.Id).Count();
                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
                {
                    post.IsPostLikedByCurrentUser = true;
                }
                else
                {
                    post.IsPostLikedByCurrentUser = false;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.School;
                    post.SchoolName = "";
                    post.ParentId = school.SchoolId;
                    post.IsParentVerified = school.IsVarified;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = await _classRepository.GetAll().Where(x => x.ClassId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Class;
                    post.SchoolName = classes.School.SchoolName;
                    post.ParentId = classes.ClassId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = await _courseRepository.GetAll().Where(x => x.CourseId == post.ParentId).Include(x => x.School).FirstOrDefaultAsync();
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.Course;
                    post.SchoolName = course.School.SchoolName;
                    post.ParentId = course.CourseId;
                }
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.PostAuthorType = (int)PostAuthorTypeEnum.User;
                    post.SchoolName = "";
                    post.ParentId = new Guid(user.Id);
                    post.IsParentVerified = user.IsVarified;
                }

            }

            foreach (var post in result)
            {
                var tags = await GetTagsByPostId(post.Id);
                post.PostTags = tags;
            }

            return result;
        }

        public async Task<List<PostDetailsViewModel>> GetLikedSliderReels(string userId, Guid lastPostId, ScrollTypesEnum scrollType)
        {
            var requiredResults = new List<Post>();
            var likedReelList = await _likeRepository.GetAll().Include(x => x.Post).Where(x => x.UserId == userId && x.Post.PostType == (int)PostTypeEnum.Reel).OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.DateTime).Select(x => x.Post).ToListAsync();

            //var savedPosts = await _savedPostRepository.GetAll().ToListAsync();
            //var sharedPosts = await _userSharedPostRepository.GetAll().ToListAsync();







            if (scrollType == ScrollTypesEnum.None)
            {

                var attachment = _postAttachmentRepository.GetById(lastPostId);
                int index = likedReelList.FindIndex(x => x.Id == attachment.PostId);
                int startIndex = Math.Max(0, index - 3);
                int totalItems = 7;
                requiredResults = likedReelList.GetRange(startIndex, Math.Min(totalItems, likedReelList.Count - startIndex));


            }
            if (scrollType == ScrollTypesEnum.Down)
            {
                requiredResults = likedReelList.SkipWhile(x => x.Id != lastPostId).Skip(1).Take(3).ToList();

            }
            if (scrollType == ScrollTypesEnum.Up)
            {
                requiredResults = likedReelList.TakeWhile(x => x.Id != lastPostId).Reverse().Take(3).Reverse().ToList();

            }
            var result = _mapper.Map<List<PostDetailsViewModel>>(requiredResults);

            foreach (var post in result)
            {
                if (post.PostAuthorType == (int)PostAuthorTypeEnum.School)
                {
                    var school = _schoolRepository.GetById(post.ParentId);
                    post.ParentName = school.SchoolName;
                    post.ParentImageUrl = school.Avatar;
                    post.IsParentVerified = school.IsVarified;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Class)
                {
                    var classes = _classRepository.GetById(post.ParentId);
                    post.ParentName = classes.ClassName;
                    post.ParentImageUrl = classes.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.Course)
                {
                    var course = _courseRepository.GetById(post.ParentId);
                    post.ParentName = course.CourseName;
                    post.ParentImageUrl = course.Avatar;
                }

                if (post.PostAuthorType == (int)PostAuthorTypeEnum.User)
                {
                    var user = _userRepository.GetById(post.ParentId.ToString());
                    post.ParentName = user.FirstName + " " + user.LastName;
                    post.ParentImageUrl = user.Avatar;
                    post.IsParentVerified = user.IsVarified;
                }

                post.PostAttachments = await GetAttachmentsByPostId(post.Id);
                post.Likes = await _userService.GetLikesOnPost(post.Id);
                post.Views = await _userService.GetViewsOnPost(post.Id);
                post.CommentsCount = await _userService.GetCommentsCountOnPost(post.Id);
                post.PostSharedCount = await _userSharedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                post.SavedPostsCount = await _savedPostRepository.GetAll().Where(x => x.PostId == post.Id).CountAsync();
                if (post.Likes.Any(x => x.UserId == userId && x.PostId == post.Id))
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

        public async Task<bool> PinUnpinSavedPost(Guid postId, bool isPinned, string userId)
        {
            var savedPost = await _savedPostRepository.GetAll().Where(x => x.PostId == postId && x.UserId == userId).FirstOrDefaultAsync();

            if (savedPost != null)
            {
                savedPost.IsPinned = isPinned;
                _savedPostRepository.Update(savedPost);
                _savedPostRepository.Save();
                return true;
            }

            return false;


        }

        public async Task<bool> PinUnpinLikedPost(Guid postId, bool isPinned, string userId)
        {
            var likedPost = await _likeRepository.GetAll().Where(x => x.PostId == postId && x.UserId == userId).FirstOrDefaultAsync();

            if (likedPost != null)
            {
                likedPost.IsPinned = isPinned;
                _likeRepository.Update(likedPost);
                _likeRepository.Save();
                return true;
            }

            return false;

        }

        public async Task<bool> DeletePost(Guid id)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                return false;
            }
            var postAttachments = await _postAttachmentRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (postAttachments.Count() != 0)
            {
                _postAttachmentRepository.DeleteAll(postAttachments);
                //_postAttachmentRepository.Save();
            }

            var postTags = await _postTagRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (postTags.Count() != 0)
            {
                _postTagRepository.DeleteAll(postTags);
                //_postTagRepository.Save();
            }

            var postLikes = await _likeRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (postLikes.Count() != 0)
            {
                _likeRepository.DeleteAll(postLikes);
                //_postTagRepository.Save();
            }

            var postSaved = await _savedPostRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (postSaved.Count() != 0)
            {
                _savedPostRepository.DeleteAll(postSaved);
                //_postTagRepository.Save();
            }

            var postShared = await _userSharedPostRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (postShared.Count() != 0)
            {
                _userSharedPostRepository.DeleteAll(postShared);
                //_postTagRepository.Save();
            }

            var notifications = await _notificationRepository.GetAll().Where(x => x.PostId == id).ToListAsync();
            if (notifications.Count() != 0)
            {
                _notificationRepository.DeleteAll(notifications);
                //_postTagRepository.Save();
            }

            _postRepository.Delete(post.Id);
            _postRepository.Save();
            return true;
        }

        public async Task UpdateCommentThrottling(Guid postId, int noOfComments)
        {
            var post = _postRepository.GetById(postId);

            if (post != null)
            {
                post.CommentsPerMinute = noOfComments;
                _postRepository.Update(post);
                _postRepository.Save();
            }
        }

        public async Task SaveStreamAsPost(Guid postId)
        {
            var post = _postRepository.GetById(postId);
            post.PostType = (int)PostTypeEnum.Post;
            post.IsLive = false;
            post.IsLiveStreamEnded = true;
            _postRepository.Update(post);
            _postRepository.Save();

            var postAttachment = await _postAttachmentRepository.GetAll().Where(x => x.PostId == post.Id).ToListAsync();

            var postAttachmentImage = postAttachment.Where(x => x.FileType == (int)FileTypeEnum.Image).First();
            var postAttachmentVideo = postAttachment.Where(x => x.FileType == (int)FileTypeEnum.Video).First();


            postAttachmentVideo.FileThumbnail = postAttachmentImage.FileUrl;
            _postAttachmentRepository.Update(postAttachmentVideo);
            _postAttachmentRepository.Save();

            _postAttachmentRepository.Delete(postAttachmentImage.Id);
            _postAttachmentRepository.Save();
        }

        public async Task SaveLiveVideoTime(Guid postId, float videoTotalTime, float videoLiveTime)
        {
            var postAttachment = _postAttachmentRepository.GetAll().Where(x => x.FileType == (int)FileTypeEnum.Video && x.PostId == postId).First();
            postAttachment.VideoLiveTime = videoLiveTime;
            if (postAttachment.VideoTotalTime == null)
            {
                postAttachment.VideoTotalTime = videoTotalTime;
            }

            _postAttachmentRepository.Update(postAttachment);
            _postAttachmentRepository.Save();

        }

        public async Task<bool> EnableLiveStream(Guid postId)
        {
            var post = await _postRepository.GetAll().Where(x => x.Id == postId && x.IsLiveStreamEnded == false).FirstOrDefaultAsync();
            if (post != null)
            {
                post.IsLive = true;
                _postRepository.Update(post);
                _postRepository.Save();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<GlobalSearchViewModel>> PostsGlobalSearch(string searchString, int pageNumber, int pageSize)
        {

            var posts = await _postRepository.GetAll().Where(x => x.Tags.Any(x => x.PostTagValue.Contains(searchString))).Select(x => new
            {
                x.Id,
                x.Title,
                x.PostType,
                x.ParentId,
                x.PostAuthorType
            }).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var postViewModel = new List<GlobalSearchViewModel>();
            var postAttachment = new PostAttachment();
            foreach (var post in posts)
            {
                string avatar = _userService.GetPostParentImage(post.ParentId, post.PostAuthorType);
                if (post.PostType == 3)
                {
                    postAttachment = await _postAttachmentRepository.GetAll().Where(x => x.PostId == post.Id).FirstAsync();
                }
                postViewModel.Add(new GlobalSearchViewModel
                {
                    Id = post.PostType == 1 ? post.Id : postAttachment.Id,
                    Name = post.Title,
                    SchoolName = null,
                    PostType = post.PostType,
                    Avatar = avatar,
                    IsPost = true
                });
            }

            return postViewModel;

        }



    }
}
