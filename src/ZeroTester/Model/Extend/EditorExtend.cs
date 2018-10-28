using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Markup;
using System.Windows.Resources;
using Agebull.EntityModel.Designer;

namespace Agebull.EntityModel
{
    /// <summary>
    /// WPFÀ©Õ¹
    /// </summary>
    public static class EditorExtend
    {
        public static void RegistResource(Assembly assembly, string file)
        {
            var name = assembly.FullName.Split(',')[0];
            //var file = Path.GetFileNameWithoutExtension();
            var uri = new Uri($"/{name};component/{file.Trim('/')}", UriKind.Relative);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            // ReSharper disable PossibleNullReferenceException
            if (info != null)
            {
                var asm = XamlReader.Load(new Baml2006Reader(info.Stream)) as ResourceDictionary;
                RegistResource(asm);
            }
        }
        public static void RegistResource(string file)
        {
            var uri = new Uri(file, UriKind.Relative);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            // ReSharper disable PossibleNullReferenceException
            if (info != null)
            {
                var asm = XamlReader.Load(new Baml2006Reader(info.Stream)) as ResourceDictionary;
                RegistResource(asm);
            }
        }

        public static List<ResourceDictionary> Resources { get; } = new List<ResourceDictionary>();

        public static void RegistResource(ResourceDictionary resource)
        {
            Resources.Add(resource);
        }
    }
}