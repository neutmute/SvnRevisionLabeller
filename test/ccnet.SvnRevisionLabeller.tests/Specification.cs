using NUnit.Framework;
using Rhino.Mocks;

namespace CcNet.Labeller.Tests
{
	/// <summary>
	/// Provides a base implementation for Arrange-Act-Assert testing.
	/// </summary>
	/// <remarks>
	/// All test setup should take place in the <c>Arrange()</c> method; the object being tested
	/// should be exercised in the <c>Act()</c> method; assertions are made in the <c>[Test]</c>
	/// methods. Any test cleanup takes place in the <c>After()</c> method.
	/// </remarks>
	public abstract class Specification
	{
		/// <summary>
		/// Calls the <c>Arrange()</c> and <c>Act()</c> method implementations sequentially before
		/// the tests (assertions) are called.
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_mockery = new MockRepository();

			Arrange();
			Act();
		}

		/// <summary>
		/// Gets the mock repository.
		/// </summary>
		/// <value>The mock repository.</value>
		protected MockRepository Mockery
		{
			get { return _mockery; }
		}

		/// <summary>
		/// Calls the <c>After()</c> method once the tests (assertions) are called.
		/// </summary>
		[TearDown]
		public void Teardown()
		{
			After();
		}

		/// <summary>
		/// Sets up the context for the tests. Any initialisation of data should take place here.
		/// </summary>
		protected abstract void Arrange();

		/// <summary>
		/// Exercise the object under test. Only one object/operation should be tested per class.
		/// </summary>
		/// <remarks>
		/// To control the potential explosion of classes in Visual Studio's Solution Explorer, it could be 
		/// beneficial to create an outer class for each object, and then an inner class for each operation.
		/// </remarks>
		protected abstract void Act();

		/// <summary>
		/// Tidies up after each test. No test should leave artifacts behind which may affect other tests,
		/// especially tests which use a persistence store, such as a database.
		/// </summary>
		protected virtual void After()
		{
		}

		protected MockRepository _mockery;
	}
}