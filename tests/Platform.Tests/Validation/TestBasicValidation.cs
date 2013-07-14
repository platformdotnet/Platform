using System;
using System.Collections.Generic;
using NUnit.Framework;
using Platform.Validation;

namespace Platform.Tests.Validation
{
	[TestFixture]
	public class TestBasicValidation
	{
		public class NoAttributesClass
		{
			public string Name
			{
				get;
				set;
			}
		}

		[Test]
		public void Test_Object_with_no_Attributes()
		{
			var obj = new NoAttributesClass();

			Assert.IsTrue(obj.Validate().IsSuccess);
		}
        
		[Test]
		public void Test_default_value()
		{
			var person = new Person();

			person.Minimum = -1;
			person.Maximum = 10; 
			person.Age = 10;
			person.Name = "Mars";

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);
			var validator2 = Validator<Person>.NewValidator(ValidatorOptions.Empty);
			var validator3 = Validator.NewValidator(typeof(Person), ValidatorOptions.Empty);

			Assert.AreSame(validator, validator2);
			Assert.AreSame(validator, validator3);

			var result = validator.Validate(person);

			Assert.That(result.IsSuccess, Is.True);

			Assert.AreEqual(11, person.Minimum);
		}

		[Test]
		public void Test_nullable_default_value_when_null()
		{
			var person = new Person();
			person.Name = "Mars";
			person.Age = 10;
			person.LibraryAccountNo = null;

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var result = validator.Validate(person);
			Assert.That(result.IsSuccess, Is.True);

			Assert.AreEqual(1001, person.LibraryAccountNo);
		}

		[Test]
		public void Test_nullable_default_value_when_valid()
		{
			var person = new Person();
			person.Name = "Mars";
			person.Age = 10;
			person.LibraryAccountNo = 0;

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var result = validator.Validate(person);
			Assert.That(result.IsSuccess, Is.True);

			Assert.AreEqual(0, person.LibraryAccountNo);
		}


		[Test]
		public void Test_nullable_default_value_when_invalid()
		{
			var person = new Person();
			person.Name = "Mars";
			person.Age = 10;
			person.LibraryAccountNo = -5;

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var result = validator.Validate(person);
			Assert.That(result.IsSuccess, Is.False);

			person.LibraryAccountNo = 1000002354;
			validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			result = validator.Validate(person);
			Assert.That(result.IsSuccess, Is.False);
		}

		private static void WriteValidationExceptions(ValidationResult result)
		{
			foreach (var ve in result.ValidationExceptions)
			{
				Console.WriteLine(ve);
			}
		}

		[Test]
		public void Test_SizeConstraint()
		{
			var person = new Person();
			person.Minimum = -1;
			person.Maximum = 10;
			person.Age = 10;
			person.Name = "Mars";

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var result = validator.Validate(person);

			Assert.IsTrue(result.IsSuccess);
			
			person.Name = "0123456789";
			result = validator.Validate(person);
			Assert.IsTrue(result.IsSuccess);

			person.Name = "01234567891";
			result = validator.Validate(person);
			Assert.IsFalse(result.IsSuccess);
			WriteValidationExceptions(result);

			person.Name = "";
			result = validator.Validate(person);
			Assert.IsFalse(result.IsSuccess);
			WriteValidationExceptions(result);

			person.Name = null;
			result = validator.Validate(person);
			Assert.IsFalse(result.IsSuccess);
			WriteValidationExceptions(result);

			person.Name = "1";
			result = validator.Validate(person);
			Assert.IsTrue(result.IsSuccess);
		}

		[Test]
		public void Test_ValueRequired1()
		{
			var person = new Person();
			
			person.Minimum = -1;
			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var results = validator.Validate(person);
		
			Assert.AreEqual(3, results.ValidationExceptions.Count);
			// Defaults should not be set if there is a failure
			Assert.AreEqual(-1, person.Minimum);
			Assert.AreEqual("Name", results.ValidationExceptions[0].PropertyValidationContext.PropertyInfo.Name);
			Assert.AreEqual("Name", results.ValidationExceptions[1].PropertyValidationContext.PropertyInfo.Name);
			Assert.AreEqual("Age", results.ValidationExceptions[2].PropertyValidationContext.PropertyInfo.Name);
		}

		[Test]
		public void Test_ValueRequired_2()
		{
			var person = new Person();
			person.Name = "";

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);

			var results = validator.Validate(person);

			Assert.AreEqual(3, results.ValidationExceptions.Count);
			Assert.AreEqual("Name", results.ValidationExceptions[0].PropertyValidationContext.PropertyInfo.Name);
			Assert.AreEqual("Name", results.ValidationExceptions[1].PropertyValidationContext.PropertyInfo.Name);
			Assert.AreEqual("Age", results.ValidationExceptions[2].PropertyValidationContext.PropertyInfo.Name);

			person.Name = "Mars";
			person.Age = 10;

			results = validator.Validate(person);

			Assert.IsTrue(results.IsSuccess);
		}

		[Test]
		public void Test_ValueConstraint()
		{
			var person = new Person();
			person.Name = "Mars";
			person.Age = 10;

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);
			var results = validator.Validate(person);
			Assert.IsTrue(results.IsSuccess);

			person.Age = 11;
			results = validator.Validate(person);
			Assert.IsFalse(results.IsSuccess);

			person.Age = 0;
			results = validator.Validate(person);
			// Fails cause Age has value required set
			Assert.IsFalse(results.IsSuccess);

			person.Age = 1;
			results = validator.Validate(person);
			Assert.IsTrue(results.IsSuccess);

			person.Age = -1;
			results = validator.Validate(person);

			Assert.IsFalse(results.IsSuccess);
		}

		[Test]
		public void Test_value_constraint_for_nullable_types()
		{
			var person = new User();
			person.Name = null;
			person.Tags = new List<string>() { "a" };
			person.HairLength = 0.1m;

			person.Length = 10;
			var results = person.Validate();
			Assert.That(results.IsSuccess, Is.True);

			person.Length = 0;
			results = person.Validate();
			Assert.That(results.IsSuccess, Is.False);

			person.Length = null;
			results = person.Validate();
			Assert.That(results.IsSuccess, Is.True);

			person.Length = -1;
			results = person.Validate();
			Assert.That(results.IsSuccess, Is.False);
		}

		[Test]
		public void Test_constraint_expression()
		{
			var person = new Person();
			person.Name = "Mars";
			person.Age = 10;
			person.Id = 2;

			var validator = Validator<Person>.NewValidator(ValidatorOptions.Empty);
			var results = validator.Validate(person);
			
			Assert.IsTrue(results.IsSuccess);
		}

		[Test]
		public void TestConstraintExpression()
		{
			var request = new SearchRequest();
			var validator = Validator<SearchRequest>.NewValidator(ValidatorOptions.Empty);

			request.Start = 10;
			request.End = 9;

			var result = validator.Validate(request);

			Assert.IsFalse(result.IsSuccess);

			request.Start = 10;
			request.End = 12;

			result = validator.Validate(request);

			Assert.IsTrue(result.IsSuccess);
		}

		[Test]
		public void Test_default_value_with_User()
		{
			var user = new User();

			user.Name = null;
			user.Tags = new List<string>() { "a" };
			user.HairLength = 0.1m;

			var result = user.Validate();

			Assert.That(result.IsSuccess, Is.True);

			Assert.AreEqual("Fernando", user.Name);
		}

		[Test]
		public void Test_pattern_regex()
		{
			var user = new User();

			user.Name = "Fernando";
			user.Tags = new List<string> { "a", "b" };
			user.HairLength = 0.1m;

			var result = user.Validate();

			Assert.That(result.IsSuccess, Is.True);

			user.Name = "Fernando1";

			result = user.Validate();

			Assert.That(result.IsSuccess, Is.False);
		}

		[Test]
		public void Test_value_required_for_decimal()
		{
			var user = new User();

			user.Name = "Fernando";
			user.Tags = new List<string> { "a", "b" };
			user.HairLength = 0.1m;

			var result = user.Validate();

			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Test_value_required_for_collection()
		{
			var user = new User();

			user.Name = "Fernando";
			user.Tags = new List<string> { "a", "b" };
			user.HairLength = 0.1m;

			var result = user.Validate();

			Assert.That(result.IsSuccess, Is.True);

			user.Name = "Fernando";
			user.Tags = null;

			Assert.That(user.Validate().IsSuccess, Is.False);

			user.Name = "Fernando";
			user.Tags = new List<string>();

			Assert.That(user.Validate().IsSuccess, Is.False);
		}

		[Test]
		public void Test_default_value_and_range()
		{
			var page = new PageRequest();

			page.Skip = -1;
			page.Take = 0;

			var result = page.Validate();

			Assert.That(result.IsSuccess, Is.False);

			page.Skip = 0;
			page.Take = -1;

			result = page.Validate();

			Assert.That(result.IsSuccess, Is.False);
			
			page.Skip = 0;
			page.Take = 0;

			result = page.Validate();

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(page.Take, Is.EqualTo(20));
		}
	}
}
