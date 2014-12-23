using System;
using System.Diagnostics;
using System.IO;

namespace iLynx.Configuration
{
    /// <summary>
    ///     Simple helper class that contains a <see cref="System.Configuration.Configuration" /> object for the entry assembly
    /// </summary>
    public static class ExeConfig
    {
        //private static Func<IConfigSection> builder = () => new BinaryConfigSection(); 
        private static Func<IConfigSection> builder = () => new XmlConfigSection(); 
        private static IConfigSection configurableValuesSection;
        private const string ConfigFile = "Configuration";
        private readonly static string TargetPath = Path.Combine(Environment.CurrentDirectory, ConfigFile);

        /// <summary>
        /// Gets the configurable values section.
        /// </summary>
        /// <value>
        /// The configurable values section.
        /// </value>
        public static IConfigSection ConfigurableValuesSection
        {
            get { return configurableValuesSection ?? (configurableValuesSection = GetConfigurableValueSection()); }
        }

        private static IConfigSection GetConfigurableValueSection()
        {
            if (null != configurableValuesSection) return configurableValuesSection;
            configurableValuesSection = Load();
            return configurableValuesSection;
        }

        public static void SetUp(Func<IConfigSection> configSectionBuilder)
        {
            builder = configSectionBuilder;
            configurableValuesSection = null;
        }

        private static string GetFileName(IConfigSection section)
        {
            var ext = section.FileExtension;
            if (ext.StartsWith("."))
                ext = ext.Remove(0, 1);
            if (0 == ext.Length) throw new InvalidOperationException("The specified configuration section does not have a valid file extension");
            return string.Format("{0}.{1}", TargetPath, ext);
        }

        private static IConfigSection Load()
        {
            Trace.WriteLine("::::: ExeConfig.Load() :::::");
            var section = builder();
            var fileName = GetFileName(section);
            if (!File.Exists(fileName)) return section;
            using (var source = File.OpenRead(fileName))
                section.ReadFrom(source);
            return section;
        }

        public static void Save()
        {
            Trace.WriteLine("::::: ExeConfig.Save() :::::");
            var section = ConfigurableValuesSection;
            var fileName = GetFileName(section);
            if (File.Exists(fileName))
                File.Delete(fileName);
            using (var target = File.Open(fileName, FileMode.Create))
                section.WriteTo(target);
        }
    }
}