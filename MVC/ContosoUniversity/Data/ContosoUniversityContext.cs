using Microsoft.EntityFrameworkCore;

public class ContosoUniversityContext(DbContextOptions<ContosoUniversityContext> options) : DbContext(options)
{
    public DbSet<ContosoUniversity.Models.Student> Student { get; set; } = default!;
}
