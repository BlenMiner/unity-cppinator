#if UNITY_MONO_CECIL
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.CppInator.Codegen
{
    internal class AssemblyResolver : IAssemblyResolver
    {
        private readonly string[] m_AssemblyReferences;
        private readonly Dictionary<string, AssemblyDefinition> m_AssemblyCache = new ();
        private readonly ICompiledAssembly m_CompiledAssembly;
        private AssemblyDefinition m_SelfAssembly;

        public AssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            m_CompiledAssembly = compiledAssembly;
            m_AssemblyReferences = compiledAssembly.References;
        }

        public void Dispose() { }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters(ReadingMode.Deferred));

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            lock (m_AssemblyCache)
            {
                if (name.Name == m_CompiledAssembly.Name)
                {
                    return m_SelfAssembly;
                }

                var fileName = FindFile(name);
                if (fileName == null)
                {
                    return null;
                }

                var lastWriteTime = File.GetLastWriteTime(fileName);
                var cacheKey = $"{fileName}{lastWriteTime}";
                if (m_AssemblyCache.TryGetValue(cacheKey, out var result))
                {
                    return result;
                }

                parameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);
                var pdb = $"{fileName}.pdb";
                if (File.Exists(pdb))
                {
                    parameters.SymbolStream = MemoryStreamFor(pdb);
                }

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                m_AssemblyCache.Add(cacheKey, assemblyDefinition);

                return assemblyDefinition;
            }
        }

        private string FindFile(AssemblyNameReference name)
        {
            var fileName = m_AssemblyReferences.FirstOrDefault(r => Path.GetFileName(r) == $"{name.Name}.dll");
            if (fileName != null)
            {
                return fileName;
            }

            // perhaps the type comes from an exe instead
            fileName = m_AssemblyReferences.FirstOrDefault(r => Path.GetFileName(r) == $"{name.Name}.exe");
            if (fileName != null)
            {
                return fileName;
            }

            return m_AssemblyReferences
                .Select(Path.GetDirectoryName)
                .Distinct()
                .Select(parentDir => Path.Combine(parentDir, $"{name.Name}.dll"))
                .FirstOrDefault(File.Exists);
        }

        private static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () =>
            {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                    {
                        throw new InvalidOperationException("File read length is not full length of file.");
                    }
                }

                return new MemoryStream(byteArray);
            });
        }

        private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);

                return Retry(retryCount - 1, waitTime, func);
            }
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            m_SelfAssembly = assemblyDefinition;
        }
    }
}
#endif
