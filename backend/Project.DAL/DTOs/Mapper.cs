using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Repositories;
using System.Reflection;

namespace Project.DAL.DTOs
{
    public class Mapper(ProjectDbContext ctx)
    {
        protected readonly ProjectDbContext _ctx = ctx;
        public static OutputType? Map<InputType, OutputType>(InputType? input) where OutputType : new()
        {
            if (input is not null)
            {

                OutputType output = new();

                foreach (PropertyInfo inputProperty in typeof(InputType).GetProperties())
                {
                    PropertyInfo? outputProperty = typeof(OutputType).GetProperty(inputProperty.Name);
                    if (outputProperty != null && inputProperty.PropertyType == outputProperty.PropertyType)
                    {
                        outputProperty.SetValue(output, inputProperty.GetValue(input));
                    }
                }

                return output;
            }

            return default;
        }

        public MappedUser MapUserToDTO(User user) {
        
            var userRoles = _ctx.Set<UserRole>()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.RoleId)
                .ToList();

            var roles = _ctx.Set<Role>()
                .Where(x => userRoles.Contains(x.Id))
                .ToArray();

            return new()
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Active = user.Active,
                Name = user.Name,
                Surname = user.Surname,
                ProfilePic = user.ProfilePic,
                Roles = roles.Select(role => role.Name).ToArray() ?? []
            };
        }
    }
}