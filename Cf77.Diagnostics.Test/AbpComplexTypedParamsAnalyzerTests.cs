using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Cf77.Diagnostics.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyFileTest()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TwoComplexTypesTest()
        {
            var test = @"
namespace Abp.Application.Services
{
    public interface IApplicationService
    {
    }
}

namespace TestApp
{
    class ComplexType
    {

    }

    class SomeClass: Abp.Application.Services.IApplicationService
    {
        public void Method(string str, ComplexType p, ComplexType d)
        {
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = AbpComplexTypedParamsAnalyzer.DiagnosticId,
                Message = string.Format("Abp service method '{0}' contains more than one complex-typed parameter", "Method"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TwoComplexTypesInPrivateMethod()
        {
            var test = @"
namespace Abp.Application.Services
{
    public interface IApplicationService
    {
    }
}

namespace TestApp
{
    class ComplexType
    {

    }

    class SomeClass: Abp.Application.Services.IApplicationService
    {
        private void Method(string str, ComplexType p, ComplexType d)
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NullableParameterTest()
        {
            var test = @"
namespace Abp.Application.Services
{
    public interface IApplicationService
    {
    }
}

namespace TestApp
{
    class ComplexType
    {

    }

    class SomeClass: Abp.Application.Services.IApplicationService
    {
        public void Method(ComplexType p, int? nullableParam)
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TwoNullableComplexTypesTest()
        {
            var test = @"
namespace Abp.Application.Services
{
    public interface IApplicationService
    {
    }
}

namespace TestApp
{
    struct ComplexType
    {

    }

    class SomeClass: Abp.Application.Services.IApplicationService
    {
        public void Method(string str, ComplexType? p, ComplexType? d)
        {
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = AbpComplexTypedParamsAnalyzer.DiagnosticId,
                Message = string.Format("Abp service method '{0}' contains more than one complex-typed parameter", "Method"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TwoComplexValueTypesTest()
        {
            var test = @"
namespace Abp.Application.Services
{
    public interface IApplicationService
    {
    }
}

namespace TestApp
{
    struct ComplexType
    {

    }

    class SomeClass: Abp.Application.Services.IApplicationService
    {
        public void Method(string str, ComplexType p, ComplexType d)
        {
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = AbpComplexTypedParamsAnalyzer.DiagnosticId,
                Message = string.Format("Abp service method '{0}' contains more than one complex-typed parameter", "Method"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AbpComplexTypedParamsAnalyzer();
        }
    }
}
