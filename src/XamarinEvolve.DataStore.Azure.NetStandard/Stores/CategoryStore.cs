using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
	public class CategoryStore : BaseStore<Category>, ICategoryStore
    {
		public CategoryStore() : base (new DependencyServiceWrapper())
		{ }

		public CategoryStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "Category";
    }
}

