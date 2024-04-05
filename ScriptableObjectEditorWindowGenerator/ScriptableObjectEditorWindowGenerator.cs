using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace ScriptableObjectEditorWindowGenerator
{
    [Generator]
    public class ScriptableObjectEditorWindowGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Console.WriteLine(System.DateTime.Now.ToString());

            var sourceBuilder = new StringBuilder(
            @"
            using System;
using UnityEngine;
using System.Reflection;


                internal static class ExampleSourceGenerated
                {
                    public static string GetTestText()
                    {
                        return ""This is from source generator ");

            sourceBuilder.Append(System.DateTime.Now.ToString());

            sourceBuilder.Append(
                @""";
Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                UnityEngine.Debug.Log(assembly.FullName);
            }
                    }
    }

");

            context.AddSource("exampleSourceGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

            AddHasEditorAttribute(context);
            AddFilterableAttribute(context);
        }

        private void AddHasEditorAttribute(GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
        using System;
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        internal class HasEditorAttribute : Attribute
        {
            public string savePath;
            public string[] savePathParams;
            public HasEditorAttribute(string savePath, string[] savePathParams)
            {
                this.savePath = savePath;
                this.savePathParams = savePathParams;
            }
        }");

            context.AddSource("HasEditorAttribute.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
        private void AddFilterableAttribute(GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using System;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
internal class FilterableAttribute : Attribute
{

}

");
            context.AddSource("FilterableAttribute.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

        }


        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
