﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebCrawler.Infrastructure.Persistence;

#nullable disable

namespace WebCrawler.Infrastructure.Migrations
{
    [DbContext(typeof(CrawlerDbContext))]
    [Migration("20250220172653_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebCrawler.Domain.Models.CrawlerExecution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("JsonFilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PagesCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("TotalRecords")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CrawlerExecutions", (string)null);
                });

            modelBuilder.Entity("WebCrawler.Domain.Models.ProxyInfo", b =>
                {
                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Port")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Protocol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Proxies", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
