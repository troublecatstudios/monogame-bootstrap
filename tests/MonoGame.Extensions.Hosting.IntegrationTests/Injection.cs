using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoGame.Extensions.Hosting.IntegrationTests;

internal static class Injection
{
    public static void Replace(MethodInfo methodToReplace, MethodInfo methodToInject)
    {
        _ = methodToReplace ?? throw new ArgumentNullException(nameof(methodToReplace));
        _ = methodToInject ?? throw new ArgumentNullException(nameof(methodToInject));

        if (Debugger.IsAttached)
        {
            throw new NotSupportedException("In debug, the compiler adds some middleman code and we might not have the right address.");
        }

        RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
        RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

        if (methodToReplace.IsVirtual)
        {
            ReplaceVirtualInner(methodToReplace, methodToInject);
        }
        else
        {
            ReplaceInner(methodToReplace, methodToInject);
        }
    }

    private static void ReplaceInner(MethodInfo methodToReplace, MethodInfo methodToInject)
    {
        unsafe
        {
            if (IntPtr.Size == 4)
            {
                int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
                *tar = *inj;
            }
            else
            {
                ulong* inj = (ulong*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                ulong* tar = (ulong*)methodToReplace.MethodHandle.Value.ToPointer() + 1;
                *tar = *inj;           
            }
        }
    }

    private static void ReplaceVirtualInner(MethodInfo methodToReplace, MethodInfo methodToInject)
    {
        unsafe
        {
            UInt64* methodDesc = (UInt64*)(methodToReplace.MethodHandle.Value.ToPointer());
            int index = (int)(((*methodDesc) >> 32) & 0xFF);
            if (IntPtr.Size == 4)
            {
                uint* classStart = (uint*)methodToReplace!.DeclaringType!.TypeHandle.Value.ToPointer();
                classStart += 10;
                classStart = (uint*)*classStart;
                uint* tar = classStart + index;

                uint* inj = (uint*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                //int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
                *tar = *inj;
            }
            else
            {
                ulong* classStart = (ulong*)methodToReplace!.DeclaringType!.TypeHandle.Value.ToPointer();
                classStart += 8;
                classStart = (ulong*)*classStart;
                ulong* tar = classStart + index;

                ulong* inj = (ulong*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                //ulong* tar = (ulong*)methodToReplace.MethodHandle.Value.ToPointer() + 1;
                *tar = *inj;
            }
        }
    }
}