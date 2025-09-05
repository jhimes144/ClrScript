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
            var typeBuildersByShape = new Dictionary<ClrScriptObjectShape, TypeBuilder>();
            var masterObjectShapes = new HashSet<ClrScriptObjectShape>();

            foreach (var shape in _registeredObjectShapes)
            {
                masterObjectShapes.Add(shape.GetMasterShape());
            }

            foreach (var objShape in masterObjectShapes)
            {
                var typeBuilder = moduleBuilder.DefineType
                    ($"ClrScriptGen_{Guid.NewGuid().ToString().Replace('-', '_')}",
                    TypeAttributes.Public,
                    typeof(ClrScriptObject));

                typeBuildersByShape[objShape] = typeBuilder;
            }

            foreach (var (shape, builder) in typeBuildersByShape)
            {
                foreach (var (prop, propShape) in shape.ShapeInfoByPropName)
                {
                    Type type;

                    if (propShape is ClrScriptObjectShape objShape)
                    {
                        type = typeBuildersByShape[objShape];
                    }
                    else
                    {
                        type = propShape.InferredType;
                    }

                    var fieldBuilder = builder.DefineField(prop, type, FieldAttributes.Public);

                    var attributeConstructor = typeof(ClrScriptMemberAttribute).GetConstructor(Type.EmptyTypes);
                    var attributeBuilder = new CustomAttributeBuilder(attributeConstructor, new object[0]);
                    fieldBuilder.SetCustomAttribute(attributeBuilder);
                }
            }

            foreach (var (shape, builder) in typeBuildersByShape)
            {
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

            if (masterShape == null)
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

            if (masterShape is ClrArrayObjectShape a1
                && sourceShape is ClrArrayObjectShape a2)
            {
                return masterShape;
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
                        o1.ShapeInfoByPropName[prop] = DeriveShape(propShape, otherPropShape);
                        intersectingPropNames.Add(prop);
                    }
                }

                foreach (var (prop, propShape) in o2.ShapeInfoByPropName.ToArray())
                {
                    if (!intersectingPropNames.Contains(prop))
                    {
                        o1.ShapeInfoByPropName[prop] = propShape;
                    }
                }

                o2.ParentShape = o1;
                return masterShape;
            }

            return new UnknownShape();
        }
    }

    abstract class ShapeInfo
    {
        public abstract Type InferredType { get; }
    }

    class UnknownShape : ShapeInfo
    {
        public override Type InferredType => typeof(object);
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

        public Type GeneratedClrScriptObjType { get; set; }

        // This is on purpose mutable dictionary, because it translates to reference tracking of
        // objects
        // i.e this can handle
        // var obj = { name: "bob" };
        // var obj2 = obj;
        // obj.name = 12; // name gets updated to unknown shape
        // // ^ The shape for obj and obj2 is the same shape instance, so property modifications go for each

        public Dictionary<string, ShapeInfo> ShapeInfoByPropName { get; }

        public ClrScriptObjectShape(Dictionary<string, ShapeInfo> shapeInfoByPropName)
        {
            ShapeInfoByPropName = shapeInfoByPropName;
        }
    }

    class ClrArrayObjectShape : ShapeInfo
    {
        public override Type InferredType => typeof(ClrScriptArray);

        public ShapeInfo ArrayShape { get; }
    }
}
