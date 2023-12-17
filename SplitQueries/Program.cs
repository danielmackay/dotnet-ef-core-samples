﻿using Microsoft.EntityFrameworkCore;

using SplitQueries;

Console.WriteLine("Split Queries Sample");

using var db = new BloggingContext(false);
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var tags = new List<Tag>()
{
    new Tag() { Name = "Tag 1" },
    new Tag() { Name = "Tag 2" },
    new Tag() { Name = "Tag 3" },
    new Tag() { Name = "Tag 4" },
    new Tag() { Name = "Tag 5" }
};

var blogs = new List<Blog>()
{
    new Blog()
    {
        Name = "Blog 1",
        Url = "http://blog1.com",
        Posts = new List<Post>()
        {
            new Post()
            {
                Title = "Post 1",
                Content = "Post 1 Content",
                Tags = tags
            }
        }
    },
    new Blog()
    {
        Name = "Blog 2",
        Url = "http://blog2.com",
        Posts = new List<Post>()
        {
            new Post()
            {
                Title = "Post 2",
                Content = "Post 2 Content",
                Tags = tags
            }
        }
    },
    new Blog()
    {
        Name = "Blog 3",
        Url = "http://blog3.com",
        Posts = new List<Post>()
        {
            new Post()
            {
                Title = "Post 3",
                Content = "Post 3 Content",
                Tags = tags
            }
        }
    },
    new Blog()
    {
        Name = "Blog 4",
        Url = "http://blog4.com",
        Posts = new List<Post>()
        {
            new Post()
            {
                Title = "Post 4",
                Content = "Post 4 Content",
                Tags = tags
            }
        }
    },
    new Blog()
    {
        Name = "Blog 5",
        Url = "http://blog5.com",
        Posts = new List<Post>()
        {
            new Post()
            {
                Title = "Post 5",
                Content = "Post 5 Content",
                Tags = tags
            }
        }
    },
};

db.Blogs.AddRange(blogs);
db.SaveChanges();

_ = db.Posts
    .Include(b => b.Blogs)
    .Include(p => p.Tags)
    .TagWith("Default Single Query")
    .ToList();

_ = db.Posts
    .AsSplitQuery()
    .Include(b => b.Blogs)
    .Include(p => p.Tags)
    .TagWith("Split Query")
    .ToList();

using var db2 = new BloggingContext(true);

_ = db.Posts
    .Include(b => b.Blogs)
    .Include(p => p.Tags)
    .TagWith("Default Split Query")
    .ToList();

_ = db.Posts
    .AsSingleQuery()
    .Include(b => b.Blogs)
    .Include(p => p.Tags)
    .TagWith("Single Query")
    .ToList();

Console.ReadLine();