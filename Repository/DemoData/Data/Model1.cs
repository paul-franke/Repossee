using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace DemoData.Data
{

    public partial class SampDB : DbContext
    {
        public SampDB()
            : base("name=SampDB")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual ISet<Student> Student { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<StudentCourse> StudentCourse { get; set; }

        public virtual DbSet<InternShip> InternShip { get; set; }
        public virtual DbSet<Level> Level { get; set; }
        public virtual DbSet<Mentor> Mentor { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Student>()
    .HasRequired(t => t.Level)
    .WithMany(t => t.Student)
    .HasForeignKey(d => d.Level_Id)
    .WillCascadeOnDelete(false);
 
        }
    }
}
