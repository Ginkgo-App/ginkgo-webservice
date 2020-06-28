using System;
using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Models;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public interface IPostService
    {
        bool AddNewPost(Post post);
        bool Update(Post post);
        bool DeletePost(int postId);
        bool GetPostByTourId(int tourId, out Post post);
        bool GetPostByUserId(int userId, int page, int pageSize, out List<Post> posts, out Pagination pagination);

        bool GetUserLike(int userId, int postId, int page, int pageSize, out List<SimpleUser> simpleUsers,
            out Pagination pagination);

        bool GetUserComment(int userId, int postId, int page, int pageSize, out List<PostComment> postComments,
            out Pagination pagination);

        bool GetPostById(int id, out Post post);
        bool LikePost(int postId, int userId);
        bool DislikePost(int postId, int userId);
        bool CommentPost(PostComment postComment);
        bool RemoveComment(int postCommentId);
    }

    public class PostService : IPostService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private readonly FriendService _friendService;

        public PostService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _friendService = new FriendService(appSettings);
        }

        public bool AddNewPost(Post post)
        {
            try
            {
                DbService.ConnectDb(out _context);

                if (post.TourId != null)
                {
                    var _ = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null) ??
                            throw new ExceptionWithMessage("Tour not found");
                }
                else
                {
                    post.Rating = null;
                }

                _context.Posts.Add(post);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool Update(Post post)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Tour not found");
                var postCheck = _context.Posts.FirstOrDefault(p => p.Id == post.Id) ??
                                throw new ExceptionWithMessage("Post not found");

                _context.Posts.Update(post);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool DeletePost(int postId)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var posContext = _context.Posts.FirstOrDefault(p => p.Id == postId) ??
                                 throw new ExceptionWithMessage("Post not found");

                posContext.Delete();
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetPostByTourId(int tourId, out Post post)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Tours.FirstOrDefault(t => t.Id == tourId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Tour not found");
                var postDb = _context.Posts.FirstOrDefault(p => p.TourId == tourId && p.DeletedAt == null) ??
                             throw new ExceptionWithMessage("Post not found");

                post = postDb;

                var postComments = _context.PostComments.Where(pc => pc.PostId == postDb.Id)
                    .OrderByDescending(pc => pc.CreateAt)?.ToList();

                post.FeaturedComment = postComments != null ? postComments[0] : null;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetPostByUserId(int userId, int page, int pageSize, out List<Post> posts, out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Users.FirstOrDefault(t => t.Id == userId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Tour not found");
                var allPosts = _context.Posts.Where(p => p.AuthorId == userId && p.DeletedAt == null)
                    .OrderByDescending(p => p.CreateAt)
                    .ToList();

                var total = allPosts.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    posts = pageSize <= 0
                        ? allPosts
                        : allPosts
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    posts = new List<Post>();
                }

                foreach (var post in posts)
                {
                    GetPostAdditionalData(userId, post);
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetUserLike(int userId, int postId, int page, int pageSize, out List<SimpleUser> simpleUsers,
            out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Posts.FirstOrDefault(t => t.Id == postId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Post not found");

                var users = (from pl in _context.PostLikes
                             join user in _context.Users on pl.UserId equals user.Id
                             select user).ToList();

                var total = users.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;
                List<User> resultUsers;

                if (canPage)
                {
                    resultUsers = pageSize <= 0
                        ? users
                        : users
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    resultUsers = new List<User>();
                }

                simpleUsers = new List<SimpleUser>();

                foreach (var user in resultUsers)
                {
                    var friendType = _friendService.CalculateIsFriend(userId, user.Id);
                    simpleUsers.Add(user.ToSimpleUser(friendType));
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetUserComment(int userId, int postId, int page, int pageSize, out List<PostComment> postComments,
            out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Posts.FirstOrDefault(t => t.Id == postId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Post not found");

                var userComments = (from pc in _context.PostComments
                                    join user in _context.Users on pc.UserId equals user.Id
                                    where pc.DeletedAt == null && pc.PostId == postId
                                    select new
                                    {
                                        user,
                                        pc
                                    }).ToList();

                var postCommentDb = new List<PostComment>();

                foreach (var userComment in userComments)
                {
                    var friendType = _friendService.CalculateIsFriend(userId, userComment.user.Id);
                    userComment.pc.Author = userComment.user.ToSimpleUser(friendType);
                    postCommentDb.Add(userComment.pc);
                }

                var total = postCommentDb.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;
                List<User> resultUsers;

                if (canPage)
                {
                    postComments = pageSize <= 0
                        ? postCommentDb
                        : postCommentDb
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    postComments = new List<PostComment>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetPostById(int id, out Post post)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var postDb = _context.Posts.FirstOrDefault(p => p.Id == id && p.DeletedAt == null) ??
                             throw new ExceptionWithMessage("Post not found");

                post = postDb;

                var postComments = _context.PostComments.Where(pc => pc.PostId == postDb.Id)
                    .OrderByDescending(pc => pc.CreateAt)?.ToList();

                post.FeaturedComment = postComments != null && postComments.Count > 0 ? postComments[0] : null;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool LikePost(int postId, int userId)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var post = _context.Posts.FirstOrDefault(p => p.Id == postId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("Post not found");
                var user = _context.Users.FirstOrDefault(p => p.Id == userId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("User not found");
                var postLike = _context.PostLikes.FirstOrDefault(p => p.PostId == postId && p.UserId == userId);

                if (postLike == null)
                {
                    var newPostLike = new PostLike(userId, postId);
                    _context.PostLikes.Add(newPostLike);
                    post.TotalLike++;
                }
                else if (postLike.DeletedAt != null)
                {
                    postLike.CreateAt = DateTime.Now;
                    postLike.DeletedAt = null;
                    post.TotalLike++;
                }
                else
                {
                    throw new ExceptionWithMessage("You already like this post");
                }

                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool DislikePost(int postId, int userId)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var post = _context.Posts.FirstOrDefault(p => p.Id == postId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("Post not found");
                var user = _context.Users.FirstOrDefault(p => p.Id == userId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("User not found");
                var postLike = _context.PostLikes.FirstOrDefault(p => p.PostId == postId && p.UserId == userId) ??
                               throw new ExceptionWithMessage("You did not like the post yet");

                if (postLike.DeletedAt == null)
                {
                    postLike.DeletedAt = DateTime.Now;
                    post.TotalLike--;
                }
                else
                {
                    throw new ExceptionWithMessage("You did not like the post yet");
                }

                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool CommentPost(PostComment postComment)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var post = _context.Posts.FirstOrDefault(p => p.Id == postComment.PostId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("Post not found");
                var user = _context.Users.FirstOrDefault(p => p.Id == postComment.UserId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("User not found");

                _context.PostComments.Add(postComment);
                post.TotalComment++;

                _context.SaveChanges();

                var friendType = _friendService.CalculateIsFriend(post.AuthorId, post.AuthorId);
                postComment.Author = user.ToSimpleUser(friendType);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool RemoveComment(int postCommentId)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var postComment =
                    _context.PostComments.FirstOrDefault(p => p.Id == postCommentId && p.DeletedAt == null) ??
                    throw new ExceptionWithMessage("Comment not found");

                var post = _context.Posts.FirstOrDefault(p => p.Id == postComment.PostId && p.DeletedAt == null) ??
                           throw new ExceptionWithMessage("Post not found");

                var user = _context.Users.FirstOrDefault(u => u.Id == postComment.UserId && post.DeletedAt == null) ??
                           throw new ExceptionWithMessage("User not found");

                postComment.Delete();
                post.TotalComment--;

                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        private void GetPostAdditionalData(int userId, Post post)
        {
            var authorId = post.AuthorId;
            var author = _context.Users.FirstOrDefault(u => u.Id == authorId && u.DeletedAt == null);
            post.Author = author?.ToSimpleUser(_friendService.CalculateIsFriend(userId, authorId));

            var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
                .OrderByDescending(pc => pc.CreateAt)?.ToList();

            post.FeaturedComment = postComments.Count > 0 ? postComments[0] : null;
            var commentAuthor = _context.Users.FirstOrDefault(u => u.Id == authorId && u.DeletedAt == null);
            post.FeaturedComment.Author = commentAuthor?.ToSimpleUser(_friendService.CalculateIsFriend(userId, authorId));

            var tour = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null);


            post.Tour = tour;

            var isLike = _context.PostLikes.FirstOrDefault(pl =>
                pl.PostId == post.Id && pl.UserId == userId && pl.DeletedAt == null) != null;

            post.IsLike = isLike;

            return;
        }

        // private void GetPostAdditionalData_join(int userId, out Post result)
        // {
        //     var rs = from post in _context.Posts
        //         join user in _context.Users on post.AuthorId equals user.Id
        //         join tour in _context.Tours on post.TourId equals tour.Id
        //         join tourInfo in _context.TourInfos on tour.TourInfoId equals tourInfo.Id
        //         join plContexts in _context.PostLikes on new {post.Id, post.AuthorId} equals new
        //             {plContexts.PostId, plContexts.UserId} into pl
        //         from subPl in pl.DefaultIfEmpty()
        //         select new
        //         {
        //         };
        //
        //     post.Author = author?.ToSimpleUser(_friendService.CalculateIsFriend(userId, authorId));
        //
        //     var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
        //         .OrderByDescending(pc => pc.CreateAt)?.ToList();
        //
        //     post.FeaturedComment = postComments.Count > 0 ? postComments[0] : null;
        //
        //     post.Tour = tour;
        //
        //     var isLike = _context.PostLikes.FirstOrDefault(pl =>
        //         pl.PostId == post.Id && pl.UserId == userId && pl.DeletedAt == null) != null;
        //
        //     post.IsLike = isLike;
        //
        //     return;
        // }
    }
}