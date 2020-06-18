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

        public PostService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public bool AddNewPost(Post post)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var _ = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null) ??
                        throw new ExceptionWithMessage("Tour not found");

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
                post = _context.Posts.FirstOrDefault(p => p.TourId == tourId && p.DeletedAt == null) ??
                       throw new ExceptionWithMessage("Post not found");
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
                var allPosts = _context.Posts.Where(p => p.AuthorId == userId && p.DeletedAt == null).ToList();

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

                post = _context.Posts.FirstOrDefault(p => p.Id == id && p.DeletedAt == null) ??
                       throw new ExceptionWithMessage("Post not found");
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
                    var newPostLike = new PostLike(postId, userId);
                    _context.PostLikes.Add(newPostLike);
                    post.TotalLike++;
                }
                else
                {
                    postLike.CreateAt = DateTime.Now;
                    postLike.DeletedAt = null;
                    post.TotalLike++;
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

                postLike.DeletedAt = DateTime.Now;
                post.TotalLike--;

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
    }
}