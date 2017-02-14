using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEF1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, world!");

            using (var db = new BloggingContext())
            {
                if (db.Blogs.Count() == 0)
                {
                    db.Blogs.Add(new Blog { Url = "www.google.com" });
                    db.Blogs.Add(new Blog { Url = "www.yandex.ru" });
                    var count = db.SaveChanges();
                    Console.WriteLine("{0} records saved to database", count);
                }

                var b1 = db.Blogs.Include(b => b.Posts).Single(b => b.Url == "www.google.com");
                if (b1.Posts.Count() == 0)
                {
                    b1.Posts.Add(new Post() { Title = "post 1", Content = "qwe asd zxc" });
                    db.SaveChanges();
                }

                Console.WriteLine();
                Console.WriteLine("All blogs in database:");
                foreach (var b in db.Blogs.Include(b => b.Posts))
                {
                    Console.WriteLine($"{b.BlogId}: {b.Url} - {string.Join(",", b.Posts.Select(p => p.Title))}");
                }
            }

            using (var db = new BloggingContext())
            {
                var serviceProvider = db.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(new MyFilteredLoggerProvider());

                foreach (var b in db.Blogs.Include(b => b.Posts))
                {
                    Console.WriteLine($"{b.BlogId} - {string.Join(",", b.Posts.Select(p => p.Title))}");
                }

                var q1 = db.Blogs
                    .Include(b => b.Posts)
                    .Where(b => b.Url != null)
                    .Select(b => new { b.BlogId });
                Console.WriteLine("q1: {0}", q1.ToSql());

                var q2 = db.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title.Length > 2)
                    .Select(p => new { p.PostId, p.Blog.Url });
                Console.WriteLine("q2: {0}", q2.ToSql());

                foreach(var x in q2)
                {
                    Console.WriteLine($"{x.PostId} {x.Url}");
                }
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
