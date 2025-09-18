using ClrScript.Elements;
using ClrScript.Elements.Expressions;
using ClrScript.Runtime.Builtins;
using ClrScript.Visitation.Analysis;
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
    class ShapeTable
    {
        readonly ShapeCollection _rootShapeCollection 
            = new ShapeCollection();

        readonly HashSet<ClrScriptObjectShape> _registeredObjectShapes 
            = new HashSet<ClrScriptObjectShape>();

        readonly List<ShapeCollection> _shapeScopes = new List<ShapeCollection>();

        public TypeShape InTypeShape { get; }

        public ShapeTable(Type inType)
        {
            InTypeShape = new TypeShape(inType);
        }

        public void GenerateRuntimeTypes(ModuleBuilder moduleBuilder)
        {
            Debug.Assert(_shapeScopes.Count == 0);
            generateObjTypes(moduleBuilder);
            generateLambdas(moduleBuilder);
        }

        void generateLambdas(ModuleBuilder moduleBuilder)
        {
            var capturelessLambdaContainerType = moduleBuilder.DefineType("ClrScript_Lambda_Captureless_Container",
                TypeAttributes.NotPublic | TypeAttributes.Sealed);

            void createLambdas(IReadOnlyDictionary<Element, ShapeInfo> allShapes)
            {
                var methodShapes = allShapes
                    .Select(p => p.Value)
                    .Where(p => p is MethodShape m && m.Declaration != null)
                    .Distinct()
                    .Cast<MethodShape>();

                foreach (var shape in methodShapes)
                {
                    var distinctSigs = new List<CallSignature>();

                    foreach (var sig in shape.CallSignatures)
                    {
                        var alreadyContains = false;

                        foreach (var oSig in distinctSigs)
                        {
                            if (oSig.Return.InferredType != sig.Return.InferredType)
                            {
                                continue;
                            }

                            if (!oSig.Arguments.Select(a => a.InferredType)
                                .SequenceEqual(sig.Arguments.Select(a => a.InferredType)))
                            {
                                continue;
                            }

                            alreadyContains = true;
                            break;
                        }

                        if (!alreadyContains)
                        {
                            distinctSigs.Add(sig);
                        }
                    }

                    foreach (var sig in distinctSigs)
                    {
                        createLambdas(sig.ShapesByElement);

                        var genName = $"ClrScript_Lambda_Gen_{Guid.NewGuid().ToString().Replace('-', '_')}";

                        var methodArgTypes = new List<Type>
                        {
                            InTypeShape.InferredType
                        };

                        methodArgTypes.AddRange(sig.Arguments.Select(a => a.InferredType));

                        // TODO: need to pass type manager
                        // TODO: have path for capturing variables
                        var methodBuilder = capturelessLambdaContainerType.DefineMethod(
                            genName,
                            MethodAttributes.Public | MethodAttributes.Static,
                            sig.Return.InferredType,
                            methodArgTypes.ToArray()
                        );

                        sig.GenMethodBuilder = methodBuilder;
                    }
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
                    ($"ClrScript_Obj_Gen_{Guid.NewGuid().ToString().Replace('-', '_')}",
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

        public void EnterShapeScope()
        {
            _shapeScopes.Add(new ShapeCollection());
        }

        public void EnterShapeScope(IReadOnlyDictionary<Element, ShapeInfo> shapes)
        {
            _shapeScopes.Add(shapes.ToDictionary(k => k.Key, k => k.Value));
        }

        public ShapeCollection EndShapeScope()
        {
            var shapeCollection = _shapeScopes[_shapeScopes.Count - 1];
            _shapeScopes.Remove(shapeCollection);
            return shapeCollection;
        }

        public ShapeInfo GetShape(Element element)
        {
            foreach (var shapeCol in _shapeScopes)
            {
                if (shapeCol.TryGetValue(element, out var shape))
                {
                    return shape;
                }
            }

            return _rootShapeCollection.GetValueOrDefault(element);
        }

        public void SetShape(Element element, ShapeInfo shapeInfo, bool overrid = false)
        {
            var col = _shapeScopes.LastOrDefault() ?? _rootShapeCollection;

            if (!overrid && col.ContainsKey(element))
            {
                throw new Exception("Element already has shape defined.");
            }

            col[element] = shapeInfo;

            if (shapeInfo is ClrScriptObjectShape objectShape)
            {
                _registeredObjectShapes.Add(objectShape);
            }
        }

        public ShapeInfo DeriveShape(Element element1, Element element2)
        {
            return DeriveShape(GetShape(element1), GetShape(element2));
        }

        public ShapeInfo DeriveShape(ShapeInfo masterShape, ShapeInfo sourceShape)
        {
            if (sourceShape == null)
            {
                throw new Exception("Shape wasn't defined");
            }

            if (masterShape == null || masterShape is UndeterminedShape)
            {
                return sourceShape;
            }

            if (masterShape == sourceShape)
            {
                return masterShape;
            }

            if (masterShape is TypeShape b1
                && sourceShape is TypeShape b2
                && b1.InferredType == b2.InferredType)
            {
                return masterShape;
            }

            if (masterShape is MethodShape m1
                && sourceShape is MethodShape m2)
            {
                return UnknownShape.Instance;
            }

            if (masterShape is ClrScriptArrayShape a1
                && sourceShape is ClrScriptArrayShape a2)
            {
                var derivedContentShape = DeriveShape(a1.ContentShape, a2.ContentShape);
                return new ClrScriptArrayShape(derivedContentShape);
            }

            if (masterShape is ClrScriptObjectShape o1
                && sourceShape is ClrScriptObjectShape o2)
            {
                o1 = o1.GetMasterShape();
                o2 = o2.GetMasterShape();

                var intersectingPropNames = new HashSet<string>();

                foreach (var (prop, propShape) in o1.ShapeInfoByPropName.ToArray())
                {
                    if (o2.ShapeInfoByPropName.TryGetValue(prop, out var otherPropShape))
                    {
                        o1.SetShapeInfoForProp(prop, DeriveShape(propShape, otherPropShape));
                        intersectingPropNames.Add(prop);
                    }
                }

                foreach (var (prop, propShape) in o2.ShapeInfoByPropName.ToArray())
                {
                    if (!intersectingPropNames.Contains(prop))
                    {
                        o1.SetShapeInfoForProp(prop, propShape);
                    }
                }

                o2.ParentShape = o1;
                return masterShape;
            }

            return UnknownShape.Instance;
        }
    }

    abstract class ShapeInfo
    {
        /// <summary>
        /// The inferred type. IMPORTANT: Certain shapes, like ClrScriptObject, will not have their final InferredType until after
        /// the type generation process.
        /// </summary>
        public abstract Type InferredType { get; }
    }

    class UnknownShape : ShapeInfo
    {
        public override Type InferredType => typeof(object);

        public static UnknownShape Instance { get; } = new UnknownShape();

        public override bool Equals(object obj)
        {
            return obj is UnknownShape other;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(UnknownShape left, UnknownShape right)
        {
            return true;
        }

        public static bool operator !=(UnknownShape left, UnknownShape right)
        {
            return false;
        }
    }

    class UndeterminedShape : UnknownShape
    {
        public static new UndeterminedShape Instance { get; } = new UndeterminedShape();

        public override bool Equals(object obj)
        {
            return obj is UndeterminedShape other;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(UndeterminedShape left, UndeterminedShape right)
        {
            return true;
        }

        public static bool operator !=(UndeterminedShape left, UndeterminedShape right)
        {
            return false;
        }
    }

    class TypeShape : ShapeInfo
    {
        public override Type InferredType { get; }

        public TypeShape(Type inferredType)
        {
            InferredType = inferredType;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeShape other)
            {
                return InferredType == other.InferredType;
            }
                
            return false;
        }

        public override int GetHashCode()
        {
            return InferredType?.GetHashCode() ?? 0;
        }

        public static bool operator ==(TypeShape left, TypeShape right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.InferredType == right.InferredType;
        }

        public static bool operator !=(TypeShape left, TypeShape right)
        {
            return !(left == right);
        }
    }

    class MethodShape : ShapeInfo
    {
        public override Type InferredType => CallSignatures.FirstOrDefault()?.Return?.InferredType;

        /// <summary>
        /// Indicates method is not a lambda
        /// </summary>
        public bool IsTypeMethod { get; }

        // We will distinct by inferred types at the compilation phase
        public List<CallSignature> CallSignatures { get; }

        // we can be sure the declaration belongs to this shape, because if a variable or member
        // changes, than the the method shape is discarded
        public Lambda Declaration { get; }

        public CallSignature GetMatchingSignature(IReadOnlyList<ShapeInfo> argShapes)
        {
            foreach (var sig in CallSignatures)
            {
                if (sig.Arguments.Count != argShapes.Count)
                {
                    continue;
                }

                var argsMatch = true;
                for (var i = 0; i < sig.Arguments.Count; i++)
                {
                    if (sig.Arguments[i] != argShapes[i])
                    {
                        argsMatch = false;
                        break;
                    }
                }

                if (argsMatch)
                {
                    return sig;
                }
            }

            return null;
        }

        public MethodShape(ShapeInfo @return, List<ShapeInfo> arguments)
        {
            IsTypeMethod = true;

            CallSignatures = new List<CallSignature>
            {
                new CallSignature(null, @return, arguments)
            };
        }

        public MethodShape(Lambda declaration)
        {
            Declaration = declaration;
            CallSignatures = new List<CallSignature>();
        }
    }

    class CallSignature
    {
        public ShapeInfo Return { get; }

        public IReadOnlyList<ShapeInfo> Arguments { get; }

        public IReadOnlyDictionary<Element, ShapeInfo> ShapesByElement { get; }

        public MethodBuilder GenMethodBuilder { get; set; }

        public CallSignature(IReadOnlyDictionary<Element, ShapeInfo> shapes, 
            ShapeInfo @return, IReadOnlyList<ShapeInfo> arguments)
        {
            ShapesByElement = shapes;
            Return = @return;
            Arguments = arguments;
        }
    }

    class ClrScriptArrayShape : ShapeInfo
    {
        public override Type InferredType
        {
            get
            {
                if (typeof(ClrScriptArray).IsAssignableFrom(ContentShape.InferredType) &&
                    ContentShape.InferredType.GenericTypeArguments[0] == typeof(object))
                {
                    return typeof(ClrScriptArray<>)
                        .MakeGenericType(typeof(object));
                }

                return typeof(ClrScriptArray<>)
                    .MakeGenericType(ContentShape.InferredType);
            }
        }


        // supports reference tracking
        public ShapeInfo ContentShape { get; set; }

        public ClrScriptArrayShape(ShapeInfo contentShape)
        {
            ContentShape = contentShape;
        }
    }

    class ClrScriptObjectShape : ShapeInfo
    {
        public override Type InferredType => GetMasterShape().GeneratedClrScriptObjType;

        public ClrScriptObjectShape ParentShape { get; set; }

        public ClrScriptObjectShape GetMasterShape()
        {
            var shape = this;

            while (true)
            {
                if (shape.ParentShape != null)
                {
                    shape = shape.ParentShape;
                }
                else
                {
                    return shape;
                }
            }
        }

        public Type GeneratedClrScriptObjType { get; set; } = typeof(ClrScriptObject);

        // This is on purpose mutable dictionary, because it translates to reference tracking of
        // objects
        // i.e this can handle
        // var obj = { name: "bob" };
        // var obj2 = obj;
        // obj.name = 12; // name gets updated to unknown shape
        // // ^ The shape for obj and obj2 is the same shape instance, so property modifications go for each
        readonly Dictionary<string, ShapeInfo> _shapeInfoByPropName;

        public IReadOnlyDictionary<string, ShapeInfo> ShapeInfoByPropName => _shapeInfoByPropName;

        public ClrScriptObjectShape(Dictionary<string, ShapeInfo> shapeInfoByPropName)
        {
            _shapeInfoByPropName = new Dictionary<string, ShapeInfo>();

            foreach (var item in shapeInfoByPropName)
            {
                SetShapeInfoForProp(item.Key, item.Value);
            }
        }

        public void SetShapeInfoForProp(string propName, ShapeInfo shapeInfo)
        {
            //if (shapeInfo is TypeShape typeShape && typeShape.InferredType.IsValueType)
            //{
            //    shapeInfo = new TypeShape(typeof(Nullable<>)
            //        .MakeGenericType(typeShape.InferredType));
            //}

            _shapeInfoByPropName[propName] = shapeInfo;
        }
    }
}
