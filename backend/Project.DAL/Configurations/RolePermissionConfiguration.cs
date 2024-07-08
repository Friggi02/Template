using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.DAL.Entities;
using Project.DAL.Permit;

namespace Project.DAL.Configurations
{
    internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PermissionId });

            builder.HasData(
                Create(Role.Registered, Permissions.ManageMyself),
                Create(Role.Admin, Permissions.ManageMyself),
                Create(Role.Admin, Permissions.ManageUsers)
            );
        }

        private static RolePermission Create(Role role, Permissions permission)
        {
            return new RolePermission
            {
                RoleId = role.Id,
                PermissionId = (int)permission,
            };
        }
    }
}