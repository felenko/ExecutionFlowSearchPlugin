using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ControlFlowSearch.Analiser
{

    internal class HierarchyTreeNode<T>
    {
        public List<HierarchyTreeNode<T>> Children;

        T Item { get; set; }

        public HierarchyTreeNode(T item)
        {
            Item = item;
        }

        public HierarchyTreeNode<T> AddChild(T item)
        {
            HierarchyTreeNode<T> nodeItem = new HierarchyTreeNode<T>(item);
            if (Children == null) Children = new List<HierarchyTreeNode<T>>();
            Children.Add(nodeItem);
            return nodeItem;
        }
    }
    public class Analizer
    {   
        private string formatting;
        private HierarchyTreeNode<string> hierarchyTree;

        public void FindDeclarationAtPosition(string solutionPath, string projFullName,  string sourceFilePath, int curretPosition)
        {
            
           
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionPath).Result;
           
            //solution.GetIsolatedSolution();
            var projects = solution.Projects;
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            Project currentProject = GetProjectCurrentProjectByFileName(solution, sourceFilePath);
            
            DebugWriteProjectFileNames(currentProject);
            
            var compilation = projects.Where(p => p.Name == currentProject.Name).First().GetCompilationAsync().Result;
            //   var symbol = solution.Projects.Where(p => /*something*/).Single().GetCompilation().GetTypeFromMetadataName(fullyQualifiedTypeName).GetMembers(memberName).Single();
            var sourceText = File.ReadAllText(sourceFilePath);
           
            SyntaxTree syntaxTree = null;
            // SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            syntaxTree = compilation.SyntaxTrees.First(s => s.FilePath.Contains(fileName));
            var root = syntaxTree.GetRoot();
            var method = FindMethodDeclarationByPos(curretPosition, root);


            hierarchy = new StringBuilder();


            var model = compilation.GetSemanticModel(syntaxTree);
             hierarchyTree =new HierarchyTreeNode<string>("root");
            


            BuildCallHierarcy(method, model, solution, string.Empty, hierarchyTree);
            File.WriteAllText(@"C:\temp\result.txt", hierarchy.ToString());

            var test = File.ReadAllText(@"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\JoinOperators\Program.cs");
            var substring = test.Substring(3469, 40);
            var m = model.GetDeclaredSymbol(method);
            var symInfo = model.GetSymbolInfo(method);
            ControlFlowAnalysis controlFlow = model.AnalyzeControlFlow(method);
            //  FindReferences()
        }

        private string GetSymbolName(MethodDeclarationSyntax method, SemanticModel model)
        {
            var metodSymbol = model.GetDeclaredSymbol(method);
            return string.Format("{0}.{1}\n", metodSymbol.ContainingType.MetadataName, metodSymbol.MetadataName);
        }

        private static MethodDeclarationSyntax FindMethodDeclarationByPos(int curretPosition, SyntaxNode root)
        {
            var classDeclarationList = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            int index = 0;
            var classDecl = classDeclarationList[index];
            while (classDecl.FullSpan.Start < curretPosition && index < classDeclarationList.Count - 2)
                classDecl = classDeclarationList[++index];
            var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            if (classDecl.FullSpan.Start > curretPosition) classDecl = classDeclarationList[--index];
            index = 0;
            var method = methods[index];
            while (method.FullSpan.Start < curretPosition && index < methods.Count - 2) method = method = methods[++index];
            if (method.FullSpan.Start > curretPosition) method = methods[--index];
            return method;
        }

        private static void DebugWriteProjectFileNames(Project currentProject)
        {
            foreach (var doc in currentProject.Documents)
            {
                Debug.WriteLine(doc);
            }
        }

        private Project GetProjectCurrentProjectByFileName(Solution solution, string sourceFilePath)
        {
            
            foreach (var proj in solution.Projects)
            {
                if (proj.Documents.Any(d => string.Compare(d.FilePath, sourceFilePath, StringComparison.InvariantCultureIgnoreCase) == 0 )) return  proj;
            }
            return null;
        }

        public StringBuilder hierarchy { get; set; }


        private void BuildCallHierarcy(MethodDeclarationSyntax method, SemanticModel semanticModel, Solution solution, string indent, HierarchyTreeNode<string> root)
        {
            formatting = string.Empty;
            var metodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (string.IsNullOrEmpty(indent)) hierarchy.AppendFormat(indent + "{0}.{1}\n", metodSymbol.ContainingType.MetadataName, metodSymbol.MetadataName);
            root.AddChild(GetSymbolName(method, semanticModel));
            root = root.Children.Last();
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var inv in invocations.ToList())
            {

                var info = semanticModel.GetSymbolInfo(inv);

                var symbol = info.Symbol;

                var classdeclaration = FindClassBySymbol(symbol, solution, indent + "\t", root );
                if (classdeclaration == null) continue;
                var methoddeclaration = FindMehodInClass(symbol, classdeclaration);
                if (methoddeclaration != null)
                {
                    BuildCallHierarcy(methoddeclaration, classdeclaration.SemanticModel, solution, indent + "\t", root.Children.Last());

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

        private TypeSearchResult FindClassBySymbol(ISymbol symbol, Solution solution, string indent, HierarchyTreeNode<string> root)
        {
            string moduleName = symbol.ContainingModule.MetadataName;
            string typeName = symbol.ContainingType.MetadataName;
            string name = symbol.MetadataName;

            hierarchy.AppendFormat(indent + "{0}.{1}\n", typeName, name);
            root.AddChild(string.Format("{0}.{1}", typeName, name));
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
