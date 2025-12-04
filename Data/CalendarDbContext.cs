using Microsoft.EntityFrameworkCore;
using Semestralka.Models;

namespace Semestralka.Data
{
    public class CalendarDbContext : DbContext
    {
        public CalendarDbContext(DbContextOptions<CalendarDbContext> options)
            : base(options) {}

        public DbSet<User> Users => Set<User>();
        public DbSet<Calendar> Calendars => Set<Calendar>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<CalendarShare> CalendarShares => Set<CalendarShare>(); 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Calendar>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.Calendars)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Event>()
                .HasOne(e => e.Calendar)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Participant>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<CalendarShare>()
                .HasOne(s => s.Calendar)
                .WithMany(c => c.SharedWith)
                .HasForeignKey(s => s.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CalendarShare>()
                .HasOne(s => s.User)
                .WithMany(u => u.SharedCalendars)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
