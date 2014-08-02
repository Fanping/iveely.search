using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Iveely.STSdb4.Data
{
    public class SlotsBuilder
    {
        private static ConcurrentDictionary<TypeArray, Type> map = new ConcurrentDictionary<TypeArray, Type>();

        private static Type BuildType(Type baseInterface, string className, string fieldsPrefix, params Type[] types)
        {
            var assemblyName = new AssemblyName(className);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            string[] genericParameters = new string[types.Length];
            for (int i = 0; i < types.Length; i++)
                genericParameters[i] = "T" + className + i;

            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Class | TypeAttributes.Public);

            typeBuilder.AddInterfaceImplementation(baseInterface);

            CustomAttributeBuilder customAttribute = new CustomAttributeBuilder(typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes), new object[] { });
            typeBuilder.SetCustomAttribute(customAttribute);

            var typeParams = typeBuilder.DefineGenericParameters(genericParameters);

            FieldBuilder[] fields = new FieldBuilder[types.Length];
            for (int i = 0; i < types.Length; i++)
                fields[i] = typeBuilder.DefineField(fieldsPrefix + i, typeParams[i], FieldAttributes.Public);

            var defConstructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var constr = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, typeParams);
            var ilGenerator = constr.GetILGenerator();

            for (int i = 0; i < types.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_S, i + 1);
                ilGenerator.Emit(OpCodes.Stfld, fields[i]);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return typeBuilder.CreateType().MakeGenericType(types);
        }

        public static Type BuildType(params Type[] types)
        {
            if (types.Length == 0)
                throw new ArgumentException("types array is empty.");

            switch (types.Length)
            {
                case 01: return typeof(Slots<>).MakeGenericType(types);
                case 02: return typeof(Slots<,>).MakeGenericType(types);
                case 03: return typeof(Slots<,,>).MakeGenericType(types);
                case 04: return typeof(Slots<,,,>).MakeGenericType(types);
                case 05: return typeof(Slots<,,,,>).MakeGenericType(types);
                case 06: return typeof(Slots<,,,,,>).MakeGenericType(types);
                case 07: return typeof(Slots<,,,,,,>).MakeGenericType(types);
                case 08: return typeof(Slots<,,,,,,,>).MakeGenericType(types);
                case 09: return typeof(Slots<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Slots<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Slots<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Slots<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Slots<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Slots<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Slots<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Slots<,,,,,,,,,,,,,,,>).MakeGenericType(types);
            }

            return map.GetOrAdd(new TypeArray(types), BuildType(typeof(ISlots), "Slots", "Slot", types));
        }

        private class TypeArray : IEquatable<TypeArray>
        {
            private int? hashcode;

            public readonly Type[] Types;

            public TypeArray(Type[] types)
            {
                Types = types;
            }

            public bool Equals(TypeArray other)
            {
                if (Object.ReferenceEquals(this, other))
                    return true;

                if (Object.ReferenceEquals(other, null))
                    return false;

                if (this.Types.Length != other.Types.Length)
                    return false;

                for (int i = 0; i < Types.Length; i++)
                {
                    if (this.Types[i] != other.Types[i])
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                if (hashcode == null)
                {
                    int code = 0;
                    for (int i = 0; i < Types.Length; i++)
                        code ^= Types[i].GetHashCode();

                    hashcode = code;
                }

                return hashcode.Value;
            }
        }
    }
}
