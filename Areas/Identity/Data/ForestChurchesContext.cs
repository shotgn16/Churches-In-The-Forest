﻿using ForestChurches.Components.Roles;
using ForestChurches.Components.Users;
using ForestChurches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ForestChurches.Data;

public class ForestChurchesContext : IdentityDbContext<ChurchAccount, ChurchRoles, string>
{
    public ForestChurchesContext(DbContextOptions<ForestChurchesContext> options)
        : base(options)
    {
    }
    public DbSet<ChurchAccount> ChurchAccount { get; set; }
    public DbSet<ChurchRoles> ChurchRoles { get; set; }
    public DbSet<EventsModel> Events { get; set; }

    // Implementation of role permissions
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<PendingRegistration> Registrations { get; set; }

    // Implementation of Church Information and Whitelisted users
    public DbSet<WhitelistedUsers> WhitelistedUsers { get; set; }
    public DbSet<ChurchInformation> ChurchInformation { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EventsModel>()
            .HasKey(pk => pk.ID);

        builder.Entity<ChurchRoles>()
            .HasKey(pk => pk.Id);

        builder.Entity<ChurchAccount>()
            .Property(b => b.Role)
            .IsRequired(false);

        builder.Entity<WhitelistedUsers>()
            .HasKey(pk => pk.ID);

        // Implementation of role permissiond
        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        // Assigning Church Information table to users table 
        builder.Entity<ChurchAccount>()
            .HasOne(ca => ca.ChurchInformation)
            .WithOne(ci => ci.ChurchAccount)
            .HasForeignKey<ChurchInformation>(ci => ci.ChurchAccountId);

        builder.Entity<ChurchInformation>()
            .HasIndex(ci => ci.ChurchAccountId)
            .IsUnique();
    }
}
