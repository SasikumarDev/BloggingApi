using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BloggingAPI.BlogModel
{
    public partial class BloggingContext : DbContext
    {
        public BloggingContext()
        {
        }

        public BloggingContext(DbContextOptions<BloggingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Answers> Answers { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Likes> Likes { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<PersonalDetails> PersonalDetails { get; set; }
        public virtual DbSet<Questions> Questions { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-OD6KN4V;Database=Blogging;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Answers>(entity =>
            {
                entity.HasKey(e => e.Aid)
                    .HasName("PK__Answers__C69007C8FFB9B3C2");

                entity.Property(e => e.Aid).HasColumnName("AID");

                entity.Property(e => e.AnswredOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Qid).HasColumnName("QID");

                entity.HasOne(d => d.AnswredByNavigation)
                    .WithMany(p => p.Answers)
                    .HasForeignKey(d => d.AnswredBy)
                    .HasConstraintName("FK_AUSID");

                entity.HasOne(d => d.Q)
                    .WithMany(p => p.Answers)
                    .HasForeignKey(d => d.Qid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AQID");
            });

            modelBuilder.Entity<Comments>(entity =>
            {
                entity.HasKey(e => e.ComId)
                    .HasName("PK__Comments__E15F41320CBB06E7");

                entity.Property(e => e.ComId).HasColumnName("ComID");

                entity.Property(e => e.ComText).HasMaxLength(300);

                entity.Property(e => e.ComType)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Likes>(entity =>
            {
                entity.HasKey(e => e.Lkid)
                    .HasName("PK__Likes__4F79A03141E77FA5");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotId)
                    .HasName("PK__Notifica__4FB200AAD4F986F3");

                entity.Property(e => e.NotId).HasColumnName("NotID");

                entity.Property(e => e.NotDateTime).HasColumnType("smalldatetime");

                entity.Property(e => e.NotRoute).HasMaxLength(100);
            });

            modelBuilder.Entity<PersonalDetails>(entity =>
            {
                entity.HasKey(e => e.PId)
                    .HasName("PK__Personal__DD36D50246E74DBB");

                entity.Property(e => e.PId).HasColumnName("pID");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Company).HasMaxLength(200);

                entity.Property(e => e.FaceBook).HasMaxLength(100);

                entity.Property(e => e.Github)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Job)
                    .HasColumnName("job")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UsId).HasColumnName("UsID");
            });

            modelBuilder.Entity<Questions>(entity =>
            {
                entity.HasKey(e => e.Qid)
                    .HasName("PK__Question__CAB147CB57B92258");

                entity.Property(e => e.Qid).HasColumnName("QID");

                entity.Property(e => e.AskedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Tags)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.AskedByNavigation)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.AskedBy)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_QUSID");
            });

            modelBuilder.Entity<Tags>(entity =>
            {
                entity.HasKey(e => e.TgId)
                    .HasName("PK__Tags__8DF8E356B03D6838");

                entity.Property(e => e.TgId).HasColumnName("TgID");

                entity.Property(e => e.Tagname)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UsId)
                    .HasName("PK__Users__BD21E37F27DF719A");

                entity.Property(e => e.UsId).HasColumnName("UsID");

                entity.Property(e => e.CreateDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Dob)
                    .HasColumnName("DOB")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.EmailId)
                    .HasColumnName("EmailID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ImagePath)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Password).HasMaxLength(90);

                entity.Property(e => e.Updatedate)
                    .HasColumnName("updatedate")
                    .HasColumnType("smalldatetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
