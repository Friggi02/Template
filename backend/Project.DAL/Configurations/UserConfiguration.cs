using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.DAL.Entities;

namespace Project.DAL.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public User BASE_ADMIN = new()
        {
            Id = Guid.Parse("e5521f4c-c677-4b6e-81e4-e0dcd8a0ea2d"),
            Username = "fritz",
            Email = "fritz@gmail.com",
            Name = "Andrea",
            Surname = "Frigerio",
            ProfilePic = "https://avatars.githubusercontent.com/u/71127905?v=4",
            PasswordHash = "AQAAAAIAAYagAAAAEBtWmWPRWhAePW7/CyuQ6NPRF+FCCe73X5PNx7jQeeDEaKnGNBYBnkik3DTP86QgQw==",
            Active = true,
            AccessFailedCount = 0,
            LockoutEnd = null,
        };
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasMany(x => x.Roles).WithMany().UsingEntity<UserRole>();
            builder.HasData(BASE_ADMIN);
        }
    }
}