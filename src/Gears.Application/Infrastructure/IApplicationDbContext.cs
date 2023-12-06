﻿using Gears.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gears.Application.Infrastructure;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; set; }

    DbSet<Role> Roles { get; set; }
}