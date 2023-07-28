﻿using Hangfire;
using iText.IO.Util;
using LMS.Common.Enums;
using LMS.Common.ViewModels.Post;
using LMS.Data.Entity;
using LMS.Services;
using LMS.Services.Blob;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Collections;

namespace LMS.App.Controllers
{
    [Authorize]
    [Route("posts")]
    public class PostController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly IPostService _postService;
        private readonly IBlobService _blobService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(UserManager<User> userManager,
            IPostService postService, IBlobService blobService, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _postService = postService;
            _blobService = blobService;
            _webHostEnvironment = webHostEnvironment;
        }

        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        [Route("uploadOnBlob")]
        [HttpPost]
        public async Task<IActionResult> UploadOnBlob()
        {
            var formCollection = await Request.ReadFormAsync();
            var uploadVideo = formCollection.Files[0];
            string containerName = "posts";
            var reponse = await _blobService.UploadFileAsync(uploadVideo, containerName, true);
            return Ok("success");

        }

        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        [Route("uploadOnServer")]
        [HttpPost]
        public async Task<IActionResult> UploadOnServer()
        {
            var formCollection = await Request.ReadFormAsync();
            var uploadVideo = formCollection.Files[0];
            byte[] byteArray;
            using (var memoryStream = new MemoryStream())
            {
                await uploadVideo.CopyToAsync(memoryStream);
                byteArray = memoryStream.ToArray();
            }

            var path = _webHostEnvironment.ContentRootPath;
            var tempDirectoryPath = Path.Combine(path, "FfmpegVideos/");
            var fileName = Guid.NewGuid().ToString();
            System.IO.File.WriteAllBytes(tempDirectoryPath + fileName + "test.mp4", byteArray);
            return Ok("success");
        }

        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        [Route("uploadPost")]
        [HttpPost]
        public async Task<IActionResult> UploadPost()
        {
            var postViewModel = new PostViewModel();
            var userId = await GetUserIdAsync(this._userManager);
            var formCollection = await Request.ReadFormAsync();
            var images = (formCollection.Files.GetFiles("uploadImages")).ToList<IFormFile>();
            var videos = (formCollection.Files.GetFiles("uploadVideos")).ToList<IFormFile>();
            var videoThumbnail = (formCollection.Files.GetFiles("uploadVideosThumbnail")).ToList<IFormFile>();
            postViewModel.UploadVideos = videos;
            postViewModel.UploadImages = images;
            postViewModel.UploadVideosThumbnail = videoThumbnail;
            var authorId = formCollection["authorId"].ToString();
            postViewModel.AuthorId = Guid.Parse(authorId);
            var ownerId = formCollection["ownerId"].ToString();
            postViewModel.OwnerId = Guid.Parse(ownerId);
            var parentId = formCollection["parentId"].ToString();
            postViewModel.ParentId = Guid.Parse(parentId);
            postViewModel.Title = formCollection["title"].ToString();
            postViewModel.Description = formCollection["description"].ToString();

            var postType = int.Parse(formCollection["postType"].ToString());
            postViewModel.PostType = postType;

            var postAuthorType = int.Parse(formCollection["postAuthorType"].ToString());
            postViewModel.PostAuthorType = postAuthorType;

            var attachments = (formCollection.Files.GetFiles("uploadAttachments")).ToList<IFormFile>();
            postViewModel.UploadAttachments = attachments;

            var postTags = (formCollection["postTags"]).ToList<string>();
            postViewModel.PostTags = postTags;

            var uploadFromFileStorage = (formCollection["UploadFromFileStorage"]).ToList<string>();
            postViewModel.UploadFromFileStorage = uploadFromFileStorage;

            postViewModel.CreatedBy = userId;
            postViewModel.CreatedOn = DateTime.UtcNow;
            postViewModel.IsDeleted = false;
            postViewModel.IsPinned = false;
            postViewModel.IsCommentsDisabled = false;

            var streamUrl = formCollection["streamUrl"].ToString() ?? null;
            postViewModel.StreamUrl = streamUrl;

            var reelId = formCollection["reelId"].ToString();
            if (String.IsNullOrEmpty(reelId))
            {
                postViewModel.ReelId = null;
            }
            else
            {
                postViewModel.ReelId = Guid.Parse(reelId);
            }



            var commentsPerMinute = formCollection["commentsPerMinute"].ToString();


            if (!string.IsNullOrEmpty(commentsPerMinute) && int.Parse(commentsPerMinute) > 0)
            {
                postViewModel.CommentsPerMinute = int.Parse(commentsPerMinute);
            }
            else
            {
                postViewModel.CommentsPerMinute = 0;
            }

            var response = await _postService.SavePost(postViewModel, userId);
            //}
            return Ok(response);


        }

        //[HttpPost, DisableRequestSizeLimit]
        //[Route("uploadPost")]
        //public async Task<IActionResult> SavePost()
        //{
        //    var response = new PostViewModel();

        //    var formCollection = await Request.ReadFormAsync();
        //    var imagesFiles = formCollection.Files.GetFiles("uploadImages");
        //    var AuthorId = formCollection["AuthorId"];


        //    var userId = await GetUserIdAsync(this._userManager);
        //    if (postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.School)
        //    {
        //        postViewModel.OwnerId = new Guid(userId);
        //    }

        //    if (postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.Class || postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.Course)
        //    {
        //        postViewModel.AuthorId = new Guid(userId);
        //    }
        //    response = await _postService.SavePost(postViewModel, userId);
        //    //}
        //    return Ok(response);
        //}

        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        [Route("savePost")]
        [HttpPost]
        public async Task<IActionResult> SavePost(PostViewModel postViewModel)
        {
            var response = new PostViewModel();
            var userId = await GetUserIdAsync(this._userManager);
            if (postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.School)
            {
                postViewModel.OwnerId = new Guid(userId);
            }

            if (postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.Class || postViewModel.PostAuthorType == (int)PostAuthorTypeEnum.Course)
            {
                postViewModel.AuthorId = new Guid(userId);
            }
            response = await _postService.SavePost(postViewModel, userId);
            //}
            return Ok(response);
        }

        [Route("getReelById")]
        [HttpGet]
        public async Task<IActionResult> GetReelById(Guid id)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _postService.GetReelById(id, userId);
            return Ok(response);
        }

        [AllowAnonymous]
        [Route("getPostById")]
        [HttpGet]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _postService.GetPostById(id, userId);
            return Ok(response);
        }

        [Route("pinUnpinPost")]
        [HttpPost]
        public async Task<IActionResult> PinUnpinPost(Guid attachmentId, bool isPinned)
        {
            var response = await _postService.PinUnpinPost(attachmentId, isPinned);
            return Ok(response);
        }

        [Route("likeUnlikePost")]
        [HttpPost]
        public async Task<IActionResult> LikeUnlikePost([FromBody] LikeUnlikeViewModel model)
        {
            var userId = await GetUserIdAsync(this._userManager);
            model.UserId = userId;
            var response = await _postService.LikeUnlikePost(model);
            return Ok(response);
        }

        [Route("postView")]
        [HttpPost]
        public async Task<IActionResult> PostView([FromBody] PostViewsViewModel model)
        {
            var userId = await GetUserIdAsync(this._userManager);
            model.UserId = userId;
            var response = await _postService.PostView(model);
            return Ok(response);
        }

        [Route("likeUnlikeComment")]
        [HttpPost]
        public async Task<IActionResult> LikeUnlikeComment(Guid commentId, bool isLike)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _postService.LikeUnlikeComment(commentId, isLike, userId);
            return Ok(response);
        }

        [Route("enableDisableComments")]
        [HttpPost]
        public async Task<IActionResult> EnableDisableComments(Guid postId, bool isHideComments)
        {
            await _postService.EnableDisableComments(postId, isHideComments);
            return Ok();
        }

        [Route("saveUserSharedPost")]
        [HttpPost]
        public async Task<IActionResult> SaveUserSharedPost(string userId, Guid postId)
        {
            await _postService.SaveUserSharedPost(userId, postId);
            return Ok();
        }

        [Route("savePostByUser")]
        [HttpPost]
        public async Task<IActionResult> SavePostByUser(string userId, Guid postId)
        {
            await _postService.SavePostByUser(userId, postId);
            return Ok();
        }

        [Route("getSavedPostsByUser")]
        [HttpPost]
        public async Task<IActionResult> GetSavedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            var response = await _postService.GetSavedPostsByUser(userId, pageNumber, type);
            return Ok(response);
        }

        [Route("getSharedPostsByUser")]
        [HttpPost]
        public async Task<IActionResult> GetSharedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            var response = await _postService.GetSharedPostsByUser(userId, pageNumber, type);
            return Ok(response);
        }

        [Route("getLikedPostsByUser")]
        [HttpPost]
        public async Task<IActionResult> GetLikedPostsByUser(string userId, int pageNumber, PostTypeEnum type)
        {
            var response = await _postService.GetLikedPostsByUser(userId, pageNumber, type);
            return Ok(response);
        }

        [Route("pinUnpinSavedPost")]
        [HttpPost]
        public async Task<IActionResult> PinUnpinSavedPost(Guid attachmentId, bool isPinned)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _postService.PinUnpinSavedPost(attachmentId, isPinned, userId);
            return Ok(response);
        }

        //[Route("pinUnpinSharedPost")]
        //[HttpPost]
        //public async Task<IActionResult> PinUnpinSharedPost(Guid attachmentId, bool isPinned)
        //{
        //    var response = await _postService.PinUnpinSharedPost(attachmentId, isPinned);
        //    return Ok(response);
        //}

        [Route("pinUnpinLikedPost")]
        [HttpPost]
        public async Task<IActionResult> PinUnpinLikedPost(Guid attachmentId, bool isPinned)
        {
            var userId = await GetUserIdAsync(this._userManager);
            var response = await _postService.PinUnpinLikedPost(attachmentId, isPinned, userId);
            return Ok(response);
        }

        [Route("deletePost")]
        [HttpPost]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            await _postService.DeletePost(id);
            return Ok();
        }

        [Route("updateCommentThrottling")]
        [HttpPost]
        public async Task<IActionResult> UpdateCommentThrottling(Guid postId, int noOfComments)
        {
            await _postService.UpdateCommentThrottling(postId, noOfComments);
            return Ok();
        }

        [Route("saveStreamAsPost")]
        [HttpPost]
        public async Task<IActionResult> SaveStreamAsPost(Guid postId)
        {
            await _postService.SaveStreamAsPost(postId);
            return Ok();
        }

        [Route("saveLiveVideoTime")]
        [HttpPost]
        public async Task<IActionResult> SaveLiveVideoTime(Guid postId, float videoTotalTime, float videoLiveTime)
        {
            await _postService.SaveLiveVideoTime(postId, videoTotalTime, videoLiveTime);
            return Ok();
        }



    }
}
