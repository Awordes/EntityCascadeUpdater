using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityCascadeUpdater
{
    #region Models

        internal interface IDependsOn<TMaster>
        { }

        public class Foo
        {
            public int SomeField { get; set; }

            public Bar Bar { get; set; }
            
            public Baz Baz { get; set; }
        }

        public class Bar: IDependsOn<Foo>
        {
            public int SomeField { get; set; }

            public Baz Baz { get; set; }
        }

        public class Baz : IDependsOn<Foo>
        {
            public int SomeField { get; set; }
        }

    #endregion
    
    public static class Extensions
    {
        public static void CascadeUpdate<TEntity, TProperty>(this TEntity master,
            Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            var propertyName = (propertyExpression.Body as MemberExpression)?.Member.Name;

            if (propertyName is null) return;

            var masterType = master.GetType();

            foreach (var propertyInfo in masterType.GetProperties())
            {
                if (propertyInfo.PropertyType.GetInterfaces()
                        .Any(x => x.IsGenericType
                               && x.GetGenericTypeDefinition() == typeof(IDependsOn<>))
                    && propertyInfo.PropertyType.GetInterface(typeof(IDependsOn<>).Name)?.GetGenericArguments()[0] == masterType)
                {
                    var propertyValue = propertyInfo.GetValue(master);
                    
                    var type = propertyValue?.GetType().GetProperty(propertyName)?.GetType();
                    
                    if (type is not null)
                    {
                        propertyValue.CascadeUpdate(propertyName, value);
                    }
                }
                
                if (propertyInfo.Name == propertyName)
                    propertyInfo.SetValue(master, value);
            }
        }
        
        private static void CascadeUpdate<TEntity, TProperty>(this TEntity master, string propertyName, TProperty value)
        {
            if (propertyName is null) return;

            var masterType = master.GetType();

            foreach (var propertyInfo in masterType.GetProperties())
            {
                if (propertyInfo.PropertyType.GetInterfaces()
                        .Any(x => x.IsGenericType
                                  && x.GetGenericTypeDefinition() == typeof(IDependsOn<>))
                    && propertyInfo.PropertyType.GetInterface(typeof(IDependsOn<>).Name)?.GetGenericArguments()[0] == masterType)
                {
                    var propertyValue = propertyInfo.GetValue(master);
                    
                    var type = propertyValue?.GetType().GetProperty(propertyName)?.GetType();
                    
                    if (type is not null)
                    {
                        propertyValue.CascadeUpdate(propertyName, value);
                    }
                }
                
                if (propertyInfo.Name == propertyName)
                    propertyInfo.SetValue(master, value);
            }
        }
    }
    
    class Program
    {
        static void Main()
        {
            var foo = new Foo
            {
                SomeField = 1,
                Bar = new Bar
                {
                    SomeField = 1,
                    Baz = new Baz
                    {
                        SomeField = 1
                    }
                },
                Baz = new Baz
                {
                    SomeField = 1
                }
            };
            
            Console.WriteLine("Default values:");
            Console.WriteLine($"foo.SomeField = {foo.SomeField}");
            Console.WriteLine($"foo.Bar.SomeField = {foo.Bar.SomeField}");
            Console.WriteLine($"foo.Baz.SomeField = {foo.Baz.SomeField}");
            Console.WriteLine($"foo.Bar.Baz.SomeField = {foo.Bar.Baz.SomeField}");
            Console.WriteLine("=============================");
            
            foo.CascadeUpdate(x => x.SomeField, 2);
            Console.WriteLine("Cascade change default value:");
            Console.WriteLine($"foo.SomeField = {foo.SomeField}");
            Console.WriteLine($"foo.Bar.SomeField = {foo.Bar.SomeField}");
            Console.WriteLine($"foo.Baz.SomeField = {foo.Baz.SomeField}");
            Console.WriteLine($"foo.Bar.Baz.SomeField = {foo.Bar.Baz.SomeField}");
        }
    }
}
