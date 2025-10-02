using ClrScript.Elements;
using ClrScript.Elements.Expressions;
using ClrScript.Elements.Statements;
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

        public TypeShape InTypeShape { get; }

        public ShapeTable(Type inType)
        {
            InTypeShape = new TypeShape(inType);
        }

        public OldTypeGenerator CreateTypeGenerator(TypeBuilder clrScriptGenType)
        {
            return new OldTypeGenerator(_rootShapeCollection, _registeredObjectShapes, clrScriptGenType, InTypeShape);
        }

        public ShapeInfo GetShape(Element element)
        {
            return _rootShapeCollection.GetValueOrDefault(element);
        }

        public void SetShape(Element element, ShapeInfo shapeInfo)
        {
            _rootShapeCollection[element] = shapeInfo;

            if (shapeInfo is ClrScriptObjectShape objectShape)
            {
                _registeredObjectShapes.Add(objectShape);
            }
        }

        public ShapeInfo DeriveShape(Element element1, Element element2)
        {
            return DeriveShape(GetShape(element1), GetShape(element2));
        }

        public ShapeInfo DeriveShape(ShapeInfo masterShape, ShapeInfo sourceShape, bool noDynDerive = false)
        {
            if (sourceShape == null)
            {
                throw new Exception("Shape wasn't defined");
            }

            if (masterShape == null || masterShape is OldUndeterminedShape)
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

            if (masterShape is OldMethodShape m1
                && sourceShape is OldMethodShape m2)
            {
                return OldUnknownShape.Instance;
            }

            //if (masterShape is MethodReturnShape mR1
            //    && sourceShape is MethodReturnShape mR2)
            //{
            //    if (!noDynDerive)
            //    {
            //        return new DynDeriveShape(this, mR1, mR2);
            //    }
            //    else
            //    {
            //        return DeriveShape(mR1.PointsTo, mR2.PointsTo, noDynDerive);
            //    }
            //}

            if (masterShape is ClrScriptArrayShape a1
                && sourceShape is ClrScriptArrayShape a2)
            {
                var derivedContentShape = DeriveShape(a1.ContentShape, a2.ContentShape, noDynDerive);
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
                        o1.SetShapeInfoForProp(prop, DeriveShape(propShape, otherPropShape, noDynDerive));
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

            // special case -> Test -> Calls.Lambda_Dyn_Stowaway
            if (masterShape is OldMethodShape stowawayMethod)
            {
                // if were here, its because a method is being
                // sent down a dynamic code path.
                // we cannot perform any optimizations on this method

                stowawayMethod.IsStowaway = true;
            }

            return OldUnknownShape.Instance;
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

    class OldUnknownShape : ShapeInfo
    {
        public override Type InferredType => typeof(object);

        public static OldUnknownShape Instance { get; } = new OldUnknownShape();

        public override bool Equals(object obj)
        {
            return obj is OldUnknownShape other;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(OldUnknownShape left, OldUnknownShape right)
        {
            return true;
        }

        public static bool operator !=(OldUnknownShape left, OldUnknownShape right)
        {
            return false;
        }
    }

    class OldUndeterminedShape : OldUnknownShape
    {
        public static new OldUndeterminedShape Instance { get; } = new OldUndeterminedShape();

        public override bool Equals(object obj)
        {
            return obj is OldUndeterminedShape other;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(OldUndeterminedShape left, OldUndeterminedShape right)
        {
            return true;
        }

        public static bool operator !=(OldUndeterminedShape left, OldUndeterminedShape right)
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

    class OldMethodShape : ShapeInfo
    {
        public override Type InferredType => CallSignature?.DelegateType;

        /// <summary>
        /// Indicates method is not a lambda
        /// </summary>
        public bool IsTypeMethod { get; }

        public MethodCallSignature CallSignature { get; set; }

        public bool IsStowaway { get; set; }

        // we can be sure the declaration belongs to this shape, because if a variable or member
        // changes, than the the method shape is discarded
        public Lambda Declaration { get; }

        public OldMethodShape(ShapeInfo @return, List<ShapeInfo> arguments)
        {
            IsTypeMethod = true;
            CallSignature = new MethodCallSignature(@return, arguments, Array.Empty<VarStmt>());
        }

        public OldMethodShape(Lambda declaration)
        {
            Declaration = declaration;
        }
    }

    class MethodCallSignature
    {
        public ShapeInfo Return { get; }

        public IReadOnlyList<ShapeInfo> Arguments { get; }

        public IReadOnlyList<VarStmt> VariableCaptures { get; }

        public MethodBuilder GenMethodBuilder { get; set; }

        public Type DelegateType => Util.CreateDelegateType(Return.InferredType, 
            Arguments.Select(a => a.InferredType).ToArray());

        public FieldInfo LambdaCacheField { get; set; }

        public TypeBuilder MethodContainerType { get; set; }

        public MethodCallSignature(ShapeInfo @return, IReadOnlyList<ShapeInfo> arguments,
            IReadOnlyList<VarStmt> variableCaptures)
        {
            Return = @return;
            Arguments = arguments;
            VariableCaptures = variableCaptures ?? new List<VarStmt>();
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
