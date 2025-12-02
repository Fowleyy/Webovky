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
        }
    }
}
