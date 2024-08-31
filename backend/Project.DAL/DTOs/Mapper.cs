using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using System.Reflection;

namespace Project.DAL.DTOs
{
    public class Mapper
    {
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

        public static MappedUser MapUserToDTO(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Active = user.Active,
            Name = user.Name,
            Surname = user.Surname,
            ProfilePic = user.ProfilePic,
            Roles = user.Roles.Select(role => role.Name).ToArray()
        };
    }
}