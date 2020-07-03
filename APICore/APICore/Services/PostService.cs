using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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

        bool GetPostByUserId(int myUserId, int userId, int page, int pageSize, out List<Post> posts,
            out Pagination pagination);

        bool GetUserLike(int userId, int postId, int page, int pageSize, out List<SimpleUser> simpleUsers,
            out Pagination pagination);

        bool GetUserComment(int userId, int postId, int page, int pageSize, out List<PostComment> postComments,
            out Pagination pagination);

        bool GetPostById(int id, out Post post);
        bool LikePost(int postId, int userId);
        bool DislikePost(int postId, int userId);
        bool CommentPost(PostComment postComment);
        bool RemoveComment(int postCommentId);

        bool GetPostByUserId_join(int myUserId, int userId, int page, int pageSize, out List<Post> result,
            out Pagination pagination);

        bool GetNewFeed(int myUserId, int page, int pageSize, out List<Post> result,
            out Pagination pagination);

        bool GetTopUser(int myUserId, int page, int pageSize, out List<SimpleUser> result,
            out Pagination pagination);
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
                    var tour = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null) ??
                               throw new ExceptionWithMessage("Tour not found");

                    var tourInfo = _context.TourInfos.FirstOrDefault(ti => ti.Id == tour.TourInfoId) ??
                                   throw new ExceptionWithMessage("Tour info not found");

                    tourInfo.Rating ??= 0;
                    tourInfo.Rating = (tourInfo.TotalRating * tourInfo.Rating + post.Rating) /
                                      (tourInfo.TotalRating + 1);
                    tourInfo.TotalRating++;
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

        public bool GetPostByUserId(int myUserId, int userId, int page, int pageSize, out List<Post> posts,
            out Pagination pagination)
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
                    }).OrderByDescending(a => a.pc.Id).ToList();

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


            var tour = _context.Tours.FirstOrDefault(t => t.Id == post.TourId && t.DeletedAt == null);
            var tourHost = _context.Users.FirstOrDefault(u => u.Id == tour.CreateById);
            var friendType = _friendService.CalculateIsFriend(userId, tourHost.Id);
            var totalMember =
                _context.TourMembers.Count(t => t.TourId == tour.Id && t.AcceptedAt != null && t.DeletedAt == null);
            var tourInfo = _context.TourInfos.FirstOrDefault(t => t.Id == tour.Id);

            // var result = 
            //
            // post.Tour = new SimpleTour(
            //     tour.Id,
            //     tour.Name,
            //     tour.StartDay,
            //     tour.EndDay,
            //     totalMember,
            //     tourHost,
            //     null,
            //     tour.Price,
            //     tourInfo,
            //     );

            var isLike = _context.PostLikes.FirstOrDefault(pl =>
                pl.PostId == post.Id && pl.UserId == userId && pl.DeletedAt == null) != null;

            post.FeaturedComment = postComments.Count > 0 ? postComments[0] : null;
            if (post.FeaturedComment != null)
            {
                var commentAuthor =
                    _context.Users.FirstOrDefault(u => u.Id == post.FeaturedComment.UserId && u.DeletedAt == null);
                post.FeaturedComment.Author =
                    commentAuthor?.ToSimpleUser(_friendService.CalculateIsFriend(userId, authorId));
            }

            post.IsLike = isLike;

            return;
        }

        public bool GetPostByUserId_join(int myUserId, int userId, int page, int pageSize, out List<Post> result,
            out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var rs = (from post in _context.Posts
                    join author in _context.Users on post.AuthorId equals author.Id
                    join tour in _context.Tours on post.TourId equals tour.Id
                    join tourInfo in _context.TourInfos on tour.TourInfoId equals tourInfo.Id
                    join tourInfoAuthor in _context.Users on tourInfo.CreateById equals tourInfoAuthor.Id
                    join startPlace in _context.Places on tourInfo.StartPlaceId equals startPlace.Id
                    join destinationPlace in _context.Places on tourInfo.DestinatePlaceId equals destinationPlace.Id
                    join host in _context.Users on tourInfo.CreateById equals host.Id
                    join plContexts in _context.PostLikes on new {PostId = post.Id, UserId = myUserId} equals new
                        {PostId = plContexts.PostId, UserId = plContexts.UserId} into pl
                    from subPl in pl.DefaultIfEmpty()
                    join tmContexts in _context.TourMembers on new {TourId = post.Id, UserId = myUserId} equals new
                        {TourId = tmContexts.TourId, UserId = tmContexts.UserId} into tm
                    from subTm in tm.DefaultIfEmpty()
                    let f = (from tourMember in _context.TourMembers
                            join friend in (from fr in _context.Friends.Where(fr =>
                                        fr.AcceptedAt != null &&
                                        (fr.UserId == myUserId || fr.RequestedUserId == myUserId))
                                    select new
                                    {
                                        Id = fr.UserId == myUserId ? fr.RequestedUserId : fr.UserId
                                    }
                                ) on tourMember.UserId equals friend.Id
                            join user in _context.Users on friend.Id equals user.Id
                            select user
                        )
                    where (post.AuthorId == userId)
                    select new
                    {
                        Id = post.Id,
                        Post = post,
                        Author = author,
                        Host = host,
                        Tour = tour,
                        TourInfo = tourInfo,
                        PostLike = subPl,
                        Friends = f.ToList(),
                        TourMember = subTm,
                        StartPlace = startPlace,
                        DestinationPlace = destinationPlace,
                        TourInfoAuthor = tourInfoAuthor,
                    })?.AsEnumerable()?.ToList();

                var rswn = (from post in _context.Posts
                    join author in _context.Users on post.AuthorId equals author.Id
                    join plContexts in _context.PostLikes on new {PostId = post.Id, UserId = myUserId} equals new
                        {PostId = plContexts.PostId, UserId = plContexts.UserId} into pl
                    from subPl in pl.DefaultIfEmpty()
                    where (post.AuthorId == userId && post.TourId == null)
                    select new
                    {
                        Id = post.Id,
                        Post = post,
                        Author = author,
                        Host = (User) null,
                        Tour = (Tour) null,
                        TourInfo = (TourInfo) null,
                        PostLike = subPl,
                        Friends = (List<User>) null,
                        TourMember = (TourMember) null,
                        StartPlace = (Place) null,
                        DestinationPlace = (Place) null,
                        TourInfoAuthor = (User) null,
                    })?.AsEnumerable()?.ToList();

                rs.AddRange(rswn);

                result = new List<Post>();

                // Sort by start date
                rs = rs.OrderByDescending(t => t.Id).ToList();

                var total = rs.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                result = rs
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var post = e.Post;

                        if (post.TourId != null)
                        {
                            var friendType = _friendService.CalculateIsFriend(myUserId, e.Author.Id);
                            var hostFriendType = _friendService.CalculateIsFriend(myUserId, e.Host.Id);
                            var tourInfoAuthorFriendType =
                                _friendService.CalculateIsFriend(myUserId, e.TourInfo.CreateById);

                            var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
                                .OrderByDescending(pc => pc.CreateAt)?.ToList();
                            var totalMember = _context.TourMembers.Count(u =>
                                u.TourId == e.Post.TourId && u.DeletedAt == null && u.AcceptedAt != null);
                            var featuredComment = postComments.Count > 0 ? postComments[0] : null;

                            if (featuredComment != null)
                            {
                                var commentAuthorFriendType =
                                    _friendService.CalculateIsFriend(myUserId, featuredComment.UserId);
                                var commentAuthor = _context.Users.FirstOrDefault(u => u.Id == featuredComment.UserId);
                                featuredComment.Author = commentAuthor?.ToSimpleUser(commentAuthorFriendType);
                            }

                            post.Author = e.Author.ToSimpleUser(friendType);
                            post.IsLike = e.PostLike != null && e.PostLike.DeletedAt == null;
                            post.FeaturedComment = featuredComment;

                            var listFriend = e.Friends.Any()
                                ? e.Friends.Select(u => u.ToSimpleUser(FriendType.Accepted)).ToList()
                                : new List<SimpleUser>();

                            // Add info for tour info
                            e.TourInfo.StartPlace = e.StartPlace;
                            e.TourInfo.DestinatePlace = e.DestinationPlace;
                            e.TourInfo.CreateBy = e.TourInfoAuthor.ToSimpleUser(tourInfoAuthorFriendType);

                            post.Tour = new SimpleTour(
                                e.Tour.Id,
                                e.Tour.Name,
                                e.Tour.StartDay,
                                e.Tour.EndDay,
                                totalMember,
                                e.Host.ToSimpleUser(hostFriendType),
                                listFriend,
                                e.Tour.Price,
                                e.TourInfo,
                                e.TourMember?.JoinAt,
                                e.TourMember?.AcceptedAt
                            );
                        }
                        // Post dont have tour
                        else
                        {
                            var friendType = _friendService.CalculateIsFriend(myUserId, e.Author.Id);
                            var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
                                .OrderByDescending(pc => pc.CreateAt)?.ToList();
                            var featuredComment = postComments.Count > 0 ? postComments[0] : null;

                            if (featuredComment != null)
                            {
                                var commentAuthorFriendType =
                                    _friendService.CalculateIsFriend(myUserId, featuredComment.UserId);
                                var commentAuthor = _context.Users.FirstOrDefault(u => u.Id == featuredComment.UserId);
                                featuredComment.Author = commentAuthor?.ToSimpleUser(commentAuthorFriendType);
                            }

                            post.Author = e.Author.ToSimpleUser(friendType);
                            post.IsLike = e.PostLike != null && e.PostLike.DeletedAt == null;
                            post.FeaturedComment = featuredComment;
                        }


                        return post;
                    })
                    .ToList();

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }


            //
            // foreach (var r in rs)
            // {
            //     var post = r.Post;
            //     
            //     var friendType = _friendService.CalculateIsFriend(userId, r.Author.Id);
            //     var hostFriendType = _friendService.CalculateIsFriend(userId, r.Host.Id);
            //     
            //     var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
            //         .OrderByDescending(pc => pc.CreateAt)?.ToList();
            //     var totalMember = _context.TourMembers.Count(u =>
            //         u.TourId == r.Post.TourId && u.DeletedAt == null && u.AcceptedAt != null);
            //    
            //     post.Author = r.Author.ToSimpleUser(friendType);
            //     post.IsLike = r.PostLike != null && r.PostLike.DeletedAt == null;
            //     post.FeaturedComment = postComments.Count > 0 ? postComments[0] : null;
            //     
            //     post.Tour = new SimpleTour(
            //         r.Tour.Id,
            //         r.Tour.Name,
            //         r.Tour.StartDay,
            //         r.Tour.EndDay,
            //         totalMember,
            //         r.Host.ToSimpleUser(hostFriendType),
            //         r.Friends.Select(f =>
            //         {
            //             var type = _friendService.CalculateIsFriend(userId, f.Id);
            //             return f.ToSimpleUser(type);
            //         }).ToList(),
            //         r.Tour.Price,
            //         r.TourInfo,
            //         r.TourMember.JoinAt,
            //         r.TourMember.AcceptedAt
            //         );
            // }

            // post.Author = rs?.ToSimpleUser(_friendService.CalculateIsFriend(userId, authorId));
            //
            //
            //
            // post.FeaturedComment = postComments.Count > 0 ? postComments[0] : null;
            //
            // post.Tour = tour;
            //
            // var isLike = _context.PostLikes.FirstOrDefault(pl =>
            //     pl.PostId == post.Id && pl.UserId == userId && pl.DeletedAt == null) != null;
            //
            // post.IsLike = isLike;

            return true;
        }

        public bool GetNewFeed(int myUserId, int page, int pageSize, out List<Post> result, out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var rs = (from post in _context.Posts
                    join author in _context.Users on post.AuthorId equals author.Id
                    join tour in _context.Tours on post.TourId equals tour.Id
                    join tourInfo in _context.TourInfos on tour.TourInfoId equals tourInfo.Id
                    join tourInfoAuthor in _context.Users on tourInfo.CreateById equals tourInfoAuthor.Id
                    join startPlace in _context.Places on tourInfo.StartPlaceId equals startPlace.Id
                    join destinationPlace in _context.Places on tourInfo.DestinatePlaceId equals destinationPlace.Id
                    join host in _context.Users on tourInfo.CreateById equals host.Id
                    join plContexts in _context.PostLikes on new {PostId = post.Id, UserId = myUserId} equals new
                        {PostId = plContexts.PostId, UserId = plContexts.UserId} into pl
                    from subPl in pl.DefaultIfEmpty()
                    join tmContexts in _context.TourMembers on new {TourId = post.Id, UserId = myUserId} equals new
                        {TourId = tmContexts.TourId, UserId = tmContexts.UserId} into tm
                    from subTm in tm.DefaultIfEmpty()
                    let f = (from tourMember in _context.TourMembers
                            join friend in (from fr in _context.Friends.Where(fr =>
                                        fr.AcceptedAt != null &&
                                        (fr.UserId == myUserId || fr.RequestedUserId == myUserId))
                                    select new
                                    {
                                        Id = fr.UserId == myUserId ? fr.RequestedUserId : fr.UserId
                                    }
                                ) on tourMember.UserId equals friend.Id
                            join user in _context.Users on friend.Id equals user.Id
                            select user
                        )
                    select new
                    {
                        Id = post.Id,
                        Post = post,
                        Author = author,
                        Host = host,
                        Tour = tour,
                        TourInfo = tourInfo,
                        PostLike = subPl,
                        Friends = f.ToList(),
                        TourMember = subTm,
                        StartPlace = startPlace,
                        DestinationPlace = destinationPlace,
                        TourInfoAuthor = tourInfoAuthor,
                    })?.AsEnumerable()?.ToList();

                var rswn = (from post in _context.Posts
                    join author in _context.Users on post.AuthorId equals author.Id
                    join plContexts in _context.PostLikes on new {PostId = post.Id, UserId = myUserId} equals new
                        {PostId = plContexts.PostId, UserId = plContexts.UserId} into pl
                    from subPl in pl.DefaultIfEmpty()
                    where post.TourId == null
                    select new
                    {
                        Id = post.Id,
                        Post = post,
                        Author = author,
                        Host = (User) null,
                        Tour = (Tour) null,
                        TourInfo = (TourInfo) null,
                        PostLike = subPl,
                        Friends = (List<User>) null,
                        TourMember = (TourMember) null,
                        StartPlace = (Place) null,
                        DestinationPlace = (Place) null,
                        TourInfoAuthor = (User) null,
                    })?.AsEnumerable()?.ToList();

                rs.AddRange(rswn);

                result = new List<Post>();

                // Sort by start date
                rs = rs.OrderByDescending(t => t.Id).ToList();

                var total = rs.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                result = rs
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var post = e.Post;

                        if (post.TourId != null)
                        {
                            var friendType = _friendService.CalculateIsFriend(myUserId, e.Author.Id);
                            var hostFriendType = _friendService.CalculateIsFriend(myUserId, e.Host.Id);
                            var tourInfoAuthorFriendType =
                                _friendService.CalculateIsFriend(myUserId, e.TourInfo.CreateById);

                            var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
                                .OrderByDescending(pc => pc.CreateAt)?.ToList();
                            var totalMember = _context.TourMembers.Count(u =>
                                u.TourId == e.Post.TourId && u.DeletedAt == null && u.AcceptedAt != null);
                            var featuredComment = postComments.Count > 0 ? postComments[0] : null;

                            if (featuredComment != null)
                            {
                                var commentAuthorFriendType =
                                    _friendService.CalculateIsFriend(myUserId, featuredComment.UserId);
                                var commentAuthor = _context.Users.FirstOrDefault(u => u.Id == featuredComment.UserId);
                                featuredComment.Author = commentAuthor?.ToSimpleUser(commentAuthorFriendType);
                            }

                            post.Author = e.Author.ToSimpleUser(friendType);
                            post.IsLike = e.PostLike != null && e.PostLike.DeletedAt == null;
                            post.FeaturedComment = featuredComment;

                            var listFriend = e.Friends.Any()
                                ? e.Friends.Select(u => u.ToSimpleUser(FriendType.Accepted)).ToList()
                                : new List<SimpleUser>();

                            // Add info for tour info
                            e.TourInfo.StartPlace = e.StartPlace;
                            e.TourInfo.DestinatePlace = e.DestinationPlace;
                            e.TourInfo.CreateBy = e.TourInfoAuthor.ToSimpleUser(tourInfoAuthorFriendType);

                            post.Tour = new SimpleTour(
                                e.Tour.Id,
                                e.Tour.Name,
                                e.Tour.StartDay,
                                e.Tour.EndDay,
                                totalMember,
                                e.Host.ToSimpleUser(hostFriendType),
                                listFriend,
                                e.Tour.Price,
                                e.TourInfo,
                                e.TourMember?.JoinAt,
                                e.TourMember?.AcceptedAt
                            );
                        }
                        // Post dont have tour
                        else
                        {
                            var friendType = _friendService.CalculateIsFriend(myUserId, e.Author.Id);
                            var postComments = _context.PostComments.Where(pc => pc.PostId == post.Id)
                                .OrderByDescending(pc => pc.CreateAt)?.ToList();
                            var featuredComment = postComments.Count > 0 ? postComments[0] : null;

                            if (featuredComment != null)
                            {
                                var commentAuthorFriendType =
                                    _friendService.CalculateIsFriend(myUserId, featuredComment.UserId);
                                var commentAuthor = _context.Users.FirstOrDefault(u => u.Id == featuredComment.UserId);
                                featuredComment.Author = commentAuthor?.ToSimpleUser(commentAuthorFriendType);
                            }

                            post.Author = e.Author.ToSimpleUser(friendType);
                            post.IsLike = e.PostLike != null && e.PostLike.DeletedAt == null;
                            post.FeaturedComment = featuredComment;
                        }


                        return post;
                    })
                    .ToList();

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetTopUser(int myUserId, int page, int pageSize, out List<SimpleUser> result,
            out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                result = new List<SimpleUser>();

                var postGroup = from p in _context.Posts
                    group p by p.AuthorId
                    into pg
                    select new
                    {
                        Key = pg.Key,
                        Count = pg.Count()
                    };

                var total = postGroup.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                result = postGroup
                    .AsEnumerable()
                    .ToList()
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var author = _context.Users.FirstOrDefault(u => u.Id == e.Key);
                        var friendType = _friendService.CalculateIsFriend(myUserId, author.Id);
                        var simpleAuthor = author.ToSimpleUser(friendType, e.Count);
                        return simpleAuthor;
                    }).ToList();

                result = result.OrderByDescending(e => e.TotalPost).ToList();

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }
    }
}