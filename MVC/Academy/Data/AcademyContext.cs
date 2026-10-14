using Academy.Models;
using Microsoft.EntityFrameworkCore;
namespace Academy.Data
{
	public class AcademyContext : DbContext
	{
		public AcademyContext(DbContextOptions<AcademyContext> options) : base(options) { }
		public DbSet<Direction> Directions { get; set; }
		public DbSet<Group> Groups { get; set; }
		public DbSet<Student> Students { get; set; }
		public DbSet<Discipline> Disciplines { get; set; }
		public DbSet<Teacher> Teachers { get; set; }
		public DbSet<TeachersDisciplinesRelation> TeachersDisciplinesRelation { get; set; }
	}
}
