using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.DAL.Entities;

namespace Project.DAL.Configurations
{
    internal class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Permissions).WithMany().UsingEntity<RolePermission>();
            builder.HasMany(x => x.Users).WithMany().UsingEntity<UserRole>();
            builder.HasData(Role.GetValues());
        }
    }
}