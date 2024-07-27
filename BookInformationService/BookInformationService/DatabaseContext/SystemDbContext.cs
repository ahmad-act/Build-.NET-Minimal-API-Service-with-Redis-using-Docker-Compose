﻿using BookInformationService.BookInformation.Facade;
using Microsoft.EntityFrameworkCore;


/* Ensure Migration and Database Initialization:

Add the package:
"
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
"

Open Package Manager Console:
Navigate to Tools > NuGet Package Manager > Package Manager Console in Visual Studio.

Go to the .csproj folder.

Execute the following command:
    Add-Migration InitialCreate
    Update-Database

*/

namespace BookInformationService.DatabaseContext;

public class SystemDbContext : DbContext
{
    public DbSet<BookInformationModel> BookInformation { get; set; } // EF will create a table by the name "BookInformation" 

    public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Ignore<ReservedBookInfo>();
        //modelBuilder.Ignore<ReservationHistory>();

        modelBuilder.Entity<BookInformationModel>()
       .HasKey(b => b.Id);

        modelBuilder.Entity<BookInformationModel>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd(); // Ensure that Id is auto-generated

        // Configure unique constraint on Title
        modelBuilder.Entity<BookInformationModel>()
                .HasIndex(b => b.Title)
                .IsUnique();
    }

}

