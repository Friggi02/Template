﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Project.DAL;

#nullable disable

namespace Project.DAL.Migrations
{
    [DbContext(typeof(ProjectDbContext))]
    partial class ProjectDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Project.DAL.Entities.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Permissions", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "ManageMyself"
                        },
                        new
                        {
                            Id = 2,
                            Name = "ManageUsers"
                        });
                });

            modelBuilder.Entity("Project.DAL.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("a0d3a989-91a1-424d-ae31-67fff4149f7e"),
                            Name = "Registered"
                        },
                        new
                        {
                            Id = new Guid("374df0d2-c83d-4b0e-8131-c7595e00422a"),
                            Name = "Admin"
                        });
                });

            modelBuilder.Entity("Project.DAL.Entities.RolePermission", b =>
                {
                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("PermissionId")
                        .HasColumnType("int");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermission");

                    b.HasData(
                        new
                        {
                            RoleId = new Guid("a0d3a989-91a1-424d-ae31-67fff4149f7e"),
                            PermissionId = 1
                        },
                        new
                        {
                            RoleId = new Guid("374df0d2-c83d-4b0e-8131-c7595e00422a"),
                            PermissionId = 1
                        },
                        new
                        {
                            RoleId = new Guid("374df0d2-c83d-4b0e-8131-c7595e00422a"),
                            PermissionId = 2
                        });
                });

            modelBuilder.Entity("Project.DAL.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfilePic")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Surname")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("User");

                    b.HasData(
                        new
                        {
                            Id = new Guid("e5521f4c-c677-4b6e-81e4-e0dcd8a0ea2d"),
                            AccessFailedCount = 0,
                            Active = true,
                            Email = "fritz@gmail.com",
                            Name = "Andrea",
                            PasswordHash = "AQAAAAIAAYagAAAAEJSd7RIki7IIFC9MMSNtkoowQgYZFl2hpFlw/GO0GwOCleg6mVEkDTql4mi1mG77hA==",
                            ProfilePic = "https://avatars.githubusercontent.com/u/71127905?v=4",
                            Surname = "Frigerio",
                            Username = "fritz"
                        });
                });

            modelBuilder.Entity("Project.DAL.Entities.UserRole", b =>
                {
                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RoleId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRole");

                    b.HasData(
                        new
                        {
                            RoleId = new Guid("374df0d2-c83d-4b0e-8131-c7595e00422a"),
                            UserId = new Guid("e5521f4c-c677-4b6e-81e4-e0dcd8a0ea2d")
                        });
                });

            modelBuilder.Entity("Project.DAL.Entities.RolePermission", b =>
                {
                    b.HasOne("Project.DAL.Entities.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Project.DAL.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Project.DAL.Entities.UserRole", b =>
                {
                    b.HasOne("Project.DAL.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Project.DAL.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
