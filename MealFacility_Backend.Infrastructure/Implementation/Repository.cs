using MealFacility_Backend.DAL;
using MealFacility_Backend.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using MealFacility_Backend.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealFacility_Backend.Infrastructure.Implementation
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private AppDbContext appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<UserDTO> CreateAsync(UserDTO userDTO)
        {
            await appDbContext.AddAsync(userDTO);
            await appDbContext.SaveChangesAsync();
            return userDTO;
        }
    }
}
