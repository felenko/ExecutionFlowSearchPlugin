using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace ControlFlowSearch.Analiser
{
    public class Analizer
    {
        private string formatting;

        public void TextParseTreeRoundtrip(string solutionPath , string filename, int curretPosition)
        {
            
            string slnPath = @"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\CodeBlog\CodeBlog.sln";
            var workspace = MSBuildWorkspace.Create();
            var task = workspace.OpenSolutionAsync(slnPath);
            task.Wait();
            var solution = task.Result;
            var projects = solution.Projects;
            var compilation = projects.Where(p => p.Name == "CodeBlog").First().GetCompilationAsync().Result;
            //   var symbol = solution.Projects.Where(p => /*something*/).Single().GetCompilation().GetTypeFromMetadataName(fullyQualifiedTypeName).GetMembers(memberName).Single();
            var sourceText = File.ReadAllText(@"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\CodeBlog\CodeBlog\BlogEntry.cs");
            int index = 0;
            SyntaxTree tree3 = compilation.SyntaxTrees.Skip(index).First();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            tree3 = compilation.SyntaxTrees.First(s => s.FilePath.Contains("CodeBlogPackage"));
            var model = compilation.GetSemanticModel(tree3);
            var root = tree3.GetRoot();
            var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var method = methods.Skip(1).First();
            hierarchy = new StringBuilder();
            BuildCallHierarcy(method, model, solution, string.Empty);
            File.WriteAllText(@"C:\temp\result.txt", hierarchy.ToString());

            var test = File.ReadAllText(@"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\JoinOperators\Program.cs");
            var substring = test.Substring(3469, 40);
            var m = model.GetDeclaredSymbol(method);
            var symInfo = model.GetSymbolInfo(method);
            ControlFlowAnalysis controlFlow = model.AnalyzeControlFlow(method);
            //  FindReferences()
        }

        public StringBuilder hierarchy { get; set; }


        private void BuildCallHierarcy(MethodDeclarationSyntax method, SemanticModel semanticModel, Solution solution, string indent)
        {
            formatting = string.Empty;
            var metodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (string.IsNullOrEmpty(indent)) hierarchy.AppendFormat(indent + "{0}.{1}\n", metodSymbol.ContainingType.MetadataName, metodSymbol.MetadataName);
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var inv in invocations.ToList())
            {

                var info = semanticModel.GetSymbolInfo(inv);

                var symbol = info.Symbol;

                var classdeclaration = FindClassBySymbol(symbol, solution, indent + "\t");
                if (classdeclaration == null) continue;
                var methoddeclaration = FindMehodInClass(symbol, classdeclaration);
                if (methoddeclaration != null)
                {
                    BuildCallHierarcy(methoddeclaration, classdeclaration.SemanticModel, solution, indent + "\t");

                }
                //var childnodes = inv.DescendantNodes().OfType<>();
                //var target = (invocations.First().SyntaxTree as MethodDeclarationSyntax);
            }


        }
        private MethodDeclarationSyntax FindMehodInClass(ISymbol symbol, TypeSearchResult classdeclaration)
        {
            string name = symbol.MetadataName;
            foreach (var method in classdeclaration.Syntax.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var methodSymbol = classdeclaration.SemanticModel.GetDeclaredSymbol(method);
                if (methodSymbol.MetadataName == symbol.MetadataName) return method;
            }

            return null;
        }

        private TypeSearchResult FindClassBySymbol(ISymbol symbol, Solution solution, string indent)
        {
            string moduleName = symbol.ContainingModule.MetadataName;
            string typeName = symbol.ContainingType.MetadataName;
            string name = symbol.MetadataName;
            hierarchy.AppendFormat(indent + "{0}.{1}\n", typeName, name);

            foreach (var project in solution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanicModel = compilation.GetSemanticModel(syntaxTree);
                    var typeDeclaration =
                        syntaxTree.GetRoot()
                            .DescendantNodes()
                            .OfType<ClassDeclarationSyntax>()
                            .FirstOrDefault(cl => semanicModel.GetDeclaredSymbol(cl).MetadataName == typeName);
                    if (typeDeclaration != null)
                    {
                        var result = new TypeSearchResult()
                        {
                            Syntax = typeDeclaration,
                            SemanticModel = semanicModel,
                            Symbol = semanicModel.GetDeclaredSymbol(typeDeclaration)

                        };
                        //hierarchy.AppendFormat(indent + "{0}.{1}\n", result.Symbol.Name,name);
                        return result;

                    }
                }

            }
            return null;
        }
        internal class TypeSearchResult
        {
            public ClassDeclarationSyntax Syntax { get; set; }
            public SemanticModel SemanticModel { get; set; }
            public INamedTypeSymbol Symbol { get; set; }
        }
    }
}
