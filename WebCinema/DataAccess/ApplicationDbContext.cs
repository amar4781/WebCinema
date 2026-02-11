using Microsoft.EntityFrameworkCore;

namespace WebCinema.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Cinema> Cinemas { get; set; } 
        public DbSet<Movie> Movies { get; set; } 
        public DbSet<Actor> Actors { get; set; } 
        public DbSet<MovieActor> MovieActors { get; set; } 
        public DbSet<MovieSubImg> MovieSubImgs { get; set; } 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=WebCinema;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }
    }
}
