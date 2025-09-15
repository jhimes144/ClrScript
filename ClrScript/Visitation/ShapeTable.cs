using ClrScript.Elements;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    class ShapeTable
    {
        readonly Dictionary<Element, ShapeInfo> _shapeInfoByElement 
            = new Dictionary<Element, ShapeInfo>();

        readonly HashSet<ClrScriptObjectShape> _registeredObjectShapes 
            = new HashSet<ClrScriptObjectShape>();

        public ShapeInfo InTypeShape { get; }

        public ShapeTable(Type inType)
        {
            InTypeShape = new TypeShape(inType);
        }

        public void GenerateRuntimeTypes(ModuleBuilder moduleBuilder)
        {
            generateObjTypes(moduleBuilder);
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

        public ShapeInfo GetShape(Element element)
        {
            return _shapeInfoByElement.GetValueOrDefault(element);
        }

        public void SetShape(Element element, ShapeInfo shapeInfo, bool overrid = false)
        {
            if (!overrid && _shapeInfoByElement.ContainsKey(element) && shapeInfo != null)
            {
                throw new Exception("Element already has shape defined.");
            }

            _shapeInfoByElement[element] = shapeInfo;

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
                && sourceShape is MethodShape m2
                && m1.InferredType == m2.InferredType)
            {
                // this cannot be inferred type
                return masterShape;
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
        /// The inferred type. IMPORTANT: This is usually not accessible until after types are generated
        /// UNLESS the shape is a TypeShape.
        /// </summary>
        public abstract Type InferredType { get; }
    }

    class UnknownShape : ShapeInfo
    {
        public override Type InferredType => typeof(object);

        public static UnknownShape Instance { get; } = new UnknownShape();
    }

    class UndeterminedShape : UnknownShape
    {
        public static new UndeterminedShape Instance { get; } = new UndeterminedShape();
    }

    class TypeShape : ShapeInfo
    {
        public override Type InferredType { get; }

        public TypeShape(Type inferredType)
        {
            InferredType = inferredType;
        }
    }

    class MethodShape : ShapeInfo
    {
        public override Type InferredType => Return?.InferredType;

        public bool IsTypeMethod { get; }

        public TypeShape DelegateShape { get; }

        public ShapeInfo Return { get; }

        public IReadOnlyList<ShapeInfo> Arguments { get; }

        public MethodShape(ShapeInfo @return, IReadOnlyList<ShapeInfo> arguments)
        {
            IsTypeMethod = true;
            Return = @return;
            Arguments = arguments;
        }

        public MethodShape(TypeShape delegateShape, ShapeInfo @return, IReadOnlyList<ShapeInfo> arguments)
        {
            DelegateShape = delegateShape;
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
        public override Type InferredType => GetMasterShape().GeneratedClrScriptObjType
            ?? throw new Exception("Clr type not generated");

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

        public Type GeneratedClrScriptObjType { get; set; }

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
