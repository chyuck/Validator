﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validator.Attributes;

namespace Validator.Tests
{
    [TestClass]
    public class ValidatorTests
    {
        #region Test classes
        
        private class TestEmptyClass
        {
        }

        private class TestClassWithoutAttrs
        {
            public string TestProperty { get; set; } 

            public ValidationError TestMethod()
            {
                throw new NotImplementedException();
            }
        }

        private class TestClassWithAttrs
        {
            public string TestPropertyWithoutAttr { get; set; }

            public ValidationError TestMethodWithoutAttr()
            {
                throw new NotImplementedException();
            }

            [NotNullValidate("Static_Property", Key = "staticproperty")]
            public static string TestStaticPropertyWithAttr { get; set; }

            [MethodValidate]
            public static ValidationError TestStaticMethodWithAttr()
            {
                return new ValidationError("staticmethod", "Static_Method");
            }

            [NotNullValidate("Public_Property", Key = "publicproperty")]
            public string TestPublicPropertyWithAttr { get; set; }

            [MethodValidate]
            public ValidationError TestPublicMethodWithAttr()
            {
                return new ValidationError("publicmethod", "Public_Method");
            }

            [NotNullValidate("Internal_Property", Key = "internalproperty")]
            internal string TestInternalPropertyWithAttr { get; set; }

            [MethodValidate]
            internal ValidationError TestInternalMethodWithAttr()
            {
                return new ValidationError("internalmethod", "Internal_Method");
            }

            [NotNullValidate("Protected_Property", Key = "protectedproperty")]
            protected string TestProtectedPropertyWithAttr { get; set; }

            [MethodValidate]
            protected ValidationError TestProtectedMethodWithAttr()
            {
                return new ValidationError("protectedmethod", "Protected_Method");
            }

            [NotNullValidate("Private_Property", Key = "privateproperty")]
            private string TestPrivatePropertyWithAttr { get; set; }

            [MethodValidate]
            private ValidationError TestPrivateMethodWithAttr()
            {
                return new ValidationError("privatemethod", "Private_Method");
            }
        }
        
        private class TestClassWithAnotherAttr
        {
            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
            private class TestAttrAttribute : Attribute
            {
            }

            [TestAttr]
            public string TestProperty { get; set; }

            [TestAttr]
            public ValidationError TestMethod()
            {
                throw new NotImplementedException();
            }
        }

        private class TestClassWithValidValues
        {
            public TestClassWithValidValues()
            {
                TestProperty = "value";
            }

            [NotNullValidate("Public_Property", Key = "publicproperty")]
            private string TestProperty { get; set; }

            [MethodValidate]
            public ValidationError TestMethod()
            {
                return null;
            }
        }

        private class TestClassWithInvalidMethod
        {
            [MethodValidate]
            public ValidationError TestMethod(int t)
            {
                return new ValidationError("publicmethod", "Public_Method");
            }
        }

        private class TestClassWithAttrWithoutKey
        {
            [NotNullValidate("Public_Property")]
            private string TestProperty { get; set; }
        }

        private class TestClassWithSimilarValidationErrors
        {
            [NotNullValidate("Public_Property", Key = "similarError")]
            public string TestPropertyWithAttr { get; set; }

            [MethodValidate]
            public ValidationError TestMethodWithAttr()
            {
                return new ValidationError("similarError", "Public_Property");
            }
        }
        
        private class TestBaseClass { }

        private class TestDerivedClass : TestBaseClass
        {
            [NotNullValidate("Public_Property", Key = "testKey")]
            public string TestPropertyWithAttr { get; set; }
        }

        private class TestClass
        {
            [NotNullValidate("Error1")]
            public string TestProperty1 { get; set; }

            [NotNullValidate("Error2")]
            public string TestProperty2 { get; set; }
        }

        private class TestClassWithReferenceToAnotherClass
        {
            [ComplexTypeValidate]
            public TestClass TestProperty { get; set; }
        }

        private class TestClassWithCollectionOfReferencesToAnotherClass
        {
            [ComplexTypeValidate]
            public TestClass[] TestProperty { get; set; }
        }

        #endregion


        #region Tests
       
        [TestMethod]
        public void Validate_Should_Return_Null_When_Empty_Class()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestEmptyClass();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Class_Without_Attributes()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithoutAttrs();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Errors()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithAttrs();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.AreSame(testClass, errors.Object);
            Assert.AreEqual(10, errors.Errors.Length);

            CollectionAssert.AreEquivalent(
                new[]
                    {
                        new ValidationError("staticproperty", "Static_Property"),
                        new ValidationError("publicproperty", "Public_Property"),
                        new ValidationError("internalproperty", "Internal_Property"),
                        new ValidationError("protectedproperty", "Protected_Property"),
                        new ValidationError("privateproperty", "Private_Property"),

                        new ValidationError("staticmethod", "Static_Method"),
                        new ValidationError("publicmethod", "Public_Method"),
                        new ValidationError("internalmethod", "Internal_Method"),
                        new ValidationError("protectedmethod", "Protected_Method"),
                        new ValidationError("privatemethod", "Private_Method")
                    },
                errors.Errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Class_With_Another_Attributes()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithAnotherAttr();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Class_Is_Valid()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithValidValues();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Class_With_Invalid_Method()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithInvalidMethod();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_ValidationError_With_Property_Name_As_Key()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithAttrWithoutKey();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.AreEqual("TestProperty", errors.Errors[0].Key);
        }

        [TestMethod]
        public void Validate_Should_Return_Only_One_ValidationError_When_Two_Similar_Errors_Are_Triggered()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestClassWithSimilarValidationErrors();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.AreEqual(1, errors.Errors.Length);
            Assert.AreEqual("similarError", errors.Errors[0].Key);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_Object_When_T_Is_Base_Type()
        {
            // Arrange
            var validator = new Validator();
            var testClass = new TestDerivedClass();

            // Act
            var errors = validator.Validate<TestBaseClass>(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Error_When_Referenced_Object_Has_Errors()
        {
            // Arrange
            var testClass =
                new TestClassWithReferenceToAnotherClass
                {
                    TestProperty = new TestClass { TestProperty1 = null, TestProperty2 = "ABC"}
                };

            var validator = new Validator();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Errors.Length);
            Assert.AreEqual(testClass, errors.Object);

            Assert.AreEqual("TestProperty1", errors.Errors[0].Key);
            Assert.AreEqual("Error1", errors.Errors[0].Message);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Referenced_Object_Property_Is_Null()
        {
            // Arrange
            var testClass =
                new TestClassWithReferenceToAnotherClass
                {
                    TestProperty = null
                };

            var validator = new Validator();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_Error_When_Referenced_Collection_Of_Objects_Have_Errors()
        {
            // Arrange
            var testClass =
                new TestClassWithCollectionOfReferencesToAnotherClass
                {
                    TestProperty = 
                        new[]
                        {
                            new TestClass { TestProperty1 = "ABC", TestProperty2 = null },
                            new TestClass { TestProperty1 = "ABC", TestProperty2 = "ABC"},
                            new TestClass { TestProperty1 = null, TestProperty2 = "ABC"}
                        }
                };

            var validator = new Validator();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(testClass, errors.Object);
            Assert.AreEqual(2, errors.Errors.Length);

            Assert.AreEqual("TestProperty2", errors.Errors[0].Key);
            Assert.AreEqual("Error2", errors.Errors[0].Message);
            Assert.AreEqual("TestProperty1", errors.Errors[1].Key);
            Assert.AreEqual("Error1", errors.Errors[1].Message);
        }

        [TestMethod]
        public void Validate_Should_Return_Null_When_Referenced_Collection_Of_Objects_Is_Null()
        {
            // Arrange
            var testClass =
                new TestClassWithCollectionOfReferencesToAnotherClass
                {
                    TestProperty = null
                };

            var validator = new Validator();

            // Act
            var errors = validator.Validate(testClass);

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_No_Errors_When_String_Object_Is_Validated()
        {
            // Arrange
            var validator = new Validator();

            // Act
            var errors = validator.Validate("str");

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_No_Errors_When_Array_Of_Int_Object_Is_Validated()
        {
            // Arrange
            var validator = new Validator();

            // Act
            var errors = validator.Validate(new[] { 1, 2});

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_No_Errors_When_Array_Of_String_Object_Is_Validated()
        {
            // Arrange
            var validator = new Validator();

            // Act
            var errors = validator.Validate(new[] { "a", "b" });

            // Assert
            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_Should_Return_No_Errors_When_dotNet_Object_Is_Validated()
        {
            // Arrange
            var validator = new Validator();

            // Act
            var errors = validator.Validate(new DirectoryInfo(@"c:\"));

            // Assert
            Assert.IsNull(errors);
        }

        #endregion
    }
}
