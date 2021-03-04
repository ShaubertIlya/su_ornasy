using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Extensions
{
    public  static class GroupByManyExtension
    {

        public static IEnumerable<GroupResult<TElement>> GroupByMany<TElement>(
        this IEnumerable<TElement> elements,
        params Func<TElement, object>[] groupSelectors)
        {
            if (groupSelectors.Length > 0)
            {
                var selector = groupSelectors.First();

                //reduce the list recursively until zero
                var nextSelectors = groupSelectors.Skip(1).ToArray();
                return
                    elements.GroupBy(selector).Select(
                        g => new GroupResult<TElement>
                        {
                            Key = g.Key,
                            Count = g.Count(),
                            Items = g.ToList(),
                            SubGroups = g.GroupByMany(nextSelectors)
                        });
            }
            else
                return null;
        }

        public static IEnumerable<GroupResult<TElement>> GroupByMany<TElement>(
            this IEnumerable<TElement> elements, params string[] groupSelectors)
        {
            var selectors =
                new List<Func<TElement, object>>(groupSelectors.Length);
            foreach (var selector in groupSelectors)
            {
                LambdaExpression l =
                    System.Linq.Dynamic.DynamicExpression.ParseLambda(
                        typeof(TElement), typeof(object), selector);
                selectors.Add((Func<TElement, object>)l.Compile());
            }
            return elements.GroupByMany(selectors.ToArray());
        }
    }

    public class GroupResult<TElement>
    {
        public object Key { get; set; }
        public int Count { get; set; }
        public List<TElement> Items { get; set; }
        public IEnumerable<GroupResult<TElement>> SubGroups { get; set; }
        public override string ToString()
        { return string.Format("{0} ({1})", Key, Count); }
    }
}
