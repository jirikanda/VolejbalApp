using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havit.NewProjectTemplate.Model.Infrastructure
{
	[NotMapped]
	public class FilteringCollection<T> : ICollection<T>
	{
		private readonly List<T> source;
		private readonly Func<T, bool> filter;

		public FilteringCollection(List<T> source, Func<T, bool> filter)
		{
			this.source = source;
			this.filter = filter;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return source.Where(filter).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			source.Add(item);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			source.AddRange(collection);
		}

		public void Clear()
		{
			source.Clear();
		}

		public bool Contains(T item)
		{
			return source.Where(filter).Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			source.Where(filter).ToList().CopyTo(array, arrayIndex);
		}

		public void ForEach(Action<T> action)
		{
			source.ForEach(action);
		}

		public bool Remove(T item)
		{
			return source.Remove(item);
		}

		public int RemoveAll(Predicate<T> predicate)
		{
			return source.RemoveAll(predicate);			
		}

		public int Count => source.Where(filter).Count();

		public bool IsReadOnly => false;
	}
}
