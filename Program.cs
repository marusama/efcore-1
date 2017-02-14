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

            //using (var db = new BloggingContext())
            //{
            //    db.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            //    var count = db.SaveChanges();
            //    Console.WriteLine("{0} records saved to database", count);

            //    Console.WriteLine();
            //    Console.WriteLine("All blogs in database:");
            //    foreach (var blog in db.Blogs)
            //    {
            //        Console.WriteLine(" - {0}", blog.Url);
            //    }
            //}

            using (var db = new BloggingContext())
            {
                var serviceProvider = db.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(new MyFilteredLoggerProvider());

                foreach (var b in db.Blogs.Include(b => b.Posts))
                {
                    //b.Posts.Add(new Post() { Title = "qwe", Content = "test" });
                    Console.WriteLine($"{b.BlogId} - {string.Join(",", b.Posts.Select(p => p.Title))}");
                }

                var q1 = db.Blogs
                    .Include(b => b.Posts)
                    .Where(b => b.Url != null)
                    .Select(b => new { b.BlogId });

                var q2 = db.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title.Length > 2)
                    .Select(p => new { p.PostId, p.Blog.Url });

                //db.SaveChanges();
            }

            Console.ReadLine();
        }
    }
}
