using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Uno.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StaticClassInitializerAnalyzer : DiagnosticAnalyzer
	{
		internal const string Title = "Large number of static methods in a type with a static initializer";
		internal const string MessageFormat = "{0} has a static type initializer and contains too many static methods. Refactor to use instance methods or remove the static type initializer.";
		internal const string Description = "This type has a static type initializer and contains too many static methods; this creates a large boiler plate code in every static method";
		internal const string Category = "Performance";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			"UNOM0003",
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

			context.RegisterCompilationStartAction(csa =>
			{
				var container = new Container(csa.Compilation, this);

				csa.RegisterSymbolAction(c => container.OnSymbolAction(c), SymbolKind.NamedType);
			});
		}

		public class Container
		{
			private readonly StaticClassInitializerAnalyzer _owner;


			public Container(Compilation compilation, StaticClassInitializerAnalyzer owner)
			{
				_owner = owner;
			}

			internal void OnSymbolAction(SymbolAnalysisContext contextAnalysis)
			{
				if(contextAnalysis.Symbol is INamedTypeSymbol namedTypeSymbol)
				{
					var hasStaticInitializer = namedTypeSymbol.Constructors.Any(c => c.IsStatic);
					var staticMethodsCound = namedTypeSymbol
						.GetMembers()
						.Count(m => m.Kind == SymbolKind.Method && m.IsStatic);

					if(hasStaticInitializer && staticMethodsCound > 50)
					{
						var diagnostic = Diagnostic.Create(
							_owner.SupportedDiagnostics.First(),
							namedTypeSymbol.Locations.FirstOrDefault(),
							namedTypeSymbol.ToDisplayString()
						);
						contextAnalysis.ReportDiagnostic(diagnostic);
					}
				}
			}
		}
	}
}


