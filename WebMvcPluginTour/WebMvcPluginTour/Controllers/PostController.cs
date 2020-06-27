using System;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + TourVars.Version + "/posts")]
    public class PostController : ControllerBase
    {
        private readonly ITourInfoService _tourInfoService;
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;
        private readonly ITourService _tourService;
        private readonly IServiceService _serviceService;
        private readonly IPostService _postService;

        public PostController(ITourInfoService tourInfoService, IUserService userService,
            IFriendService friendService, ITourService tourService, IServiceService serviceService,
            IPostService postService)
        {
            _tourInfoService = tourInfoService;
            _userService = userService;
            _friendService = friendService;
            _tourService = tourService;
            _serviceService = serviceService;
            _postService = postService;
        }

        [HttpGet]
        public object GetMyPosts([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetPostByUserId(userId, page, pageSize, out var posts, out var pagination))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(posts);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("{id}/users-liked")]
        public object GetUserLikePost(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetUserLike(userId, id, page, pageSize, out var users, out var pagination))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(users);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("{id}/comments")]
        public object GetPostComment(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetUserComment(userId, id, page, pageSize, out var postComments,
                        out var pagination))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(postComments);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("tours/{tourId}")]
        public object GetPostByTour(int tourId)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    if (_postService.GetPostByTourId(tourId, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("{id}")]
        public object GetPostById(int id)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost]
        public object CreateNewPost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonContent, body, "Content",
                            JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images",
                            JTokenType.Array, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonTourId, body, "TourId",
                            JTokenType.Integer, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonRating, body, "Rating",
                            JTokenType.Integer, ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    var isTourIdParsed =
                        int.TryParse(jsonTourId?.ToString(), out var tourId);
                    var isRatingParsed =
                        int.TryParse(jsonRating?.ToString(), out var rating);
                    var content = jsonContent?.ToString();
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    if (isTourIdParsed)
                    {
                        if (!isRatingParsed || rating < 1 || rating > 5)
                        {
                            throw new ExceptionWithMessage("Rating is invalid");
                        }
                    }

                    var post = new Post(
                        content: content,
                        tourId: isTourIdParsed ? tourId : (int?) null,
                        images: images!,
                        createAt: DateTime.Now,
                        authorId: userId,
                        rating: isRatingParsed ? rating : (int?) null
                    );

                    if (!_postService.AddNewPost(post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPut("{id}")]
        public object UpdatePost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonContent, body, "Content",
                            JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images",
                            JTokenType.Array, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonRating, body, "Rating",
                            JTokenType.Float, ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    var isRatingParsed =
                        float.TryParse(jsonRating?.ToString(), out var rating);

                    var content = jsonContent?.ToString();
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    if (post.AuthorId != userId)
                    {
                        throw new ExceptionWithMessage("You dont have permission");
                    }

                    post.Update(
                        content: content,
                        images: images!,
                        rating: isRatingParsed ? rating : (float?) null
                    );

                    if (!_postService.Update(post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{id}")]
        public object DeletePost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    post.Delete();

                    if (!_postService.Update(post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("{id}/like")]
        public object LikePost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    if (!_postService.LikePost(post.Id, userId))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{id}/like")]
        public object DislikePost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    if (!_postService.DislikePost(post.Id, userId))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(post)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("{id}/comments")]
        public object CommentPost(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonContent, body, "Content",
                        JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    var content = jsonContent?.ToString();

                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var postComment = new PostComment(content, userId, id, DateTime.Now);

                    if (!_postService.CommentPost(postComment))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray()
                    {
                        JObject.FromObject(postComment)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
        
        [HttpDelete("{id}/comments/{commentId}")]
        public object CommentPost(int id, int commentId)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!_postService.GetPostById(id, out var post))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    if (!_postService.RemoveComment(commentId))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray();
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
    }
}