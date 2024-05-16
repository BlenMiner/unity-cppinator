#if UNITY_MONO_CECIL
using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.CppInator.Codegen
{
    [UsedImplicitly]
    public class CodeManipulator : ILPostProcessor
    {
        public override ILPostProcessor GetInstance()
        {
            return this; 
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            var isEditorDll = compiledAssembly.Name.EndsWith(".Editor");
            return !isEditorDll;
        }

        static void LogError(ICollection<DiagnosticMessage> messages, MethodDefinition method, Collection<Instruction> instructions, int instruction, string message, bool crash = true)
        {
            SequencePoint sequencePoint = null;

            if (method.DebugInformation != null)
            {
                for (int i = instruction; i >= 0; i--)
                {
                    sequencePoint = method.DebugInformation.GetSequencePoint(instructions[i]);
                    if (sequencePoint != null)
                        break;
                }
            }

            messages.Add(new DiagnosticMessage
            {
                MessageData = message,
                DiagnosticType = DiagnosticType.Error,
                File = sequencePoint?.Document.Url ?? "MissingSequencePoint: " + method.Module.FileName,
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0
            });

            if (crash)
                throw new NotImplementedException(message);
        }

        private static readonly List<TypeReference> _typesToCheck = new ();

        static void ProcessMethod(bool isEditor, MethodDefinition method, List<DiagnosticMessage> messages)
        {
            if (method?.Body?.Instructions == null)
                return;

            var instructions = method.Body.Instructions;
            var ilProcessor = method.Body.GetILProcessor();
            
            if (ilProcessor == null)
                return;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference methodReference)
                {
                    var targetMethod = methodReference.Resolve();
                    
                    if (targetMethod == null)
                        continue;
                    
                    var dllTarget = targetMethod.PInvokeInfo?.Module?.Name;

                    if (targetMethod.IsPInvokeImpl && dllTarget == "__Internal")
                    {
                        LogError(messages, method, instructions, i, 
                            "Please use Native.Invoke instead of direct PInvoke calls.\n" +
                            $"For example: <b>Native.Invoke({methodReference.Name}, ...);</b>\n" +
                            "You need to add <b>using CppInator.Runtime</b>; to the top of the file."
                        );
                        
                        continue;
                    }
                    
                    bool isPInvoke = targetMethod.Name == "Invoke" && 
                                     targetMethod.DeclaringType.Name == "Native" && 
                                     targetMethod.DeclaringType.Namespace == "CppInator.Runtime";
                    
                    if (!isEditor && isPInvoke)
                    {
                        MethodReference ftn = null;

                         /*string finalResultf = "";

                        for (int inst = 0; inst < instructions.Count; inst++)
                            finalResultf += instructions[inst] + "\n";*/

                        int startIdx = 0;
                        
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (instructions[j].OpCode == OpCodes.Ldftn)
                            {
                                var mref = ((MethodReference)instructions[j].Operand).Resolve();
                                if (mref.IsPInvokeImpl && mref.Parameters.Count == targetMethod.Parameters.Count - 1)
                                {
                                    startIdx = j;

                                    for (int k = 0; k < mref.Parameters.Count; ++k)
                                    {
                                        var usedTypeForInteropWithCpp = mref.Parameters[k].ParameterType;
                                        _typesToCheck.Add(usedTypeForInteropWithCpp);
                                    }
                                    
                                    break;
                                }
                            }
                        }

                        int indexOfNull = i;

                        for (int j = startIdx; j >= 0; j--)
                        {
                            var nullInstruction = instructions[j];
                            if (nullInstruction.OpCode != OpCodes.Ldnull) continue;

                            var nextInstruction = instructions[j + 1];
                            if (nextInstruction.OpCode != OpCodes.Ldftn) continue;

                            ftn = (MethodReference)nextInstruction.Operand;

                            var newObjectInstruction = instructions[j + 2];
                            if (newObjectInstruction.OpCode != OpCodes.Newobj) continue;

                            var methodRef = (MethodReference)newObjectInstruction.Operand;
                            var name = methodRef.DeclaringType.Name;

                            if (name.StartsWith("Action") || name.StartsWith("Func"))
                            {
                                indexOfNull = j;
                                break;
                            }
                        }

                        if (ftn == null)
                            continue;

                        instruction.Operand = ftn;
                        // ilProcessor.Replace(instruction, ilProcessor.Create(OpCodes.Call, ftn));
                        
                        ilProcessor.Replace(indexOfNull, ilProcessor.Create(OpCodes.Nop));
                        ilProcessor.Replace(indexOfNull + 1, ilProcessor.Create(OpCodes.Nop));
                        ilProcessor.Replace(indexOfNull + 2, ilProcessor.Create(OpCodes.Nop));
 
                        /*string finalResult = "";

                        for (int inst = 0; inst < instructions.Count; inst++)
                        {
                            finalResult += instructions[inst] + "\n";
                        }

                        LogError(messages, method, instructions, i, $"Before: {finalResultf}\nAfter: {finalResult}", false);*/
                    }
                    else if (isEditor && isPInvoke)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (instructions[j].OpCode == OpCodes.Ldftn)
                            {
                                var mref = ((MethodReference)instructions[j].Operand).Resolve();
                                if (mref.IsPInvokeImpl && mref.Parameters.Count == targetMethod.Parameters.Count - 1)
                                { 
                                    for (int k = 0; k < mref.Parameters.Count; ++k)
                                    {
                                        var usedTypeForInteropWithCpp = mref.Parameters[k].ParameterType;
                                        _typesToCheck.Add(usedTypeForInteropWithCpp);
                                    }
                                    
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return default!;
            
            var messages = new List<DiagnosticMessage>();
            
            using var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
            using var pdbStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData);
            
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, new ReaderParameters
            {
                ReadSymbols = true,
                SymbolStream = pdbStream,
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = new AssemblyResolver(compiledAssembly)
            });

             bool isEditor = false;

            foreach (var define in compiledAssembly.Defines)
            {
                if (define == "UNITY_EDITOR")
                {
                    isEditor = true;
                    break;
                }
            }

            try
            {
                _typesToCheck.Clear();

                foreach (var module in assemblyDefinition.Modules)
                {
                    foreach (var type in module.Types)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.IsPInvokeImpl && method.PInvokeInfo.Module.Name == "__Internal" && !method.PInvokeInfo.IsCallConvCdecl)
                            {
                                method.PInvokeInfo.IsCallConvCdecl = true;
                                continue;
                            }
                            
                            ProcessMethod(isEditor, method, messages);
                        }
                    }
                }
                
                foreach (var type in _typesToCheck)
                {
                    if (type.MetadataType != MetadataType.ValueType) continue;

                    var resolvedType = type.Resolve();

                    if (resolvedType.IsSequentialLayout)
                        continue;
                    
                    resolvedType.IsSequentialLayout = true;
                    
                    messages.Add(new DiagnosticMessage
                    {
                        MessageData = $"Struct '{resolvedType.FullName}' has invalid layout, forced <b>[StructLayout(LayoutKind.Sequential)]</b> attribute. {type.Module.FileName}",
                        DiagnosticType = DiagnosticType.Warning,
                        File = resolvedType.Module.FileName,
                        Line = 0,
                        Column = 0
                    });
                }
            }
            catch (NotImplementedException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                messages.Add(new DiagnosticMessage
                {
                    MessageData = ex.Message + "\n" + ex.StackTrace,
                    DiagnosticType = DiagnosticType.Error,
                    File = "Unknown",
                    Line = 0,
                    Column = 0
                });
            }

            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            
            var writerParameters = new WriterParameters
            {
                WriteSymbols = true,
                SymbolStream = pdb,
                SymbolWriterProvider = new PortablePdbWriterProvider()
            };

            assemblyDefinition.Write(pe, writerParameters);
            
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), messages);
        }
    }
}
#endif