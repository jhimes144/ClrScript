using ClrScript.Elements;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ShapeCollection = System.Collections.Generic.Dictionary<ClrScript.Elements.Element, ClrScript.Visitation.Analysis.ShapeInfo>;

namespace ClrScript.Visitation.Analysis
{
    class TypeGenerator
    {
        readonly ShapeCollection _rootShapeCollection
            = new ShapeCollection();

        readonly HashSet<ClrScriptObjectShape> _registeredObjectShapes
            = new HashSet<ClrScriptObjectShape>();

        public TypeShape InTypeShape { get; }

        public TypeBuilder LambdaCapturelessContainerType { get; private set; }

        int _lambdaId;
        TypeBuilder _clrScriptGenType;

        public TypeGenerator(ShapeCollection rootShapeCollection,
            HashSet<ClrScriptObjectShape> registeredObjectShapes,
            TypeBuilder clrScriptGenType,
            TypeShape inTypeShape)
        {
            _rootShapeCollection = rootShapeCollection;
            _registeredObjectShapes = registeredObjectShapes;
            InTypeShape = inTypeShape;
            _clrScriptGenType = clrScriptGenType;
        }

        public void PreGenerateRuntimeTypes(ModuleBuilder moduleBuilder)
        {
            generateObjTypes(moduleBuilder);
            generateLambdas(moduleBuilder);
        }

        void generateLambdas(ModuleBuilder moduleBuilder)
        {
            void createLambdas(IReadOnlyDictionary<Element, ShapeInfo> allShapes)
            {
                var methodShapes = allShapes
                    .Select(p => p.Value)
                    .Where(p => p is MethodShape m && m.Declaration != null)
                    .Distinct()
                    .Cast<MethodShape>();

                foreach (var shape in methodShapes)
                {
                    var sig = shape.CallSignature;

                    createLambdas(sig.ShapesByElement);

                    //var methodArgTypes = new List<Type>
                    //    {
                    //        InTypeShape.InferredType
                    //    };

                    var methodArgTypes = new List<Type>();
                    methodArgTypes.AddRange(sig.Arguments.Select(a => a.InferredType));
                    var methodArgTypesA = methodArgTypes.ToArray();

                    var containerType = moduleBuilder.DefineType($"<ClrScript_Lambda_Captureless_{_lambdaId}>",
                            TypeAttributes.Public);

                    // TODO: need to pass type manager
                    // TODO: have path for capturing variables
                    var methodBuilder = containerType.DefineMethod(
                        $"<Lambda_Gen>_{_lambdaId}",
                        MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                        sig.Return.InferredType,
                        methodArgTypesA
                    );

                    var delType = Util.CreateDelegateType(sig.Return.InferredType, methodArgTypesA);
                    var cacheField = containerType.DefineField($"<Lambda_Gen_Cache>_{_lambdaId}", delType,
                        FieldAttributes.Static | FieldAttributes.Public);

                    sig.GenMethodBuilder = methodBuilder;
                    sig.LambdaCacheField = cacheField;
                    sig.GenDelegateType = delType;
                    sig.MethodContainerType = containerType;

                    _lambdaId++;
                }
            }

            createLambdas(_rootShapeCollection);
        }

        void generateObjTypes(ModuleBuilder moduleBuilder)
        {
            var typeBuildersByShape = new Dictionary<ClrScriptObjectShape, TypeBuilder>();
            var masterObjectShapes = new HashSet<ClrScriptObjectShape>();

            foreach (var shape in _registeredObjectShapes)
            {
                masterObjectShapes.Add(shape.GetMasterShape());
            }

            foreach (var objShape in masterObjectShapes)
            {
                var typeBuilder = moduleBuilder.DefineType
                    ($"<ClrScript_Obj_Gen>_{Guid.NewGuid().ToString().Replace('-', '_')}",
                    TypeAttributes.Public,
                    typeof(ClrScriptObject));

                typeBuildersByShape[objShape] = typeBuilder;
            }

            Type getTypeForShape(ShapeInfo shapeInfo)
            {
                if (shapeInfo is ClrScriptObjectShape objShape)
                {
                    return typeBuildersByShape[objShape];
                }
                else if (shapeInfo is ClrScriptArrayShape objArrayShape)
                {
                    var arrayContentType = getTypeForShape(objArrayShape.ContentShape);
                    return typeof(ClrScriptArray<>).MakeGenericType(arrayContentType);
                }
                else
                {
                    return shapeInfo.InferredType;
                }
            }

            foreach (var (shape, builder) in typeBuildersByShape)
            {
                foreach (var (prop, propShape) in shape.ShapeInfoByPropName)
                {
                    var type = getTypeForShape(propShape);

                    var fieldBuilder = builder.DefineField(prop, type, FieldAttributes.Public);

                    var attributeConstructor = typeof(ClrScriptMemberAttribute).GetConstructor(Type.EmptyTypes);
                    var attributeBuilder = new CustomAttributeBuilder(attributeConstructor, new object[0]);
                    fieldBuilder.SetCustomAttribute(attributeBuilder);
                }
            }

            var hasFieldsField = typeof(ClrScriptObject).GetField("_hasFields",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var objConstructor = typeof(ClrScriptObject).GetConstructor(Type.EmptyTypes);

            foreach (var (shape, builder) in typeBuildersByShape)
            {
                if (shape.ShapeInfoByPropName.Count > 0)
                {
                    var constructorBuilder = builder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        Type.EmptyTypes);

                    var ilGenerator = constructorBuilder.GetILGenerator();

                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Call, objConstructor);

                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    ilGenerator.Emit(OpCodes.Stfld, hasFieldsField);

                    ilGenerator.Emit(OpCodes.Ret);
                }

                shape.GeneratedClrScriptObjType = builder.CreateType();
            }
        }
    }
}
