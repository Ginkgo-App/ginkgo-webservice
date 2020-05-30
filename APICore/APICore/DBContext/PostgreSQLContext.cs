using APICore.Entities;
using Microsoft.EntityFrameworkCore;

namespace APICore.DBContext
{
    public class PostgreSQLContext : DbContext
    {
        // private static PostgreSQLContext instance = null;
        // private static readonly object padlock = new object();

        public PostgreSQLContext(DbContextOptions<PostgreSQLContext> options) : base(options)
        {
        }

        // public static PostgreSQLContext Instance
        // {
        //     get
        //     {
        //         if (instance != null) return instance;
        //         lock (padlock)
        //         {
        //             if (instance != null) return instance;
        //             var options = new DbContextOptions<PostgreSQLContext>();
        //             instance = new PostgreSQLContext(options);
        //         }
        //         return instance;
        //     }
        // }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(Vars.ConnectionString);
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AuthProvider> AuthProviders { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FeedbackComment> FeedbackComments { get; set; }
        public DbSet<FeedbackLike> FeedbackLikes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<ChildPlace> ChildPlaces { get; set; }
        public DbSet<PlaceType> PlaceTypes { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceDetail> ServiceDetails { get; set; }
        public DbSet<TimeLine> TimeLines { get; set; }
        public DbSet<TimelinePlace> TimelinePlaces { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourInfo> TourInfos { get; set; }
        public DbSet<TourMember> TourMembers { get; set; }
        public DbSet<TourService> TourServices { get; set; }
        public DbSet<Friend> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<AuthProvider>()
            //    .HasOne(p => p.User)
            //    .WithMany(b => b.AuthProviders)
            //    .IsRequired();

            modelBuilder.Entity<Feedback>().HasKey(fb => new { fb.AuthorId, fb.TourId });
            modelBuilder.Entity<TimelinePlace>().HasKey(fb => new { fb.PlaceId, fb.TimelineId });
            modelBuilder.Entity<TourService>().HasKey(fb => new { fb.ServiceId, fb.TourId });
            modelBuilder.Entity<TourMember>().HasKey(fb => new { fb.TourId, fb.UserId });
            modelBuilder.Entity<TourMember>().HasKey(fb => new { fb.TourId, fb.UserId });
            modelBuilder.Entity<FeedbackLike>().HasKey(fb => new { fb.UserId, fb.TourInfoId, fb.AuthorId });
            modelBuilder.Entity<ChildPlace>().HasKey(fb => new { fb.ParentId, fb.ChildId});
            modelBuilder.Entity<PostLike>().HasKey(fb => new { fb.UserId, fb.PostId});
            modelBuilder.Entity<Friend>().HasKey(fb => new { fb.UserId, fb.RequestedUserId});
        }
    }
}
