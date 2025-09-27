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

        readonly Dictionary<ClrScriptObjectShape, TypeBuilder> _typeBuildersByShape 
            = new Dictionary<ClrScriptObjectShape, TypeBuilder>();

        public TypeShape InTypeShape { get; }

        public TypeBuilder LambdaCapturelessContainerType { get; private set; }

        bool _typesGenerated;
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

        public void GenerateRuntimeTypes(ModuleBuilder moduleBuilder)
        {
            if (_typesGenerated)
            {
                throw new Exception("Types already generated.");
            }

            generateTypeBuilders(moduleBuilder);
            generateObjTypes(moduleBuilder);
            generateLambdas(moduleBuilder);

            _typesGenerated = true;
        }

        void generateTypeBuilders(ModuleBuilder moduleBuilder)
        {
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

                _typeBuildersByShape[objShape] = typeBuilder;
            }
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

                    // TODO: have path for capturing variables
                    var methodBuilder = _clrScriptGenType.DefineMethod(
                        $"<Lambda_Gen>_{_lambdaId}",
                        MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                        sig.Return.InferredType,
                        sig.Arguments.Select(a => a.InferredType).ToArray()
                    );

                    var cacheField = _clrScriptGenType.DefineField($"<Lambda_Gen_Cache>_{_lambdaId}", sig.DelegateType,
                        FieldAttributes.Static | FieldAttributes.Public);

                    sig.GenMethodBuilder = methodBuilder;
                    sig.LambdaCacheField = cacheField;

                    _lambdaId++;
                }
            }

            createLambdas(_rootShapeCollection);
        }

        void generateObjTypes(ModuleBuilder moduleBuilder)
        {
            Type getTypeForShape(ShapeInfo shapeInfo)
            {
                if (shapeInfo is ClrScriptObjectShape objShape)
                {
                    return _typeBuildersByShape[objShape];
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

            foreach (var (shape, builder) in _typeBuildersByShape)
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

            foreach (var (shape, builder) in _typeBuildersByShape)
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
