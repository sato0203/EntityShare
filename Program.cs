using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityShare
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("ソース元フォルダと出力先ファイルを入力してください");
                return;
            }
            var csfolder = args[0];
            var dest = args[1];

            var files = Directory.GetFiles(csfolder, "*.cs", SearchOption.AllDirectories);
            var sources = files.Select(file => new StreamReader(file, Encoding.Default).ReadToEnd())
                               .Select(source => EntityShareManager.Compile(source));

            var answer = "";
            sources.ForEach(source => answer += source + "\n");
            //Console.WriteLine(answer);
            var writer = new StreamWriter(dest, false, Encoding.Default);
            writer.Write(answer);
            writer.Close();
            Console.WriteLine("Compile Success");
        }
    }

    public static class EntityShareManager{
        public static string Compile(string sourceCode){
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode); // ソースコードをパースしてシンタックス ツリーに
            var rootNode = syntaxTree.GetRoot();             // ルートのノードを取得
            var rootTokens = rootNode.DescendantTokens();
            var classNodes = rootNode.DescendantNodesAndSelf().Where(node => node.IsKind(SyntaxKind.ClassDeclaration));
            var classes = new List<TsClass>();
            foreach (var classNode in classNodes)
            {
                var className = EntityShareManager.FindClassName(classNode);
                var propertyNodes = classNode.DescendantNodes().Where(node => node.IsKind(SyntaxKind.PropertyDeclaration));
                var properties = propertyNodes.Select(node => new TsProperty { Name = EntityShareManager.FindPropertyName(node), Type = EntityShareManager.FindPropertyType(node) });
                classes.Add(new TsClass { Name = className, Properties = properties });
            }
            var tsSources = classes.Select(tsClass => (TsClass.Compile(tsClass)));
            var answer = "";
            tsSources.ForEach(source => answer += $"{source}\n");
            return answer;
        }

        public static string FindClassName(SyntaxNode classNode){
            var tokens = classNode.DescendantTokens().SkipWhile(token => !token.IsKind(SyntaxKind.ClassKeyword)).Skip(1);
            var classToken = tokens.First();
            var className = tokens.First().Text;
            return className;
        }
        public static IEnumerable<SyntaxNode> FindPropertyNodes(SyntaxNode classNode)
        {
            var propertyNodes = classNode.DescendantNodes().Where(token => token.IsKind(SyntaxKind.PropertyDeclaration));
            return propertyNodes;
        }
        public static string FindPropertyName(SyntaxNode propertyNode){
            //propertyNode.DescendantTokens().ForEach(token => Console.WriteLine($"kind:{token.Kind()},text:{token.Text}"));
            //return "";
            var tokens = propertyNode.DescendantTokens();
            var newTokens = new List<SyntaxToken>();
            bool isOpen = false;
            foreach(var token in tokens){
                if (token.IsKind(SyntaxKind.OpenBracketToken))
                    isOpen = true;
                if (!isOpen)
                    newTokens.Add(token);
                if (token.IsKind(SyntaxKind.CloseBracketToken))
                    isOpen = false;
            }

            return newTokens.Where(token => token.IsKind(SyntaxKind.IdentifierToken)).Last().Text;
        }
        public static string FindPropertyType(SyntaxNode propertyNode){
            var shouldRemoveNodes = new List<SyntaxNode>();
            var attributeAndAccessorLists = propertyNode.DescendantNodes().Where(node => node.IsKind(SyntaxKind.AttributeList) || node.IsKind(SyntaxKind.AccessorList));
            if (!attributeAndAccessorLists.IsEmpty())
            {
                foreach (var attributeList in attributeAndAccessorLists)
                {
                    shouldRemoveNodes.AddRange(attributeList.DescendantNodesAndSelf());
                }
            }
            var newPropertyDecNodes = propertyNode.DescendantNodes().ToList();
            shouldRemoveNodes.ForEach(node => newPropertyDecNodes.Remove(node));
            //newPropertyDecNodes.ForEach(node => Console.WriteLine($"kind:{node.Kind()},text:{node.GetText()}"));
            var typeNode = newPropertyDecNodes.Where(node => node.IsKind(SyntaxKind.ArrayType) || node.IsKind(SyntaxKind.IdentifierName) || node.IsKind(SyntaxKind.PredefinedType)).First();
            var typeName = typeNode.GetText().ToString().Trim();
            if (typeNode.IsKind(SyntaxKind.PredefinedType))
            {

                if (typeName == "byte"
                       || typeName == "sbyte"
                       || typeName == "decimal"
                       || typeName == "double"
                       || typeName == "float"
                       || typeName == "int"
                       || typeName == "uint"
                       || typeName == "long"
                       || typeName == "ulong"
                       || typeName == "short"
                       || typeName == "ushort"
                       )
                {
                    typeName = "number";
                }
                if (typeName.Contains("long"))
                {
                    typeName = "number";
                }
                if (typeName == "char")
                    typeName = "string";
                if (typeName == "object")
                    typeName = "Object";
            }
            return typeName;
        }
    }

    public class TsClass
    {
        public virtual string Name { get; set; }
        public virtual IEnumerable<TsProperty> Properties { get; set; }
        public static string Compile(TsClass tsClass)
        {
            var answer = "";
            answer += $"export interface {tsClass.Name} " + "{\n";
            foreach (var property in tsClass.Properties)
            {
                answer += $"\t{property.Name}:{property.Type}\n";
            }
            answer += "}";
            return answer;
        }
    }

    public class TsProperty{
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
    }
}
