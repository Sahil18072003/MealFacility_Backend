using MealFacility_Backend.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealFacility_Backend.Infrastructure.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<UserDTO> CreateAsync(UserDTO userDTO);
        Task<UserDTO> GetUserdetail(UserDTO userDTO);
    }
}
