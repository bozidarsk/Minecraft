using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils.Collections 
{
	public class Stack<T> 
	{
		public int Count { get { return list.Count; } }
		public int Capacity { get { return list.Capacity; } }

		public T this[int index] 
		{
			set { list[index] = value; }
			get { return list[index]; }
		}

		private List<T> list;

		public T Pop() { T item = list[list.Count - 1]; list.RemoveAt(list.Count - 1); return item; }
		public void Push(T item) { list.Add(item); }

		public Stack() { this.list = new List<T>(); }
		public Stack(int size) { this.list = new List<T>(size); }
	}
}