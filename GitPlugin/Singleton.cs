// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;

namespace Aurora
{
	public sealed class Singleton<T> where T : class, new()
	{
		private Singleton() {}
		public static readonly T Instance = new T();
	}
}
