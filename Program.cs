using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEF1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, world!");

            if (CSharp7())
            {
                return;
            }

            using (var db = new BloggingContext())
            {
                if (db.Blogs.Count() == 0)
                {
                    db.Blogs.Add(new Blog { Url = "www.google.com", Posts = new List<Post>{
                        new Post() { Title = "post 1", Content = "qwe asd zxc" }
                    } });
                    db.Blogs.Add(new Blog { Url = "www.yandex.ru" });
                    var count = db.SaveChanges();
                    Console.WriteLine("{0} records saved to database", count);
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

        public static bool CSharp7()
        {
            // tuples
            var t1 = GetTuple();
            Console.WriteLine($"Tuple: {t1}");
            System.Console.WriteLine($"{t1.a} {t1.b} {t1.s}");
            // deconstruction
            (var first, var middle, var last) = GetTuple();
            System.Console.WriteLine($"{first} {middle} {last}");
            (int a1, _, _) = GetTuple();
            System.Console.WriteLine("We need only first param: " + a1);

            // More expression bodied members
            var p1 = new Person("Petr Petrov");

            // Throw expressions
            var p2 = new AnotherPerson("Ivan Ivanov");
            
            // pattern matching
            object[] data = { null, 42, "43", p1, p2, new Person("Matthias Nagel"), new Person("Katharina Nagel") };
            foreach (var o in data)
            {
                IsPattern(o);
                SwitchPattern(o);

                if (o is int i || (o is string s && int.TryParse(s, out i))) /* use i */ 
                    System.Console.WriteLine($"Found int: {i}");
            }

            // out variables
            if (int.TryParse("42", out int n)) {
                System.Console.WriteLine($"It's {n}!");
            }
            GetCoordinates(out var x, out _); // I only care about x
            System.Console.WriteLine($"X coordinate = {x}");

            // local function
            System.Console.WriteLine("Fibonacci(5) = " + Fibonacci(5));

            // Literal improvements
            var d3 = 123_456;
            var x3 = 0xAB_CD_EF;
            var b3 = 0b1010_1011_1100_1101_1110_1111;
            System.Console.WriteLine($"Literal improvements: {d3} {x3} {b3}");

            // ref return
            RefReturn();

            // Generalized async return types
            var asyncResult1 = GeneralizedAsync(false);
            var asyncResult2 = GeneralizedAsync(true);
            System.Console.WriteLine("asyncResult1.Result: " + asyncResult1.Result);
            System.Console.WriteLine("asyncResult2.Result: " + asyncResult2.Result);

            return true;
        }

        private static void GetCoordinates(out int x, out int y)
        {
            x = 1;
            y = 2;
        }

        class Person
        {
            private static ConcurrentDictionary<int, string> names = new ConcurrentDictionary<int, string>();
            private int id = GetId();

            public Person(string name) => names.TryAdd(id, name); // constructors
            ~Person() => names.TryRemove(id, out _);              // destructors
            public string Name
            {
                get => names[id];                                 // getters
                set => names[id] = value;                         // setters
            }
        }

        class AnotherPerson
        {
            public string Name { get; }
            public AnotherPerson(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
            public string GetFirstName()
            {
                var parts = Name.Split(' ');
                return (parts.Length > 0) ? parts[0] : throw new InvalidOperationException("No name!");
            }
            public string GetLastName() => throw new NotImplementedException();
        }

        private static int _id = 0;
        public static int GetId()
        {
            _id++;
            return _id;
        }

        public static (int a, int b, string s) GetTuple()
        {
            var r = (1, 2, "3");
            return r;
        }

        public static void IsPattern(object o)
        {
            if (o is null) Console.WriteLine("it's a const pattern");
            if (o is 42) Console.WriteLine("it's 42");
            if (o is int i) Console.WriteLine($"it's a type pattern with an int and the value {i}");
            if (o is Person p) Console.WriteLine($"it's a person: {p.Name}");
            if (o is Person p2 && p2.Name.StartsWith("Ka")) 
                Console.WriteLine($"it's a person starting with Ka {p2.Name}");
            if (o is var x) Console.WriteLine($"it's a var pattern with the type {x?.GetType()?.Name}");
        }

        public static void SwitchPattern(object o)
        {
            switch (o)
            {
                case null: Console.WriteLine("it's a constant pattern"); break;
                case int i: Console.WriteLine("it's an int"); break;
                case Person p when p.Name.StartsWith("Ka"): Console.WriteLine($"a Ka person {p.Name}"); break;
                case Person p: Console.WriteLine($"any other person {p.Name}"); break;
                case var x: Console.WriteLine($"it's a var pattern with the type {x?.GetType().Name} "); break;
                default: break;
            }
        }

        public static int Fibonacci(int x)
        {
            if (x < 0) throw new ArgumentException("Less negativity please!", nameof(x));
            return Fib(x).current;

            (int current, int previous) Fib(int i)
            {
                if (i == 0) return (1, 0);
                var (p, pp) = Fib(i - 1);
                return (p + pp, p);
            }
        }

        public static void RefReturn()
        {
            ref int Find(int number, int[] numbers)
            {
                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] == number) 
                    {
                        return ref numbers[i]; // return the storage location, not the value
                    }
                }
                throw new IndexOutOfRangeException($"{nameof(number)} not found");
            }

            int[] array = { 1, 15, -39, 0, 7, 14, -12 };
            System.Console.WriteLine("Array before: " + string.Join(", ", array));

            ref int place = ref Find(7, array); // aliases 7's place in the array
            place = 9; // replaces 7 with 9 in the array
            System.Console.WriteLine("Ref mangled: " + array[4]); // prints 9
            System.Console.WriteLine("Array  after: " + string.Join(", ", array));
        }

        public static async ValueTask<long> GeneralizedAsync(bool b)
        {
            if (b)
                return 42;
            else 
                return await Task.Run<long>(()=> Fibonacci(10));
        }
    }
}
