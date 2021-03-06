﻿using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.Portable
{
	public interface IPlatformActionWrapper<T> where T : BaseDataObject
	{
		void Before(T contextEntity);
		void Success(T contextEntity);
		void Error(T contextEntity);
	}
}
