﻿using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Generators.CSharp;
using CppSharp.Types;

namespace QtSharp
{
    [TypeMap("QString")]
    public class QString : TypeMap
    {
        public override string CSharpSignature(CSharpTypePrinterContext ctx)
        {
            if (ctx.CSharpKind == CSharpTypePrinterContextKind.Native)
            {
                if (ctx.Type.IsPointer())
                {
                    return "QString.Internal*";
                }
                return "QString.Internal";
            }
            return "string";
        }

        public override void CSharpMarshalToNative(MarshalContext ctx)
        {
            ctx.SupportBefore.WriteLine("var __qstring{0} = QString.FromUtf16((ushort*) Marshal.StringToHGlobalUni({1}).ToPointer(), {1}.Length);",
                                        ctx.ParameterIndex, ctx.Parameter.Name);
            Type type = ctx.Parameter.Type.Desugar();
            if (type.IsAddress())
            {
                ctx.Return.Write("ReferenceEquals(__qstring{0}, null) ? global::System.IntPtr.Zero : __qstring{0}.{1}",
                                 ctx.ParameterIndex, Helpers.InstanceIdentifier);
                return;
            }
            Class @class;
            type.IsTagDecl(out @class);
            if (@class == null)
            {
                Type.IsTagDecl(out @class);
            }
            var qualifiedIdentifier = CSharpMarshalNativeToManagedPrinter.QualifiedIdentifier(@class.OriginalClass ?? @class);
            ctx.Return.Write("ReferenceEquals(__qstring{0}, null) ? new {1}.Internal() : *({1}.Internal*) (__qstring{0}.{2})",
                             ctx.ParameterIndex, qualifiedIdentifier, Helpers.InstanceIdentifier);
        }

        public override void CSharpMarshalToManaged(MarshalContext ctx)
        {
            ctx.Return.Write("Marshal.PtrToStringUni(new IntPtr(new QString({0}).Utf16))", ctx.ReturnVarName);
        }
    }
}
